// AudioConfig.cs - 音频配置
// 定义所有BGM、音效、语音的路径，支持分类管理和音量配置

using UnityEngine;
using System.Collections.Generic;

namespace JRPG.Assets.Audio
{
    /// <summary>
    /// 音频配置 - ScriptableObject管理所有游戏音频资源
    /// 支持BGM、音效、语音分类管理，提供默认音量配置
    /// </summary>
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "JRPG/Audio/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        #region Singleton
        
        private static AudioConfig _instance;
        public static AudioConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<AudioConfig>("Audio/AudioConfig");
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region 默认音量设置
        
        [Header("===== 默认音量设置 =====")]
        
        [Range(0f, 1f)]
        [Tooltip("BGM默认音量")]
        public float defaultBGMVolume = 0.7f;
        
        [Range(0f, 1f)]
        [Tooltip("音效默认音量")]
        public float defaultSFXVolume = 0.8f;
        
        [Range(0f, 1f)]
        [Tooltip("语音默认音量")]
        public float defaultVoiceVolume = 1.0f;
        
        [Range(0f, 1f)]
        [Tooltip("环境音默认音量")]
        public float defaultAmbientVolume = 0.5f;
        
        #endregion
        
        #region BGM配置
        
        [Header("===== BGM配置 =====")]
        
        [Tooltip("主菜单BGM")]
        public string mainMenuBGM = "Audio/BGM/MainMenu/MainMenu";
        
        [Tooltip("战斗BGM-普通战斗")]
        public string battleNormalBGM = "Audio/BGM/Battle/BattleNormal";
        
        [Tooltip("战斗BGM-精英战斗")]
        public string battleEliteBGM = "Audio/BGM/Battle/BattleElite";
        
        [Tooltip("战斗BGM-BOSS战斗")]
        public string battleBossBGM = "Audio/BGM/Battle/BattleBoss";
        
        [Tooltip("战斗胜利BGM")]
        public string victoryBGM = "Audio/BGM/Battle/Victory";
        
        [Tooltip("战斗失败BGM")]
        public string defeatBGM = "Audio/BGM/Battle/Defeat";
        
        [Tooltip("城镇BGM")]
        public string townBGM = "Audio/BGM/Town/Town";
        
        [Tooltip("野外BGM")]
        public string fieldBGM = "Audio/BGM/Field/Field";
        
        [Tooltip("迷宫BGM")]
        public string dungeonBGM = "Audio/BGM/Dungeon/Dungeon";
        
        [Tooltip("商店BGM")]
        public string shopBGM = "Audio/BGM/Shop/Shop";
        
        [Tooltip("抽卡BGM")]
        public string gachaBGM = "Audio/BGM/Gacha/Gacha";
        
        [Tooltip("抽卡结果BGM")]
        public string gachaResultBGM = "Audio/BGM/Gacha/GachaResult";
        
        // BGM映射表
        [Tooltip("BGM场景映射")]
        public List<BGMEntry> bgmEntries = new List<BGMEntry>();
        
        [System.Serializable]
        public class BGMEntry
        {
            [Tooltip("场景名称")]
            public string sceneName;
            
            [Tooltip("BGM路径")]
            public string bgmPath;
            
            [Tooltip("是否循环播放")]
            public bool loop = true;
        }
        
        #endregion
        
        #region 战斗音效
        
        [Header("===== 战斗音效 =====")]
        
        // 攻击音效
        [Tooltip("普通攻击音效")]
        public string attackSound = "Audio/SFX/Battle/Attack";
        
        [Tooltip("暴击攻击音效")]
        public string criticalAttackSound = "Audio/SFX/Battle/CriticalAttack";
        
        [Tooltip("技能释放音效")]
        public string skillCastSound = "Audio/SFX/Battle/SkillCast";
        
        [Tooltip("终极技能释放音效")]
        public string ultimateSkillSound = "Audio/SFX/Battle/UltimateSkill";
        
        // 命中相关
        [Tooltip("命中音效")]
        public string hitSound = "Audio/SFX/Battle/Hit";
        
        [Tooltip("闪避音效")]
        public string dodgeSound = "Audio/SFX/Battle/Dodge";
        
        [Tooltip("格挡音效")]
        public string blockSound = "Audio/SFX/Battle/Block";
        
        [Tooltip("反击音效")]
        public string counterSound = "Audio/SFX/Battle/Counter";
        
        // 生命值相关
        [Tooltip("治疗音效")]
        public string healSound = "Audio/SFX/Battle/Heal";
        
        [Tooltip("护盾生效音效")]
        public string shieldSound = "Audio/SFX/Battle/Shield";
        
        [Tooltip("复活音效")]
        public string reviveSound = "Audio/SFX/Battle/Revive";
        
        // 状态效果
        [Tooltip("增益音效")]
        public string buffSound = "Audio/SFX/Battle/Buff";
        
        [Tooltip("减益音效")]
        public string debuffSound = "Audio/SFX/Battle/Debuff";
        
        [Tooltip("眩晕音效")]
        public string stunSound = "Audio/SFX/Battle/Stun";
        
        [Tooltip("冻结音效")]
        public string freezeSound = "Audio/SFX/Battle/Freeze";
        
        // 战斗流程
        [Tooltip("使用道具音效")]
        public string useItemSound = "Audio/SFX/Battle/UseItem";
        
        [Tooltip("逃跑成功音效")]
        public string fleeSuccessSound = "Audio/SFX/Battle/FleeSuccess";
        
        [Tooltip("逃跑失败音效")]
        public string fleeFailSound = "Audio/SFX/Battle/FleeFail";
        
        [Tooltip("防御音效")]
        public string defendSound = "Audio/SFX/Battle/Defend";
        
        // 死亡和胜利
        [Tooltip("角色死亡音效")]
        public string deathSound = "Audio/SFX/Battle/Death";
        
        [Tooltip("敌人死亡音效")]
        public string enemyDeathSound = "Audio/SFX/Battle/EnemyDeath";
        
        [Tooltip("战斗胜利音效")]
        public string battleVictorySound = "Audio/SFX/Battle/Victory";
        
        [Tooltip("战斗失败音效")]
        public string battleDefeatSound = "Audio/SFX/Battle/Defeat";
        
        // 伤害数字
        [Tooltip("伤害数字弹出音效")]
        public string damageNumberSound = "Audio/SFX/Battle/DamageNumber";
        
        [Tooltip("暴击伤害数字音效")]
        public string criticalDamageSound = "Audio/SFX/Battle/CriticalDamage";
        
        #endregion
        
        #region UI音效
        
        [Header("===== UI音效 =====")]
        
        [Tooltip("按钮点击音效")]
        public string buttonClickSound = "Audio/SFX/UI/ButtonClick";
        
        [Tooltip("按钮悬停音效")]
        public string buttonHoverSound = "Audio/SFX/UI/ButtonHover";
        
        [Tooltip("确认音效")]
        public string confirmSound = "Audio/SFX/UI/Confirm";
        
        [Tooltip("取消音效")]
        public string cancelSound = "Audio/SFX/UI/Cancel";
        
        [Tooltip("错误提示音效")]
        public string errorSound = "Audio/SFX/UI/Error";
        
        [Tooltip("成功提示音效")]
        public string successSound = "Audio/SFX/UI/Success";
        
        [Tooltip("选择音效")]
        public string selectSound = "Audio/SFX/UI/Select";
        
        [Tooltip("切换音效")]
        public string tabSwitchSound = "Audio/SFX/UI/TabSwitch";
        
        [Tooltip("打开面板音效")]
        public string panelOpenSound = "Audio/SFX/UI/PanelOpen";
        
        [Tooltip("关闭面板音效")]
        public string panelCloseSound = "Audio/SFX/UI/PanelClose";
        
        [Tooltip("弹出提示音效")]
        public string popupSound = "Audio/SFX/UI/Popup";
        
        [Tooltip("升级音效")]
        public string levelUpSound = "Audio/SFX/UI/LevelUp";
        
        [Tooltip("突破音效")]
        public string breakthroughSound = "Audio/SFX/UI/Breakthrough";
        
        [Tooltip("获得物品音效")]
        public string obtainItemSound = "Audio/SFX/UI/ObtainItem";
        
        [Tooltip("获得角色音效")]
        public string obtainCharacterSound = "Audio/SFX/UI/ObtainCharacter";
        
        [Tooltip("装备更换音效")]
        public string equipChangeSound = "Audio/SFX/UI/EquipChange";
        
        #endregion
        
        #region 抽卡音效
        
        [Header("===== 抽卡音效 =====")]
        
        [Tooltip("抽卡开始音效")]
        public string gachaStartSound = "Audio/SFX/Gacha/GachaStart";
        
        [Tooltip("单抽音效")]
        public string gachaSingleSound = "Audio/SFX/Gacha/GachaSingle";
        
        [Tooltip("十连音效")]
        public string gachaMultiSound = "Audio/SFX/Gacha/GachaMulti";
        
        [Tooltip("普通品质音效")]
        public string gachaCommonSound = "Audio/SFX/Gacha/GachaCommon";
        
        [Tooltip("稀有品质音效")]
        public string gachaRareSound = "Audio/SFX/Gacha/GachaRare";
        
        [Tooltip("史诗品质音效")]
        public string gachaEpicSound = "Audio/SFX/Gacha/GachaEpic";
        
        [Tooltip("传说品质音效")]
        public string gachaLegendarySound = "Audio/SFX/Gacha/GachaLegendary";
        
        [Tooltip("保底触发音效")]
        public string gachaPitySound = "Audio/SFX/Gacha/GachaPity";
        
        #endregion
        
        #region 环境音
        
        [Header("===== 环境音 =====")]
        
        [Tooltip("森林环境音")]
        public string forestAmbient = "Audio/Ambient/Forest";
        
        [Tooltip("洞穴环境音")]
        public string caveAmbient = "Audio/Ambient/Cave";
        
        [Tooltip("城镇环境音")]
        public string townAmbient = "Audio/Ambient/Town";
        
        [Tooltip("海边环境音")]
        public string beachAmbient = "Audio/Ambient/Beach";
        
        [Tooltip("火山环境音")]
        public string volcanoAmbient = "Audio/Ambient/Volcano";
        
        [Tooltip("雪地环境音")]
        public string snowAmbient = "Audio/Ambient/Snow";
        
        [Tooltip("雨天环境音")]
        public string rainAmbient = "Audio/Ambient/Rain";
        
        [Tooltip("夜晚环境音")]
        public string nightAmbient = "Audio/Ambient/Night";
        
        #endregion
        
        #region 语音配置
        
        [Header("===== 角色语音 =====")]
        
        [Tooltip("角色语音映射表")]
        public List<VoiceEntry> voiceEntries = new List<VoiceEntry>();
        
        [System.Serializable]
        public class VoiceEntry
        {
            [Tooltip("角色ID")]
            public int characterId;
            
            [Tooltip("角色名称")]
            public string characterName;
            
            [Tooltip("攻击语音")]
            public string attackVoice = "Audio/Voice/Characters/Attack";
            
            [Tooltip("技能语音")]
            public string skillVoice = "Audio/Voice/Characters/Skill";
            
            [Tooltip("终极技能语音")]
            public string ultimateVoice = "Audio/Voice/Characters/Ultimate";
            
            [Tooltip("受伤语音")]
            public string hurtVoice = "Audio/Voice/Characters/Hurt";
            
            [Tooltip("死亡语音")]
            public string deathVoice = "Audio/Voice/Characters/Death";
            
            [Tooltip("胜利语音")]
            public string victoryVoice = "Audio/Voice/Characters/Victory";
            
            [Tooltip("待机语音")]
            public string idleVoice = "Audio/Voice/Characters/Idle";
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 根据场景名称获取BGM路径
        /// </summary>
        public string GetBGMByScene(string sceneName)
        {
            var entry = bgmEntries.Find(x => x.sceneName == sceneName);
            if (entry != null && !string.IsNullOrEmpty(entry.bgmPath))
            {
                return entry.bgmPath;
            }
            return mainMenuBGM;
        }
        
        /// <summary>
        /// 根据角色ID获取攻击语音
        /// </summary>
        public string GetAttackVoice(int characterId)
        {
            var entry = voiceEntries.Find(x => x.characterId == characterId);
            if (entry != null && !string.IsNullOrEmpty(entry.attackVoice))
            {
                return entry.attackVoice;
            }
            return attackSound;
        }
        
        /// <summary>
        /// 根据角色ID获取技能语音
        /// </summary>
        public string GetSkillVoice(int characterId)
        {
            var entry = voiceEntries.Find(x => x.characterId == characterId);
            if (entry != null && !string.IsNullOrEmpty(entry.skillVoice))
            {
                return entry.skillVoice;
            }
            return skillCastSound;
        }
        
        /// <summary>
        /// 异步加载音频clip
        /// </summary>
        public void LoadAudioClipAsync(string path, System.Action<AudioClip> onLoaded)
        {
            Resources.LoadAsync<AudioClip>(path).completed += (operation) =>
            {
                var request = operation as ResourceRequest;
                if (request != null && request.asset != null)
                {
                    onLoaded?.Invoke(request.asset as AudioClip);
                }
                else
                {
                    onLoaded?.Invoke(null);
                }
            };
        }
        
        /// <summary>
        /// 预加载音频资源
        /// </summary>
        public void PreloadAudioClips(System.Action<float> onProgress = null)
        {
            List<string> pathsToLoad = new List<string>
            {
                buttonClickSound,
                confirmSound,
                cancelSound,
                attackSound,
                skillCastSound,
                healSound,
                buffSound,
                debuffSound
            };
            
            int total = pathsToLoad.Count;
            int loaded = 0;
            
            foreach (var path in pathsToLoad)
            {
                Resources.LoadAsync<AudioClip>(path).completed += (operation) =>
                {
                    loaded++;
                    onProgress?.Invoke((float)loaded / total);
                };
            }
        }
        
        #endregion
    }
}
