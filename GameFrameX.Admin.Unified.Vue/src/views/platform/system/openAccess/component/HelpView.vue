<template>
  <div class="sys-notice-container">
    <Dialog v-model:visible="state.dialogVisible" :draggable="true" :modal="true" :closable="true" :style="{width: 'min(900px, calc(100vw - 32px))'}">
      <template #header>
        <div>
          <i class="fa-solid fa-circle-question" style="margin-right: 3px; font-size: 16px; vertical-align: middle"></i>
          <span> {{ t('system.openAccess.helpView') }} </span>
        </div>
      </template>
      <div class="text-content">
        <h2>OpenAPI 使用</h2>
        <ul>
          <li>
            在需要使用 Signature 身份验证的 Api 中贴上
            <p>
              <Tag>[Authorize(AuthenticationSchemes = SignatureAuthenticationDefaults.AuthenticationScheme)]</Tag>
            </p>
          </li>
          <li>
            如果 Api 需要保留 Jwt 方式的身份验证，可贴上
            <p>
              <Tag>[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," +
                SignatureAuthenticationDefaults.AuthenticationScheme)]
              </Tag>
            </p>
          </li>
          <li>
            通过对请求的签名，可以达到以下目的：
            <ul>
              <li>免登录识别访问接口用户的身份</li>
              <li>防止潜在的重放攻击</li>
            </ul>
          </li>
        </ul>
        <Divider/>
        <h2>OpenAPI 签名流程</h2>
        客户端在请求时，需要按照如下步骤生成签名 Signature，并添加公共参数：
        <h3>公共请求参数</h3>
        <p>在原始请求的基础上添加 Header 请求参数</p>
        <ul>
          <li>
            <Tag severity="secondary">accessKey</Tag>
            ：身份标识
          </li>
          <li>
            <Tag severity="secondary">timestamp</Tag>
            ：时间戳，精确到秒
          </li>
          <li>
            <Tag severity="secondary">nonce</Tag>
            ：唯一随机数，建议为一个6位的随机数
          </li>
          <li>
            <Tag severity="secondary">sign</Tag>
            ：签名数据（见"计算签名"部分）
          </li>
        </ul>
        <h3>计算签名</h3>
        <ul>
          <li>
            按照如下顺序对请求中的参数进行排序，各个参数通过&进行拼接（中间不含空格）：
            <p>
              <Tag>method & url & accessKey & timestamp & nonce</Tag>
            </p>
            <ul>
              <li>
                <Tag severity="secondary">method</Tag>
                需要大写，如：GET
              </li>
              <li>
                <Tag severity="secondary">url</Tag>
                去除协议、域名、参数，以 / 开头，如：/api/demo/helloWord
              </li>
            </ul>
          </li>
          <li>使用 HMAC-SHA256 协议创建基于哈希的消息身份验证代码 (HMAC)，以
            <Tag severity="secondary">appSecret</Tag>
            作为密钥，对上面拼接的参数进行计算签名，所得签名进行 Base-64 编码
          </li>
        </ul>
      </div>
      <div class="p-inline-message p-inline-message-info" style="width: 100%">
        HMAC-SHA256 在线计算：
        <a href="https://1024tools.com/hmac" target="_blank" style="color: var(--p-primary-color)">https://1024tools.com/hmac</a>
      </div>
    </Dialog>
  </div>
</template>

<script lang="ts" setup name="systemOpenAccessHelpView">
import {reactive} from 'vue';

const state = reactive({
  dialogVisible: false,
});

const openDialog = () => {
  state.dialogVisible = true;
};

defineExpose({openDialog});
</script>
<style scoped lang="scss">
.text-content {
  h1 {
    margin: 8px 0;
  }

  h2 {
    margin: 8px 0;
  }

  h3 {
    margin: 8px 0;
  }

  p {
    margin: 8px 0;
  }

  ul {
    padding: 0 0 0 30px;

    li {
      margin: 8px 0;
    }
  }
}
</style>
