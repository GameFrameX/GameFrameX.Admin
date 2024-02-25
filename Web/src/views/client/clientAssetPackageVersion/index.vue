<template>
  <div class="clientAssetPackageVersion-container">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }">
      <el-form :model="queryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="关键字">
              <el-input v-model="queryParams.searchKey" clearable="" placeholder="请输入模糊查询关键字"/>

            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="渠道名称">
              <el-select clearable="" v-model="queryParams.channel" placeholder="请选择渠道名称">
                <el-option v-for="item in querySelectorParams.channel" :key="item.value" :label="item.label" :value="item.value"><span>{{ item.value }} - {{ item.label }}</span></el-option>

              </el-select>

            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="平台">
              <el-select clearable="" v-model="queryParams.platform" placeholder="请选择平台">
                <el-option v-for="item in querySelectorParams.platform" :key="item.value" :label="item.label" :value="item.value"><span>{{ item.value }} - {{ item.label }}</span></el-option>

              </el-select>

            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="游戏版本">
              <el-select clearable="" v-model="queryParams.appVersion" placeholder="请选择游戏版本">
                <el-option v-for="item in querySelectorParams.appVersion" :key="item.value" :label="item.label" :value="item.value"><span>{{ item.value }} - {{ item.label }}</span></el-option>


              </el-select>

            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="包名">
              <el-select clearable="" v-model="queryParams.package" placeholder="请选择包名">
                <el-option v-for="item in querySelectorParams.package" :key="item.value" :label="item.label" :value="item.value"><span>{{ item.value }} - {{ item.label }}</span></el-option>


              </el-select>

            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="语言">
              <el-select clearable="" v-model="queryParams.language" placeholder="请选择语言">
                <el-option v-for="item in querySelectorParams.language" :key="item.value" :label="item.label" :value="item.value"><span>{{ item.value }} - {{ item.label }}</span></el-option>


              </el-select>

            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="showAdvanceQueryUI">
            <el-form-item label="资源包名称">
              <el-select clearable="" v-model="queryParams.assetPackageName" placeholder="请选择资源包名称">
                <el-option v-for="item in querySelectorParams.assetPackageName" :key="item.value" :label="item.label" :value="item.value"><span>{{ item.value }} - {{ item.label }}</span></el-option>


              </el-select>

            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="8" class="mb10">
            <el-form-item>
              <el-button-group>
                <el-button type="primary" icon="ele-Plus" @click="openAddClientAssetPackageVersion" v-auth="'clientAssetPackageVersion:add'"> 新增</el-button>

                <el-button type="primary" icon="ele-Search" @click="handleQuery" v-auth="'clientAssetPackageVersion:page'"> 查询</el-button>
                <el-button icon="ele-Refresh" @click="() => queryParams = {}"> 重置</el-button>
                <el-button icon="ele-ZoomIn" @click="changeAdvanceQueryUI" v-if="!showAdvanceQueryUI"> 高级</el-button>
                <el-button icon="ele-ZoomOut" @click="changeAdvanceQueryUI" v-if="showAdvanceQueryUI"> 隐藏</el-button>

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
        <el-table-column prop="channel" label="渠道名称" width="140" show-overflow-tooltip="">
        </el-table-column>
        <el-table-column prop="platform" label="平台" width="140" show-overflow-tooltip="">
        </el-table-column>
        <el-table-column prop="appVersion" label="游戏版本" width="140" show-overflow-tooltip="">
        </el-table-column>
        <el-table-column prop="package" label="包名" width="140" show-overflow-tooltip="">
        </el-table-column>
        <el-table-column prop="language" label="语言" width="140" show-overflow-tooltip="">
        </el-table-column>
        <el-table-column prop="assetPackageName" label="资源包名称" width="140" show-overflow-tooltip="">
        </el-table-column>
        <el-table-column prop="version" label="资源版本" width="140" show-overflow-tooltip=""/>
        <el-table-column prop="rootPath" label="资源下载根路径" width="140" show-overflow-tooltip=""/>
        <el-table-column prop="description" label="详细描述" width="140" show-overflow-tooltip=""/>
        <el-table-column prop="createUserName" label="创建者姓名" width="140" show-overflow-tooltip=""/>
        <el-table-column prop="updateUserName" label="修改者姓名" width="140" show-overflow-tooltip=""/>
        <el-table-column label="操作" width="200" align="center" fixed="right" show-overflow-tooltip="" v-if="auth('clientAssetPackageVersion:edit') || auth('clientAssetPackageVersion:delete')">
          <template #default="scope">
            <el-button icon="ele-Document" size="small" text="" type="primary" @click="openClientAssetPackageVersionHistory(scope.row)" v-auth="'clientAssetPackageVersion:history'"> 记录</el-button>
            <el-button icon="ele-Edit" size="small" text="" type="primary" @click="openEditClientAssetPackageVersion(scope.row)" v-auth="'clientAssetPackageVersion:edit'"> 编辑</el-button>
            <el-button icon="ele-Delete" size="small" text="" type="primary" @click="delClientAssetPackageVersion(scope.row)" v-auth="'clientAssetPackageVersion:delete'"> 删除</el-button>
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
          :title="editClientAssetPackageVersionTitle"
          @reloadTable="handleQuery"
      />
      <updateHistoryDialog
          ref="updateHistoryDialogRef"
          :title="historyClientAssetPackageVersionTitle"
          @reloadTable="handleQuery"
      />
    </el-card>
  </div>
</template>

<script lang="ts" setup="" name="clientAssetPackageVersion">
import {ref} from "vue";
import {ElMessageBox, ElMessage} from "element-plus";
import {auth} from '/@/utils/authFunction';
import {getDictDataItem as di, getDictDataList as dl} from '/@/utils/dict-utils';
//import { formatDate } from '/@/utils/formatTime';

import editDialog from '/@/views/client/clientAssetPackageVersion/component/editDialog.vue'
import updateHistoryDialog from '/@/views/client/clientAssetPackageVersion/component/updateHistory.vue'
import {pageClientAssetPackageVersion, deleteClientAssetPackageVersion} from '/@/api/client/clientAssetPackageVersion';
import {selectClientChannel} from "/@/api/client/channel";
import {selectClientPlatform} from "/@/api/client/appplatform";
import {selectClientVersion} from "/@/api/client/version";
import {selectClientPackage} from "/@/api/client/package";
import {selectClientLanguage} from "/@/api/client/language";
import {selectClientAssetPackage} from "/@/api/client/clientAssetPackage";


const showAdvanceQueryUI = ref(false);
const editDialogRef = ref();
const updateHistoryDialogRef = ref();
const loading = ref(false);
const tableData = ref<any>([]);
const queryParams = ref<any>({});
const querySelectorParams = ref<any>({});
const tableParams = ref({
  page: 1,
  pageSize: 10,
  total: 0,
});
const editClientAssetPackageVersionTitle = ref("");
const historyClientAssetPackageVersionTitle = ref("");

// 改变高级查询的控件显示状态
const changeAdvanceQueryUI = () => {
  showAdvanceQueryUI.value = !showAdvanceQueryUI.value;
}


// 查询操作
const handleQuery = async () => {
  loading.value = true;
  let res = await pageClientAssetPackageVersion(Object.assign(queryParams.value, tableParams.value));
  tableData.value = res.data.result?.items ?? [];
  tableParams.value.total = res.data.result?.total;
  loading.value = false;
};

// 打开新增页面
const openAddClientAssetPackageVersion = () => {
  editClientAssetPackageVersionTitle.value = '添加资源包版本';
  editDialogRef.value.isEditor = false;
  editDialogRef.value.openDialog({});
};

// 打开编辑页面
const openEditClientAssetPackageVersion = (row: any) => {
  editClientAssetPackageVersionTitle.value = '编辑资源包版本';
  editDialogRef.value.isEditor = true;
  editDialogRef.value.openDialog(row);
};
// 打开历史记录页面
const openClientAssetPackageVersionHistory = (row: any) => {
  historyClientAssetPackageVersionTitle.value = "历史记录"
  updateHistoryDialogRef.value.openDialog(row);
};

// 删除
const delClientAssetPackageVersion = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  })
      .then(async () => {
        await deleteClientAssetPackageVersion(row);
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


//
const handleQuerySelectorChannel = async () => {
  const res = await selectClientChannel({});
  querySelectorParams.value.channel = res.data.result;
};
//
handleQuerySelectorChannel();
//
const handleQuerySelectorPlatform = async () => {
  const res = await selectClientPlatform({});
  querySelectorParams.value.platform = res.data.result;
};
//
handleQuerySelectorPlatform();
//
const handleQuerySelectorAppVersion = async () => {
  const res = await selectClientVersion({});
  querySelectorParams.value.appVersion = res.data.result;
};
//
handleQuerySelectorAppVersion();
//
const handleQuerySelectorPackage = async () => {
  const res = await selectClientPackage({});
  querySelectorParams.value.package = res.data.result;
};
//
handleQuerySelectorPackage();
//
const handleQuerySelectorLanguage = async () => {
  const res = await selectClientLanguage({});
  querySelectorParams.value.language = res.data.result;
};
//
handleQuerySelectorLanguage();
//
const handleQuerySelectorAssetPackageName = async () => {
  const res = await selectClientAssetPackage({});
  querySelectorParams.value.assetPackageName = res.data.result;
};
//
handleQuerySelectorAssetPackageName();


handleQuery();
</script>


<style scoped>
:deep(.el-ipnut),
:deep(.el-select),
:deep(.el-input-number) {
  width: 100%;
}
</style>

