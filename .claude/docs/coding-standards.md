# 编码标准 - JRPG-Studio

本文档定义了日式回合制RPG项目的编码规范。

## 命名规范

### 类命名
- PascalCase: `BattleManager`, `CharacterData`
- 接口以I开头: `IDamageable`, `IStatusEffect`
- ScriptableObject以Data结尾: `CharacterData`, `SkillData`

### 方法命名
- PascalCase: `DealDamage()`, `ApplyStatus()`
- 布尔返回以Is/Can/Has开头: `IsAlive()`, `CanAct()`
- 事件处理以On开头: `OnBattleStart()`, `OnTurnEnd()`

### 变量命名
- camelCase私有字段带_前缀: `_currentHp`, `_statusEffects`
- 公有属性PascalCase: `CurrentHp`, `MaxHp`
- 常量全大写: `MAX_PARTY_SIZE`, `DEFAULT_TURN_DURATION`

### Unity特定
- 序列化私有字段: `[SerializeField] private int _value;`
- Inspector显示: `[Header("战斗属性")]`, `[Tooltip("基础攻击力")]`

## 代码结构

### MonoBehaviour组件
```csharp
public class BattleUnit : MonoBehaviour
{
    // 1. 序列化字段
    [Header("References")]
    [SerializeField] private Animator _animator;
    
    // 2. 公有属性
    public bool IsAlive => _currentHp > 0;
    
    // 3. 私有字段
    private int _currentHp;
    
    // 4. Unity生命周期
    void Awake() { }
    void Start() { }
    void Update() { }
    
    // 5. 公有方法
    public void TakeDamage(int amount) { }
    
    // 6. 私有方法
    private void UpdateHealthBar() { }
    
    // 7. 协程
    private IEnumerator ExecuteAttack() { }
}
```

### ScriptableObject数据
```csharp
[CreateAssetMenu(fileName = "NewSkill", menuName = "JRPG/Skill")]
public class SkillData : ScriptableObject
{
    [Header("基础信息")]
    public int skillId;
    public string skillName;
    [TextArea] public string description;
    
    [Header("战斗属性")]
    public int mpCost;
    public int baseDamage;
    public float damageMultiplier;
    
    [Header("目标设置")]
    public TargetType targetType;
    public int targetCount;
}
```

## 回合制战斗规范

### 状态机模式
- 使用状态机管理战斗流程
- 每个状态独立类，实现IState接口
- 状态转换通过事件触发

### 行动队列
- 使用优先队列管理行动顺序
- 速度值决定行动顺序
- 支持行动插队（加速技能）

### 伤害计算
- 所有伤害计算在单独的DamageCalculator类
- 支持伤害修正链（Buff/Debuff叠加）
- 使用事件通知伤害结果

### 状态效果
- 每个状态效果独立类，实现IStatusEffect
- 使用组合模式管理多个状态效果
- 回合结束时统一结算

## 性能优化

### 对象池
- 战斗单位使用对象池
- 特效对象使用对象池
- UI面板使用对象池

### 避免GC
- 使用StringBuiler拼接字符串
- 缓存GetComponent结果
- 避免在Update中new对象

### 资源加载
- 使用Addressables异步加载
- 预加载战斗所需资源
- 场景切换时卸载不需要的资源

## 注释规范

### XML文档注释
```csharp
/// <summary>
/// 对目标造成伤害
/// </summary>
/// <param name="target">目标单位</param>
/// <param name="amount">基础伤害量</param>
/// <returns>实际造成的伤害</returns>
public int DealDamage(BattleUnit target, int amount)
```

### TODO格式
```csharp
// TODO: [功能] 后续需要添加暴击系统
// FIXME: [问题] 当目标为空时会崩溃
// HACK: [临时] 使用硬编码值，后续改为配置
```

## Git提交规范

### 提交信息格式
```
<type>(<scope>): <subject>

<body>

<footer>
```

### Type类型
- `feat`: 新功能
- `fix`: 修复bug
- `docs`: 文档更新
- `style`: 代码格式
- `refactor`: 重构
- `test`: 测试
- `chore`: 构建/工具

### 示例
```
feat(battle): 添加ATB行动条系统

- 实现基于速度值的行动顺序
- 添加行动条UI显示
- 支持加速/减速效果

Closes #123
```
