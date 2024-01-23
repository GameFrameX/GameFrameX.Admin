<template>
  <div class="clientGlobalInfo-container">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }">
      <el-form :model="queryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="渠道名称">
              <el-select clearable="" v-model="queryParams.channel" placeholder="请选择">
                <el-option
                    v-for="item in querySelections.channel"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                >
                  <span>{{ item.value }} - {{ item.label }}</span>
                </el-option>

              </el-select>

            </el-form-item>
          </el-col>


          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="平台">
              <el-select clearable="" v-model="queryParams.platform" placeholder="请选择">
                <el-option
                    v-for="item in querySelections.platform"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                >
                  <span>{{ item.value }} - {{ item.label }}</span>
                </el-option>

              </el-select>

            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="版本">
              <el-select clearable="" v-model="queryParams.version" placeholder="请选择">
                <el-option
                    v-for="item in querySelections.version"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                >
                  <span>{{ item.value }} - {{ item.label }}</span>
                </el-option>

              </el-select>

            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="包名">
              <el-select clearable="" v-model="queryParams.package" placeholder="请选择">
                <el-option
                    v-for="item in querySelections.package"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                >
                  <span>{{ item.value }} - {{ item.label }}</span>
                </el-option>

              </el-select>

            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="语言名称">
              <el-select clearable="" v-model="queryParams.language" placeholder="请选择">
                <el-option
                    v-for="item in querySelections.language"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                >
                  <span>{{ item.value }} - {{ item.label }}</span>
                </el-option>

              </el-select>

            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="6" :xl="6" class="mb10">
            <el-form-item>
              <el-button-group>
                <el-button type="primary" icon="ele-Search" @click="handleQuery" v-auth="'clientGlobalInfo:page'">
                  查询
                </el-button>
                <el-button icon="ele-Refresh" @click="() => queryParams = {}"> 重置</el-button>

              </el-button-group>

              <el-button-group style="margin-left:20px">
                <el-button type="primary" icon="ele-Plus" @click="openAddClientGlobalInfo"
                           v-auth="'clientGlobalInfo:add'"> 新增
                </el-button>

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
        <el-table-column prop="channel" label="渠道名称" width="105" show-overflow-tooltip=""/>
        <el-table-column prop="platform" label="平台名称" width="90" show-overflow-tooltip=""/>
        <el-table-column prop="version" label="新版本" width="80" show-overflow-tooltip=""/>
        <el-table-column prop="package" label="包名" width="105" show-overflow-tooltip=""/>
        <el-table-column prop="language" label="语言名称" width="120" show-overflow-tooltip=""/>
        <el-table-column prop="checkResourceUrl" label="检测资源地址" width="240" show-overflow-tooltip=""/>
        <el-table-column prop="checkAppVersionUrl" label="检测app版本地址" width="270" show-overflow-tooltip=""/>
        <el-table-column prop="content" label="内容" width="105" show-overflow-tooltip=""/>
        <el-table-column prop="description" label="描述" width="165" show-overflow-tooltip=""/>
        <el-table-column prop="createUserName" label="创建人" width="210" show-overflow-tooltip=""/>
        <el-table-column prop="updateUserName" label="最后更新人" width="210" show-overflow-tooltip=""/>
        <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip=""
                         v-if="auth('clientGlobalInfo:edit') || auth('clientGlobalInfo:delete')">
          <template #default="scope">
            <el-button icon="ele-Edit" size="small" text="" type="primary" @click="openEditClientGlobalInfo(scope.row)"
                       v-auth="'clientGlobalInfo:edit'"> 编辑
            </el-button>
            <el-button icon="ele-Delete" size="small" text="" type="primary" @click="delClientGlobalInfo(scope.row)"
                       v-auth="'clientGlobalInfo:delete'"> 删除
            </el-button>
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
          :title="editClientGlobalInfoTitle"
          @reloadTable="handleQuery"
      />
    </el-card>
  </div>
</template>

<script lang="ts" setup="" name="clientGlobalInfo">
import {ref} from "vue";
import {ElMessageBox, ElMessage} from "element-plus";
import {auth} from '/@/utils/authFunction';
//import { formatDate } from '/@/utils/formatTime';

import editDialog from '/@/views/client/globalinfo/component/editDialog.vue'
import {pageClientGlobalInfo, deleteClientGlobalInfo} from '/@/api/client/clientGlobalInfo';
import {selectClientLanguage} from "/@/api/client/language";
import {selectClientChannel} from "/@/api/client/channel";
import {selectClientPlatform} from "/@/api/client/appplatform";
import {selectClientVersion} from "/@/api/client/version";
import {selectClientPackage} from "/@/api/client/package";


const editDialogRef = ref();
const loading = ref(false);
const tableData = ref<any>([]);
const queryParams = ref<any>({});
const querySelections = ref<any>({
  platform: [],
  language: [],
  channel: [],
});
const tableParams = ref({
  page: 1,
  pageSize: 10,
  total: 0,
});
const editClientGlobalInfoTitle = ref('');


const handleLanguageQuery = async () => {
  const res = await selectClientLanguage({});
  querySelections.value.language = res.data.result;
};

const handleChannelQuery = async () => {
  const res = await selectClientChannel({});
  querySelections.value.channel = res.data.result;
};

const handlePlatformQuery = async () => {
  const res = await selectClientPlatform({});
  querySelections.value.platform = res.data.result;
};

const handleVersionQuery = async () => {
  const res = await selectClientVersion({});
  querySelections.value.version = res.data.result;
  querySelections.value.oldVersion = res.data.result;
};

const handlePackageQuery = async () => {
  const res = await selectClientPackage({});
  querySelections.value.package = res.data.result;
};

// 查询操作
const handleQuery = async () => {
  loading.value = true;
  var res = await pageClientGlobalInfo(Object.assign(queryParams.value, tableParams.value));
  tableData.value = res.data.result?.items ?? [];
  tableParams.value.total = res.data.result?.total;
  loading.value = false;
};

// 打开新增页面
const openAddClientGlobalInfo = () => {
  editClientGlobalInfoTitle.value = '添加全局信息管理';
  editDialogRef.value.isEditor= false;
  editDialogRef.value.openDialog({});
};

// 打开编辑页面
const openEditClientGlobalInfo = (row: any) => {
  editClientGlobalInfoTitle.value = '编辑全局信息管理';
  editDialogRef.value.isEditor= true;
  editDialogRef.value.openDialog(row);
};

// 删除
const delClientGlobalInfo = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  })
      .then(async () => {
        await deleteClientGlobalInfo(row);
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
handleLanguageQuery();
handlePackageQuery();
handlePlatformQuery();
handleChannelQuery();
handleVersionQuery();
</script>
<style scoped>
:deep(.el-ipnut),
:deep(.el-select),
:deep(.el-input-number) {
  width: 100%;
}
</style>

