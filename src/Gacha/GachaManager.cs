// GachaManager.cs - 抽卡管理器
// 实现抽卡的核心逻辑

using System;
using System.Collections.Generic;
using UnityEngine;
using JRPG.Character;

namespace JRPG.Gacha
{
    /// <summary>
    /// 抽卡管理器单例类
    /// </summary>
    public class GachaManager
    {
        #region 单例
        
        private static GachaManager _instance;
        public static GachaManager Instance => _instance ??= new GachaManager();
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 抽卡开始事件
        /// </summary>
        public event Action<GachaType, string> OnGachaStarted;
        
        /// <summary>
        /// 单抽完成事件
        /// </summary>
        public event Action<GachaResult> OnSinglePullComplete;
        
        /// <summary>
        /// 十连完成事件
        /// </summary>
        public event Action<GachaResult> OnTenPullComplete;
        
        /// <summary>
        /// 抽到SSR事件
        /// </summary>
        public event Action<GachaResultEntry> OnSSRPulled;
        
        /// <summary>
        /// 保底触发事件
        /// </summary>
        public event Action<PityType> OnPityTriggered;
        
        #endregion
        
        #region 私有变量
        
        /// <summary>
        /// 卡池配置列表
        /// </summary>
        private Dictionary<string, GachaPool> _pools = new Dictionary<string, GachaPool>();
        
        /// <summary>
        /// 当前选中的卡池
        /// </summary>
        private string _currentPoolId;
        
        /// <summary>
        /// 抽卡历史
        /// </summary>
        private GachaHistory _history = new GachaHistory();
        
        /// <summary>
        /// 随机数生成器
        /// </summary>
        private System.Random _random = new System.Random();
        
        #endregion
        
        #region 构造函数
        
        private GachaManager()
        {
        }
        
        #endregion
        
        #region 公开方法
        
        /// <summary>
        /// 初始化卡池
        /// </summary>
        public void Initialize()
        {
            LoadPools();
            _currentPoolId = "standard";
        }
        
        /// <summary>
        /// 加载所有卡池配置
        /// </summary>
        private void LoadPools()
        {
            // 从Resources加载卡池配置
            var pools = Resources.LoadAll<GachaPool>("Data/Gacha/");
            foreach (var pool in pools)
            {
                _pools[pool.poolId] = pool;
            }
            
            Debug.Log($"已加载 {_pools.Count} 个卡池");
        }
        
        /// <summary>
        /// 获取当前卡池
        /// </summary>
        public GachaPool GetCurrentPool()
        {
            return GetPool(_currentPoolId);
        }
        
        /// <summary>
        /// 获取指定卡池
        /// </summary>
        public GachaPool GetPool(string poolId)
        {
            return _pools.TryGetValue(poolId, out var pool) ? pool : null;
        }
        
        /// <summary>
        /// 切换卡池
        /// </summary>
        public void SwitchPool(string poolId)
        {
            if (_pools.ContainsKey(poolId))
            {
                _currentPoolId = poolId;
                Debug.Log($"切换到卡池: {_pools[poolId].poolName}");
            }
        }
        
        /// <summary>
        /// 执行单抽
        /// </summary>
        public GachaResult SinglePull()
        {
            var pool = GetCurrentPool();
            if (pool == null)
            {
                Debug.LogError("当前没有可用卡池");
                return null;
            }
            
            // 检查消耗
            if (!CanAffordPull(pool, 1))
            {
                Debug.LogWarning("星辉不足，无法单抽");
                return null;
            }
            
            // 消耗货币
            ResourcesManager.Instance.RemoveItem(pool.currencyId, pool.singlePullCost);
            
            // 执行抽卡
            var result = Pull(pool, GachaType.Single, 1);
            
            // 触发事件
            OnSinglePullComplete?.Invoke(result);
            
            if (result.hasSSR)
            {
                var ssrEntry = result.entries.Find(e => e.rarity == CharacterRarity.SSR);
                OnSSRPulled?.Invoke(ssrEntry);
            }
            
            return result;
        }
        
        /// <summary>
        /// 执行十连
        /// </summary>
        public GachaResult TenPull()
        {
            var pool = GetCurrentPool();
            if (pool == null)
            {
                Debug.LogError("当前没有可用卡池");
                return null;
            }
            
            // 检查消耗
            if (!CanAffordPull(pool, 10))
            {
                Debug.LogWarning("星辉不足，无法十连");
                return null;
            }
            
            // 消耗货币
            int cost = pool.tenPullCost;
            if (pool.tenPullDiscountEnabled)
                cost = Mathf.RoundToInt(cost * 0.8f);
            ResourcesManager.Instance.RemoveItem(pool.currencyId, cost);
            
            // 执行抽卡
            var result = Pull(pool, GachaType.TenPull, 10);
            
            // 触发事件
            OnTenPullComplete?.Invoke(result);
            
            if (result.hasSSR)
            {
                var ssrEntry = result.entries.Find(e => e.rarity == CharacterRarity.SSR);
                OnSSRPulled?.Invoke(ssrEntry);
            }
            
            return result;
        }
        
        /// <summary>
        /// 检查是否可以抽卡
        /// </summary>
        public bool CanAffordPull(GachaPool pool, int pullCount)
        {
            int cost = pullCount == 1 ? pool.singlePullCost : pool.tenPullCost;
            if (pool.tenPullDiscountEnabled && pullCount == 10)
                cost = Mathf.RoundToInt(cost * 0.8f);
            
            return ResourcesManager.Instance.HasEnoughItem(pool.currencyId, cost);
        }
        
        /// <summary>
        /// 获取保底信息
        /// </summary>
        public PitySystem.PityState GetPityInfo()
        {
            var pool = GetCurrentPool();
            if (pool == null)
                return new PitySystem.PityState();
            
            return PitySystem.Instance.GetPityState(pool.poolId, pool.pityInherited);
        }
        
        /// <summary>
        /// 获取保底剩余抽数
        /// </summary>
        public (int mini, int grand) GetPityRemaining()
        {
            var pool = GetCurrentPool();
            if (pool == null)
                return (0, 0);
            
            int mini = PitySystem.Instance.GetMiniPityRemaining(pool.poolId, pool.pityInherited);
            int grand = PitySystem.Instance.GetGrandPityRemaining(pool.poolId, pool.pityInherited);
            
            return (mini, grand);
        }
        
        /// <summary>
        /// 获取抽卡历史
        /// </summary>
        public GachaHistory GetHistory()
        {
            return _history;
        }
        
        /// <summary>
        /// 重置保底
        /// </summary>
        public void ResetPity()
        {
            var pool = GetCurrentPool();
            if (pool != null)
            {
                PitySystem.Instance.ResetPityState(pool.poolId);
            }
        }
        
        #endregion
        
        #region 核心抽卡逻辑
        
        /// <summary>
        /// 执行抽卡
        /// </summary>
        private GachaResult Pull(GachaPool pool, GachaType type, int count)
        {
            var result = new GachaResult
            {
                poolId = pool.poolId,
                gachaType = type,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            
            bool hasMiniPity = false;
            bool hasGrandPity = false;
            
            for (int i = 0; i < count; i++)
            {
                var entry = PullOnce(pool, ref hasMiniPity, ref hasGrandPity);
                result.entries.Add(entry);
                
                // 更新历史
                _history.AddEntry(result);
            }
            
            // 确保十连至少有一个SR或以上
            if (type == GachaType.TenPull && !result.hasSR && !result.hasSSR)
            {
                // 用小保底替换最后一张
                var lastEntry = result.entries[count - 1];
                var newEntry = RerollWithPity(pool, CharacterRarity.SR);
                result.entries[count - 1] = newEntry;
            }
            
            return result;
        }
        
        /// <summary>
        /// 单次抽卡
        /// </summary>
        private GachaResultEntry PullOnce(GachaPool pool, ref bool hasMiniPity, ref bool hasGrandPity)
        {
            // 检查保底
            bool triggerMiniPity = PitySystem.Instance.CheckMiniPity(pool.poolId, pool.pityInherited);
            bool triggerGrandPity = PitySystem.Instance.CheckGrandPity(pool.poolId, pool.pityInherited);
            bool guaranteedUp = PitySystem.Instance.CheckGuaranteedUp(pool.poolId);
            
            // 确定稀有度
            CharacterRarity rarity = DetermineRarity(pool, triggerMiniPity, triggerGrandPity);
            
            // 更新保底计数
            bool isUpCharacter = pool.IsUpCharacter(GetRandomCharacterId(pool, rarity));
            PitySystem.Instance.UpdatePityAfterPull(pool.poolId, rarity, isUpCharacter, pool.pityInherited);
            
            if (triggerGrandPity)
            {
                hasGrandPity = true;
                OnPityTriggered?.Invoke(PityType.Grand);
            }
            else if (triggerMiniPity)
            {
                hasMiniPity = true;
                OnPityTriggered?.Invoke(PityType.Mini);
            }
            
            if (guaranteanteedUp && rarity == CharacterRarity.SSR)
            {
                OnPityTriggered?.Invoke(PityType.Up);
            }
            
            // 获取角色
            int characterId = SelectCharacter(pool, rarity, guaranteedUp);
            var characterData = GetCharacterData(characterId);
            
            return new GachaResultEntry
            {
                characterId = characterId,
                characterData = characterData,
                rarity = rarity,
                isNew = !IsCharacterOwned(characterId),
                isUpCharacter = pool.IsUpCharacter(characterId),
                triggeredPity = hasGrandPity || hasMiniPity,
                pityType = hasGrandPity ? PityType.Grand : (hasMiniPity ? PityType.Mini : PityType.None)
            };
        }
        
        /// <summary>
        /// 确定抽卡稀有度
        /// </summary>
        private CharacterRarity DetermineRarity(GachaPool pool, bool triggerMiniPity, bool triggerGrandPity)
        {
            // 大保底必出SSR
            if (triggerGrandPity)
            {
                return CharacterRarity.SSR;
            }
            
            // 小保底必出SR以上
            if (triggerMiniPity)
            {
                // 在SR和SSR之间随机
                float ssrRate = pool.ssrBaseRate / (pool.ssrBaseRate + pool.srBaseRate);
                return UnityEngine.Random.value < ssrRate ? CharacterRarity.SSR : CharacterRarity.SR;
            }
            
            // 正常概率
            float roll = UnityEngine.Random.value * 100f;
            float ssrActualRate = PitySystem.Instance.CalculateSSRRate(pool.poolId, pool.ssrBaseRate, pool.pityInherited);
            
            if (roll < ssrActualRate)
                return CharacterRarity.SSR;
            
            roll = UnityEngine.Random.value * 100f;
            if (roll < pool.srBaseRate)
                return CharacterRarity.SR;
            
            if (roll < pool.srBaseRate + pool.rBaseRate)
                return CharacterRarity.R;
            
            return CharacterRarity.N;
        }
        
        /// <summary>
        /// 选择角色
        /// </summary>
        private int SelectCharacter(GachaPool pool, CharacterRarity rarity, bool guaranteedUp)
        {
            var characterPool = pool.GetCharacterPool(rarity);
            if (characterPool == null || characterPool.Count == 0)
            {
                Debug.LogWarning($"卡池 {pool.poolId} 中没有 {rarity} 角色");
                return 0;
            }
            
            // 限定池+必出UP+是SSR
            if (guaranteantedUp && pool.hasUpCharacter && rarity == CharacterRarity.SSR)
            {
                return pool.upCharacter.characterId;
            }
            
            // 计算总权重
            int totalWeight = 0;
            foreach (var entry in characterPool)
            {
                // UP角色权重翻倍
                totalWeight += entry.isUp ? entry.weight * 2 : entry.weight;
            }
            
            // 随机选择
            int randomValue = _random.Next(totalWeight);
            int currentWeight = 0;
            
            foreach (var entry in characterPool)
            {
                currentWeight += entry.isUp ? entry.weight * 2 : entry.weight;
                if (randomValue < currentWeight)
                {
                    return entry.characterId;
                }
            }
            
            return characterPool[0].characterId;
        }
        
        /// <summary>
        /// 保底重抽（保证SR以上）
        /// </summary>
        private GachaResultEntry RerollWithPity(GachaPool pool, CharacterRarity minRarity)
        {
            CharacterRarity rarity = minRarity;
            if (minRarity == CharacterRarity.SR)
            {
                float ssrChance = pool.ssrBaseRate / (pool.ssrBaseRate + pool.srBaseRate);
                if (UnityEngine.Random.value < ssrChance)
                    rarity = CharacterRarity.SSR;
            }
            
            int characterId = SelectCharacter(pool, rarity, false);
            var characterData = GetCharacterData(characterId);
            
            return new GachaResultEntry
            {
                characterId = characterId,
                characterData = characterData,
                rarity = rarity,
                isNew = !IsCharacterOwned(characterId),
                isUpCharacter = pool.IsUpCharacter(characterId),
                triggeredPity = true,
                pityType = PityType.Mini
            };
        }
        
        /// <summary>
        /// 获取随机角色ID（用于判断UP）
        /// </summary>
        private int GetRandomCharacterId(GachaPool pool, CharacterRarity rarity)
        {
            var characterPool = pool.GetCharacterPool(rarity);
            if (characterPool == null || characterPool.Count == 0)
                return 0;
            
            return characterPool[_random.Next(characterPool.Count)].characterId;
        }
        
        /// <summary>
        /// 获取角色数据
        /// </summary>
        private CharacterData GetCharacterData(int characterId)
        {
            return Resources.Load<CharacterData>($"Data/Characters/Character_{characterId}");
        }
        
        /// <summary>
        /// 检查角色是否已拥有
        /// </summary>
        private bool IsCharacterOwned(int characterId)
        {
            // 实际实现应从角色管理器检查
            return false;
        }
        
        #endregion
        
        #region 数据保存/加载
        
        /// <summary>
        /// 保存抽卡数据
        /// </summary>
        public GachaSaveData SaveToData()
        {
            return new GachaSaveData
            {
                history = _history,
                pityData = PitySystem.Instance.SaveToData()
            };
        }
        
        /// <summary>
        /// 加载抽卡数据
        /// </summary>
        public void LoadFromData(GachaSaveData data)
        {
            if (data == null)
                return;
            
            _history = data.history ?? new GachaHistory();
            PitySystem.Instance.LoadFromData(data.pityData);
        }
        
        #endregion
    }
    
    /// <summary>
    /// 抽卡保存数据
    /// </summary>
    [Serializable]
    public class GachaSaveData
    {
        public GachaHistory history;
        public PitySaveData pityData;
    }
}
