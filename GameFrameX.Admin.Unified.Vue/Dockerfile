# node镜像
FROM node:24 AS build-stage
RUN echo "-------------------- start build --------------------"

RUN echo "-------------------- web environment configuration --------------------"

# # 指定接下来的工作路径为/app  - 类似于cd命令
WORKDIR /app

RUN corepack enable

# 单仓整合（2026-10-19）：docker build 上下文已上移为仓库根；
# Vue 项目内的依赖清单通过 GameFrameX.Admin.Unified.Vue/ 子路径引用。
COPY GameFrameX.Admin.Unified.Vue/package.json GameFrameX.Admin.Unified.Vue/pnpm-lock.yaml GameFrameX.Admin.Unified.Vue/.pnpmrc GameFrameX.Admin.Unified.Vue/pnpm-workspace.yaml ./

RUN echo "-------------------- start install --------------------"

RUN pnpm install --frozen-lockfile

RUN echo "-------------------- end install --------------------"

# 拷贝 Vue 子项目全部源（vite.config.ts / src / public / 等）到 /app
# 仓库根 .dockerignore 需排除 .git/**、**/node_modules、GameFrameX.Admin.Unified.Api/** 等
COPY GameFrameX.Admin.Unified.Vue/ ./

# 设置构建环境变量（vite loadEnv 依赖 NODE_ENV 匹配 .env.production）
# 必须在 pnpm install 之后设置，否则包管理器会跳过 devDependencies
ENV NODE_ENV=production

RUN echo "-------------------- build start  --------------------"

RUN pnpm run build

RUN echo "-------------------- build end  --------------------"


FROM nginx:1.20.2
EXPOSE 80 443
RUN rm /etc/nginx/conf.d/default.conf

# nginx 站点配置 + entrypoint 已随 GameFrameX.Admin.Unified.Vue/build 上移仓库根 build/
# 这里直接从仓库根 build context 引入
COPY build/default.conf /etc/nginx/conf.d/
COPY build/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Vite 输出 build/dist（在 build-stage 内为 /app/build/dist，相对 cwd 计算）
COPY --from=build-stage /app/build/dist /usr/share/nginx/html/

ENTRYPOINT ["/entrypoint.sh"]
