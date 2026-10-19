import { test } from 'node:test'
import assert from 'node:assert/strict'
import {
  CONTROLLED_COMMAND_META,
  CONTROLLED_COMMAND_KINDS,
  REASON_MAX_LENGTH,
  createControlledCommandForm,
  validateControlledCommandForm,
  buildControlledCommandRequest,
} from './commandForm.ts'

// C356 联调对齐 —— 受控操作表单纯逻辑单测。
// 契约：command 数字枚举严格四值（1~4）；目标标识按命令恰好一个；
// direction（1=Apply 2=Lift）开关型必填 / 单向禁填；理由必填；
// reviewerOperatorId 全部命令必填（双人复核全量口径）；请求只携带命令对应目标字段。

const validForm = () => ({
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

test('命令四值元数据：数字枚举 + 语义名 + 目标字段 + 开关型/高危分级', () => {
  assert.deepEqual([...CONTROLLED_COMMAND_KINDS].sort(), [1, 2, 3, 4])
  assert.deepEqual(CONTROLLED_COMMAND_KINDS.map((kind) => CONTROLLED_COMMAND_META[kind].name), [
    'CancelTicket', 'EndAbnormalMatch', 'SuspendResumePlayer', 'MatchIsolation',
  ])
  assert.equal(CONTROLLED_COMMAND_META[1].targetField, 'ticketId')
  assert.equal(CONTROLLED_COMMAND_META[2].targetField, 'matchId')
  assert.equal(CONTROLLED_COMMAND_META[3].targetField, 'playerId')
  assert.equal(CONTROLLED_COMMAND_META[4].targetField, 'playerId')
  // direction 分级：开关型（3/4）必填，单向（1/2）禁填
  assert.equal(CONTROLLED_COMMAND_META[1].directional, false)
  assert.equal(CONTROLLED_COMMAND_META[2].directional, false)
  assert.equal(CONTROLLED_COMMAND_META[3].directional, true)
  assert.equal(CONTROLLED_COMMAND_META[4].directional, true)
  // 高危分级（提交前危险确认）
  assert.equal(CONTROLLED_COMMAND_META[1].highRisk, false)
  assert.equal(CONTROLLED_COMMAND_META[2].highRisk, false)
  assert.equal(CONTROLLED_COMMAND_META[3].highRisk, true)
  assert.equal(CONTROLLED_COMMAND_META[4].highRisk, true)
})

test('未选命令 → commandKind 必选错误', () => {
  const form = { ...validForm(), commandKind: '' }
  const errors = validateControlledCommandForm(form)
  assert.ok(errors.commandKind)
})

test('目标标识按命令类型恰好校验对应字段', () => {
  // CancelTicket：ticketId 空 → 错误；matchId/playerId 填了也不参与校验
  const noTicket = { ...validForm(), ticketId: '', matchId: 'M-1', playerId: 'p-1' }
  assert.ok(validateControlledCommandForm(noTicket).ticketId)

  // EndAbnormalMatch：matchId 必填
  const noMatch = { ...validForm(), commandKind: 2, ticketId: '', matchId: '' }
  assert.ok(validateControlledCommandForm(noMatch).matchId)

  // SuspendResumePlayer / MatchIsolation：playerId 必填
  const noPlayer = { ...validForm(), commandKind: 3, ticketId: '', playerId: '' }
  assert.ok(validateControlledCommandForm(noPlayer).playerId)
  const noPlayerIso = { ...validForm(), commandKind: 4, ticketId: '', playerId: '' }
  assert.ok(validateControlledCommandForm(noPlayerIso).playerId)
})

test('direction：开关型必填（1/2 合法），单向命令禁填', () => {
  // SuspendResumePlayer 开关型：direction 空 → 错误
  const missing = { ...validForm(), commandKind: 3, ticketId: '', playerId: 'player-100', direction: '' }
  assert.ok(validateControlledCommandForm(missing).direction)
  // 越界值（3）同样非法
  assert.ok(validateControlledCommandForm({ ...missing, direction: 3 }).direction)
  // 1=Apply / 2=Lift 均通过
  assert.equal(validateControlledCommandForm({ ...missing, direction: 1 }).direction, undefined)
  assert.equal(validateControlledCommandForm({ ...missing, direction: 2 }).direction, undefined)
  // CancelTicket 单向：填 direction → 错误；缺省 → 通过
  assert.ok(validateControlledCommandForm({ ...validForm(), direction: 1 }).direction)
  assert.equal(validateControlledCommandForm(validForm()).direction, undefined)
})

test('理由必填 + 长度上限 500', () => {
  assert.ok(validateControlledCommandForm({ ...validForm(), reason: '   ' }).reason)
  assert.ok(validateControlledCommandForm({ ...validForm(), reason: 'x'.repeat(REASON_MAX_LENGTH + 1) }).reason)
  const errors = validateControlledCommandForm({ ...validForm(), reason: 'x'.repeat(REASON_MAX_LENGTH) })
  assert.equal(errors.reason, undefined)
})

test('复核人全部命令必填（双人复核全量口径）', () => {
  // 低危 CancelTicket：空白复核人 → 错误
  const low = { ...validForm(), reviewerOperatorId: '   ' }
  assert.ok(validateControlledCommandForm(low).reviewerOperatorId)
  // 开关型 SuspendResumePlayer：同样必填
  const toggle = {
    ...validForm(), commandKind: 3, ticketId: '', playerId: 'player-100', direction: 1, reviewerOperatorId: '',
  }
  assert.ok(validateControlledCommandForm(toggle).reviewerOperatorId)
  assert.equal(validateControlledCommandForm({ ...toggle, reviewerOperatorId: 'op-reviewer' }).reviewerOperatorId, undefined)
})

test('请求组装：command 数字 + 恰好一个目标字段 + 开关型带 direction + reviewer 必带', () => {
  const form = {
    ...validForm(),
    commandKind: 3,
    ticketId: 'SHOULD-NOT-SEND',
    matchId: 'SHOULD-NOT-SEND',
    playerId: 'player-100',
    direction: 2,
  }
  const req = buildControlledCommandRequest(form)
  assert.equal(req.command, 3)
  assert.equal(req.appId, '9000000000001')
  assert.equal(req.serverId, 'srv-1')
  assert.equal(req.playerId, 'player-100')
  assert.equal(req.direction, 2)
  assert.equal(req.reason, 'abnormal matchmaking stuck')
  assert.equal(req.reviewerOperatorId, 'op-reviewer')
  assert.equal('ticketId' in req, false)
  assert.equal('matchId' in req, false)
})

test('单向命令请求不携带 direction 字段', () => {
  const req = buildControlledCommandRequest(validForm())
  assert.equal(req.command, 1)
  assert.equal('direction' in req, false)
})

test('组装请求剥离目标字段两侧空白', () => {
  const req = buildControlledCommandRequest({ ...validForm(), ticketId: '  TICKET-1  ', reason: '  reason  ' })
  assert.equal(req.ticketId, 'TICKET-1')
  assert.equal(req.reason, 'reason')
})

test('createControlledCommandForm：空表单初值 + query 预填 playerId/serverId', () => {
  const blank = createControlledCommandForm()
  assert.equal(blank.commandKind, '')
  assert.equal(blank.direction, '')
  assert.equal(blank.ticketId, '')
  assert.equal(blank.playerId, '')
  assert.equal(blank.reason, '')
  assert.equal(blank.reviewerOperatorId, '')
  const prefilled = createControlledCommandForm({ playerId: 'player-9', serverId: 'srv-2' })
  assert.equal(prefilled.playerId, 'player-9')
  assert.equal(prefilled.serverId, 'srv-2')
})
