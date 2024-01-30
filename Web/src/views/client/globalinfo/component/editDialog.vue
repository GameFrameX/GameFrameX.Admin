<template>
  <div class="clientGlobalInfo-container">
    <el-dialog v-model="isShowDialog" width="80%" draggable="">
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

          <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
            <el-form-item label="检测资源地址" prop="checkResourceUrl">
              <el-input v-model="ruleForm.checkResourceUrl" placeholder="请输入CheckResourceUrl" maxlength="128"
                        show-word-limit clearable/>

            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
            <el-form-item label="检测应用版本地址" prop="checkAppVersionUrl">
              <el-input v-model="ruleForm.checkAppVersionUrl" placeholder="请输入CheckAppVersionUrl" maxlength="128"
                        show-word-limit clearable/>

            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
            <el-form-item label="内容(放JSON对象)" prop="content">
              <el-input v-model="ruleForm.content" placeholder="请输入Content" type="textarea" maxlength="4096"
                        show-word-limit clearable/>

            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
            <el-form-item label="描述信息" prop="description">
              <el-input v-model="ruleForm.description" placeholder="请输入描述" type="textarea" maxlength="512"
                        show-word-limit clearable/>

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
import {addClientGlobalInfo, updateClientGlobalInfo} from "/@/api/client/clientGlobalInfo";
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
const querySelections = ref<any>({
  platform: [],
  language: [],
  channel: [],
});
const ruleForm = ref<any>({});
//自行添加其他规则
const rules = ref<FormRules>({
  channel: [{ required: true, message: '请选择Channel！', trigger: 'change', },],
  checkAppVersionUrl: [{ required: true, message: '请输入checkAppVersionUrl！', trigger: 'blur', },],
  checkResourceUrl: [{ required: true, message: '请输入checkResourceUrl', trigger: 'blur', },],
  content: [{ required: true, message: '请输入content', trigger: 'blur', },],
  platform: [{ required: true, message: '请选择Platform！', trigger: 'change', },],
  version: [{ required: true, message: '请选择Version！', trigger: 'change', },],
  package: [{ required: true, message: '请选择Package！', trigger: 'change', },],
  language: [{ required: true, message: '请选择Language！', trigger: 'change', },],
});

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
const isEditor = ref(false);
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
        await addClientGlobalInfo(values);
      } else {
        await updateClientGlobalInfo(values);
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




