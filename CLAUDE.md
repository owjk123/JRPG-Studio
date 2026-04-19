# JRPG-Studio — 日式回合制RPG手游开发框架
基于Claude Code Game Studios架构，专为数日式回合制RPG手游设计的AI协作开发系统。

## 技术栈
- **引擎**: Unity 2022.3 LTS
- **语言**: C# (.NET Standard 2.1)
- **版本控制**: Git (trunk-based development)
- **构建系统**: Unity Cloud Build / Jenkins
- **目标平台**: Android (API 24+)

## 项目代号
`JRPG-Studio`

## 当前阶段
- [ ] Phase 1: 核心框架搭建
- [ ] Phase 2: 战斗系统原型
- [ ] Phase 3: 角色与养成系统
- [ ] Phase 4: 剧情与UI系统
- [ ] Phase 5: 联网与后端集成
- [ ] Phase 6: 测试与发布

## 核心设计理念
1. **回合制战斗**: 经典ATB/CTB系统，策略深度
2. **二次元美术**: 日式动漫风格，高质量立绘
3. **角色养成**: 多维度成长，收集驱动
4. **叙事体验**: 视觉小说风格剧情
5. **长线运营**: 日常系统、活动、抽卡

## 项目结构
```
src/
├── Core/           # 核心框架、单例管理器
├── Battle/         # 回合制战斗系统
├── Character/      # 角色数据与成长
├── Story/          # 剧情与对话系统
├── UI/             # 界面系统
├── Network/        # 联网功能
├── Data/           # ScriptableObject数据
└── Utilities/      # 工具类
```

## 协作协议
**问题 → 选项 → 决定 → 草稿 → 批准**
- Agent必须请求权限后才能写入文件
- 用户拥有所有最终决定权
- 使用 AskUserQuestion 提供结构化决策

## 编码标准
@.claude/docs/coding-standards.md

## 协调规则
@.claude/docs/coordination-rules.md

## 第一次使用?
运行 `/start` 开始引导式项目初始化流程。
