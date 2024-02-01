import request from '/@/utils/request';
enum Api {
  AddGameBannerAnnouncementEntity = '/api/gameBannerAnnouncementEntity/add',
  DeleteGameBannerAnnouncementEntity = '/api/gameBannerAnnouncementEntity/delete',
  UpdateGameBannerAnnouncementEntity = '/api/gameBannerAnnouncementEntity/update',
  PageGameBannerAnnouncementEntity = '/api/gameBannerAnnouncementEntity/page',
}

// 增加游戏内横幅公告
export const addGameBannerAnnouncementEntity = (params?: any) =>
	request({
		url: Api.AddGameBannerAnnouncementEntity,
		method: 'post',
		data: params,
	});

// 删除游戏内横幅公告
export const deleteGameBannerAnnouncementEntity = (params?: any) => 
	request({
			url: Api.DeleteGameBannerAnnouncementEntity,
			method: 'post',
			data: params,
		});

// 编辑游戏内横幅公告
export const updateGameBannerAnnouncementEntity = (params?: any) => 
	request({
			url: Api.UpdateGameBannerAnnouncementEntity,
			method: 'post',
			data: params,
		});

// 分页查询游戏内横幅公告
export const pageGameBannerAnnouncementEntity = (params?: any) => 
	request({
			url: Api.PageGameBannerAnnouncementEntity,
			method: 'post',
			data: params,
		});


