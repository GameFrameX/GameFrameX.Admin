import request from '/@/utils/request';
enum Api {
  AddGameRoleEntity = '/api/gameRoleEntity/add',
  DeleteGameRoleEntity = '/api/gameRoleEntity/delete',
  UpdateGameRoleEntity = '/api/gameRoleEntity/update',
  PageGameRoleEntity = '/api/gameRoleEntity/page',
}

// 增加游戏角色
export const addGameRoleEntity = (params?: any) =>
	request({
		url: Api.AddGameRoleEntity,
		method: 'post',
		data: params,
	});

// 删除游戏角色
export const deleteGameRoleEntity = (params?: any) => 
	request({
			url: Api.DeleteGameRoleEntity,
			method: 'post',
			data: params,
		});

// 编辑游戏角色
export const updateGameRoleEntity = (params?: any) => 
	request({
			url: Api.UpdateGameRoleEntity,
			method: 'post',
			data: params,
		});

// 分页查询游戏角色
export const pageGameRoleEntity = (params?: any) => 
	request({
			url: Api.PageGameRoleEntity,
			method: 'post',
			data: params,
		});


