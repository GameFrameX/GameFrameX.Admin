# GameFrameX.Apps.GrafanaLoki

## 概述

GrafanaLoki 模块是 GameFrameX 框架中用于集成 Grafana Loki 日志分析系统的组件。该模块提供了统一的日志收集、分析和可视化功能，帮助开发者更好地监控和分析游戏服务器的运行状态。

## 功能特性

- 🔍 **实时日志收集**: 自动收集游戏服务器的各类日志信息
- 📊 **数据分析**: 提供丰富的日志分析和统计功能
- 📈 **可视化展示**: 集成 Grafana 仪表板，直观展示日志数据
- ⚡ **高性能**: 异步处理，不影响游戏服务器性能
- 🔧 **易于配置**: 简单的配置即可快速集成

## 核心组件

### GrafanaLokiAnalyticsManager

主要的分析管理器，负责：

- 日志数据的收集和处理
- 与 Loki 服务的通信
- 分析结果的缓存和管理
- 性能指标的统计

## 快速开始

### 1. 配置依赖

确保项目中已引用必要的 NuGet 包：

```xml
<PackageReference Include="GameFrameX.Core" Version="最新版本" />
<PackageReference Include="Serilog.Sinks.Loki" Version="最新版本" />
```

### 2. 配置 Loki 连接

在配置文件中添加 Loki 服务器配置：

```json
{
  "GrafanaLoki": {
    "ServerUrl": "http://localhost:3100",
    "Labels": {
      "application": "GameFrameX",
      "environment": "development"
    },
    "BatchSize": 1000,
    "FlushInterval": "00:00:05"
  }
}
```

### 3. 初始化服务

```csharp
// 在启动代码中注册服务
services.AddSingleton<GrafanaLokiAnalyticsManager>();

// 初始化分析管理器
var analyticsManager = serviceProvider.GetService<GrafanaLokiAnalyticsManager>();
await analyticsManager.InitializeAsync();
```

### 4. 使用示例

```csharp
// 记录游戏事件
analyticsManager.LogPlayerAction("player_login", new 
{
    PlayerId = 12345,
    LoginTime = DateTime.UtcNow,
    GameAppVersionDefinition = "1.0.0"
});

// 记录性能指标
analyticsManager.LogPerformanceMetric("server_response_time", 150);

// 记录错误信息
analyticsManager.LogError("database_connection_failed", exception);
```

## 配置选项

| 配置项            | 类型       | 默认值 | 说明            |
|-------------------|------------|--------|-----------------|
| ServerUrl         | string     | -      | Loki 服务器地址 |
| Labels            | Dictionary | -      | 默认标签        |
| BatchSize         | int        | 1000   | 批量发送大小    |
| FlushInterval     | TimeSpan   | 5秒    | 刷新间隔        |
| EnableCompression | bool       | true   | 是否启用压缩    |
| RetryCount        | int        | 3      | 重试次数        |

## 监控指标

### 系统指标

- CPU 使用率
- 内存使用情况
- 网络 I/O
- 磁盘 I/O

### 游戏指标

- 在线玩家数
- 请求响应时间
- 错误率
- 业务操作统计

### 自定义指标

- 支持自定义业务指标
- 灵活的标签系统
- 多维度数据分析

## Grafana 仪表板

模块提供了预配置的 Grafana 仪表板模板：

1. **系统监控面板**: 展示服务器基础指标
2. **游戏业务面板**: 展示游戏相关业务指标
3. **错误监控面板**: 展示错误和异常信息
4. **性能分析面板**: 展示性能相关指标

## 最佳实践

### 1. 标签使用

```csharp
// 使用有意义的标签进行分类
analyticsManager.LogEvent("player_action", data, new Dictionary<string, string>
{
    ["action_type"] = "combat",
    ["server_id"] = "server_001",
    ["region"] = "asia"
});
```

### 2. 批量处理

```csharp
// 对于高频事件，使用批量处理
var events = new List<LogEvent>();
// ... 收集事件
analyticsManager.LogBatch(events);
```

### 3. 错误处理

```csharp
try
{
    // 业务逻辑
}
catch (Exception ex)
{
    analyticsManager.LogError("business_error", ex, new 
    {
        UserId = userId,
        Operation = "purchase_item"
    });
}
```

## 故障排除

### 常见问题

1. **连接失败**
    - 检查 Loki 服务器地址和端口
    - 确认网络连通性
    - 验证认证配置

2. **数据丢失**
    - 检查批量大小配置
    - 确认刷新间隔设置
    - 查看错误日志

3. **性能问题**
    - 调整批量大小
    - 优化标签使用
    - 考虑异步处理

### 调试模式

启用调试模式获取详细日志：

```json
{
  "Logging": {
    "LogLevel": {
      "GameFrameX.Apps.GrafanaLoki": "Debug"
    }
  }
}
```

## 版本历史

- **v1.0.0**: 初始版本，基础日志收集功能
- **v1.1.0**: 添加性能指标支持
- **v1.2.0**: 集成 Grafana 仪表板模板

## 贡献指南

欢迎提交 Issue 和 Pull Request 来改进这个模块。

## 开源协议

详见 [LICENSE](LICENSE) 文件。

## 相关链接

- [Grafana Loki 官方文档](https://grafana.com/docs/loki/)
- [GameFrameX 框架文档](https://github.com/GameFrameX/GameFrameX)
- [Serilog.Sinks.Loki](https://github.com/JosephWoodward/Serilog-Sinks-Loki)