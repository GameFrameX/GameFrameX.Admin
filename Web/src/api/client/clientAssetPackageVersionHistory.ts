import request from '/@/utils/request';
enum Api {
  PageClientAssetPackageVersionHistory = '/api/clientAssetPackageVersionHistory/page',
}
// 分页查询资源包版本记录
export const pageClientAssetPackageVersionHistory = (params?: any) =>
	request({
			url: Api.PageClientAssetPackageVersionHistory,
			method: 'post',
			data: params,
		});


