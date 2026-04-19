// TutorialStep.cs - 新手引导步骤定义
// 定义新手引导的每个步骤及其相关配置

using UnityEngine;
using System;

namespace JRPG.Tutorial
{
    /// <summary>
    /// 引导类型枚举
    /// </summary>
    public enum TutorialType
    {
        None,               // 无
        Click,              // 点击指定对象
        Drag,               // 拖拽操作
        Wait,               // 等待（自动继续）
        Selection,          // 选择操作
        Battle,             // 战斗引导
        Gacha               // 抽卡引导
    }

    /// <summary>
    /// 目标类型
    /// </summary>
    public enum TutorialTargetType
    {
        UIElement,          // UI元素
        WorldObject,        // 世界对象
        Character,          // 角色
        Any                 // 任意点击
    }

    /// <summary>
    /// 引导高亮样式
    /// </summary>
    public enum HighlightStyle
    {
        Glow,               // 发光效果
        Outline,            // 边框轮廓
        Arrow,              // 箭头指示
        Finger              // 手指指示
    }

    /// <summary>
    /// 新手引导步骤数据
    /// 定义单个引导步骤的所有属性
    /// </summary>
    [Serializable]
    public class TutorialStep
    {
        [Header("步骤标识")]
        /// <summary>
        /// 步骤ID（唯一）
        /// </summary>
        public int stepId;

        /// <summary>
        /// 步骤名称
        /// </summary>
        public string stepName;

        [Header("引导配置")]
        /// <summary>
        /// 引导类型
        /// </summary>
        public TutorialType tutorialType = TutorialType.Click;

        /// <summary>
        /// 目标类型
        /// </summary>
        public TutorialTargetType targetType = TutorialTargetType.UIElement;

        /// <summary>
        /// 目标路径或名称（用于查找目标对象）
        /// </summary>
        public string targetPath;

        /// <summary>
        /// 是否高亮目标
        /// </summary>
        public bool highlightTarget = true;

        /// <summary>
        /// 高亮样式
        /// </summary>
        public HighlightStyle highlightStyle = HighlightStyle.Glow;

        [Header("对话配置")]
        /// <summary>
        /// 引导说明文本
        /// </summary>
        [TextArea(2, 4)]
        public string description;

        /// <summary>
        /// 对话头像（可选）
        /// </summary>
        public Sprite dialoguePortrait;

        /// <summary>
        /// 对话者名称
        /// </summary>
        public string speakerName;

        [Header("时间配置")]
        /// <summary>
        /// 显示持续时间（对于Wait类型）
        /// </summary>
        public float duration = 0f;

        /// <summary>
        /// 步骤超时时间（0表示不超时）
        /// </summary>
        public float timeout = 0f;

        [Header("下一步配置")]
        /// <summary>
        /// 完成后自动进入下一步
        /// </summary>
        public bool autoAdvance = true;

        /// <summary>
        /// 下一步骤ID（-1表示引导结束）
        /// </summary>
        public int nextStepId = -1;

        [Header("条件配置")]
        /// <summary>
        /// 需要完成的先决条件步骤ID列表
        /// </summary>
        public int[] prerequisiteSteps = new int[0];

        /// <summary>
        /// 是否跳过此步骤（用于调试）
        /// </summary>
        public bool skipInDebug = false;
    }

    /// <summary>
    /// 引导阶段
    /// </summary>
    public enum TutorialPhase
    {
        MainStory,          // 主线引导
        Combat,             // 战斗引导
        Gacha,              // 抽卡引导
        Upgrade,            // 养成引导
        Endgame             // 后期引导
    }

    /// <summary>
    /// 新手引导数据资产
    /// 包含完整的新手引导流程配置
    /// </summary>
    [CreateAssetMenu(fileName = "TutorialData", menuName = "JRPG/Tutorial/Tutorial Data")]
    public class TutorialData : ScriptableObject
    {
        [Header("基本信息")]
        /// <summary>
        /// 引导数据ID
        /// </summary>
        public int tutorialId;

        /// <summary>
        /// 引导名称
        /// </summary>
        public string tutorialName;

        /// <summary>
        /// 引导所属阶段
        /// </summary>
        public TutorialPhase phase;

        [Header("步骤配置")]
        /// <summary>
        /// 所有引导步骤列表
        /// </summary>
        public TutorialStep[] steps;

        [Header("全局配置")]
        /// <summary>
        /// 是否启用新手引导
        /// </summary>
        public bool enabled = true;

        /// <summary>
        /// 是否可以跳过
        /// </summary>
        public bool canSkip = false;

        /// <summary>
        /// 跳过是否需要确认
        /// </summary>
        public bool skipConfirmation = true;

        /// <summary>
        /// 完成引导后的奖励
        /// </summary>
        public TutorialReward completionReward;

        /// <summary>
        /// 获取指定ID的步骤
        /// </summary>
        public TutorialStep GetStep(int stepId)
        {
            if (steps == null) return null;
            
            foreach (var step in steps)
            {
                if (step.stepId == stepId)
                    return step;
            }
            return null;
        }

        /// <summary>
        /// 获取首个步骤
        /// </summary>
        public TutorialStep GetFirstStep()
        {
            if (steps == null || steps.Length == 0) return null;
            return steps[0];
        }
    }

    /// <summary>
    /// 引导奖励配置
    /// </summary>
    [Serializable]
    public class TutorialReward
    {
        /// <summary>
        /// 金币奖励
        /// </summary>
        public int goldReward = 0;

        /// <summary>
        /// 钻石奖励
        /// </summary>
        public int diamondReward = 0;

        /// <summary>
        /// 经验奖励
        /// </summary>
        public int expReward = 0;

        /// <summary>
        /// 物品奖励ID列表
        /// </summary>
        public int[] itemRewardIds = new int[0];

        /// <summary>
        /// 物品奖励数量
        /// </summary>
        public int[] itemRewardCounts = new int[0];
    }
}
