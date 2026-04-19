// GachaPool.cs - 卡池数据定义
// 定义各类抽卡卡池的配置

using System;
using System.Collections.Generic;
using UnityEngine;
using JRPG.Character;

namespace JRPG.Gacha
{
    /// <summary>
    /// 卡池类型枚举
    /// </summary>
    public enum GachaPoolType
    {
        /// <summary>常驻祈愿 - 命运交汇</summary>
        Standard,
        
        /// <summary>限定祈愿 - 星之邂逅</summary>
        Limited,
        
        /// <summary>种族祈愿 - 血脉觉醒</summary>
        Race,
        
        /// <summary>武器祈愿 - 神器铸造</summary>
        Weapon,
        
        /// <summary>新手祈愿 - 觉醒之始</summary>
        Beginner
    }
    
    /// <summary>
    /// 卡池配置数据
    /// </summary>
    [Serializable]
    public class GachaPoolConfig
    {
        [Header("基础信息")]
        [Tooltip("卡池ID")]
        public string poolId;
        
        [Tooltip("卡池名称")]
        public string poolName;
        
        [Tooltip("卡池类型")]
        public GachaPoolType poolType;
        
        [Tooltip("卡池描述")]
        public string description;
        
        [Tooltip("卡池图标")]
        public Sprite icon;
        
        [Header("概率配置")]
        [Tooltip("SSR概率")]
        [Range(0f, 100f)]
        public float ssrRate = 2f;
        
        [Tooltip("SR概率")]
        [Range(0f, 100f)]
        public float srRate = 10f;
        
        [Tooltip("R概率")]
        [Range(0f, 100f)]
        public float rRate = 35f;
        
        [Tooltip("N概率")]
        [Range(0f, 100f)]
        public float nRate = 53f;
        
        [Header("保底配置")]
        [Tooltip("小保底（必出SR以上）抽数")]
        public int miniPityCount = 10;
        
        [Tooltip("大保底（必出SSR）抽数")]
        public int grandPityCount = 90;
        
        [Tooltip("保底是否跨卡池继承")]
        public bool pityInherited = false;
        
        [Header("消耗配置")]
        [Tooltip("单抽消耗货币ID")]
        public string singleCostItemId = "star_shine";
        
        [Tooltip("单抽消耗数量")]
        public int singleCostAmount = 160;
        
        [Tooltip("十连消耗货币ID")]
        public string tenPullCostItemId = "star_shine";
        
        [Tooltip("十连消耗数量")]
        public int tenPullCostAmount = 1600;
        
        [Tooltip("十连是否有折扣")]
        public bool tenPullDiscount = false;
        
        [Header("UP角色")]
        [Tooltip("UP角色ID列表（限定池用）")]
        public List<int> upCharacterIds = new List<int>();
        
        [Tooltip("UP角色概率提升")]
        [Range(0f, 100f)]
        public float upCharacterRate = 50f;
        
        [Header("种族配置")]
        [Tooltip("限定种族（种族池用）")]
        public string limitedRace;
        
        [Header("限制配置")]
        [Tooltip("是否仅限新账号一次")]
        public bool oneTimeOnly = false;
        
        [Tooltip("是否显示概率")]
        public bool showRates = true;
    }
    
    /// <summary>
    /// 卡池数据管理
    /// </summary>
    [CreateAssetMenu(fileName = "GachaPool", menuName = "JRPG/Gacha Pool")]
    public class GachaPool : ScriptableObject
    {
        #region 配置
        
        [Header("基础信息")]
        public string poolId;
        public string poolName;
        public GachaPoolType poolType;
        [TextArea(2, 4)]
        public string description;
        public Sprite bannerImage;
        
        [Header("概率配置")]
        [Range(0f, 100f)]
        public float ssrBaseRate = 2f;      // SSR基础概率 2%
        [Range(0f, 100f)]
        public float srBaseRate = 10f;      // SR基础概率 10%
        [Range(0f, 100f)]
        public float rBaseRate = 35f;       // R基础概率 35%
        [Range(0f, 100f)]
        public float nBaseRate = 53f;        // N基础概率 53%
        
        [Header("保底配置")]
        public int miniPityPulls = 10;       // 小保底10抽
        public int grandPityPulls = 90;      // 大保底90抽
        
        [Header("UP配置")]
        public bool hasUpCharacter = false;
        public CharacterData upCharacter;
        public float upCharacterRate = 50f; // UP角色占SSR的50%
        
        [Header("种族限定")]
        public bool isRaceLimited = false;
        public string limitedRaceId;
        
        [Header("消耗配置")]
        public string currencyId = "star_shine";
        public int singlePullCost = 160;
        public int tenPullCost = 1600;
        public bool tenPullDiscountEnabled = false;
        
        [Header("角色池")]
        public List<GachaCharacterEntry> ssrCharacters = new List<GachaCharacterEntry>();
        public List<GachaCharacterEntry> srCharacters = new List<GachaCharacterEntry>();
        public List<GachaCharacterEntry> rCharacters = new List<GachaCharacterEntry>();
        public List<GachaCharacterEntry> nCharacters = new List<GachaCharacterEntry>();
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 获取指定稀有度的角色池
        /// </summary>
        public List<GachaCharacterEntry> GetCharacterPool(CharacterRarity rarity)
        {
            return rarity switch
            {
                CharacterRarity.SSR => ssrCharacters,
                CharacterRarity.SR => srCharacters,
                CharacterRarity.R => rCharacters,
                CharacterRarity.N => nCharacters,
                _ => nCharacters
            };
        }
        
        /// <summary>
        /// 检查角色是否在卡池中
        /// </summary>
        public bool IsCharacterInPool(int characterId, CharacterRarity rarity)
        {
            var pool = GetCharacterPool(rarity);
            return pool.Exists(e => e.characterId == characterId);
        }
        
        /// <summary>
        /// 获取UP角色（如果有限定UP）
        /// </summary>
        public CharacterData GetUpCharacter()
        {
            return hasUpCharacter ? upCharacter : null;
        }
        
        /// <summary>
        /// 是否是UP角色
        /// </summary>
        public bool IsUpCharacter(int characterId)
        {
            return hasUpCharacter && upCharacter != null && upCharacter.characterId == characterId;
        }
        
        /// <summary>
        /// 获取实际抽数价格
        /// </summary>
        public int GetPullCost(int pullCount)
        {
            if (pullCount == 1)
                return singlePullCost;
            
            int cost = tenPullCost;
            if (tenPullDiscountEnabled)
                cost = Mathf.RoundToInt(cost * 0.8f); // 8折
            return cost;
        }
        
        #endregion
        
        #region 概率计算
        
        /// <summary>
        /// 获取实际概率（考虑保底）
        /// </summary>
        public float GetActualSSRRate(int pullCountWithoutSSR)
        {
            // 从第73抽开始，SSR概率逐抽提升
            if (pullCountWithoutSSR < 73)
                return ssrBaseRate;
            
            // 73-90抽，每抽+2%
            int extraPulls = pullCountWithoutSSR - 72;
            return ssrBaseRate + (extraPulls * 2f);
        }
        
        #endregion
    }
    
    /// <summary>
    /// 抽卡角色条目
    /// </summary>
    [Serializable]
    public class GachaCharacterEntry
    {
        [Tooltip("角色ID")]
        public int characterId;
        
        [Tooltip("权重（越高概率越大）")]
        public int weight = 1;
        
        [Tooltip("是否UP角色")]
        public bool isUp = false;
    }
    
    /// <summary>
    /// 角色稀有度（抽卡用）
    /// </summary>
    public enum CharacterRarity
    {
        /// <summary>普通</summary>
        N = 0,
        
        /// <summary>稀有</summary>
        R = 1,
        
        /// <summary>超稀有</summary>
        SR = 2,
        
        /// <summary>传说</summary>
        SSR = 3
    }
    
    /// <summary>
    /// 抽卡角色稀有度配置
    /// </summary>
    public static class GachaRarityConfig
    {
        /// <summary>
        /// 获取稀有度显示名称
        /// </summary>
        public static string GetRarityName(CharacterRarity rarity)
        {
            return rarity switch
            {
                CharacterRarity.N => "N",
                CharacterRarity.R => "R",
                CharacterRarity.SR => "SR",
                CharacterRarity.SSR => "SSR",
                _ => "N"
            };
        }
        
        /// <summary>
        /// 获取稀有度颜色
        /// </summary>
        public static string GetRarityColor(CharacterRarity rarity)
        {
            return rarity switch
            {
                CharacterRarity.N => "#FFFFFF",
                CharacterRarity.R => "#4CAF50",
                CharacterRarity.SR => "#2196F3",
                CharacterRarity.SSR => "#FFD700",
                _ => "#FFFFFF"
            };
        }
        
        /// <summary>
        /// 稀有度转换为角色稀有度
        /// </summary>
        public static CharacterRarity FromCharacterRarity(Character.Rarity rarity)
        {
            return rarity switch
            {
                Character.Rarity.N => CharacterRarity.N,
                Character.Rarity.R => CharacterRarity.R,
                Character.Rarity.SR => CharacterRarity.SR,
                Character.Rarity.SSR => CharacterRarity.SSR,
                _ => CharacterRarity.N
            };
        }
    }
}
