// CharacterSpriteConfig.cs - 角色立绘配置
// 定义所有角色立绘的Sprite资源路径，支持种族、职业分类管理

using UnityEngine;
using System.Collections.Generic;
using JRPG.Battle;

namespace JRPG.Assets.Characters
{
    /// <summary>
    /// 角色立绘配置 - ScriptableObject管理角色立绘和头像路径
    /// 支持按种族、职业分组管理，默认占位符机制确保资源缺失时正常显示
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterSpriteConfig", menuName = "JRPG/Characters/Sprite Config")]
    public class CharacterSpriteConfig : ScriptableObject
    {
        #region Singleton
        
        private static CharacterSpriteConfig _instance;
        public static CharacterSpriteConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<CharacterSpriteConfig>("Characters/CharacterSpriteConfig");
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region 默认占位符
        
        [Header("===== 默认占位符 =====")]
        
        [Tooltip("默认角色头像")]
        public string defaultPortrait = "Characters/Portraits/DefaultPortrait";
        
        [Tooltip("默认角色立绘")]
        public string defaultFullBody = "Characters/FullBody/DefaultFullBody";
        
        [Tooltip("默认战斗立绘")]
        public string defaultBattleSprite = "Characters/Battle/DefaultBattleSprite";
        
        [Tooltip("默认缩略图")]
        public string defaultThumbnail = "Characters/Thumbnails/DefaultThumbnail";
        
        #endregion
        
        #region 种族默认立绘
        
        [Header("===== 种族默认立绘 =====")]
        
        [Tooltip("人族默认头像")]
        public string humanPortrait = "Characters/Portraits/Human/DefaultPortrait";
        
        [Tooltip("人族默认立绘")]
        public string humanFullBody = "Characters/FullBody/Human/Default";
        
        [Tooltip("人族默认战斗立绘")]
        public string humanBattleSprite = "Characters/Battle/Human/Default";
        
        [Tooltip("兽人族默认头像")]
        public string beastPortrait = "Characters/Portraits/Beast/DefaultPortrait";
        
        [Tooltip("兽人族默认立绘")]
        public string beastFullBody = "Characters/FullBody/Beast/Default";
        
        [Tooltip("兽人族默认战斗立绘")]
        public string beastBattleSprite = "Characters/Battle/Beast/Default";
        
        [Tooltip("吸血鬼默认头像")]
        public string vampirePortrait = "Characters/Portraits/Vampire/DefaultPortrait";
        
        [Tooltip("吸血鬼默认立绘")]
        public string vampireFullBody = "Characters/FullBody/Vampire/Default";
        
        [Tooltip("吸血鬼默认战斗立绘")]
        public string vampireBattleSprite = "Characters/Battle/Vampire/Default";
        
        [Tooltip("天使族默认头像")]
        public string angelPortrait = "Characters/Portraits/Angel/DefaultPortrait";
        
        [Tooltip("天使族默认立绘")]
        public string angelFullBody = "Characters/FullBody/Angel/Default";
        
        [Tooltip("天使族默认战斗立绘")]
        public string angelBattleSprite = "Characters/Battle/Angel/Default";
        
        [Tooltip("魔人族默认头像")]
        public string demonPortrait = "Characters/Portraits/Demon/DefaultPortrait";
        
        [Tooltip("魔人族默认立绘")]
        public string demonFullBody = "Characters/FullBody/Demon/Default";
        
        [Tooltip("魔人族默认战斗立绘")]
        public string demonBattleSprite = "Characters/Battle/Demon/Default";
        
        #endregion
        
        #region 职业默认立绘
        
        [Header("===== 职业默认立绘 =====")]
        
        [Tooltip("骑士默认头像")]
        public string knightPortrait = "Characters/Portraits/Knight/DefaultPortrait";
        
        [Tooltip("骑士默认战斗立绘")]
        public string knightBattleSprite = "Characters/Battle/Knight/Default";
        
        [Tooltip("法师默认头像")]
        public string magePortrait = "Characters/Portraits/Mage/DefaultPortrait";
        
        [Tooltip("法师默认战斗立绘")]
        public string mageBattleSprite = "Characters/Battle/Mage/Default";
        
        [Tooltip("弓箭手默认头像")]
        public string archerPortrait = "Characters/Portraits/Archer/DefaultPortrait";
        
        [Tooltip("弓箭手默认战斗立绘")]
        public string archerBattleSprite = "Characters/Battle/Archer/Default";
        
        [Tooltip("战士默认头像")]
        public string warriorPortrait = "Characters/Portraits/Warrior/DefaultPortrait";
        
        [Tooltip("战士默认战斗立绘")]
        public string warriorBattleSprite = "Characters/Battle/Warrior/Default";
        
        [Tooltip("刺客默认头像")]
        public string assassinPortrait = "Characters/Portraits/Assassin/DefaultPortrait";
        
        [Tooltip("刺客默认战斗立绘")]
        public string assassinBattleSprite = "Characters/Battle/Assassin/Default";
        
        [Tooltip("牧师默认头像")]
        public string priestPortrait = "Characters/Portraits/Priest/DefaultPortrait";
        
        [Tooltip("牧师默认战斗立绘")]
        public string priestBattleSprite = "Characters/Battle/Priest/Default";
        
        [Tooltip("盗贼默认头像")]
        public string roguePortrait = "Characters/Portraits/Rogue/DefaultPortrait";
        
        [Tooltip("盗贼默认战斗立绘")]
        public string rogueBattleSprite = "Characters/Battle/Rogue/Default";
        
        #endregion
        
        #region 特殊状态立绘
        
        [Header("===== 特殊状态立绘 =====")]
        
        [Tooltip("受伤状态立绘后缀")]
        public string injuredSuffix = "_injured";
        
        [Tooltip("眩晕状态立绘后缀")]
        public string stunnedSuffix = "_stunned";
        
        [Tooltip("死亡状态立绘后缀")]
        public string deadSuffix = "_dead";
        
        [Tooltip("觉醒状态立绘后缀")]
        public string awakenedSuffix = "_awakened";
        
        #endregion
        
        #region 动画帧路径
        
        [Header("===== 动画帧路径 =====")]
        
        [Tooltip("攻击动画帧序列")]
        public string attackAnimationPath = "Characters/Animations/Attack/";
        
        [Tooltip("受伤动画帧序列")]
        public string hurtAnimationPath = "Characters/Animations/Hurt/";
        
        [Tooltip("死亡动画帧序列")]
        public string deathAnimationPath = "Characters/Animations/Death/";
        
        [Tooltip("待机动画帧序列")]
        public string idleAnimationPath = "Characters/Animations/Idle/";
        
        [Tooltip("技能动画帧序列")]
        public string skillAnimationPath = "Characters/Animations/Skill/";
        
        #endregion
        
        #region 角色头像映射表
        
        [Header("===== 角色头像映射表 =====")]
        
        [Tooltip("角色头像路径映射（用于快速查找）")]
        public List<CharacterPortraitEntry> characterPortraits = new List<CharacterPortraitEntry>();
        
        [System.Serializable]
        public class CharacterPortraitEntry
        {
            [Tooltip("角色ID")]
            public int characterId;
            
            [Tooltip("角色名称")]
            public string characterName;
            
            [Tooltip("头像路径")]
            public string portraitPath;
            
            [Tooltip("立绘路径")]
            public string fullBodyPath;
            
            [Tooltip("战斗立绘路径")]
            public string battleSpritePath;
            
            [Tooltip("缩略图路径")]
            public string thumbnailPath;
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 根据种族获取默认头像路径
        /// </summary>
        public string GetDefaultPortraitByRace(Race race)
        {
            switch (race)
            {
                case Race.Human: return humanPortrait;
                case Race.Beast: return beastPortrait;
                case Race.Vampire: return vampirePortrait;
                case Race.Angel: return angelPortrait;
                case Race.Demon: return demonPortrait;
                default: return defaultPortrait;
            }
        }
        
        /// <summary>
        /// 根据种族获取默认战斗立绘路径
        /// </summary>
        public string GetDefaultBattleSpriteByRace(Race race)
        {
            switch (race)
            {
                case Race.Human: return humanBattleSprite;
                case Race.Beast: return beastBattleSprite;
                case Race.Vampire: return vampireBattleSprite;
                case Race.Angel: return angelBattleSprite;
                case Race.Demon: return demonBattleSprite;
                default: return defaultBattleSprite;
            }
        }
        
        /// <summary>
        /// 根据武器类型获取默认头像路径
        /// </summary>
        public string GetDefaultPortraitByWeapon(WeaponType weaponType)
        {
            switch (weaponType)
            {
                case WeaponType.Sword:
                case WeaponType.Shield: return knightPortrait;
                case WeaponType.Staff: return magePortrait;
                case WeaponType.Bow: return archerPortrait;
                case WeaponType.Hammer: return warriorPortrait;
                case WeaponType.Dagger: return assassinPortrait;
                case WeaponType.Fist: return roguePortrait;
                default: return defaultPortrait;
            }
        }
        
        /// <summary>
        /// 根据角色ID查找头像路径
        /// </summary>
        public string GetPortraitPath(int characterId)
        {
            var entry = characterPortraits.Find(x => x.characterId == characterId);
            if (entry != null && !string.IsNullOrEmpty(entry.portraitPath))
            {
                return entry.portraitPath;
            }
            return defaultPortrait;
        }
        
        /// <summary>
        /// 根据角色ID查找战斗立绘路径
        /// </summary>
        public string GetBattleSpritePath(int characterId)
        {
            var entry = characterPortraits.Find(x => x.characterId == characterId);
            if (entry != null && !string.IsNullOrEmpty(entry.battleSpritePath))
            {
                return entry.battleSpritePath;
            }
            return defaultBattleSprite;
        }
        
        /// <summary>
        /// 根据角色ID查找缩略图路径
        /// </summary>
        public string GetThumbnailPath(int characterId)
        {
            var entry = characterPortraits.Find(x => x.characterId == characterId);
            if (entry != null && !string.IsNullOrEmpty(entry.thumbnailPath))
            {
                return entry.thumbnailPath;
            }
            return defaultThumbnail;
        }
        
        /// <summary>
        /// 获取状态变化后的立绘路径
        /// </summary>
        public string GetStateSpritePath(int characterId, string basePath, string stateSuffix)
        {
            // 移除扩展名，添加状态后缀
            string pathWithoutExtension = basePath.Replace(".png", "").Replace(".jpg", "");
            return pathWithoutExtension + stateSuffix;
        }
        
        /// <summary>
        /// 异步加载角色头像
        /// </summary>
        public void LoadPortraitAsync(int characterId, System.Action<Sprite> onLoaded)
        {
            string path = GetPortraitPath(characterId);
            Resources.LoadAsync<Sprite>(path).completed += (operation) =>
            {
                var request = operation as ResourceRequest;
                if (request != null && request.asset != null)
                {
                    onLoaded?.Invoke(request.asset as Sprite);
                }
                else
                {
                    // 加载失败，返回占位符
                    Resources.LoadAsync<Sprite>(defaultPortrait).completed += (placeholderOp) =>
                    {
                        var placeholderRequest = placeholderOp as ResourceRequest;
                        if (placeholderRequest != null && placeholderRequest.asset != null)
                        {
                            onLoaded?.Invoke(placeholderRequest.asset as Sprite);
                        }
                        else
                        {
                            onLoaded?.Invoke(null);
                        }
                    };
                }
            };
        }
        
        #endregion
    }
}
