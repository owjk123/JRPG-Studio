using UnityEngine;
using System.Collections.Generic;

namespace JRPGStudio.Data
{
    /// <summary>
    /// 抽卡池数据
    /// </summary>
    [CreateAssetMenu(fileName = "GachaPool", menuName = "JRPG/Gacha/Gacha Pool")]
    public class GachaPoolData : ScriptableObject
    {
        [Header("卡池基础信息")]
        public int poolId;
        public string poolName;
        public string poolDescription;
        public PoolType poolType;
        public bool isActive = true;

        [Header("卡池时间")]
        public System.DateTime startTime;
        public System.DateTime endTime;

        [Header("概率设置")]
        [Range(0f, 100f)] public float ssrRate = 2f;        // SSR概率 2%
        [Range(0f, 100f)] public float srRate = 10f;        // SR概率 10%
        [Range(0f, 100f)] public float rRate = 35f;         // R概率 35%
        // N概率 = 100 - SSR - SR - R = 53%

        [Header("保底设置")]
        public int softPity = 10;       // 小保底：10抽必出R以上
        public int hardPity = 90;       // 大保底：90抽必出SSR
        public float pitySSRRate = 50f; // UP角色概率 50%

        [Header("UP角色")]
        public List<int> upSSRCharacters = new List<int>(); // UP SSR角色
        public List<int> upSRCharacters = new List<int>();  // UP SR角色

        [Header("卡池角色")]
        public List<int> availableSSR = new List<int>();    // 可抽到的SSR
        public List<int> availableSR = new List<int>();     // 可抽到的SR
        public List<int> availableR = new List<int>();      // 可抽到的R
        public List<int> availableN = new List<int>();      // 可抽到的N

        [Header("费用")]
        public int singlePullCost = 160;    // 单抽钻石消耗
        public int tenPullCost = 1440;      // 十连钻石消耗（9折）
        public CurrencyType costCurrency = CurrencyType.Diamond;
    }

    /// <summary>
    /// 卡池类型
    /// </summary>
    public enum PoolType
    {
        Standard = 0,       // 常驻池
        Limited = 1,        // 限定池
        Race = 2,           // 种族池
        Weapon = 3,         // 武器池
        Beginner = 4,       // 新手池
        RateUp = 5          // 概率UP池
    }

    /// <summary>
    /// 货币类型
    /// </summary>
    public enum CurrencyType
    {
        Gold = 0,           // 金币
        Diamond = 1,        // 钻石
        Ticket = 2,         // 抽卡券
        FreeDiamond = 3     // 免费钻石
    }
}
