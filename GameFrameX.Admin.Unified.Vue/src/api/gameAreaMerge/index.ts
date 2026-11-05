import request from '../../utils/request'
import type { GameAreaMergePlanResponse } from './types'

interface MergeEnvelope<T> { code: number; data: T }
interface MergePage { items: GameAreaMergePlanResponse[]; total: number }

export const gameAreaMergePagePost = (data: { page: number; pageSize: number }) =>
  request.post<MergeEnvelope<MergePage>>('/api/gameAreaMerge/page', data)
export const gameAreaMergeDetailGet = (data: { id: number }) =>
  request.get<MergeEnvelope<GameAreaMergePlanResponse>>('/api/gameAreaMerge/detail', { params: data })
export const gameAreaMergeAddPost = (data: { targetServerId: number; sourceServerIds: number[]; reason: string }) =>
  request.post<MergeEnvelope<GameAreaMergePlanResponse>>('/api/gameAreaMerge/add', data)
export const gameAreaMergeDryRunPost = (data: { id: number }) =>
  request.post<MergeEnvelope<GameAreaMergePlanResponse>>('/api/gameAreaMerge/dryRun', data)
export const gameAreaMergeApprovePost = (data: { id: number; reason: string; reviewerOperatorId: number }) =>
  request.post<MergeEnvelope<GameAreaMergePlanResponse>>('/api/gameAreaMerge/approve', data)
export const gameAreaMergeExecutePost = (data: { id: number; backupConfirmed: boolean; backupIdentifier: string }) =>
  request.post<MergeEnvelope<GameAreaMergePlanResponse>>('/api/gameAreaMerge/execute', data, { timeout: 0 })
