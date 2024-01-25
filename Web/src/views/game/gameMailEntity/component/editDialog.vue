<template>
  <div class="gameMailEntity-container">
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
          <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
            <el-form-item label="区服列表" prop="areaIdList">

              <el-select
                  v-model="ruleForm.areaIdList"
                  multiple
                  filterable
                  show-word-limit clearable
                  remote
                  reserve-keyword
                  placeholder="请输入区服列表"
                  :remote-method="remoteSearchAreaIdMethod"
              >
                <el-option
                    v-for="item in areaIdOptions"
                    :key="item.id"
                    :label="item.label"
                    :value="item.id"
                />
                <!--                <template #loading>
                                  <svg class="circular" viewBox="0 0 50 50">
                                    <circle class="path" cx="25" cy="25" r="20" fill="none" />
                                  </svg>
                                </template>-->
              </el-select>
            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
            <el-form-item label="接收者" prop="recipientId">
              <el-input v-model="ruleForm.recipientId" placeholder="请输入接收者" type="textarea" maxlength="255"
                        show-word-limit clearable/>

            </el-form-item>

          </el-col>

          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="标题" prop="subject">
              <el-input v-model="ruleForm.subject" placeholder="请输入标题" maxlength="255" show-word-limit clearable/>

            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="邮件类型" prop="mailType">
              <el-select clearable v-model="ruleForm.mailType" placeholder="请选择邮件类型">
                <el-option v-for="(item,index) in  getEnumMailTypeData" :key="index" :value="item.value"
                           :label="`[${item.value}] ${item.describe}`"></el-option>

              </el-select>

            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
            <el-form-item label="详细内容" prop="body">
              <el-input v-model="ruleForm.body" placeholder="请输入详细内容" type="textarea" maxlength="4096"
                        show-word-limit clearable/>

            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
            <el-form-item label="附件列表" prop="attachments">
              <el-input v-model="ruleForm.attachments" placeholder="请输入附件列表" type="textarea" maxlength="4096"
                        show-word-limit clearable/>

            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="发送的时间" prop="sendDate">
              <el-date-picker v-model="ruleForm.sendDate" type="datetime" placeholder="发送的时间"/>

            </el-form-item>

          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
            <el-form-item label="邮件的过期日期" prop="expirationDate">
              <el-date-picker v-model="ruleForm.expirationDate" type="datetime" placeholder="邮件的过期日期"/>

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
import {addGameMailEntity, updateGameMailEntity} from "/@/api/game/gameMailEntity";
import {getAPI} from '/@/utils/axios-utils';
import {SysEnumApi} from '/@/api-services/api';
import {selectGameAreaEntity} from '/@/api/game/gameAreaEntity';

const getEnumStatusData = ref<any>([]);
const getEnumMailTypeData = ref<any>([]);
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

interface ListItem {
  id: number,
  value: string
  label: string
}

// 区服列表
const areaIdOptions = ref<ListItem[]>([])
//自行添加其他规则
const rules = ref<FormRules>({
  areaIdList: [{ required: true, message: '请输入区服列表！', trigger: 'blur', },],
  recipientId: [{ required: true, message: '请输入接收者！', trigger: 'blur', },],
  subject: [{ required: true, message: '请输入标题！', trigger: 'blur', },],
  body: [{ required: true, message: '请输入详细内容！', trigger: 'blur', },],
  attachments: [{ required: true, message: '请输入附件列表！', trigger: 'blur', },],
});

// 搜索区服列表
const remoteSearchAreaIdMethod = (query: string) => {
  if (query) {
    selectGameAreaEntity({ SearchKey: query }).then((areaIdres) => {
      areaIdOptions.value = areaIdres.data.result;
    }).catch((e) => {
      areaIdOptions.value = []
    });
  } else {
    areaIdOptions.value = []
  }
}

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
        await addGameMailEntity(values);
      } else {
        await updateGameMailEntity(values);
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
  getEnumStatusData.value = (await getAPI(SysEnumApi).apiSysEnumEnumDataListGet('GameMailStatus')).data.result ?? [];
  getEnumMailTypeData.value = (await getAPI(SysEnumApi).apiSysEnumEnumDataListGet('GameMailType')).data.result ?? [];
});

//将属性或者函数暴露给父组件
defineExpose({ openDialog, isEditor });
</script>




