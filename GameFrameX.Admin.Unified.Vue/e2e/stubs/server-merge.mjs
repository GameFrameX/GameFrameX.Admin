import { createServer } from 'node:http';
import { pathToFileURL } from 'node:url';

// 无依赖 ServerLifecycle 桩；仅运行在隔离端口，source id 线缆必须为逗号字符串。
export function createMergeStub() {
  const plans = new Map();
  return createServer(async (request, response) => {
    response.setHeader('content-type', 'application/json');
    const reply = (code, data, message = 'ok') => response.end(JSON.stringify({
      code: 0, message: 'ok', data: JSON.stringify({ Code: code, MessageKey: message,
        Message: message, RequestId: 'merge-stub', Data: data }),
    }));
    try {
      const action = new URL(request.url, 'http://127.0.0.1').pathname.split('/').at(-1);
      if (request.method !== 'POST' || !['plan_server_merge', 'dry_run_server_merge', 'execute_server_merge'].includes(action)) {
        response.statusCode = 404;
        return reply(4000, null, 'unknown-action');
      }
      let body = '';
      for await (const chunk of request) {
        body += chunk;
        if (body.length > 65536) throw new Error('request-too-large');
      }
      const payload = JSON.parse(body);
      const target = Number(payload.TargetServerId);
      if (typeof payload.SourceServerIds !== 'string') throw new Error('sources-must-be-comma-separated');
      const sources = payload.SourceServerIds.split(',').map(Number).sort((left, right) => left - right);
      if (!Number.isSafeInteger(target) || target <= 0 || Number(payload.ServerId) !== target || sources.length === 0
        || sources.some(source => !Number.isSafeInteger(source) || source <= 0 || source === target)
        || new Set(sources).size !== sources.length) throw new Error('invalid-server-scope');
      const key = `${target}:${sources.join(',')}`;
      if (action === 'plan_server_merge') {
        if (!plans.has(key)) plans.set(key, { planId: `merge-${target}-${sources.join('-')}`, rehearsed: false, executed: false });
        const plan = plans.get(key);
        return reply(0, { PlanId: plan.planId, TargetServerId: target, SourceServerIds: sources });
      }
      const plan = plans.get(key);
      if (!plan || (action === 'execute_server_merge' && !plan.rehearsed)) return reply(5000, null, 'dry-run-required');
      const alreadyExecuted = plan.executed;
      const dryRun = action === 'dry_run_server_merge';
      if (dryRun) plan.rehearsed = true;
      else plan.executed = true;
      return reply(0, {
        PlanId: plan.planId, TargetServerId: target, SourceServerIds: sources, DryRun: dryRun,
        AlreadyExecuted: alreadyExecuted, RowKeySchemaVersion: 1, GlobalDomainRowsTouched: 0,
        Steps: [{ Step: 'players', AffectedCount: sources.length * 10, Details: { sourceRows: sources.length * 10 } },
          { Step: 'routing', AffectedCount: sources.length, Details: { redirectedServers: sources.length } }],
      });
    } catch (error) { return reply(4000, null, error.message); }
  });
}

if (process.argv[1] && import.meta.url === pathToFileURL(process.argv[1]).href) {
  const port = Number(process.env.MERGE_STUB_PORT ?? 22390);
  const server = createMergeStub();
  server.listen(port, process.env.E2E_STUB_HOST ?? '127.0.0.1', () => process.stdout.write(`Server merge stub: http://127.0.0.1:${port}/online/admin\n`));
  for (const signal of ['SIGTERM', 'SIGINT']) process.on(signal, () => server.close());
}
