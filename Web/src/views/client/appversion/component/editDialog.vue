<template>
  <div class="appversion-container">
    <el-dialog v-model="isShowDialog" :width="1600" draggable="">
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
              <el-select clearable v-model="ruleForm.channel" placeholder="请选择渠道名称" :disabled="isEditor">
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

          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="旧版本" prop="oldVersion">
              <el-select clearable v-model="ruleForm.oldVersion" placeholder="请选择旧版本" :disabled="isEditor">
                <el-option
                    v-for="item in querySelections.oldVersion"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                >
                  <span>{{ item.value }} - {{ item.label }}</span>
                </el-option>

              </el-select>

            </el-form-item>

          </el-col>

          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="平台" prop="platform">
              <el-select clearable v-model="ruleForm.platform" placeholder="请选择" :disabled="isEditor">
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
          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="新版本" prop="version">
              <el-select clearable v-model="ruleForm.version" placeholder="请选择" :disabled="isEditor">
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
          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="包名" prop="package">
              <el-select clearable v-model="ruleForm.package" placeholder="请选择" :disabled="isEditor">
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
          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="语言名称" prop="language">
              <el-select clearable v-model="ruleForm.language" placeholder="请选择" :disabled="isEditor">
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
          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="下载地址" prop="downloadUrl">
              <el-input v-model="ruleForm.downloadUrl" placeholder="请输入下载地址" maxlength="256" show-word-limit
                        clearable/>

            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="是否是强制更新" prop="isForce">
              <el-switch v-model="ruleForm.isForce" active-text="是" inactive-text="否"/>

            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="公告" prop="announcement">
              <el-input v-model="ruleForm.announcement" placeholder="请输入公告" maxlength="4096" show-word-limit
                        clearable/>

            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="描述" prop="description">
              <el-input v-model="ruleForm.description" placeholder="请输入Description" maxlength="512" show-word-limit
                        clearable/>

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
import {ElMessage} from "element-plus";
import type {FormRules} from "element-plus";
import {addClientAppVersion, updateClientAppVersion} from "/@/api/client/appversion";
import {selectClientPlatform} from "/@/api/client/appplatform";
import {selectClientLanguage} from "/@/api/client/language";
import {selectClientVersion} from "/@/api/client/version";
import {selectClientPackage} from "/@/api/client/package";
import {selectClientChannel} from "/@/api/client/channel";

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
const ruleForm = ref<any>({});
const querySelections = ref<any>({
  platform: [],
  language: [],
  channel: [],
});
//自行添加其他规则
const rules = ref<FormRules>({
  channel: [{ required: true, message: '请选择Channel！', trigger: 'change', },],
  announcement: [{ required: true, message: '请输入Announcement！', trigger: 'blur', },],
  oldVersion: [{ required: true, message: '请选择OldVersion！', trigger: 'change', },],
  downloadUrl: [{ required: true, message: '请输入DownloadUrl！', trigger: 'blur', },],
  platform: [{ required: true, message: '请选择Platform！', trigger: 'change', },],
  version: [{ required: true, message: '请选择Version！', trigger: 'change', },],
  package: [{ required: true, message: '请选择Package！', trigger: 'change', },],
  language: [{ required: true, message: '请选择Language！', trigger: 'change', },],
});
const isEditor = ref(false);

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
        await addClientAppVersion(values);
      } else {
        await updateClientAppVersion(values);
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


// 页面加载时
onMounted(async () => {
});

//将属性或者函数暴露给父组件
defineExpose({ openDialog, isEditor });
handleLanguageQuery();
handlePackageQuery();
handlePlatformQuery();
handleChannelQuery();
handleVersionQuery();
</script>




