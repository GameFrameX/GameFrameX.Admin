<template>
  <div class="gameuser-container">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }">
      <el-form :model="queryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="用户ID">
              <el-input v-model="queryParams.id" clearable="" placeholder="请输入Id"/>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="账号">
              <el-input v-model="queryParams.userName" clearable="" placeholder="请输入账号"/>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="状态">
              <el-select clearable="" v-model="queryParams.status" placeholder="请选择状态">
                <el-option v-for="(item,index) in getEnumStatusData_Index" :key="index" :value="item.value" :label="`[${item.value}]  ${item.name} ${item.describe}`" />
                
              </el-select>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="昵称">
              <el-input v-model="queryParams.name" clearable="" placeholder="请输入昵称"/>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="6" :xl="6" class="mb10">
            <el-form-item>
              <el-button-group>
                <el-button type="primary"  icon="ele-Search" @click="handleQuery" v-auth="'gameuser:page'"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="() => queryParams = {}"> 重置 </el-button>
                
              </el-button-group>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>
    <el-card class="full-table" shadow="hover" style="margin-top: 8px">
      <el-table
				:data="tableData"
				style="width: 100%"
				v-loading="loading"
				tooltip-effect="light"
				row-key="id"
				border="">
        <el-table-column type="index" label="序号" width="55" align="center"/>
        <el-table-column prop="id" label="用户ID" width="140" show-overflow-tooltip="" />
        <el-table-column prop="userName" label="账号" width="140" show-overflow-tooltip="" />
          <el-table-column prop="status" label="状态" width="140" show-overflow-tooltip="" >
            <template #default="scope">
              <el-tag>{{ getEnumDesc(scope.row.status, getEnumStatusData_Index)}}</el-tag>
            </template>
          </el-table-column>
        <el-table-column prop="name" label="昵称" width="140" show-overflow-tooltip="" />
        <el-table-column prop="description" label="描述" width="140" show-overflow-tooltip="" />
        <el-table-column prop="updateUserName" label="最后更新人" width="140" show-overflow-tooltip="" />
        <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip="" v-if="auth('gameuser:edit') || auth('gameuser:delete')">
          <template #default="scope">
            <el-button icon="ele-Edit" size="small" text="" type="primary" @click="openEditGameUserEntity(scope.row)" v-auth="'gameuser:edit'"> 编辑 </el-button>
            <el-button icon="ele-Delete" size="small" text="" type="primary" @click="delGameUserEntity(scope.row)" v-auth="'gameuser:delete'"> 删除 </el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination
				v-model:currentPage="tableParams.page"
				v-model:page-size="tableParams.pageSize"
				:total="tableParams.total"
				:page-sizes="[10, 20, 50, 100, 200, 500]"
				small=""
				background=""
				@size-change="handleSizeChange"
				@current-change="handleCurrentChange"
				layout="total, sizes, prev, pager, next, jumper"
	/>
      <editDialog
        ref="editDialogRef"
        :title="editGameUserEntityTitle"
        @reloadTable="handleQuery"
      />
    </el-card>
  </div>
</template>

<script lang="ts" setup="" name="gameuser">
  import { ref } from "vue";
  import { ElMessageBox, ElMessage } from "element-plus";
  import { auth } from '/@/utils/authFunction';
  import { getDictDataItem as di, getDictDataList as dl } from '/src/utils/dict-utils';
  //import { formatDate } from '/@/utils/formatTime';

  import editDialog from '/@/views/game/gameuser/component/editDialog.vue'
  import { pageGameUserEntity, deleteGameUserEntity } from '/@/api/game/gameUserEntity';
	import { getAPI } from '/@/utils/axios-utils';
	import { SysEnumApi } from '/@/api-services/api';
  import commonFunction from '/@/utils/commonFunction';

  const getEnumStatusData_Index = ref<any>([]);

	const { getEnumDesc } = commonFunction();
  const showAdvanceQueryUI = ref(true);
  const editDialogRef = ref();
  const loading = ref(false);
  const tableData = ref<any>([]);
  const queryParams = ref<any>({});
  const tableParams = ref({
    page: 1,
    pageSize: 10,
    total: 0,
  });
  const editGameUserEntityTitle = ref("");

  // 改变高级查询的控件显示状态
  const changeAdvanceQueryUI = () => {
    showAdvanceQueryUI.value = !showAdvanceQueryUI.value;
  }
  

  // 查询操作
  const handleQuery = async () => {
    loading.value = true;
    var res = await pageGameUserEntity(Object.assign(queryParams.value, tableParams.value));
    tableData.value = res.data.result?.items ?? [];
    tableParams.value.total = res.data.result?.total;
    loading.value = false;
    getEnumStatusData_Index.value = (await getAPI(SysEnumApi).apiSysEnumEnumDataListGet('GameUserStatus')).data.result ?? [];
  };

  // 打开新增页面
  const openAddGameUserEntity = () => {
    editGameUserEntityTitle.value = '添加游戏账号';
    editDialogRef.value.openDialog({});
  };

  // 打开编辑页面
  const openEditGameUserEntity = (row: any) => {
    editGameUserEntityTitle.value = '编辑游戏账号';
    editDialogRef.value.openDialog(row);
  };

  // 删除
  const delGameUserEntity = (row: any) => {
    ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  })
  .then(async () => {
    await deleteGameUserEntity(row);
    handleQuery();
    ElMessage.success("删除成功");
  })
  .catch(() => {});
  };

  // 改变页面容量
  const handleSizeChange = (val: number) => {
    tableParams.value.pageSize = val;
    handleQuery();
  };

  // 改变页码序号
  const handleCurrentChange = (val: number) => {
    tableParams.value.page = val;
    handleQuery();
  };

  handleQuery();
</script>
<style scoped>
:deep(.el-ipnut),
:deep(.el-select),
:deep(.el-input-number) {
	width: 100%;
}
</style>

