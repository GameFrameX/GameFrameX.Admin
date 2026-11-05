export interface GameAreaMergePlanResponse {
  id: number
  rowVersion?: number
  createdTime: number
  targetServerId: number
  sourceServerIds: string
  planId: string
  status: number
  dryRunReportSnapshot?: string
  executionReportSnapshot?: string
  applicantOperatorId: number
  reviewerOperatorId?: number
  reason: string
  backupIdentifier?: string
  backupConfirmed: boolean
  dryRunTime?: number
  approvedTime?: number
  executionStartedTime?: number
  completedTime?: number
  failedTime?: number
  failureMessage?: string
}
