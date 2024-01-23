import request from '/@/utils/request';
enum Api {
  AddClientPackage = '/api/clientPackage/add',
  DeleteClientPackage = '/api/clientPackage/delete',
  UpdateClientPackage = '/api/clientPackage/update',
  PageClientPackage = '/api/clientPackage/page',
  SelectClientPackage='/api/clientPackage/Select',
}

// 增加包管理
export const addClientPackage = (params?: any) =>
	request({
		url: Api.AddClientPackage,
		method: 'post',
		data: params,
	});

// 删除包管理
export const deleteClientPackage = (params?: any) => 
	request({
			url: Api.DeleteClientPackage,
			method: 'post',
			data: params,
		});

// 编辑包管理
export const updateClientPackage = (params?: any) => 
	request({
			url: Api.UpdateClientPackage,
			method: 'post',
			data: params,
		});

// 分页查询包管理
export const pageClientPackage = (params?: any) => 
	request({
			url: Api.PageClientPackage,
			method: 'post',
			data: params,
		});
export const selectClientPackage = (params?: any) =>
	request({
		url: Api.SelectClientPackage,
		method: 'post',
		data: params,
	});


