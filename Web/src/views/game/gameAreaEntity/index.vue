<template>
  <div class="gameAreaEntity-container">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }">
      <el-form :model="queryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="关键字">
              <el-input v-model="queryParams.searchKey" clearable="" placeholder="请输入模糊查询关键字"/>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="是否是默认">
              <el-select clearable="" v-model="queryParams.isDefault" placeholder="请选择是否是默认">
                <el-option v-for="(item,index) in getEnumIsDefaultData_Index" :key="index" :value="item.value" :label="`[${item.value}] ${item.describe}`" />
                
              </el-select>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="是否启用">
              <el-select clearable="" v-model="queryParams.isEnable" placeholder="请选择是否启用">
                <el-option v-for="(item,index) in getEnumIsEnableData_Index" :key="index" :value="item.value" :label="`[${item.value}] ${item.describe}`" />
                
              </el-select>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="名称">
              <el-input v-model="queryParams.name" clearable="" placeholder="请输入名称"/>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="状态">
              <el-select clearable="" v-model="queryParams.serverStatus" placeholder="请选择状态">
                <el-option v-for="(item,index) in getEnumServerStatusData_Index" :key="index" :value="item.value" :label="`[${item.value}] ${item.describe}`" />
                
              </el-select>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="区域">
              <el-input v-model="queryParams.region" clearable="" placeholder="请输入区域"/>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="地址">
              <el-input v-model="queryParams.address" clearable="" placeholder="请输入地址"/>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="端口号">
              <el-input-number v-model="queryParams.port"  clearable="" placeholder="请输入端口号"/>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="最大玩家数量">
              <el-input-number v-model="queryParams.capacity"  clearable="" placeholder="请输入最大玩家数量"/>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="游戏版本号">
              <el-input v-model="queryParams.version" clearable="" placeholder="请输入游戏版本号"/>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="服务器的游戏模式类型">
              <el-input v-model="queryParams.serverType" clearable="" placeholder="请输入服务器的游戏模式类型"/>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="语言">
              <el-select clearable="" v-model="queryParams.language" placeholder="请选择语言">
                <el-option v-for="(item,index) in getEnumLanguageData_Index" :key="index" :value="item.value" :label="`[${item.value}] ${item.describe}`" />
                
              </el-select>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="6" :xl="6" class="mb10">
            <el-form-item>
              <el-button-group>
                <el-button type="primary"  icon="ele-Search" @click="handleQuery" v-auth="'gameAreaEntity:page'"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="() => queryParams = {}"> 重置 </el-button>
                <el-button icon="ele-ZoomIn" @click="changeAdvanceQueryUI" v-if="!showAdvanceQueryUI"> 高级 </el-button>
                <el-button icon="ele-ZoomOut" @click="changeAdvanceQueryUI" v-if="showAdvanceQueryUI"> 隐藏 </el-button>
                
              </el-button-group>
              
              <el-button-group style="margin-left:20px">
                <el-button type="primary" icon="ele-Plus" @click="openAddGameAreaEntity" v-auth="'gameAreaEntity:add'"> 新增 </el-button>
                
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
          <el-table-column prop="isDefault" label="是否是默认" width="140" show-overflow-tooltip="" >
            <template #default="scope">
              <el-tag>{{ getEnumDesc(scope.row.isDefault, getEnumIsDefaultData_Index)}}</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="isEnable" label="是否启用" width="140" show-overflow-tooltip="" >
            <template #default="scope">
              <el-tag>{{ getEnumDesc(scope.row.isEnable, getEnumIsEnableData_Index)}}</el-tag>
            </template>
          </el-table-column>
        <el-table-column prop="name" label="名称" width="140" show-overflow-tooltip="" />
        <el-table-column prop="description" label="详细描述" width="140" show-overflow-tooltip="" />
        <el-table-column prop="createUserName" label="创建者姓名" width="140" show-overflow-tooltip="" />
        <el-table-column prop="updateUserName" label="修改者姓名" width="140" show-overflow-tooltip="" />
          <el-table-column prop="serverStatus" label="状态" width="140" show-overflow-tooltip="" >
            <template #default="scope">
              <el-tag>{{ getEnumDesc(scope.row.serverStatus, getEnumServerStatusData_Index)}}</el-tag>
            </template>
          </el-table-column>
        <el-table-column prop="region" label="区域" width="140" show-overflow-tooltip="" />
        <el-table-column prop="address" label="地址" width="140" show-overflow-tooltip="" />
        <el-table-column prop="port" label="端口号" width="140" show-overflow-tooltip="" />
        <el-table-column prop="capacity" label="最大玩家数量" width="90" show-overflow-tooltip="" />
        <el-table-column prop="currentLoad" label="目前的玩家数量" width="105" show-overflow-tooltip="" />
        <el-table-column prop="creationDate" label="启用的日期" width="140" show-overflow-tooltip="" />
        <el-table-column prop="lastMaintenanceDate" label="服务器最后一次维护或更新的日期" width="225" show-overflow-tooltip="" />
        <el-table-column prop="version" label="游戏版本号" width="140" show-overflow-tooltip="" />
        <el-table-column prop="serverType" label="服务器的游戏模式类型" width="150" show-overflow-tooltip="" />
          <el-table-column prop="language" label="语言" width="140" show-overflow-tooltip="" >
            <template #default="scope">
              <el-tag>{{ getEnumDesc(scope.row.language, getEnumLanguageData_Index)}}</el-tag>
            </template>
          </el-table-column>
        <el-table-column prop="timeZone" label="时区" width="140" show-overflow-tooltip="" />
        <el-table-column prop="allowedPlayerProfile" label="指定可以加入此服务器的玩家类型" width="225" show-overflow-tooltip="" />
        <el-table-column prop="serverEvents" label="特定服务器举行的事件或活动信息" width="225" show-overflow-tooltip="" />
        <el-table-column prop="accessControls" label="访问权限设置，控制哪些玩家可以访问服务器" width="300" show-overflow-tooltip="" />
        <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip="" v-if="auth('gameAreaEntity:edit') || auth('gameAreaEntity:delete')">
          <template #default="scope">
            <el-button icon="ele-Edit" size="small" text="" type="primary" @click="openEditGameAreaEntity(scope.row)" v-auth="'gameAreaEntity:edit'"> 编辑 </el-button>
            <el-button icon="ele-Delete" size="small" text="" type="primary" @click="delGameAreaEntity(scope.row)" v-auth="'gameAreaEntity:delete'"> 删除 </el-button>
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
        :title="editGameAreaEntityTitle"
        @reloadTable="handleQuery"
      />
    </el-card>
  </div>
</template>

<script lang="ts" setup="" name="gameAreaEntity">
import { ref } from "vue";
import { ElMessageBox, ElMessage } from "element-plus";
import { auth } from '/@/utils/authFunction';
import { getDictDataItem as di, getDictDataList as dl } from '/@/utils/dict-utils';
//import { formatDate } from '/@/utils/formatTime';

import editDialog from '/@/views/game/gameAreaEntity/component/editDialog.vue'
import { pageGameAreaEntity, deleteGameAreaEntity } from '/@/api/game/gameAreaEntity';
	import { getAPI } from '/@/utils/axios-utils';
	import { SysEnumApi } from '/@/api-services/api';
  import commonFunction from '/@/utils/commonFunction';

  const getEnumIsDefaultData_Index = ref<any>([]);
  const getEnumIsEnableData_Index = ref<any>([]);
  const getEnumServerStatusData_Index = ref<any>([]);
  const getEnumLanguageData_Index = ref<any>([]);

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
const editGameAreaEntityTitle = ref("");

// 改变高级查询的控件显示状态
const changeAdvanceQueryUI = () => {
  showAdvanceQueryUI.value = !showAdvanceQueryUI.value;
}
  

// 查询操作
const handleQuery = async () => {
  loading.value = true;
  let res = await pageGameAreaEntity(Object.assign(queryParams.value, tableParams.value));
  tableData.value = res.data.result?.items ?? [];
  tableParams.value.total = res.data.result?.total;
  loading.value = false;
    getEnumIsDefaultData_Index.value = (await getAPI(SysEnumApi).apiSysEnumEnumDataListGet('YesNoEnum')).data.result ?? [];
    getEnumIsEnableData_Index.value = (await getAPI(SysEnumApi).apiSysEnumEnumDataListGet('YesNoEnum')).data.result ?? [];
    getEnumServerStatusData_Index.value = (await getAPI(SysEnumApi).apiSysEnumEnumDataListGet('GameServerStatus')).data.result ?? [];
    getEnumLanguageData_Index.value = (await getAPI(SysEnumApi).apiSysEnumEnumDataListGet('GameLanguage')).data.result ?? [];
};

// 打开新增页面
const openAddGameAreaEntity = () => {
  editGameAreaEntityTitle.value = '添加游戏区服';
  editDialogRef.value.isEditor= false;
  editDialogRef.value.openDialog({});
};

// 打开编辑页面
const openEditGameAreaEntity = (row: any) => {
  editGameAreaEntityTitle.value = '编辑游戏区服';
  editDialogRef.value.isEditor= true;
  editDialogRef.value.openDialog(row);
};

// 删除
const delGameAreaEntity = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  })
      .then(async () => {
        await deleteGameAreaEntity(row);
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

