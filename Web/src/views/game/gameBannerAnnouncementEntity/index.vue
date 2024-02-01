<template>
  <div class="gameBannerAnnouncementEntity-container">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }">
      <el-form :model="queryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="关键字">
              <el-input v-model="queryParams.searchKey" clearable="" placeholder="请输入模糊查询关键字"/>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="区服ID">
              <el-select clearable="" v-model="queryParams.areaId" placeholder="请选择区服ID">
                <el-option v-for="(item,index) in dl('code_gen_query_type')" :key="index" :value="item.code" :label="`[${item.code}] ${item.value}`" />
                
              </el-select>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="内容">
              <el-input v-model="queryParams.content" clearable="" placeholder="请输入内容"/>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="优先级">
              <el-input-number v-model="queryParams.priority"  clearable="" placeholder="请输入优先级"/>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="间隔时间">
              <el-input-number v-model="queryParams.interval"  clearable="" placeholder="请输入间隔时间"/>
              
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="8" class="mb10">
            <el-form-item>
              <el-button-group >
                <el-button type="primary" icon="ele-Plus" @click="openAddGameBannerAnnouncementEntity" v-auth="'gameBannerAnnouncementEntity:add'"> 新增 </el-button>
                <el-button type="primary"  icon="ele-Search" @click="handleQuery" v-auth="'gameBannerAnnouncementEntity:page'"> 查询 </el-button>
                <el-button icon="ele-Refresh" @click="() => queryParams = {}"> 重置 </el-button>
                <el-button icon="ele-ZoomIn" @click="changeAdvanceQueryUI" v-if="!showAdvanceQueryUI"> 高级 </el-button>
                <el-button icon="ele-ZoomOut" @click="changeAdvanceQueryUI" v-if="showAdvanceQueryUI"> 隐藏 </el-button>
                
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
          <el-table-column prop="areaId" label="区服ID" width="140" show-overflow-tooltip="" >
            <template #default="scope">
              <el-tag :type="di('code_gen_query_type', scope.row.areaId)?.tagType"> {{di("code_gen_query_type", scope.row.areaId)?.value}} </el-tag>
            </template>
          </el-table-column>
        <el-table-column prop="content" label="内容" width="140" show-overflow-tooltip="" />
        <el-table-column prop="priority" label="优先级" width="140" show-overflow-tooltip="" />
        <el-table-column prop="startTime" label="开始时间" width="140" show-overflow-tooltip="" />
        <el-table-column prop="endTime" label="结束时间" width="140" show-overflow-tooltip="" />
        <el-table-column prop="interval" label="间隔时间" width="140" show-overflow-tooltip="" />
        <el-table-column prop="isActive" label="是否激活" width="120" show-overflow-tooltip="">
          <template #default="scope">
            <el-tag v-if="scope.row.isActive"> 是 </el-tag>
            <el-tag type="danger" v-else> 否 </el-tag>
            
          </template>
          
        </el-table-column>
        <el-table-column prop="createUserName" label="创建者姓名" width="140" show-overflow-tooltip="" />
        <el-table-column prop="updateUserName" label="修改者姓名" width="140" show-overflow-tooltip="" />
        <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip="" v-if="auth('gameBannerAnnouncementEntity:edit') || auth('gameBannerAnnouncementEntity:delete')">
          <template #default="scope">
            <el-button icon="ele-Edit" size="small" text="" type="primary" @click="openEditGameBannerAnnouncementEntity(scope.row)" v-auth="'gameBannerAnnouncementEntity:edit'"> 编辑 </el-button>
            <el-button icon="ele-Delete" size="small" text="" type="primary" @click="delGameBannerAnnouncementEntity(scope.row)" v-auth="'gameBannerAnnouncementEntity:delete'"> 删除 </el-button>
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
        :title="editGameBannerAnnouncementEntityTitle"
        @reloadTable="handleQuery"
      />
    </el-card>
  </div>
</template>

<script lang="ts" setup="" name="gameBannerAnnouncementEntity">
import { ref } from "vue";
import { ElMessageBox, ElMessage } from "element-plus";
import { auth } from '/@/utils/authFunction';
import { getDictDataItem as di, getDictDataList as dl } from '/@/utils/dict-utils';
//import { formatDate } from '/@/utils/formatTime';

import editDialog from '/@/views/game/gameBannerAnnouncementEntity/component/editDialog.vue'
import { pageGameBannerAnnouncementEntity, deleteGameBannerAnnouncementEntity } from '/@/api/game/gameBannerAnnouncementEntity';


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
const editGameBannerAnnouncementEntityTitle = ref("");

// 改变高级查询的控件显示状态
const changeAdvanceQueryUI = () => {
  showAdvanceQueryUI.value = !showAdvanceQueryUI.value;
}
  

// 查询操作
const handleQuery = async () => {
  loading.value = true;
  let res = await pageGameBannerAnnouncementEntity(Object.assign(queryParams.value, tableParams.value));
  tableData.value = res.data.result?.items ?? [];
  tableParams.value.total = res.data.result?.total;
  loading.value = false;
};

// 打开新增页面
const openAddGameBannerAnnouncementEntity = () => {
  editGameBannerAnnouncementEntityTitle.value = '添加游戏内横幅公告';
  editDialogRef.value.isEditor= false;
  editDialogRef.value.openDialog({});
};

// 打开编辑页面
const openEditGameBannerAnnouncementEntity = (row: any) => {
  editGameBannerAnnouncementEntityTitle.value = '编辑游戏内横幅公告';
  editDialogRef.value.isEditor= true;
  editDialogRef.value.openDialog(row);
};

// 删除
const delGameBannerAnnouncementEntity = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  })
      .then(async () => {
        await deleteGameBannerAnnouncementEntity(row);
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

