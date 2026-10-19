<template>
  <div v-if="isShowIconImg" :style="setIconImgOutStyle">
    <img :src="getIconName" alt="" aria-hidden="true" :style="setIconSvgInsStyle"/>
  </div>
  <i v-else :class="getIconName" :style="setIconSvgStyle"/>
</template>

<script setup lang="ts" name="svgIcon">
import {computed} from 'vue';

const props = defineProps({
  name: {
    type: String,
  },
  size: {
    type: Number,
    default: () => 14,
  },
  color: {
    type: String,
  },
});

const linesString = ['https', 'http', '/src', '/assets', 'data:image', window.__env__.VITE_PUBLIC_PATH];

const getIconName = computed(() => props?.name);
const isShowIconImg = computed(() => linesString.find((str) => props.name?.startsWith(str)));
const setIconSvgStyle = computed(() => `font-size: ${props.size}px;color: ${props.color};`);
const setIconImgOutStyle = computed(() => `width: ${props.size}px;height: ${props.size}px;display: inline-block;overflow: hidden;`);
const setIconSvgInsStyle = computed(() => {
  const filterStyle: string[] = [];
  const compatibles: string[] = ['-webkit', '-ms', '-o', '-moz'];
  compatibles.forEach((j) => filterStyle.push(`${j}-filter: drop-shadow(${props.color} ${props.size}px 0);`));
  return `width: ${props.size}px;height: ${props.size}px;position: relative;left: -${props.size}px;${filterStyle.join('')}`;
});
</script>
