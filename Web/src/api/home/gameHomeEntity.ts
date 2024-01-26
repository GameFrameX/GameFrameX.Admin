import request from '/@/utils/request';

enum Api {
    GetOnlineGameHomeEntity = '/api/gameHomeEntity/GetOnline',
}

// 获取在线玩家
export const getOnlineGameHomeEntity = (params?: any) =>
    request({
        url: Api.GetOnlineGameHomeEntity,
        method: 'post',
        data: params,
    });
