import request from '/@/utils/request';
enum Api {
  AddGameAreaEntity = '/api/gameAreaEntity/add',
  DeleteGameAreaEntity = '/api/gameAreaEntity/delete',
  UpdateGameAreaEntity = '/api/gameAreaEntity/update',
  PageGameAreaEntity = '/api/gameAreaEntity/page',
}

// 增加游戏区服
export const addGameAreaEntity = (params?: any) =>
	request({
		url: Api.AddGameAreaEntity,
		method: 'post',
		data: params,
	});

// 删除游戏区服
export const deleteGameAreaEntity = (params?: any) => 
	request({
			url: Api.DeleteGameAreaEntity,
			method: 'post',
			data: params,
		});

// 编辑游戏区服
export const updateGameAreaEntity = (params?: any) => 
	request({
			url: Api.UpdateGameAreaEntity,
			method: 'post',
			data: params,
		});

// 分页查询游戏区服
export const pageGameAreaEntity = (params?: any) => 
	request({
			url: Api.PageGameAreaEntity,
			method: 'post',
			data: params,
		});


