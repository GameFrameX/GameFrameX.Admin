import { createServer } from 'node:http';
import { pathToFileURL } from 'node:url';

// C404 集成轮：通用 Online 查询桩（隔离栈用）。
// 区服基础夹具（E2E-区服一/二）指向本桩；gmCenter 各页发起的 Online 查询 action
// 一律返回 Foundation 成功信封 + 空 Data（页面渲染空态），三个合服 action 保持由
// server-merge.mjs 按契约应答；其余未知 action 返回失败信封（不崩溃——首页在线采样
// 等后台请求也会打到本桩）。
// 端口：MERGE_ONLINE_STUB_PORT（默认 21960）。无依赖，Node 原生 http。
const port = Number(process.env.ONLINE_GENERIC_STUB_PORT ?? 21960);

const server = createServer(async (request, response) => {
  response.setHeader('content-type', 'application/json');
  const reply = (code, data, message = 'ok') => response.end(JSON.stringify({
    code: 0, message: 'ok', data: JSON.stringify({ Code: code, MessageKey: message, Message: message, RequestId: 'online-generic-stub', Data: data }),
  }));
  try {
    const action = new URL(request.url, 'http://127.0.0.1').pathname.split('/').at(-1);
    if (request.method !== 'POST') {
      response.statusCode = 404;
      return reply(4000, null, 'unknown-action');
    }
    let body = '';
    for await (const chunk of request) {
      body += chunk;
      if (body.length > 65536) throw new Error('request-too-large');
    }
    if (body) JSON.parse(body);
    // Online 查询类 action 全部空成功：Data 必须是 JSON 对象（OnlineServerClient 把缺失 Data 判为协议错误）。
    return reply(0, {}, 'ok');
  } catch (error) {
    return reply(4000, null, error.message);
  }
});

server.listen(port, process.env.E2E_STUB_HOST ?? '127.0.0.1', () => process.stdout.write(`Online generic stub: http://127.0.0.1:${port}/online/admin\n`));
for (const signal of ['SIGTERM', 'SIGINT']) process.on(signal, () => server.close());
export { server };

if (process.argv[1] && import.meta.url === pathToFileURL(process.argv[1]).href) {
  // 独立运行入口已由上方 listen 覆盖；此分支仅为模块语义完整。
}
