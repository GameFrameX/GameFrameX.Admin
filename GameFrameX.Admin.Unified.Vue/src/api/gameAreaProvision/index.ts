import request from '../../utils/request';

export interface GameAreaProvisionTemplateResponse { id: number; name: string }
export interface GameAreaProvisionPreviewResponse { templateId: number; clonedFields: Record<string, string | null>; requiredFields: string[] }
export interface GameAreaProvisionCreateRequest {
  templateId: number; serverId: number; name: string; httpManageUrl: string;
  tcpSocketUrl: string; webSocketUrl: string; serverParameters: string;
}
export interface GameAreaProvisionStatusResponse {
  serverId: number; name: string; recordCreated: boolean; containerCreated: boolean;
  registered: boolean; isEnabled: boolean; openedAtUnixMilliseconds: number; mergedIntoServerId: number;
}
interface Envelope<T> { code: number; data: T; message: string; errorCode?: string }
export const gameAreaProvisionTemplatesGet = () => request.get<Envelope<GameAreaProvisionTemplateResponse[]>>('/api/gameAreaProvision/templates');
export const gameAreaProvisionPreviewGet = (query: { id: number }) => request.get<Envelope<GameAreaProvisionPreviewResponse>>('/api/gameAreaProvision/preview', { params: query });
export const gameAreaProvisionCreatePost = (data: GameAreaProvisionCreateRequest) => request.post<Envelope<GameAreaProvisionStatusResponse>>('/api/gameAreaProvision/create', data);
export const gameAreaProvisionExecutePost = (data: { id: number }) => request.post<Envelope<GameAreaProvisionStatusResponse>>('/api/gameAreaProvision/execute', data, { timeout: 180000 });
export const gameAreaProvisionStatusGet = (query: { id: number }) => request.get<Envelope<GameAreaProvisionStatusResponse>>('/api/gameAreaProvision/status', { params: query });
