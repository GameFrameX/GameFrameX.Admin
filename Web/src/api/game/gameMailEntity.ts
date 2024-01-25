import request from '/@/utils/request';
enum Api {
  AddGameMailEntity = '/api/gameMailEntity/add',
  DeleteGameMailEntity = '/api/gameMailEntity/delete',
  UpdateGameMailEntity = '/api/gameMailEntity/update',
  PageGameMailEntity = '/api/gameMailEntity/page',
}

// 增加游戏邮件
export const addGameMailEntity = (params?: any) =>
	request({
		url: Api.AddGameMailEntity,
		method: 'post',
		data: params,
	});

// 删除游戏邮件
export const deleteGameMailEntity = (params?: any) => 
	request({
			url: Api.DeleteGameMailEntity,
			method: 'post',
			data: params,
		});

// 编辑游戏邮件
export const updateGameMailEntity = (params?: any) => 
	request({
			url: Api.UpdateGameMailEntity,
			method: 'post',
			data: params,
		});

// 分页查询游戏邮件
export const pageGameMailEntity = (params?: any) => 
	request({
			url: Api.PageGameMailEntity,
			method: 'post',
			data: params,
		});


