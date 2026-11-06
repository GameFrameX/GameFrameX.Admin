import { createServer } from 'node:http';

const port = Number(process.env.E2E_PROVISION_STUB_PORT ?? 22240);
if (!Number.isInteger(port) || !((port >= 22200 && port <= 22299) || (port >= 21900 && port <= 21999))) {
  throw new Error('Provisioning stub requires an isolated provisioning or integration port.');
}
const containers = new Map();
const calendars = new Map();
const failures = new Map();
const registrationFailures = Number(process.env.E2E_PROVISION_REGISTRATION_FAILURES ?? 0);
function online(data, code = 0) {
  // Foundation data 是 JSON 字符串；与 OnlineServerClientTests.SuccessEnvelope 相同。
  return { code: 0, message: '', data: JSON.stringify({ Code: code, MessageKey: code ? 'server.not_ready' : 'ok',
    RequestId: 'provision-stub', Data: data }), trackId: null, errorCode: null, type: null, time: Math.floor(Date.now() / 1000), extras: null };
}
const server = createServer(async (request, response) => {
  try {
    if (request.method !== 'POST') { response.writeHead(405).end(); return; }
    const chunks = [];
    for await (const chunk of request) chunks.push(chunk);
    const body = JSON.parse(Buffer.concat(chunks).toString() || '{}');
    const url = new URL(request.url, `http://127.0.0.1:${port}`);
    let result;
    if (url.pathname === '/api/Containers/CreateAndStart') {
      if (!body.ContainerName || body.IsRecreate !== false) throw new Error('Container identity or recreate flag missing');
      const name = String(body.ContainerName);
      if (!containers.has(name)) containers.set(name, `provision-container-${name}`);
      result = { Code: 0, Message: '', Data: containers.get(name) };
    } else if (url.pathname === '/api/Containers/Stop' || url.pathname === '/api/Containers/Restart') {
      result = { Code: 0, Message: '', Data: body.Id };
    } else if (url.pathname === '/online/admin/register_server_open' || url.pathname === '/online/admin/query_server_calendar') {
      const serverId = Number(body.ServerId);
      if (!Number.isSafeInteger(serverId) || serverId <= 0) throw new Error('ServerId required');
      if (url.pathname.endsWith('/register_server_open')) {
        const attempts = failures.get(serverId) ?? 0;
        if (attempts < registrationFailures) {
          failures.set(serverId, attempts + 1);
          result = online(null, 8001);
        } else {
          const firstRegistration = !calendars.has(serverId);
          if (firstRegistration) calendars.set(serverId, Number(body.OpenedAtUnixMilliseconds ?? Date.now()));
          result = online({ ServerId: serverId, OpenedAtUnixMilliseconds: calendars.get(serverId), FirstRegistration: firstRegistration });
        }
      } else {
        result = online({ ServerId: serverId, Registered: calendars.has(serverId), OpenedAtUnixMilliseconds: calendars.get(serverId) ?? 0,
          MergedIntoServerId: 0, OpenServerDay: calendars.has(serverId) ? 1 : 0 });
      }
    } else { response.writeHead(404).end(); return; }
    response.writeHead(200, { 'Content-Type': 'application/json' }).end(JSON.stringify(result));
  } catch (error) {
    response.writeHead(400, { 'Content-Type': 'application/json' }).end(JSON.stringify({ code: 400, message: error.message }));
  }
});
server.listen(port, process.env.E2E_STUB_HOST ?? '127.0.0.1', () => process.stdout.write(`provision-ready:${port}\n`));
for (const signal of ['SIGINT', 'SIGTERM']) {
  process.on(signal, () => { server.closeAllConnections(); server.close(() => process.exit(0)); });
}
