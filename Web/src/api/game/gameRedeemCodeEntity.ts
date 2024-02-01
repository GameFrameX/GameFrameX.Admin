import request from '/@/utils/request';
enum Api {
  AddGameRedeemCodeEntity = '/api/gameRedeemCodeEntity/add',
  DeleteGameRedeemCodeEntity = '/api/gameRedeemCodeEntity/delete',
  UpdateGameRedeemCodeEntity = '/api/gameRedeemCodeEntity/update',
  PageGameRedeemCodeEntity = '/api/gameRedeemCodeEntity/page',
}

// 增加兑换码
export const addGameRedeemCodeEntity = (params?: any) =>
	request({
		url: Api.AddGameRedeemCodeEntity,
		method: 'post',
		data: params,
	});

// 删除兑换码
export const deleteGameRedeemCodeEntity = (params?: any) => 
	request({
			url: Api.DeleteGameRedeemCodeEntity,
			method: 'post',
			data: params,
		});

// 编辑兑换码
export const updateGameRedeemCodeEntity = (params?: any) => 
	request({
			url: Api.UpdateGameRedeemCodeEntity,
			method: 'post',
			data: params,
		});

// 分页查询兑换码
export const pageGameRedeemCodeEntity = (params?: any) => 
	request({
			url: Api.PageGameRedeemCodeEntity,
			method: 'post',
			data: params,
		});


