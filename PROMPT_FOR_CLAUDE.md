# 给Claude的项目接手提示词

复制以下内容发送给Claude：

---

## 提示词

```
我需要你接手一个Unity手游项目。请阅读以下信息并帮我完成后续工作。

### 项目信息

- **项目名称**：神界：混沌边境 (Divine Chaos: Edge of Worlds)
- **项目类型**：日式回合制RPG手游（类似原神、崩坏：星穹铁道）
- **GitHub仓库**：https://github.com/owjk123/JRPG-Studio
- **技术栈**：Unity 2022.3 LTS, C#, Android (API 24-33)
- **包名**：com.jrpgstudio.divinechaos

### 项目当前状态

代码框架已完成，包含：
- 战斗系统（ATB半即时制、技能、状态效果）- 约8,883行代码
- 角色系统（养成、装备、突破、抽卡、存档）- 约6,700行代码  
- UI系统（主界面、角色详情、抽卡动画、设置）- 约12,930行代码
- 数据资产（37个ScriptableObject）
- 3个Unity场景（Loading、MainMenu、Battle）
- GitHub Actions CI/CD配置

总计：186个文件，约50,000行代码

### 需要你完成的任务

**优先级1：构建APK**
1. 克隆仓库：git clone https://github.com/owjk123/JRPG-Studio
2. 用Unity 2022.3 LTS打开项目
3. 构建Android APK
4. 测试运行

**优先级2：美术资源**（如果没有资源，使用占位符）
- 角色立绘
- UI素材
- 技能特效
- BGM和音效

**优先级3：游戏内容填充**
- 扩展角色数据库
- 设计更多敌人
- 创建关卡内容
- 完善剧情对话

### 关键技术规范

- 所有Manager使用单例模式
- 数据类使用ScriptableObject
- 事件驱动架构
- 中文注释
- 命名空间：JRPG.*

### 抽卡系统参数

- SSR概率：2%（90抽保底）
- SR概率：10%（10抽保底）
- R概率：35%
- N概率：53%

### 角色养成参数

- 等级上限：200级
- 突破次数：5次（每次+20级上限）
- 装备槽位：6个（武器、头盔、铠甲、护手、饰品×2）
- 套装效果：8种

### 重要文件

- 项目说明：PROJECT_README.md
- 构建指南：BUILD_GUIDE.md
- 交接文档：HANDOVER.md
- 设计文档：design/gdd/*.md

请先克隆仓库了解项目结构，然后告诉我你打算如何推进。
```

---

## 快速命令

克隆项目：
```bash
git clone https://github.com/owjk123/JRPG-Studio
cd JRPG-Studio
```

查看项目结构：
```bash
find . -name "*.cs" | head -20
cat PROJECT_README.md
```
