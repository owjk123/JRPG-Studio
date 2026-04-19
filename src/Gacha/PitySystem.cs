// PitySystem.cs - 保底系统
// 实现抽卡的保底机制

using System;
using UnityEngine;

namespace JRPG.Gacha
{
    /// <summary>
    /// 保底系统单例类
    /// </summary>
    public class PitySystem
    {
        #region 单例
        
        private static PitySystem _instance;
        public static PitySystem Instance => _instance ??= new PitySystem();
        
        #endregion
        
        #region 保底状态数据
        
        /// <summary>
        /// 保底状态
        /// </summary>
        [Serializable]
        public class PityState
        {
            /// <summary>
            /// 距上次SSR后的抽数
            /// </summary>
            public int pullsSinceLastSSR = 0;
            
            /// <summary>
            /// 距上次SR以上后的抽数（小保底计数）
            /// </summary>
            public int pullsSinceLastSRPlus = 0;
            
            /// <summary>
            /// 歪的次数（限定池连续非UP的SSR次数）
            /// </summary>
            public int offBannerCount = 0;
            
            /// <summary>
            /// 是否已触发大保底
            /// </summary>
            public bool grandPityTriggered = false;
            
            /// <summary>
            /// 是否已触发小保底
            /// </summary>
            public bool miniPityTriggered = false;
        }
        
        #endregion
        
        #region 保底配置
        
        /// <summary>
        /// 小保底触发抽数
        /// </summary>
        private const int MiniPityThreshold = 10;
        
        /// <summary>
        /// 大保底触发抽数
        /// </summary>
        private const int GrandPityThreshold = 90;
        
        /// <summary>
        /// SSR概率开始递增的抽数
        /// </summary>
        private const int SSRRateIncreaseStart = 73;
        
        #endregion
        
        #region 私有变量
        
        /// <summary>
        /// 各卡池的保底状态（key: poolId）
        /// </summary>
        private System.Collections.Generic.Dictionary<string, PityState> _pityStates = 
            new System.Collections.Generic.Dictionary<string, PityState>();
        
        /// <summary>
        /// 全局保底状态（跨卡池继承）
        /// </summary>
        private PityState _globalPityState = new PityState();
        
        #endregion
        
        #region 构造函数
        
        private PitySystem()
        {
        }
        
        #endregion
        
        #region 公开方法
        
        /// <summary>
        /// 获取保底状态
        /// </summary>
        public PityState GetPityState(string poolId, bool inheritGlobal = false)
        {
            if (!_pityStates.TryGetValue(poolId, out var state))
            {
                state = new PityState();
                _pityStates[poolId] = state;
            }
            
            if (inheritGlobal)
            {
                // 合并全局保底
                state.pullsSinceLastSSR += _globalPityState.pullsSinceLastSSR;
                state.pullsSinceLastSRPlus += _globalPityState.pullsSinceLastSRPlus;
            }
            
            return state;
        }
        
        /// <summary>
        /// 更新保底状态（单抽后调用）
        /// </summary>
        public void UpdatePityAfterPull(string poolId, CharacterRarity resultRarity, 
            bool isUpCharacter = false, bool inheritGlobal = true)
        {
            var state = GetPityState(poolId, inheritGlobal);
            
            // 重置计数
            state.pullsSinceLastSSR++;
            state.pullsSinceLastSRPlus++;
            
            // 判断是否触发保底
            if (resultRarity >= CharacterRarity.SR)
            {
                // 触发小保底
                state.miniPityTriggered = state.pullsSinceLastSRPlus >= MiniPityThreshold;
                state.pullsSinceLastSRPlus = 0;
            }
            
            if (resultRarity >= CharacterRarity.SSR)
            {
                // 触发大保底
                state.grandPityTriggered = state.pullsSinceLastSSR >= GrandPityThreshold;
                state.pullsSinceLastSSR = 0;
                
                // 更新歪计数
                if (!isUpCharacter)
                {
                    state.offBannerCount++;
                }
                else
                {
                    state.offBannerCount = 0;
                }
            }
            
            // 保存全局状态
            _globalPityState.pullsSinceLastSSR = state.pullsSinceLastSSR;
            _globalPityState.pullsSinceLastSRPlus = state.pullsSinceLastSRPlus;
        }
        
        /// <summary>
        /// 重置保底状态
        /// </summary>
        public void ResetPityState(string poolId)
        {
            if (_pityStates.ContainsKey(poolId))
            {
                _pityStates[poolId] = new PityState();
            }
        }
        
        /// <summary>
        /// 重置全局保底状态
        /// </summary>
        public void ResetGlobalPityState()
        {
            _globalPityState = new PityState();
        }
        
        /// <summary>
        /// 检查是否触发小保底
        /// </summary>
        public bool CheckMiniPity(string poolId, bool inheritGlobal = true)
        {
            var state = GetPityState(poolId, inheritGlobal);
            return state.pullsSinceLastSRPlus >= MiniPityThreshold || state.miniPityTriggered;
        }
        
        /// <summary>
        /// 检查是否触发大保底
        /// </summary>
        public bool CheckGrandPity(string poolId, bool inheritGlobal = true)
        {
            var state = GetPityState(poolId, inheritGlobal);
            return state.pullsSinceLastSSR >= GrandPityThreshold || state.grandPityTriggered;
        }
        
        /// <summary>
        /// 检查是否必出UP角色
        /// </summary>
        public bool CheckGuaranteedUp(string poolId)
        {
            var state = GetPityState(poolId);
            return state.offBannerCount >= 2;
        }
        
        /// <summary>
        /// 获取当前抽数（距上次SSR）
        /// </summary>
        public int GetPullsSinceLastSSR(string poolId, bool inheritGlobal = true)
        {
            var state = GetPityState(poolId, inheritGlobal);
            return state.pullsSinceLastSSR;
        }
        
        /// <summary>
        /// 获取当前抽数（距上次SR以上）
        /// </summary>
        public int GetPullsSinceLastSRPlus(string poolId, bool inheritGlobal = true)
        {
            var state = GetPityState(poolId, inheritGlobal);
            return state.pullsSinceLastSRPlus;
        }
        
        /// <summary>
        /// 获取小保底剩余抽数
        /// </summary>
        public int GetMiniPityRemaining(string poolId, bool inheritGlobal = true)
        {
            var state = GetPityState(poolId, inheritGlobal);
            return Mathf.Max(0, MiniPityThreshold - state.pullsSinceLastSRPlus);
        }
        
        /// <summary>
        /// 获取大保底剩余抽数
        /// </summary>
        public int GetGrandPityRemaining(string poolId, bool inheritGlobal = true)
        {
            var state = GetPityState(poolId, inheritGlobal);
            return Mathf.Max(0, GrandPityThreshold - state.pullsSinceLastSSR);
        }
        
        /// <summary>
        /// 计算SSR实际概率
        /// </summary>
        public float CalculateSSRRate(string poolId, float baseRate, bool inheritGlobal = true)
        {
            var state = GetPityState(poolId, inheritGlobal);
            
            // 73抽前保持基础概率
            if (state.pullsSinceLastSSR < SSRRateIncreaseStart)
                return baseRate;
            
            // 73-90抽，每抽+2%
            int extraPulls = state.pullsSinceLastSSR - SSRRateIncreaseStart + 1;
            return Mathf.Min(100f, baseRate + (extraPulls * 2f));
        }
        
        #endregion
        
        #region 数据保存/加载
        
        /// <summary>
        /// 保存保底数据
        /// </summary>
        public PitySaveData SaveToData()
        {
            return new PitySaveData
            {
                pityStates = new System.Collections.Generic.Dictionary<string, PityState>(_pityStates),
                globalPityState = _globalPityState
            };
        }
        
        /// <summary>
        /// 加载保底数据
        /// </summary>
        public void LoadFromData(PitySaveData data)
        {
            if (data == null)
                return;
            
            _pityStates = data.pityStates ?? new System.Collections.Generic.Dictionary<string, PityState>();
            _globalPityState = data.globalPityState ?? new PityState();
        }
        
        #endregion
    }
    
    /// <summary>
    /// 保底保存数据
    /// </summary>
    [Serializable]
    public class PitySaveData
    {
        public System.Collections.Generic.Dictionary<string, PitySystem.PityState> pityStates;
        public PitySystem.PityState globalPityState;
    }
}
