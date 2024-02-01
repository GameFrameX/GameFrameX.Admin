<template>
	<div class="gameAnnouncementEntity-container">
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
						<el-input v-model="ruleForm.id" />
					</el-form-item>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="区服ID" prop="areaId">
              <el-select
                  v-model="ruleForm.areaId"
                  filterable
                  show-word-limit clearable
                  remote
                  reserve-keyword
                  placeholder="请输入选择区服"
                  :remote-method="remoteSearchAreaIdMethod"
              >
                <el-option
                    v-for="item in areaIdOptions"
                    :key="item.id"
                    :label="item.label"
                    :value="item.id"
                />
              </el-select>
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="标题" prop="title">
							<el-input v-model="ruleForm.title" placeholder="请输入标题" maxlength="256" show-word-limit clearable />
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="开始时间" prop="startTime">
							<el-date-picker v-model="ruleForm.startTime" type="datetime" placeholder="开始时间" />
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="结束时间" prop="endTime">
							<el-date-picker v-model="ruleForm.endTime" type="datetime" placeholder="结束时间" />
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="内容" prop="content">
							<el-input v-model="ruleForm.content" placeholder="请输入内容" type="textarea" maxlength="2048" show-word-limit clearable />
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="优先级" prop="priority">
							<el-input-number v-model="ruleForm.priority" placeholder="请输入优先级" clearable />
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="是否激活" prop="isActive">
							<el-switch v-model="ruleForm.isActive" active-text="是" inactive-text="否" />
							
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
	import { ref,onMounted } from "vue";
	import { getDictDataItem as di, getDictDataList as dl } from '/@/utils/dict-utils';
	import { ElMessage } from "element-plus";
	import type { FormRules } from "element-plus";
	import { addGameAnnouncementEntity, updateGameAnnouncementEntity } from "/@/api/game/gameAnnouncementEntity";
  import {selectGameAreaEntity} from "/@/api/game/gameAreaEntity";

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
	//自行添加其他规则
	const rules = ref<FormRules>({
		areaId: [{required: true, message: '请选择区服ID！', trigger: 'change',},],
		title: [{required: true, message: '请输入标题！', trigger: 'blur',},],
		content: [{required: true, message: '请输入内容！', trigger: 'blur',},],
		priority: [{required: true, message: '请输入优先级！', trigger: 'blur',},],
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
					await addGameAnnouncementEntity(values);
				} else {
					await updateGameAnnouncementEntity(values);
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



  interface ListItem {
    id: number,
    value: string
    label: string
  }
  // 区服列表
  const areaIdOptions = ref<ListItem[]>([])
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





	// 页面加载时
	onMounted(async () => {
	});

	//将属性或者函数暴露给父组件
	defineExpose({ openDialog, isEditor });
</script>




