<template>
  <div class="gameMailEntity-container">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }">
      <el-form :model="queryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="关键字">
              <el-input v-model="queryParams.searchKey" clearable="" placeholder="请输入模糊查询关键字"/>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="标题">
              <el-input v-model="queryParams.subject" clearable="" placeholder="请输入标题"/>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="详细内容">
              <el-input v-model="queryParams.body" clearable="" placeholder="请输入详细内容"/>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="邮件状态">
              <el-select clearable="" v-model="queryParams.status" placeholder="请选择邮件状态">
                <el-option v-for="(item,index) in getEnumStatusData_Index" :key="index" :value="item.value" :label="`[${item.value}] ${item.describe}`" />
                
              </el-select>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="邮件类型，例如个人邮件、系统邮件等。">
              <el-select clearable="" v-model="queryParams.mailType" placeholder="请选择邮件类型，例如个人邮件、系统邮件等。">
                <el-option v-for="(item,index) in getEnumMailTypeData_Index" :key="index" :value="item.value" :label="`[${item.value}] ${item.describe}`" />
                
              </el-select>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="6" :xl="6" class="mb10">
            <el-form-item>
              <el-button-group>
                <el-button type="primary"  icon="ele-Search" @click="handleQuery" v-auth="'gameMailEntity:page'"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="() => queryParams = {}"> 重置 </el-button>
                <el-button icon="ele-ZoomIn" @click="changeAdvanceQueryUI" v-if="!showAdvanceQueryUI"> 高级 </el-button>
                <el-button icon="ele-ZoomOut" @click="changeAdvanceQueryUI" v-if="showAdvanceQueryUI"> 隐藏 </el-button>
                
              </el-button-group>
              
              <el-button-group style="margin-left:20px">
                <el-button type="primary" icon="ele-Plus" @click="openAddGameMailEntity" v-auth="'gameMailEntity:add'"> 新增 </el-button>
                
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
        <el-table-column prop="areaIdList" label="区服列表" width="140" show-overflow-tooltip="" />
        <el-table-column prop="recipientId" label="接收者" width="140" show-overflow-tooltip="" />
        <el-table-column prop="sendDate" label="发送的时间" width="140" show-overflow-tooltip="" />
        <el-table-column prop="subject" label="标题" width="140" show-overflow-tooltip="" />
        <el-table-column prop="body" label="详细内容" width="140" show-overflow-tooltip="" />
        <el-table-column prop="attachments" label="附件列表" width="140" show-overflow-tooltip="" />
          <el-table-column prop="status" label="邮件状态" width="140" show-overflow-tooltip="" >
            <template #default="scope">
              <el-tag>{{ getEnumDesc(scope.row.status, getEnumStatusData_Index)}}</el-tag>
            </template>
          </el-table-column>
        <el-table-column prop="expirationDate" label="邮件的过期日期" width="105" show-overflow-tooltip="" />
          <el-table-column prop="mailType" label="邮件类型，例如个人邮件、系统邮件等。" width="270" show-overflow-tooltip="" >
            <template #default="scope">
              <el-tag>{{ getEnumDesc(scope.row.mailType, getEnumMailTypeData_Index)}}</el-tag>
            </template>
          </el-table-column>
        <el-table-column prop="createUserName" label="创建者姓名" width="140" show-overflow-tooltip="" />
        <el-table-column prop="updateUserName" label="修改者姓名" width="140" show-overflow-tooltip="" />
        <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip="" v-if="auth('gameMailEntity:edit') || auth('gameMailEntity:delete')">
          <template #default="scope">
            <el-button icon="ele-Edit" size="small" text="" type="primary" @click="openEditGameMailEntity(scope.row)" v-auth="'gameMailEntity:edit'"> 编辑 </el-button>
            <el-button icon="ele-Delete" size="small" text="" type="primary" @click="delGameMailEntity(scope.row)" v-auth="'gameMailEntity:delete'"> 删除 </el-button>
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
        :title="editGameMailEntityTitle"
        @reloadTable="handleQuery"
      />
    </el-card>
  </div>
</template>

<script lang="ts" setup="" name="gameMailEntity">
import { ref } from "vue";
import { ElMessageBox, ElMessage } from "element-plus";
import { auth } from '/@/utils/authFunction';
import { getDictDataItem as di, getDictDataList as dl } from '/@/utils/dict-utils';
//import { formatDate } from '/@/utils/formatTime';

import editDialog from '/@/views/game/gameMailEntity/component/editDialog.vue'
import { pageGameMailEntity, deleteGameMailEntity } from '/@/api/game/gameMailEntity';
	import { getAPI } from '/@/utils/axios-utils';
	import { SysEnumApi } from '/@/api-services/api';
  import commonFunction from '/@/utils/commonFunction';

  const getEnumStatusData_Index = ref<any>([]);
  const getEnumMailTypeData_Index = ref<any>([]);

	const { getEnumDesc } = commonFunction();
const showAdvanceQueryUI = ref(false);
const editDialogRef = ref();
const loading = ref(false);
const tableData = ref<any>([]);
const queryParams = ref<any>({});
const tableParams = ref({
  page: 1,
  pageSize: 10,
  total: 0,
});
const editGameMailEntityTitle = ref("");

// 改变高级查询的控件显示状态
const changeAdvanceQueryUI = () => {
  showAdvanceQueryUI.value = !showAdvanceQueryUI.value;
}
  

// 查询操作
const handleQuery = async () => {
  loading.value = true;
  let res = await pageGameMailEntity(Object.assign(queryParams.value, tableParams.value));
  tableData.value = res.data.result?.items ?? [];
  tableParams.value.total = res.data.result?.total;
  loading.value = false;
    getEnumStatusData_Index.value = (await getAPI(SysEnumApi).apiSysEnumEnumDataListGet('GameMailStatus')).data.result ?? [];
    getEnumMailTypeData_Index.value = (await getAPI(SysEnumApi).apiSysEnumEnumDataListGet('GameMailType')).data.result ?? [];
};

// 打开新增页面
const openAddGameMailEntity = () => {
  editGameMailEntityTitle.value = '添加游戏邮件';
  editDialogRef.value.isEditor= false;
  editDialogRef.value.openDialog({});
};

// 打开编辑页面
const openEditGameMailEntity = (row: any) => {
  editGameMailEntityTitle.value = '编辑游戏邮件';
  editDialogRef.value.isEditor= true;
  editDialogRef.value.openDialog(row);
};

// 删除
const delGameMailEntity = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  })
      .then(async () => {
        await deleteGameMailEntity(row);
        handleQuery();
        ElMessage.success("删除成功");
      })
      .catch(() => {
        
      });
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

