<template>
  <div>
    <Dialog v-model:visible="state.dialogVisible" :style="{ width: 'min(769px, calc(100vw - 32px))' }" modal :draggable="true" @hide="onCancel">
      <template #header>
        <div style="color: #fff">
          <i class="fa-solid fa-pen-to-square" style="font-size: 16px; margin-right: 3px; vertical-align: middle"></i>
          <span>{{ props.title }}</span>
        </div>
      </template>
      <div class="cropper-warp">
        <div class="cropper-warp-left">
          <img :src="state.cropperImg" alt="" aria-hidden="true" class="cropper-warp-left-img"/>
        </div>
        <div class="cropper-warp-right">
          <div class="cropper-warp-right-title">{{ t('message.common.label.preview') }}</div>
          <div class="cropper-warp-right-item">
            <div class="cropper-warp-right-value">
              <img :src="state.cropperImgBase64" alt="" aria-hidden="true" class="cropper-warp-right-value-img"/>
            </div>
            <div class="cropper-warp-right-label">100 x 100</div>
          </div>
          <div class="cropper-warp-right-item">
            <div class="cropper-warp-right-value">
              <img :src="state.cropperImgBase64" alt="" aria-hidden="true" class="cropper-warp-right-value-img cropper-size"/>
            </div>
            <div class="cropper-warp-right-label">50 x 50</div>
          </div>
        </div>
      </div>
      <template #footer>
        <span class="dialog-footer">
          <input ref="fileInputRef" type="file" accept=".jpg,.png" style="display: none" @change="onFileChange"/>
          <Button severity="secondary" @click="triggerFileSelect">
            <i :class="faImage" style="margin-right: 5px"></i>
            {{ t('message.common.message.selectImage') }}
          </Button>
          <Button severity="secondary" @click="onCancel">{{ t('message.common.button.cancel') }}</Button>
          <Button @click="onSubmit">{{ t('message.common.button.confirm') }}</Button>
        </span>
      </template>
    </Dialog>
  </div>
</template>

<script setup lang="ts" name="cropper">
import Dialog from 'primevue/dialog';
import Button from 'primevue/button';
import { faImage } from '../../utils/icons';
import {reactive, nextTick, ref} from 'vue';
import Cropper from 'cropperjs';
import 'cropperjs/dist/cropper.css';


const props = defineProps({
  title: {
    type: String,
    default: () => '',
  },
});
const emits = defineEmits(['uploadCropperImage']);
const fileInputRef = ref<HTMLInputElement>();
const state = reactive({
  dialogVisible: false,
  cropperImg: '',
  cropperImgBase64: '',
  cropper: '' as RefType,
});

const openDialog = (imgs: string) => {
  state.cropperImg = imgs;
  state.dialogVisible = true;
  nextTick(() => {
    initCropper();
  });
};
const closeDialog = () => {
  state.cropper.destroy();
  state.dialogVisible = false;
};
const onCancel = () => {
  closeDialog();
};
const onSubmit = async () => {
  const img = await getCroppedCanvas();
  emits('uploadCropperImage', {img: img});
  closeDialog();
};
const initCropper = () => {
  const letImg = <HTMLImageElement>document.querySelector('.cropper-warp-left-img');
  state.cropper = new Cropper(letImg, {
    viewMode: 1,
    dragMode: 'none',
    initialAspectRatio: 1,
    aspectRatio: 1,
    preview: '.before',
    background: true,
    autoCropArea: 1,
    checkCrossOrigin: false,
    crop: () => {
      state.cropperImgBase64 = state.cropper.getCroppedCanvas()!.toDataURL('image/jpeg');
    },
  });
};

const getCroppedCanvas = () => {
  return new Promise((resolve) => {
    state.cropper.getCroppedCanvas().toBlob((blob: any) => {
      resolve(blob);
    });
  });
};

const triggerFileSelect = () => {
  fileInputRef.value?.click();
};

const onFileChange = (event: Event) => {
  const input = event.target as HTMLInputElement;
  if (!input.files || !input.files[0]) return;
  const file = input.files[0];
  let URL = window.URL || window.webkitURL;
  state.cropperImg = URL.createObjectURL(file);
  state.cropper.replace(state.cropperImg);
  input.value = '';
};

defineExpose({
  openDialog,
});
</script>

<style scoped lang="scss">
.cropper-warp {
  display: flex;

  .cropper-warp-left {
    position: relative;
    display: inline-block;
    height: 350px;
    flex: 1;
    border: 1px solid var(--p-surface-300);
    background: var(--p-surface-0);
    overflow: hidden;
    background-repeat: no-repeat;
    cursor: move;
    border-radius: var(--p-border-radius);

    .cropper-warp-left-img {
      width: 100%;
      height: 100%;
    }
  }

  .cropper-warp-right {
    width: 150px;
    height: 350px;

    .cropper-warp-right-title {
      text-align: center;
      height: 20px;
      line-height: 20px;
    }

    .cropper-warp-right-item {
      margin: 15px 0;

      .cropper-warp-right-value {
        display: flex;

        .cropper-warp-right-value-img {
          width: 100px;
          height: 100px;
          border-radius: 50%;
          margin: auto;
        }

        .cropper-size {
          width: 50px;
          height: 50px;
        }
      }

      .cropper-warp-right-label {
        text-align: center;
        font-size: 12px;
        color: var(--p-text-color);
        height: 30px;
        line-height: 30px;
      }
    }
  }
}
</style>
