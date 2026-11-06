-- GameFrameX Admin 内置 PostgreSQL 初始化：首次启动自动创建主库（C406 起单主库，无独立业务库）。
-- 由 docker/postgres/initdb.d 挂载执行（仅数据卷为空时运行一次），账号由 compose 的 POSTGRES_USER 提供。
CREATE DATABASE gameframex_admin;
