<template>
  <div class="clientAssetPackageVersion-container">
    <el-dialog v-model="isShowDialog" width="75%" draggable="">
      <template #header>
        <div style="color: #fff">
          <!--<el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"> <ele-Edit /> </el-icon>-->
          <span>{{ props.title }}</span>
        </div>
      </template>
      <el-form :model="ruleForm" ref="ruleFormRef" label-width="auto" :rules="rules">
        <el-row :gutter="35">
          <el-form-item v-show="false">
            <el-input v-model="ruleForm.id"/>
          </el-form-item>
          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="渠道名称" prop="channel">
              <el-select clearable v-model="ruleForm.channel" placeholder="请选择渠道名称"  :disabled="isEditor">
                <el-option v-for="item in queryParams.channel" :key="item.value" :label="item.label" :value="item.value"><span>{{ item.value }} - {{ item.label }}</span></el-option>
              </el-select>

            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="平台" prop="platform">
              <el-select clearable v-model="ruleForm.platform" placeholder="请选择平台"  :disabled="isEditor">
                <el-option v-for="item in queryParams.platform" :key="item.value" :label="item.label" :value="item.value"><span>{{ item.value }} - {{ item.label }}</span></el-option>
              </el-select>

            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="游戏版本" prop="appVersion">
              <el-select clearable v-model="ruleForm.appVersion" placeholder="请选择游戏版本"  :disabled="isEditor">
                <el-option v-for="item in queryParams.appVersion" :key="item.value" :label="item.label" :value="item.value"><span>{{ item.value }} - {{ item.label }}</span></el-option>
              </el-select>

            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="包名" prop="package">
              <el-select clearable v-model="ruleForm.package" placeholder="请选择包名"  :disabled="isEditor">
                <el-option v-for="item in queryParams.package" :key="item.value" :label="item.label" :value="item.value"><span>{{ item.value }} - {{ item.label }}</span></el-option>
              </el-select>

            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="语言" prop="language">
              <el-select clearable v-model="ruleForm.language" placeholder="请选择语言"  :disabled="isEditor">
                <el-option v-for="item in queryParams.language" :key="item.value" :label="item.label" :value="item.value"><span>{{ item.value }} - {{ item.label }}</span></el-option>
              </el-select>

            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="资源包名称" prop="assetPackageName">
              <el-select clearable v-model="ruleForm.assetPackageName" placeholder="请选择资源包名称" :disabled="isEditor">
                <el-option v-for="item in queryParams.assetPackageName" :key="item.value" :label="item.label" :value="item.value"><span>{{ item.value }} - {{ item.label }}</span></el-option>
              </el-select>

            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="资源版本" prop="version">
              <el-input v-model="ruleForm.version" placeholder="请输入资源版本" maxlength="64" show-word-limit clearable/>

            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="详细描述" prop="description">
              <el-input v-model="ruleForm.description" placeholder="请输入详细描述" type="textarea" maxlength="512" show-word-limit clearable/>

            </el-form-item>

          </el-col>
        </el-row>
      </el-form>
      <template #footer>
				<span class="dialog-footer">
					<el-button @click="cancel">取 消</el-button>
					<el-button type="primary" @click="submit">确 定</el-button>
				</span>
      </template>
    </el-dialog>
  </div>
</template>
<style scoped>
:deep(.el-select),
:deep(.el-input-number) {
  width: 100%;
}
</style>
<script lang="ts" setup>
import {ref, onMounted} from "vue";
import {getDictDataItem as di, getDictDataList as dl} from '/@/utils/dict-utils';
import {ElMessage} from "element-plus";
import type {FormRules} from "element-plus";
import {addClientAssetPackageVersion, updateClientAssetPackageVersion} from "/@/api/client/clientAssetPackageVersion";
import {selectClientLanguage} from "/@/api/client/language";
import {selectClientPackage} from "/@/api/client/package";
import {selectClientPlatform} from "/@/api/client/appplatform";
import {selectClientChannel} from "/@/api/client/channel";
import {selectClientVersion} from "/@/api/client/version";
import {selectClientAssetPackage} from "/@/api/client/clientAssetPackage";

//父级传递来的参数
var props = defineProps({
  title: {
    type: String,
    default: "",
  },
});
//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);
const ruleFormRef = ref();
const isShowDialog = ref(false);
// 标记是否是编辑模式
const isEditor = ref(false);
const ruleForm = ref<any>({});
const queryParams = ref<any>({});
//自行添加其他规则
const rules = ref<FormRules>({
  channel: [{required: true, message: '请选择渠道名称！', trigger: 'change',},],
  platform: [{required: true, message: '请选择平台！', trigger: 'change',},],
  appVersion: [{required: true, message: '请选择游戏版本！', trigger: 'change',},],
  package: [{required: true, message: '请选择包名！', trigger: 'change',},],
  language: [{required: true, message: '请选择语言！', trigger: 'change',},],
  assetPackageName: [{required: true, message: '请选择资源包名称！', trigger: 'change',},],
  version: [{required: true, message: '请输入资源版本！', trigger: 'blur',},],
  description: [{required: true, message: '请输入详细描述！', trigger: 'blur',},],
});

// 打开弹窗
const openDialog = (row: any) => {
  ruleForm.value = JSON.parse(JSON.stringify(row));
  isShowDialog.value = true;
};

// 关闭弹窗
const closeDialog = () => {
  emit("reloadTable");
  isShowDialog.value = false;
};

// 取消
const cancel = () => {
  isShowDialog.value = false;
};

// 提交
const submit = async () => {
  ruleFormRef.value.validate(async (isValid: boolean, fields?: any) => {
    if (isValid) {
      let values = ruleForm.value;
      if (ruleForm.value.id == undefined || ruleForm.value.id == null || ruleForm.value.id == "" || ruleForm.value.id == 0) {
        await addClientAssetPackageVersion(values);
      } else {
        await updateClientAssetPackageVersion(values);
      }
      closeDialog();
    } else {
      ElMessage({
        message: `表单有${Object.keys(fields).length}处验证失败，请修改后再提交`,
        type: "error",
      });
    }
  });
};


//
const handleQuerySelectorChannel = async () => {
  const res = await selectClientChannel({});
  queryParams.value.channel = res.data.result;
};
//
handleQuerySelectorChannel();
//
const handleQuerySelectorPlatform = async () => {
  const res = await selectClientPlatform({});
  queryParams.value.platform = res.data.result;
};
//
handleQuerySelectorPlatform();
//
const handleQuerySelectorAppVersion = async () => {
  const res = await selectClientVersion({});
  queryParams.value.appVersion = res.data.result;
};
//
handleQuerySelectorAppVersion();
//
const handleQuerySelectorPackage = async () => {
  const res = await selectClientPackage({});
  queryParams.value.package = res.data.result;
};
//
handleQuerySelectorPackage();
//
const handleQuerySelectorLanguage = async () => {
  const res = await selectClientLanguage({});
  queryParams.value.language = res.data.result;
};
//
handleQuerySelectorLanguage();
//
const handleQuerySelectorAssetPackageName = async () => {
  const res = await selectClientAssetPackage({});
  queryParams.value.assetPackageName = res.data.result;
};
//
handleQuerySelectorAssetPackageName();


// 页面加载时
onMounted(async () => {
});

//将属性或者函数暴露给父组件
defineExpose({openDialog, isEditor});
</script>




