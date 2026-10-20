import { test, expect } from 'vitest'
import {
  CONTROLLED_COMMAND_META,
  CONTROLLED_COMMAND_KINDS,
  REASON_MAX_LENGTH,
  createControlledCommandForm,
  validateControlledCommandForm,
  buildControlledCommandRequest,
  type ControlledCommandFormState,
} from './commandForm'

// C356 联调对齐 —— 受控操作表单纯逻辑单测。
// 契约：command 数字枚举严格四值（1~4）；目标标识按命令恰好一个；
// direction（1=Apply 2=Lift）开关型必填 / 单向禁填；理由必填；
// reviewerOperatorId 全部命令必填（双人复核全量口径）；请求只携带命令对应目标字段。

const validForm = (): ControlledCommandFormState => ({
  commandKind: 1,
  appId: '9000000000001',
  serverId: 'srv-1',
  ticketId: 'TICKET-1',
  matchId: '',
  playerId: '',
  direction: '',
  reason: 'abnormal matchmaking stuck',
  reviewerOperatorId: 'op-reviewer',
})

// 多达 8 个测试通过 spread + 局部覆盖构造 form；helper 让每条断言保持对 source 类型 narrow。
const f = (overrides: Partial<ControlledCommandFormState>): ControlledCommandFormState => ({ ...validForm(), ...overrides })

test('命令四值元数据：数字枚举 + 语义名 + 目标字段 + 开关型/高危分级', () => {
  expect([...CONTROLLED_COMMAND_KINDS].sort()).toEqual([1, 2, 3, 4])
  expect(CONTROLLED_COMMAND_KINDS.map((kind) => CONTROLLED_COMMAND_META[kind].name)).toEqual([
    'CancelTicket', 'EndAbnormalMatch', 'SuspendResumePlayer', 'MatchIsolation',
  ])
  expect(CONTROLLED_COMMAND_META[1].targetField).toBe('ticketId')
  expect(CONTROLLED_COMMAND_META[2].targetField).toBe('matchId')
  expect(CONTROLLED_COMMAND_META[3].targetField).toBe('playerId')
  expect(CONTROLLED_COMMAND_META[4].targetField).toBe('playerId')
  // direction 分级：开关型（3/4）必填，单向（1/2）禁填
  expect(CONTROLLED_COMMAND_META[1].directional).toBe(false)
  expect(CONTROLLED_COMMAND_META[2].directional).toBe(false)
  expect(CONTROLLED_COMMAND_META[3].directional).toBe(true)
  expect(CONTROLLED_COMMAND_META[4].directional).toBe(true)
  // 高危分级（提交前危险确认）
  expect(CONTROLLED_COMMAND_META[1].highRisk).toBe(false)
  expect(CONTROLLED_COMMAND_META[2].highRisk).toBe(false)
  expect(CONTROLLED_COMMAND_META[3].highRisk).toBe(true)
  expect(CONTROLLED_COMMAND_META[4].highRisk).toBe(true)
})

test('未选命令 → commandKind 必选错误', () => {
  expect(validateControlledCommandForm(f({ commandKind: '' })).commandKind).toBeTruthy()
})

test('目标标识按命令类型恰好校验对应字段', () => {
  // CancelTicket：ticketId 空 → 错误；matchId/playerId 填了也不参与校验
  expect(validateControlledCommandForm(f({ ticketId: '', matchId: 'M-1', playerId: 'p-1' })).ticketId).toBeTruthy()

  // EndAbnormalMatch：matchId 必填
  expect(validateControlledCommandForm(f({ commandKind: 2, ticketId: '', matchId: '' })).matchId).toBeTruthy()

  // SuspendResumePlayer / MatchIsolation：playerId 必填
  expect(validateControlledCommandForm(f({ commandKind: 3, ticketId: '', playerId: '' })).playerId).toBeTruthy()
  expect(validateControlledCommandForm(f({ commandKind: 4, ticketId: '', playerId: '' })).playerId).toBeTruthy()
})

test('direction：开关型必填（1/2 合法），单向命令禁填', () => {
  // SuspendResumePlayer 开关型：direction 空 → 错误
  const missing = f({ commandKind: 3, ticketId: '', playerId: 'player-100', direction: '' })
  expect(validateControlledCommandForm(missing).direction).toBeTruthy()
  // 越界值（3）同样非法
  expect(validateControlledCommandForm(f({ ...missing, direction: 3 as ControlledCommandFormState['direction'] })).direction).toBeTruthy()
  // 1=Apply / 2=Lift 均通过
  expect(validateControlledCommandForm(f({ ...missing, direction: 1 })).direction).toBe(undefined)
  expect(validateControlledCommandForm(f({ ...missing, direction: 2 })).direction).toBe(undefined)
  // CancelTicket 单向：填 direction → 错误；缺省 → 通过
  expect(validateControlledCommandForm(f({ direction: 1 })).direction).toBeTruthy()
  expect(validateControlledCommandForm(validForm()).direction).toBe(undefined)
})

test('理由必填 + 长度上限 500', () => {
  expect(validateControlledCommandForm(f({ reason: '   ' })).reason).toBeTruthy()
  expect(validateControlledCommandForm(f({ reason: 'x'.repeat(REASON_MAX_LENGTH + 1) })).reason).toBeTruthy()
  expect(validateControlledCommandForm(f({ reason: 'x'.repeat(REASON_MAX_LENGTH) })).reason).toBe(undefined)
})

test('复核人全部命令必填（双人复核全量口径）', () => {
  // 低危 CancelTicket：空白复核人 → 错误
  expect(validateControlledCommandForm(f({ reviewerOperatorId: '   ' })).reviewerOperatorId).toBeTruthy()
  // 开关型 SuspendResumePlayer：同样必填
  const toggle = f({ commandKind: 3, ticketId: '', playerId: 'player-100', direction: 1, reviewerOperatorId: '' })
  expect(validateControlledCommandForm(toggle).reviewerOperatorId).toBeTruthy()
  expect(validateControlledCommandForm(f({ ...toggle, reviewerOperatorId: 'op-reviewer' })).reviewerOperatorId).toBe(undefined)
})

test('请求组装：command 数字 + 恰好一个目标字段 + 开关型带 direction + reviewer 必带', () => {
  const req = buildControlledCommandRequest(
    f({
      commandKind: 3,
      ticketId: 'SHOULD-NOT-SEND',
      matchId: 'SHOULD-NOT-SEND',
      playerId: 'player-100',
      direction: 2,
    }),
  )
  expect(req.command).toBe(3)
  expect(req.appId).toBe('9000000000001')
  expect(req.serverId).toBe('srv-1')
  expect(req.playerId).toBe('player-100')
  expect(req.direction).toBe(2)
  expect(req.reason).toBe('abnormal matchmaking stuck')
  expect(req.reviewerOperatorId).toBe('op-reviewer')
  expect('ticketId' in req).toBe(false)
  expect('matchId' in req).toBe(false)
})

test('单向命令请求不携带 direction 字段', () => {
  const req = buildControlledCommandRequest(validForm())
  expect(req.command).toBe(1)
  expect('direction' in req).toBe(false)
})

test('组装请求剥离目标字段两侧空白', () => {
  const req = buildControlledCommandRequest(f({ ticketId: '  TICKET-1  ', reason: '  reason  ' }))
  expect(req.ticketId).toBe('TICKET-1')
  expect(req.reason).toBe('reason')
})

test('createControlledCommandForm：空表单初值 + query 预填 playerId/serverId', () => {
  const blank = createControlledCommandForm()
  expect(blank.commandKind).toBe('')
  expect(blank.direction).toBe('')
  expect(blank.ticketId).toBe('')
  expect(blank.playerId).toBe('')
  expect(blank.reason).toBe('')
  expect(blank.reviewerOperatorId).toBe('')
  const prefilled = createControlledCommandForm({ playerId: 'player-9', serverId: 'srv-2' })
  expect(prefilled.playerId).toBe('player-9')
  expect(prefilled.serverId).toBe('srv-2')
})
