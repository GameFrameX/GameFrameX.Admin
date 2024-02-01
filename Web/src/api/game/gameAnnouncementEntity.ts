import request from '/@/utils/request';
enum Api {
  AddGameAnnouncementEntity = '/api/gameAnnouncementEntity/add',
  DeleteGameAnnouncementEntity = '/api/gameAnnouncementEntity/delete',
  UpdateGameAnnouncementEntity = '/api/gameAnnouncementEntity/update',
  PageGameAnnouncementEntity = '/api/gameAnnouncementEntity/page',
}

// 增加游戏公告
export const addGameAnnouncementEntity = (params?: any) =>
	request({
		url: Api.AddGameAnnouncementEntity,
		method: 'post',
		data: params,
	});

// 删除游戏公告
export const deleteGameAnnouncementEntity = (params?: any) => 
	request({
			url: Api.DeleteGameAnnouncementEntity,
			method: 'post',
			data: params,
		});

// 编辑游戏公告
export const updateGameAnnouncementEntity = (params?: any) => 
	request({
			url: Api.UpdateGameAnnouncementEntity,
			method: 'post',
			data: params,
		});

// 分页查询游戏公告
export const pageGameAnnouncementEntity = (params?: any) => 
	request({
			url: Api.PageGameAnnouncementEntity,
			method: 'post',
			data: params,
		});


