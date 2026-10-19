/**
 * 受控操作表单纯逻辑（C344 / GFX-761；C356 联调对齐）。
 *
 * 契约（后端 OnlineControlledCommand 数字枚举）：
 *   - command 严格四值：1=CancelTicket 2=EndAbnormalMatch 3=SuspendResumePlayer 4=MatchIsolation；
 *   - 目标标识按命令类型恰好一个：1→ticketId、2→matchId、3/4→playerId；
 *   - direction（1=Apply 2=Lift）：开关型命令（3/4）必填，单向命令（1/2）禁填；
 *   - 理由必填（≤500 字，进审计）；
 *   - reviewerOperatorId 全部四命令必填（双人复核全量口径，不再仅高危）；
 *   - 请求体只携带命令对应目标字段，防误传（buildRequest 显式白名单式组装）。
 * 无外部依赖：Vue 组件与 node --test 单测（commandForm.test.mjs）共用本文件。
 */
import type { OnlineControlledCommandRequest, OnlineControlledCommandKind, ControlledCommandDirection } from '../../../../api/onlineControlledCommand/types'

export type ControlledCommandTargetField = 'ticketId' | 'matchId' | 'playerId'

/** 命令语义名（i18n kinds.<name> 索引；与后端枚举名一致）。 */
export type ControlledCommandName = 'CancelTicket' | 'EndAbnormalMatch' | 'SuspendResumePlayer' | 'MatchIsolation'

export interface ControlledCommandMeta {
    kind: OnlineControlledCommandKind
    name: ControlledCommandName
    targetField: ControlledCommandTargetField
    /** 开关型：direction 必填（1=Apply 2=Lift）；单向命令禁填。 */
    directional: boolean
    /** 高危：提交前危险确认。 */
    highRisk: boolean
}

export const CONTROLLED_COMMAND_META: Record<OnlineControlledCommandKind, ControlledCommandMeta> = {
    1: { kind: 1, name: 'CancelTicket', targetField: 'ticketId', directional: false, highRisk: false },
    2: { kind: 2, name: 'EndAbnormalMatch', targetField: 'matchId', directional: false, highRisk: false },
    3: { kind: 3, name: 'SuspendResumePlayer', targetField: 'playerId', directional: true, highRisk: true },
    4: { kind: 4, name: 'MatchIsolation', targetField: 'playerId', directional: true, highRisk: true },
}

export const CONTROLLED_COMMAND_KINDS = Object.keys(CONTROLLED_COMMAND_META).map(Number) as OnlineControlledCommandKind[]

export const REASON_MAX_LENGTH = 500

export interface ControlledCommandFormState {
    commandKind: OnlineControlledCommandKind | ''
    appId: string | number
    serverId: string
    ticketId: string
    matchId: string
    playerId: string
    direction: ControlledCommandDirection | ''
    reason: string
    reviewerOperatorId: string
}

/** 空表单（query 预填 playerId / serverId——时间线页受控操作入口跳转承载）。 */
export function createControlledCommandForm(prefill: { playerId?: string; serverId?: string } = {}): ControlledCommandFormState {
    return {
        commandKind: '',
        appId: '',
        serverId: prefill.serverId ?? '',
        ticketId: '',
        matchId: '',
        playerId: prefill.playerId ?? '',
        direction: '',
        reason: '',
        reviewerOperatorId: '',
    }
}

const trim = (value: string): string => value.trim()

/**
 * 表单校验：返回错误映射（空对象 = 通过）。
 * key：commandKind / 目标字段 / direction / reason / reviewerOperatorId。
 */
export function validateControlledCommandForm(form: ControlledCommandFormState): Record<string, boolean> {
    const errors: Record<string, boolean> = {}
    const kind = form.commandKind
    const meta = kind ? CONTROLLED_COMMAND_META[kind] : undefined
    if (!meta) {
        errors.commandKind = true
        return errors
    }
    if (!trim(form[meta.targetField])) {
        errors[meta.targetField] = true
    }
    const isDirection = form.direction === 1 || form.direction === 2
    if (meta.directional) {
        if (!isDirection) {
            errors.direction = true
        }
    } else if (form.direction !== '') {
        errors.direction = true
    }
    const reason = trim(form.reason)
    if (!reason) {
        errors.reason = true
    } else if (reason.length > REASON_MAX_LENGTH) {
        errors.reason = true
    }
    if (!trim(form.reviewerOperatorId)) {
        errors.reviewerOperatorId = true
    }
    return errors
}

/**
 * 组装执行请求：command 为数字枚举；目标字段只携带 meta.targetField 对应项
 * （显式白名单，防误传）；direction 仅开关型命令携带；
 * reviewerOperatorId 全命令必填（validate 已保证非空）。
 */
export function buildControlledCommandRequest(form: ControlledCommandFormState): OnlineControlledCommandRequest {
    const kind = form.commandKind
    if (!kind) {
        throw new Error('controlled command kind is required (validate before build)')
    }
    const meta = CONTROLLED_COMMAND_META[kind]
    const request: OnlineControlledCommandRequest = {
        command: meta.kind,
        appId: String(form.appId),
        serverId: trim(form.serverId),
        reason: trim(form.reason),
        reviewerOperatorId: trim(form.reviewerOperatorId),
    }

    if (meta.targetField === 'ticketId') {
        request.ticketId = trim(form.ticketId)
    } else if (meta.targetField === 'matchId') {
        request.matchId = trim(form.matchId)
    } else {
        request.playerId = trim(form.playerId)
    }
    if (meta.directional) {
        request.direction = form.direction as ControlledCommandDirection
    }
    return request
}
