// GachaResult.cs - 抽卡结果
// 定义抽卡结果的结构和数据

using System;
using System.Collections.Generic;
using UnityEngine;
using JRPG.Character;

namespace JRPG.Gacha
{
    /// <summary>
    /// 抽卡结果条目
    /// </summary>
    [Serializable]
    public class GachaResultEntry
    {
        [Tooltip("角色实例ID")]
        public string characterInstanceId;
        
        [Tooltip("角色ID")]
        public int characterId;
        
        [Tooltip("角色数据")]
        public CharacterData characterData;
        
        [Tooltip("稀有度")]
        public CharacterRarity rarity;
        
        [Tooltip("是否是新角色（未拥有）")]
        public bool isNew = true;
        
        [Tooltip("是否UP角色")]
        public bool isUpCharacter = false;
        
        [Tooltip("是否触发保底")]
        public bool triggeredPity = false;
        
        [Tooltip("保底类型")]
        public PityType pityType = PityType.None;
        
        /// <summary>
        /// 获取结果描述
        /// </summary>
        public string GetDescription()
        {
            string desc = isNew ? "新获得" : "已有";
            if (triggeredPity)
                desc += $" [{pityType}]";
            if (isUpCharacter)
                desc += " [UP]";
            return desc;
        }
    }
    
    /// <summary>
    /// 抽卡结果
    /// </summary>
    [Serializable]
    public class GachaResult
    {
        [Tooltip("抽卡池ID")]
        public string poolId;
        
        [Tooltip("抽卡类型")]
        public GachaType gachaType;
        
        [Tooltip("抽卡结果条目列表")]
        public List<GachaResultEntry> entries = new List<GachaResultEntry>();
        
        [Tooltip("是否包含SSR")]
        public bool hasSSR => entries.Exists(e => e.rarity == CharacterRarity.SSR);
        
        [Tooltip("是否包含SR")]
        public bool hasSR => entries.Exists(e => e.rarity == CharacterRarity.SR);
        
        [Tooltip("SSR数量")]
        public int ssrCount => entries.FindAll(e => e.rarity == CharacterRarity.SSR).Count;
        
        [Tooltip("SR数量")]
        public int srCount => entries.FindAll(e => e.rarity == CharacterRarity.SR).Count;
        
        [Tooltip("新角色数量")]
        public int newCharacterCount => entries.FindAll(e => e.isNew).Count;
        
        [Tooltip("抽卡时间")]
        public long timestamp;
        
        /// <summary>
        /// 获取抽卡结果摘要
        /// </summary>
        public string GetSummary()
        {
            string summary = gachaType == GachaType.Single ? "单抽结果" : "十连结果";
            summary += $"\nSSR: {ssrCount} | SR: {srCount}";
            
            if (hasSSR)
            {
                var ssrEntry = entries.Find(e => e.rarity == CharacterRarity.SSR);
                summary += $"\n获得: {ssrEntry?.characterData?.characterName ?? "未知"}";
                if (ssrEntry?.isUpCharacter == true)
                    summary += " [限定UP]";
            }
            
            return summary;
        }
    }
    
    /// <summary>
    /// 抽卡类型
    /// </summary>
    public enum GachaType
    {
        /// <summary>单抽</summary>
        Single,
        
        /// <summary>十连</summary>
        TenPull
    }
    
    /// <summary>
    /// 保底类型
    /// </summary>
    public enum PityType
    {
        /// <summary>无保底</summary>
        None,
        
        /// <summary>小保底（SR以上）</summary>
        Mini,
        
        /// <summary>大保底（SSR）</summary>
        Grand,
        
        /// <summary>UP保底（必出UP角色）</summary>
        Up
    }
    
    /// <summary>
    /// 抽卡动画配置
    /// </summary>
    [Serializable]
    public class GachaAnimationConfig
    {
        [Header("动画时长")]
        public float normalCardDuration = 0.5f;
        public float rareCardDuration = 1.0f;
        public float ssrRevealDuration = 2.0f;
        public float ssrFullScreenDuration = 3.0f;
        
        [Header("动画特效")]
        public GameObject normalEffectPrefab;
        public GameObject srEffectPrefab;
        public GameObject ssrEffectPrefab;
        public GameObject upSsrEffectPrefab;
        
        [Header("音效")]
        public AudioClip normalPullSound;
        public AudioClip srRevealSound;
        public AudioClip ssrRevealSound;
        public AudioClip upSsrRevealSound;
        
        [Header("视觉配置")]
        public Color nCardColor = Color.white;
        public Color rCardColor = new Color(0.3f, 0.7f, 0.3f);    // 绿色
        public Color srCardColor = new Color(0.1f, 0.6f, 0.9f);   // 蓝色
        public Color ssrCardColor = new Color(1f, 0.84f, 0f);     // 金色
        
        /// <summary>
        /// 获取稀有度对应的颜色
        /// </summary>
        public Color GetRarityColor(CharacterRarity rarity)
        {
            return rarity switch
            {
                CharacterRarity.N => nCardColor,
                CharacterRarity.R => rCardColor,
                CharacterRarity.SR => srCardColor,
                CharacterRarity.SSR => ssrCardColor,
                _ => nCardColor
            };
        }
    }
    
    /// <summary>
    /// 抽卡历史记录
    /// </summary>
    [Serializable]
    public class GachaHistory
    {
        [Tooltip("历史记录列表")]
        public List<GachaHistoryEntry> entries = new List<GachaHistoryEntry>();
        
        [Tooltip("总抽数")]
        public int totalPulls = 0;
        
        [Tooltip("SSR数量")]
        public int totalSSR = 0;
        
        [Tooltip("SR数量")]
        public int totalSR = 0;
        
        /// <summary>
        /// 添加记录
        /// </summary>
        public void AddEntry(GachaResult result)
        {
            foreach (var entry in result.entries)
            {
                entries.Add(new GachaHistoryEntry
                {
                    poolId = result.poolId,
                    characterId = entry.characterId,
                    rarity = entry.rarity,
                    timestamp = result.timestamp,
                    isUpCharacter = entry.isUpCharacter,
                    pityType = entry.pityType
                });
                
                totalPulls++;
                if (entry.rarity == CharacterRarity.SSR)
                    totalSSR++;
                else if (entry.rarity == CharacterRarity.SR)
                    totalSR++;
            }
        }
        
        /// <summary>
        /// 获取保底计数器距离下次SSR的平均抽数
        /// </summary>
        public float GetAveragePullsPerSSR()
        {
            if (totalSSR == 0)
                return 0;
            return (float)totalPulls / totalSSR;
        }
    }
    
    /// <summary>
    /// 抽卡历史条目
    /// </summary>
    [Serializable]
    public class GachaHistoryEntry
    {
        public string poolId;
        public int characterId;
        public CharacterRarity rarity;
        public long timestamp;
        public bool isUpCharacter;
        public PityType pityType;
    }
}
