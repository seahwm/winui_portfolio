# WinUI 3 个人资产管理系统 (Portfolio Management System)

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![WinUI 3](https://img.shields.io/badge/UI-WinUI%203-0078D4?logo=windows)](https://learn.microsoft.com/windows/apps/winui/winui3/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

基于 **WinUI 3** 与 **.NET 8** 构建的现代化 Windows 桌面端个人资产与投资管理工具，遵循 Fluent Design 设计语言，支持资产快照录入、股票持仓盈亏追踪与趋势图表可视化，并采用轻量透明的本地 JSON 数据存储。

---

## ⚠️ 免责声明 (Disclaimer)

> [!WARNING]
> 本项目的绝大部分代码均为 **Vibe Coding**（AI 辅助灵感编码）生成，主要供个人财务记录与技术探索之用。
> 
> - **不保证最佳性能**：代码未经严格的高并发或大数据量性能调优。
> - **不保证 Bug-Free**：部分边界情况可能存在未捕获的异常或 UI 渲染小问题。
> - **数据安全与备份**：请用户自行定期备份 `./data/data.json` 数据文件，作者不对因使用本程序导致的数据丢失或计算误差承担任何责任。

---

## ✨ 核心特性

- **📊 资产总览 (Dashboard)**
  - 实时统计净资产、流动资产、非流动资产等关键指标。
  - 集成 [LiveCharts2](https://github.com/beto-rodriguez/LiveCharts2) (SkiaSharp)，提供直观的资产构成分布图与历史走势图表。

- **📸 资产快照 (Asset Snapshots)**
  - 按时间周期（如月度/季度）记录资产快照。
  - 支持资产明细录入，跟踪各阶段资产配置变动与回报率。

- **📈 股票与投资追踪 (Stock Tracking)**
  - 详细记录股票代码、名称、持股数、成本价、当前价与股票分类。
  - 自动计算持仓市值、浮动盈亏与投资收益率。

- **💱 币种支持**
  - 目前仅支持 **USD（美元）** 与 **MYR（马来西亚林吉特）** 两种币种。
  - 每次快照记录均包含 `usdRate`（USD 兑 MYR 汇率），用于统一资产折算与汇总统计。

- **🏷️ 自定义资产与股票类别 (Custom Categories)**
  - 灵活配置资产类型（如现金、存款、房产、公积金等）与股票类型（如美股、马股、ETF、行业分类）。

- **💾 本地离线存储与高可维护性 (Local Storage)**
  - 数据统一保存在可执行文件同级的 `./data/data.json` 目录中。
  - 零云端依赖，保护个人财务隐私；便于日常备份、版本控制与跨设备迁移。

- **🎨 现代化 Windows 11 设计**
  - 深度适配 Windows 11 Fluent Design 与 Mica（云母）半透明背景质感。

---

## 📄 数据结构说明 (`data.json`)

系统的数据完全存储在 `./data/data.json` 文件中，顶层结构包含 `assetType`、`stockType` 和 `data` 三大数组。

### 字段说明

1. **`assetType`**：资产大类定义
   - `name` (*string*)：资产类别名称（如 "现金"、"股票账户"、"公积金"）。
   - `isValOnly` (*bool*)：是否仅记录总市值（不包含下属股票/证券持仓明细）。
   - `isRetirement` (*bool*)：是否为退休/公积金资产。
   - `isDebt` (*bool*)：是否为负债项目（净资产计算时会进行扣除）。
   - `isParent` / `sysRec` (*bool*)：层级与系统预设标记。

2. **`stockType`**：股票分类标签数组
   - 字符串列表，例如 `["美股", "马股", "ETF", "高股息"]`。

3. **`data`**：快照历史记录列表
   - `date` (*string*)：快照日期，格式为 `YYYY-MM-DD`。
   - `usdRate` (*decimal*)：该快照日期的 USD 兑 MYR 汇率。
   - `assets` (*array*)：该快照下的各项资产列表：
     - `name` (*string*)：资产名称（如 "Maybank 储蓄"、"Interactive Brokers"）。
     - `value` (*decimal*)：资产当前估值。
     - `cost` (*decimal*)：本金/成本金额。
     - `currency` (*string*)：币种（目前限定 `MYR` 或 `USD`）。
     - `type` (*string*)：对应的 `assetType` 名称。
     - `remark` (*string*)：备注说明。
     - `content` (*array, 可选*)：下属持仓明细（当非 `isValOnly` 时）：
       - `symbol` (*string*)：股票代号。
       - `name` (*string*)：股票名称。
       - `stockType` (*string*)：所属股票分类。
       - `units` (*decimal*)：持股股数。
       - `avgPrice` (*decimal*)：买入均价。
       - `currentPrice` (*decimal*)：当前市场单价。
       - `dividendYield` (*decimal*)：股息率。
       - `accumulatedProfit` (*decimal*)：累计已实现收益或股息。
       - `currency` (*string*)：计价币种（`MYR` 或 `USD`）。

### 结构示例

```json
{
  "assetType": [
    {
      "name": "银行存款",
      "isValOnly": true,
      "isRetirement": false,
      "isDebt": false,
      "isParent": false,
      "sysRec": true
    },
    {
      "name": "美股投资",
      "isValOnly": false,
      "isRetirement": false,
      "isDebt": false,
      "isParent": false,
      "sysRec": false
    }
  ],
  "stockType": [
    "美股",
    "马股",
    "ETF"
  ],
  "data": [
    {
      "date": "2026-09-01",
      "usdRate": 4.25,
      "assets": [
        {
          "name": "日常储蓄账户",
          "value": 20000.0,
          "cost": 20000.0,
          "currency": "MYR",
          "type": "银行存款",
          "remark": "应急备用金"
        },
        {
          "name": "券商主账户",
          "value": 15000.0,
          "cost": 12000.0,
          "currency": "USD",
          "type": "美股投资",
          "remark": "美股长期持仓",
          "content": [
            {
              "symbol": "VOO",
              "name": "Vanguard S&P 500 ETF",
              "stockType": "ETF",
              "units": 20.0,
              "avgPrice": 450.0,
              "currentPrice": 520.0,
              "dividendYield": 1.45,
              "accumulatedProfit": 300.0,
              "currency": "USD",
              "remark": ""
            }
          ]
        }
      ]
    }
  ]
}
```

---

## 🛠️ 技术栈

| 模块 | 技术选型 | 说明 |
| :--- | :--- | :--- |
| **运行时框架** | .NET 8.0 (`net8.0-windows10.0.19041.0`) | 现代跨平台高性能 .NET 运行时 |
| **UI 框架** | Windows App SDK / WinUI 3 | Windows 原生新一代桌面 UI 体系 |
| **图表组件** | LiveChartsCore.SkiaSharpView.WinUI | 基于 SkiaSharp 的高性能图表引擎 |
| **数据持久化** | System.Text.Json | 轻量高效的本地 JSON 序列化与读写 |

---

## 🚀 快速开始

### 前置要求
- **操作系统**：Windows 10 (版本 1809 及以上) 或 Windows 11
- **开发工具**：[Visual Studio 2022](https://visualstudio.microsoft.com/)（建议 17.8 及以上版本），安装以下工作负载：
  - **.NET 桌面开发**（.NET Desktop Development）
  - **Windows 应用 SDK C# 模板**
- **.NET SDK**：[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### 克隆与运行

1. **克隆仓库**：
   ```bash
   git clone <repository-url>
   cd winui_portfolio
   ```

2. **本地调试运行**：
   ```powershell
   dotnet build -c Debug -p:Platform=x64
   dotnet run --project winui_portfolio.csproj -p:Platform=x64
   ```

---

## 📦 打包与发布 (Publish)

本项目配置为 **Unpackaged（非打包）独立部署**，支持发布为单文件（Single-File）可执行程序。

### 方式一：命令行一键发布单文件 (x64)

```powershell
# 1. 清理旧构建缓存
dotnet clean -c Release -p:Platform=x64

# 2. 发布自包含单文件应用
dotnet publish -c Release -p:Platform=x64
```

发布成功后的产物位于：
```text
bin/win-x64/publish/x64/winui_portfolio.exe
```

### 方式二：Visual Studio 界面发布

1. 在解决方案管理器中，右键项目 `winui_portfolio` -> **发布 (Publish)**。
2. 选择现有的发布配置文件（例如 `win-x64`）。
3. 点击 **发布 (Publish)** 按钮即可。

---

## 📂 数据管理与备份

程序首次运行时，会在当前工作目录下自动初始化数据文件夹及文件：
```text
winui_portfolio/
  ├── winui_portfolio.exe
  └── data/
       └── data.json    # 个人资产、快照及类型数据
```

> **提示**：若需备份或迁移数据，只需复制 `data/data.json` 文件至新设备的对应目录下即可。

---

## 📄 开源协议

本项目采用 [MIT License](LICENSE) 开源协议。
