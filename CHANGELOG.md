# 更新日志

所有重要的版本更新都将记录在此文件中。格式遵循 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/) 标准。

## [1.0.0] - 2026-04-19

### Added
- 项目初始化
- 核心框架搭建
- Unity 2022.3 LTS 配置
- URP 渲染管线配置
- Android 构建配置

### 核心系统
- GameManager - 游戏状态管理
- AudioManager - 音频管理
- SceneLoader - 场景加载
- GameInitializer - 游戏初始化

### 角色系统
- CharacterData - 角色数据系统
- CharacterStats - 角色属性
- SkillData - 技能数据系统
- CharacterRarity - 角色稀有度
- CharacterRace - 角色种族
- CharacterClass - 角色职业

### 战斗系统
- BattleManager - 战斗管理器
- BattleUnit - 战斗单位
- ATBController - ATB行动条控制器
- SkillExecutor - 技能执行器
- StatusEffectManager - 状态效果管理
- BattleFlowController - 战斗流程控制

### 资源系统
- CurrencyManager - 货币管理
- InventoryManager - 背包管理
- ItemData - 物品数据

### 抽卡系统
- GachaManager - 抽卡管理器
- GachaPool - 抽卡池
- PitySystem - 保底系统

### 装备系统
- EquipmentManager - 装备管理
- EquipmentData - 装备数据
- EquipmentSlot - 装备槽位
- SetBonus - 套装效果

### 存档系统
- SaveManager - 存档管理
- PlayerData - 玩家数据

### UI系统
- BattleHUD - 战斗界面
- ActionMenu - 行动菜单
- BattleLog - 战斗日志
- DamageNumber - 伤害数字

### 编辑器扩展
- BuildScript - APK构建脚本
- QuickBuild - 快速构建工具
- JRPGMenuItems - 自定义菜单项
- DataGenerator - 数据生成工具
- BuildTools - 构建工具增强
- PreBuildChecker - 构建前检查

### 资源文件
- MainMenu.unity - 主菜单场景
- Battle.unity - 战斗场景
- Loading.unity - 加载场景
- CharacterDataList - 角色数据列表
- SkillDatabase - 技能数据库
- GachaPoolData - 抽卡池数据
- BattleEncounterData - 战斗遭遇数据
- EnemyData - 敌人数据

### 设计文档
- 种族职业系统设计
- 战斗系统设计
- 养成系统设计
- 抽卡系统设计
- 日常活动设计
- 世界观剧情设计

## [0.1.0] - 2026-04-12

### Added
- 项目创建
- 基础目录结构
- .claude 配置文件
