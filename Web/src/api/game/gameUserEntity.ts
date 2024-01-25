import request from '/@/utils/request';
enum Api {
  AddGameUserEntity = '/api/gameUserEntity/add',
  DeleteGameUserEntity = '/api/gameUserEntity/delete',
  UpdateGameUserEntity = '/api/gameUserEntity/update',
  PageGameUserEntity = '/api/gameUserEntity/page',
}

// 增加游戏账号
export const addGameUserEntity = (params?: any) =>
	request({
		url: Api.AddGameUserEntity,
		method: 'post',
		data: params,
	});

// 删除游戏账号
export const deleteGameUserEntity = (params?: any) => 
	request({
			url: Api.DeleteGameUserEntity,
			method: 'post',
			data: params,
		});

// 编辑游戏账号
export const updateGameUserEntity = (params?: any) => 
	request({
			url: Api.UpdateGameUserEntity,
			method: 'post',
			data: params,
		});

// 分页查询游戏账号
export const pageGameUserEntity = (params?: any) => 
	request({
			url: Api.PageGameUserEntity,
			method: 'post',
			data: params,
		});


