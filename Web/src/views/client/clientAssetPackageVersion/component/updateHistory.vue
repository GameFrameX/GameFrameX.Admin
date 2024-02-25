<template>
  <div class="clientAssetPackageVersionHistory-container">

    <el-dialog v-model="isShowDialog" width="75%" draggable="">
      <template #header>
        <div style="color: #fff">
          <!--<el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"> <ele-Edit /> </el-icon>-->
          <span>{{ queryParams.assetPackageName }} 版本历史记录</span>
        </div>
      </template>

      <el-card class="full-table" shadow="hover" style="margin-top: 8px">
        <el-table
            :data="tableData"
            style="width: 100%"
            v-loading="loading"
            tooltip-effect="light"
            row-key="id"
            border="">
          <el-table-column type="index" label="序号" width="100" align="center"/>
          <el-table-column prop="createTime" label="更新时间" width="240" show-overflow-tooltip=""/>
          <el-table-column prop="oldVersion" label="旧资源版本" width="400" show-overflow-tooltip=""/>
          <el-table-column prop="version" label="新资源版本" width="400" show-overflow-tooltip=""/>
          <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip="" v-if="auth('clientAssetPackageVersion:restore')">
            <template #default="scope">
              <el-button icon="ele-Edit" size="small" text="" type="primary" @click="restoreClientAssetPackageVersionHistory(scope.row)" v-auth="'clientAssetPackageVersion:restore'"> 还原</el-button>
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
      </el-card>
      <template #footer>
				<span class="dialog-footer">
					<el-button type="primary" @click="cancel">关 闭</el-button>
				</span>
      </template>
    </el-dialog>
  </div>
</template>

<script lang="ts" setup="" name="clientAssetPackageVersionHistory">
import {ref} from "vue";
import {ElMessageBox, ElMessage} from "element-plus";
import {auth} from '/@/utils/authFunction';

import {pageClientAssetPackageVersionHistory} from '/@/api/client/clientAssetPackageVersionHistory';
import {updateClientAssetPackageVersion} from "/@/api/client/clientAssetPackageVersion";
//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);
const isShowDialog = ref(false);
const loading = ref(false);
const tableData = ref<any>([]);
const queryParams = ref<any>({assetPackageName: ''});
const tableParams = ref({
  page: 1,
  pageSize: 10,
  total: 0,
});

// 打开弹窗
const openDialog = (row: any) => {
  queryParams.value = JSON.parse(JSON.stringify(row));
  isShowDialog.value = true;
  handleQuery();
};
// 关闭弹窗
const closeDialog = () => {
  isShowDialog.value = false;
};

// 取消
const cancel = () => {
  isShowDialog.value = false;
};
// 查询操作
const handleQuery = async () => {
  loading.value = true;
  let res = await pageClientAssetPackageVersionHistory(Object.assign({versionId: queryParams.value.id}, tableParams.value));
  tableData.value = res.data.result?.items ?? [];
  tableParams.value.total = res.data.result?.total;
  loading.value = false;
};

// 还原
const restoreClientAssetPackageVersionHistory = (row: any) => {
  ElMessageBox.confirm(`确定要还原吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  })
      .then(async () => {
        let values = queryParams.value;
        values.version = row.oldVersion;
        await updateClientAssetPackageVersion(values);
        handleQuery();
        ElMessage.success("还原成功");
        closeDialog();
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

//将属性或者函数暴露给父组件
defineExpose({openDialog});
</script>


<style scoped>
:deep(.el-ipnut),
:deep(.el-select),
:deep(.el-input-number) {
  width: 100%;
}
</style>

