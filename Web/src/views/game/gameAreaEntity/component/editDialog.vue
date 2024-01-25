<template>
	<div class="gameAreaEntity-container">
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
						<el-form-item label="是否是默认" prop="isDefault">
							<el-select clearable v-model="ruleForm.isDefault" placeholder="请选择是否是默认">
								<el-option v-for="(item,index) in  getEnumIsDefaultData" :key="index" :value="item.value" :label="`[${item.value}] ${item.describe}`"></el-option>
								
							</el-select>
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="是否启用" prop="isEnable">
							<el-select clearable v-model="ruleForm.isEnable" placeholder="请选择是否启用">
								<el-option v-for="(item,index) in  getEnumIsEnableData" :key="index" :value="item.value" :label="`[${item.value}] ${item.describe}`"></el-option>
								
							</el-select>
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="名称" prop="name">
							<el-input v-model="ruleForm.name" placeholder="请输入名称" maxlength="64" show-word-limit clearable />
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="详细描述" prop="description">
							<el-input v-model="ruleForm.description" placeholder="请输入详细描述" maxlength="512" show-word-limit clearable />
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="状态" prop="serverStatus">
							<el-select clearable v-model="ruleForm.serverStatus" placeholder="请选择状态">
								<el-option v-for="(item,index) in  getEnumServerStatusData" :key="index" :value="item.value" :label="`[${item.value}] ${item.describe}`"></el-option>
								
							</el-select>
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="区域" prop="region">
							<el-input v-model="ruleForm.region" placeholder="请输入区域" maxlength="255" show-word-limit clearable />
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="地址" prop="address">
							<el-input v-model="ruleForm.address" placeholder="请输入地址" maxlength="255" show-word-limit clearable />
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="端口号" prop="port">
							<el-input-number v-model="ruleForm.port" placeholder="请输入端口号" clearable />
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="最大玩家数量" prop="capacity">
							<el-input-number v-model="ruleForm.capacity" placeholder="请输入最大玩家数量" clearable />
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="启用的日期" prop="creationDate">
							<el-date-picker v-model="ruleForm.creationDate" type="datetime" placeholder="启用的日期" />
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="服务器最后一次维护或更新的日期" prop="lastMaintenanceDate">
							<el-date-picker v-model="ruleForm.lastMaintenanceDate" type="datetime" placeholder="服务器最后一次维护或更新的日期" />
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="游戏版本号" prop="version">
							<el-input v-model="ruleForm.version" placeholder="请输入游戏版本号" maxlength="255" show-word-limit clearable />
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="服务器的游戏模式类型" prop="serverType">
							<el-input v-model="ruleForm.serverType" placeholder="请输入服务器的游戏模式类型" maxlength="255" show-word-limit clearable />
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="语言" prop="language">
							<el-select clearable v-model="ruleForm.language" placeholder="请选择语言">
								<el-option v-for="(item,index) in  getEnumLanguageData" :key="index" :value="item.value" :label="`[${item.value}] ${item.describe}`"></el-option>
								
							</el-select>
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="时区" prop="timeZone">
							<el-input v-model="ruleForm.timeZone" placeholder="请输入时区" maxlength="1" show-word-limit clearable />
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="指定可以加入此服务器的玩家类型" prop="allowedPlayerProfile">
							<el-input v-model="ruleForm.allowedPlayerProfile" placeholder="请输入指定可以加入此服务器的玩家类型" maxlength="255" show-word-limit clearable />
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="特定服务器举行的事件或活动信息" prop="serverEvents">
							<el-input v-model="ruleForm.serverEvents" placeholder="请输入特定服务器举行的事件或活动信息" maxlength="255" show-word-limit clearable />
							
						</el-form-item>
						
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="访问权限设置，控制哪些玩家可以访问服务器" prop="accessControls">
							<el-input v-model="ruleForm.accessControls" placeholder="请输入访问权限设置，控制哪些玩家可以访问服务器" maxlength="255" show-word-limit clearable />
							
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
	import { addGameAreaEntity, updateGameAreaEntity } from "/@/api/game/gameAreaEntity";
	import { getAPI } from '/@/utils/axios-utils';
	import { SysEnumApi } from '/@/api-services/api';

	const getEnumIsDefaultData = ref<any>([]);
	const getEnumIsEnableData = ref<any>([]);
	const getEnumServerStatusData = ref<any>([]);
	const getEnumLanguageData = ref<any>([]);
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
		name: [{required: true, message: '请输入名称！', trigger: 'blur',},],
		description: [{required: true, message: '请输入详细描述！', trigger: 'blur',},],
		region: [{required: true, message: '请输入区域！', trigger: 'blur',},],
		address: [{required: true, message: '请输入地址！', trigger: 'blur',},],
		port: [{required: true, message: '请输入端口号！', trigger: 'blur',},],
		capacity: [{required: true, message: '请输入最大玩家数量！', trigger: 'blur',},],
		version: [{required: true, message: '请输入游戏版本号！', trigger: 'blur',},],
		serverType: [{required: true, message: '请输入服务器的游戏模式类型！', trigger: 'blur',},],
		timeZone: [{required: true, message: '请输入时区！', trigger: 'blur',},],
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
					await addGameAreaEntity(values);
				} else {
					await updateGameAreaEntity(values);
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
			getEnumIsDefaultData.value = (await getAPI(SysEnumApi).apiSysEnumEnumDataListGet('YesNoEnum')).data.result ?? [];
			getEnumIsEnableData.value = (await getAPI(SysEnumApi).apiSysEnumEnumDataListGet('YesNoEnum')).data.result ?? [];
			getEnumServerStatusData.value = (await getAPI(SysEnumApi).apiSysEnumEnumDataListGet('GameServerStatus')).data.result ?? [];
			getEnumLanguageData.value = (await getAPI(SysEnumApi).apiSysEnumEnumDataListGet('GameLanguage')).data.result ?? [];
	});

	//将属性或者函数暴露给父组件
	defineExpose({ openDialog, isEditor });
</script>




