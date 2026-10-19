// C5（server-envelope-middleware）联调预置脚本：Postman Pre-Request Script。
//
// 用途：把明文 JSON 请求体加密为 AES-256-GCM 信封（与服务端 CryptoEnvelopeMiddleware /
// 客户端 NetLib.EnvelopeCrypto 同规格），并构造 v2 传输签名头。规格事实源：
// canting 仓 gfx-doc/dev/crypto-envelope/spec.md + archived/C4-client-crypto-envelope/test-vectors.json。
//
// 依赖 Postman 环境变量（在 Postman Environment / Collection Variables 里配置）：
//   session_kid      信封 kid（TenantSessionKeyEntityService Add/Rotate 响应的 Kid）
//   session_key_hex  会话密钥明文（64 位 hex，Add/Rotate 一次性回显；未配置则跳过信封、仅签名）
//   sig_key_id       v2 签名 keyId（TenantAppSignatureKeyEntity 的 Kid）
//   sig_key_secret   v2 签名密钥明文（HMAC-SHA256）
//
// 约定：
//   - 请求体用 raw JSON（pm.request.body.raw）作为信封 payload，flag=0x00 raw（不压缩）
//   - 信封 AAD = UTF8("env1\n" + path + "\n" + ts + "\n" + mid)，path 不含 query
//   - 签名 v2 message = "v2\nMETHOD\npath\nquery\nkeyId\nts\nmid\nhex(sha256(body))"，
//     body = 线上最终字节（信封启用时为信封 JSON）——签名先于加密在服务端验证，顺序勿倒置
//   - 本脚本只处理请求方向；响应是密文信封，肉眼读不了，需按同一 AAD（请求 path + 响应自报 mid/ts）解密
//
// ponytail: 依赖 Postman Desktop 的 Node crypto 模块（AES-GCM 不在 CryptoJS 内置能力中）；
// 云端 Runner 若不可用 require('crypto')，需换本地脚本生成请求。升级路径：Postman 自定义认证钩子。

const crypto = require('crypto');

const FLAG_RAW = 0x00;
const IV_BYTES = 12;
const TAG_BYTES = 16;

const kid = pm.environment.get('session_kid');
const sessionKeyHex = pm.environment.get('session_key_hex');
const sigKeyId = pm.environment.get('sig_key_id');
const sigKeySecret = pm.environment.get('sig_key_secret');

// ===== path / query =====
// path：与客户端 Uri.AbsolutePath.TrimEnd('/') 同口径。
// query：C267 起改走 RFC 3986 canonical（按 key 字典序 + percent-encoding，unreserved 仅 ALPHA/DIGIT/-._~），
//        与服务端 CanonicalRequestBuilder.CanonicalQuery 同规格 —— 前置代理改写参数顺序/编码不再破签。
//        规范正文（唯一权威 spec）：gfx-doc/dev/signature/spec.md。
const pathSegments = pm.request.url.path || [];
let path = '/' + pathSegments.join('/');
path = path.replace(/\/+$/, '') || path;
// encodeURIComponent 不编码 ! ' ( ) *（RFC 3986 要求编码），需补齐
const rfc3986Encode = (s) => encodeURIComponent(String(s))
    .replace(/[!'()*]/g, (c) => '%' + c.charCodeAt(0).toString(16).toUpperCase());
const query = (pm.request.url.query || [])
    .filter(item => item.key !== 'Signature')
    .map(item => ({ key: rfc3986Encode(item.key), pair: rfc3986Encode(item.key) + '=' + rfc3986Encode(item.value || '') }))
    .sort((a, b) => a.key < b.key ? -1 : (a.key > b.key ? 1 : 0))
    .map(item => item.pair)
    .join('&');

// ===== 公共字段 =====
const ts = Math.floor(Date.now() / 1000);
let mid = parseInt(pm.collectionVariables.get('mid') || '0', 10) + 1;
pm.collectionVariables.set('mid', String(mid));

// ===== 1) 信封加密（配置了会话密钥时） =====
let wireBody;
if (kid && sessionKeyHex) {
    const payload = Buffer.from(pm.request.body.raw || '', 'utf8');
    const sessionKey = Buffer.from(sessionKeyHex, 'hex');
    const sendKey = Buffer.from(crypto.hkdfSync('sha256', sessionKey, Buffer.alloc(0), Buffer.from('c2s', 'utf8'), 32));
    const iv = crypto.randomBytes(IV_BYTES);
    const aad = Buffer.from('env1\n' + path + '\n' + ts + '\n' + mid, 'utf8');

    const plain = Buffer.concat([Buffer.from([FLAG_RAW]), payload]);
    const cipher = crypto.createCipheriv('aes-256-gcm', sendKey, iv);
    const ct = Buffer.concat([cipher.update(plain), cipher.final()]);
    const combined = Buffer.concat([ct, cipher.getAuthTag()]);

    const envelope = '{"v":1,"kid":"' + kid + '","mid":' + mid + ',"ts":' + ts
        + ',"iv":"' + iv.toString('base64') + '","ct":"' + combined.toString('base64') + '"}';
    wireBody = Buffer.from(envelope, 'utf8');

    pm.request.body.raw = envelope;
    pm.request.headers.upsert({ key: 'X-Crypto', value: '1' });
} else {
    wireBody = Buffer.from(pm.request.body.raw || '', 'utf8');
}

// ===== 2) 协议版本头（客户端 D2 起恒发） =====
pm.request.headers.upsert({ key: 'X-Proto-Ver', value: 'cola/1.0.0' });

// ===== 3) v2 传输签名（签名覆盖线上信封字节） =====
if (sigKeyId && sigKeySecret) {
    const payloadHash = crypto.createHash('sha256').update(wireBody).digest('hex');
    const message = 'v2\n' + pm.request.method + '\n' + path + '\n' + query + '\n'
        + sigKeyId + '\n' + ts + '\n' + mid + '\n' + payloadHash;
    const signature = crypto.createHmac('sha256', sigKeySecret).update(message, 'utf8').digest('base64');
    pm.request.headers.upsert({ key: 'Signature', value: 'v2;' + sigKeyId + ';' + ts + ';' + mid + ';' + signature });
}

console.log('C5 envelope pre-request: path=' + path + ' query=' + query + ' mid=' + mid
    + ' envelope=' + (kid && sessionKeyHex ? 'on' : 'off') + ' signature=' + (sigKeyId ? 'v2' : 'off'));
