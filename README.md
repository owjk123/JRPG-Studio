# JRPG-Studio 🎮

> 日式回合制RPG手游开发框架
> 基于 Claude Code Game Studios 架构

## 项目状态

🚧 **初始化阶段** - 框架搭建完成，准备开始核心开发

## 快速开始

### 1. 克隆项目
```bash
git clone <your-repo-url>
cd JRPG-Studio
```

### 2. 打开 Claude Code
```bash
claude
```

### 3. 运行初始化命令
```
/start
```

## 项目结构

```
JRPG-Studio/
├── .claude/              # Claude Code 配置
│   ├── agents/           # AI Agent 定义
│   ├── skills/           # 技能命令
│   ├── hooks/            # 自动化钩子
│   └── docs/             # 文档
├── src/                  # Unity C# 代码
│   ├── Core/             # 核心框架
│   ├── Battle/           # 战斗系统
│   ├── Character/        # 角色系统
│   ├── Story/            # 剧情系统
│   └── UI/               # 界面系统
├── assets/               # 游戏资源
├── design/               # 设计文档
│   └── gdd/              # 游戏设计文档
├── docs/                 # 技术文档
├── tests/                # 测试代码
└── production/           # 生产管理
```

## 核心系统

### 战斗系统
- 回合制战斗 (ATB/CTB可配置)
- 技能与状态效果系统
- 行动队列管理
- 战斗AI

### 角色系统
- 角色数据结构 (CharacterData)
- 成长系统 (等级/突破)
- 技能系统
- 装备系统

### 商业化
- 抽卡系统
- 月卡/通行证
- 皮肤商城

## 可用命令

| 命令 | 说明 |
|------|------|
| `/start` | 项目初始化引导 |
| `/brainstorm` | 头脑风暴 |
| `/design-battle` | 战斗系统设计 |
| `/design-character` | 角色设计 |
| `/create-epics` | 创建史诗任务 |
| `/dev-story` | 开发用户故事 |
| `/code-review` | 代码审查 |
| `/help` | 查看帮助 |

## 技术栈

- **引擎**: Unity 2022.3 LTS
- **语言**: C# (.NET Standard 2.1)
- **渲染**: URP (Universal Render Pipeline)
- **平台**: Android (API 24+)

## Agent 架构

### Directors (Opus)
- `creative-director` - 创意总监
- `technical-director` - 技术总监

### Department Leads (Sonnet)
- `battle-designer` - 战斗设计师
- `character-designer` - 角色设计师
- `gacha-designer` - 抽卡设计师

### Specialists
- `unity-specialist` - Unity专家
- `battle-programmer` - 战斗程序
- `ui-programmer` - UI程序

## 开发进度

### Phase 1: 核心框架 ✅
- [x] 项目结构搭建
- [x] Agent系统配置
- [x] 基础代码框架
- [ ] Unity项目配置

### Phase 2: 战斗系统
- [ ] 战斗原型
- [ ] 技能系统
- [ ] 状态效果
- [ ] 战斗UI

### Phase 3: 角色系统
- [ ] 角色数据
- [ ] 成长系统
- [ ] 装备系统

### Phase 4: 内容填充
- [ ] 角色设计
- [ ] 技能设计
- [ ] 剧情内容

## 贡献指南

1. 所有代码变更需要通过 `/code-review`
2. 设计文档遵循 `design/gdd/` 目录结构
3. 提交信息遵循 Conventional Commits 规范

## 许可证

MIT License

---

*基于 [Claude Code Game Studios](https://github.com/Donchitos/Claude-Code-Game-Studios) 构建*
