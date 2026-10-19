-- GameFrameX Admin 内置 PostgreSQL 初始化：首次启动自动创建主库与默认租户库。
-- 由 docker/postgres/initdb.d 挂载执行（仅数据卷为空时运行一次），账号由 compose 的 POSTGRES_USER 提供。
CREATE DATABASE gameframex_admin;
CREATE DATABASE gameframex_admin_tenant_1300000000001;
