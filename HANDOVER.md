# JRPG-Studio 项目交接文档

## 项目概述

**项目名称**：神界：混沌边境 (Divine Chaos: Edge of Worlds)
**项目类型**：日式回合制RPG手游
**GitHub仓库**：https://github.com/owjk123/JRPG-Studio
**技术栈**：Unity 2022.3 LTS, C#, Android (API 24-33)

---

## 当前项目状态

### 已完成 ✅

| 模块 | 文件数 | 代码行数 | 状态 |
|------|--------|----------|------|
| 核心框架 | 15+ | ~2,000 | ✅ 完成 |
| 战斗系统 | 20+ | ~8,883 | ✅ 完成 |
| 角色系统 | 17+ | ~6,700 | ✅ 完成 |
| UI系统 | 25+ | ~12,930 | ✅ 完成 |
| 数据资产 | 37 | - | ✅ 完成 |
| Unity场景 | 3 | - | ✅ 完成 |
| GitHub CI/CD | 2 | - | ✅ 完成 |

### 待完成 🚧

- [ ] 添加美术资源（角色立绘、UI素材、技能特效）
- [ ] 添加音频资源（BGM、音效）
- [ ] 配置GitHub Actions Secrets（Unity许可证）
- [ ] 构建并测试APK
- [ ] 游戏内容填充（更多角色、敌人、关卡）

---

## 项目结构

```
JRPG-Studio/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/           # GameManager, AudioManager, SceneLoader
│   │   ├── Battle/         # ATB系统, 技能执行, 状态效果
│   │   ├── Character/      # 角色实例, 装备, 抽卡, 存档
│   │   ├── UI/             # 面板, 动画, 交互
│   │   ├── System/         # 商店, 任务系统
│   │   ├── Debug/          # 调试控制台, FPS, 内存监控
│   │   └── Startup/        # 游戏启动流程
│   ├── Resources/          # 运行时资源
│   ├── ScriptableObjects/  # 数据资产
│   ├── Editor/             # 编辑器扩展
│   └── Scenes/             # Loading, MainMenu, Battle
├── .github/workflows/      # CI/CD配置
├── design/gdd/             # 游戏设计文档
├── BUILD_GUIDE.md          # 构建指南
└── PROJECT_README.md       # 项目说明
```

---

## 关键技术规范

### Unity配置
- **版本**：Unity 2022.3 LTS
- **渲染管线**：URP
- **脚本后端**：IL2CPP
- **目标架构**：ARM64 + ARMv7
- **最低Android**：API 24 (Android 7.0)
- **目标Android**：API 33 (Android 13)

### 代码规范
- 所有Manager使用单例模式
- 数据类使用ScriptableObject
- 事件驱动架构（Action/UnityEvent）
- 完善的中文注释
- 命名空间：JRPG.*

### 游戏设计参数
- **抽卡概率**：SSR 2%, SR 10%, R 35%, N 53%
- **保底机制**：90抽必出SSR, 10抽必出SR+
- **等级上限**：200级（5次突破，每次+20级）
- **装备槽位**：武器、头盔、铠甲、护手、饰品×2
- **套装效果**：8种套装

---

## 下一步任务

### 优先级1：构建APK

1. 用Unity 2022.3 LTS打开项目
2. 等待资源导入完成
3. 菜单：`JRPG Studio > 构建 > 快速构建APK (Debug)`
4. 测试APK在Android设备上运行

### 优先级2：配置自动构建

1. 访问 https://github.com/owjk123/JRPG-Studio/settings/secrets/actions
2. 添加Secrets：
   - `UNITY_EMAIL`: Unity账号邮箱
   - `UNITY_PASSWORD`: Unity账号密码
   - `UNITY_LICENSE`: Unity许可证内容
3. 推送代码触发自动构建

### 优先级3：美术资源

需要添加的资源类型：
- 角色立绘：`Assets/Resources/Characters/`
- UI素材：`Assets/Resources/UI/`
- 技能特效：`Assets/Resources/Effects/`
- 音频文件：`Assets/Resources/Audio/`

### 优先级4：游戏内容

- 扩展角色数据库（目前5个初始角色）
- 设计更多敌人类型
- 创建更多关卡内容
- 完善主线剧情

---

## 重要文件路径

| 文件 | 路径 | 用途 |
|------|------|------|
| 项目说明 | `PROJECT_README.md` | 项目概述 |
| 构建指南 | `BUILD_GUIDE.md` | APK构建步骤 |
| 游戏配置 | `Assets/Resources/GameConfig.asset` | 全局配置 |
| 角色数据 | `Assets/ScriptableObjects/Characters/` | 角色定义 |
| 敌人数据 | `Assets/ScriptableObjects/Enemies/` | 敌人定义 |
| 关卡数据 | `Assets/ScriptableObjects/Stages/` | 关卡配置 |
| 主场景 | `Assets/Scenes/MainMenu.unity` | 主界面 |
| 战斗场景 | `Assets/Scenes/Battle.unity` | 战斗界面 |

---

## 设计文档位置

- `design/gdd/race-and-class-system.md` - 种族职业系统
- `design/gdd/battle-system.md` - 战斗系统设计
- `design/gdd/progression-system.md` - 养成系统
- `design/gdd/gacha-system.md` - 抽卡系统
- `design/gdd/world-and-story.md` - 世界观剧情
- `design/gdd/main-gdd.md` - 主设计文档

---

## 联系与支持

- GitHub仓库：https://github.com/owjk123/JRPG-Studio
- 问题反馈：GitHub Issues
- 功能请求：GitHub Discussions
