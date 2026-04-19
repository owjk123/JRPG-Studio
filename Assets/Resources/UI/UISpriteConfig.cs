// UISpriteConfig.cs - UI精灵配置
// 定义所有UI使用的Sprite资源路径，用于资源加载和占位符管理

using UnityEngine;
using System.Collections.Generic;

namespace JRPG.Assets.UI
{
    /// <summary>
    /// UI精灵配置 - ScriptableObject管理所有UI精灵路径
    /// 用于统一管理UI资源，支持运行时动态加载和占位符替换
    /// </summary>
    [CreateAssetMenu(fileName = "UISpriteConfig", menuName = "JRPG/UI/UISprite Config")]
    public class UISpriteConfig : ScriptableObject
    {
        #region Singleton
        
        private static UISpriteConfig _instance;
        public static UISpriteConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<UISpriteConfig>("UI/UISpriteConfig");
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region 通用UI元素
        
        [Header("===== 通用UI元素 =====")]
        
        [Tooltip("默认背景图路径")]
        public string defaultBackground = "UI/Backgrounds/DefaultBackground";
        
        [Tooltip("默认对话框背景")]
        public string defaultDialogBoxBg = "UI/Dialog/DialogBoxBackground";
        
        [Tooltip("默认按钮正常状态")]
        public string defaultButtonNormal = "UI/Buttons/ButtonNormal";
        
        [Tooltip("默认按钮按下状态")]
        public string defaultButtonPressed = "UI/Buttons/ButtonPressed";
        
        [Tooltip("默认按钮禁用状态")]
        public string defaultButtonDisabled = "UI/Buttons/ButtonDisabled";
        
        [Tooltip("默认头像框背景")]
        public string defaultPortraitFrame = "UI/Frames/PortraitFrame";
        
        [Tooltip("默认物品图标")]
        public string defaultItemIcon = "UI/Icons/ItemDefault";
        
        [Tooltip("通用占位图标")]
        public string placeholderIcon = "UI/Icons/Placeholder";
        
        #endregion
        
        #region 血条和资源条
        
        [Header("===== 血条和资源条 =====")]
        
        [Tooltip("HP条背景")]
        public string hpBarBackground = "UI/Bars/HPBarBackground";
        
        [Tooltip("HP条填充")]
        public string hpBarFill = "UI/Bars/HPBarFill";
        
        [Tooltip("MP条背景")]
        public string mpBarBackground = "UI/Bars/MPBarBackground";
        
        [Tooltip("MP条填充")]
        public string mpBarFill = "UI/Bars/MPBarFill";
        
        [Tooltip("经验条背景")]
        public string expBarBackground = "UI/Bars/EXPBarBackground";
        
        [Tooltip("经验条填充")]
        public string expBarFill = "UI/Bars/EXPBarFill";
        
        [Tooltip("ATB条背景")]
        public string atbBarBackground = "UI/Bars/ATBBarBackground";
        
        [Tooltip("ATB条填充")]
        public string atbBarFill = "UI/Bars/ATBBarFill";
        
        #endregion
        
        #region 战斗UI
        
        [Header("===== 战斗UI =====")]
        
        [Tooltip("战斗背景-普通")]
        public string battleBackgroundNormal = "UI/Backgrounds/BattleBackgroundNormal";
        
        [Tooltip("战斗背景-精英")]
        public string battleBackgroundElite = "UI/Backgrounds/BattleBackgroundElite";
        
        [Tooltip("战斗背景-BOSS")]
        public string battleBackgroundBoss = "UI/Backgrounds/BattleBackgroundBoss";
        
        [Tooltip("行动菜单背景")]
        public string actionMenuBg = "UI/Battle/ActionMenuBackground";
        
        [Tooltip("技能菜单背景")]
        public string skillMenuBg = "UI/Battle/SkillMenuBackground";
        
        [Tooltip("目标选择箭头")]
        public string targetArrow = "UI/Battle/TargetArrow";
        
        [Tooltip("伤害数字特效")]
        public string damageNumber = "UI/Battle/DamageNumber";
        
        [Tooltip("暴击特效")]
        public string criticalEffect = "UI/Battle/CriticalEffect";
        
        [Tooltip("治疗数字特效")]
        public string healNumber = "UI/Battle/HealNumber";
        
        #endregion
        
        #region 状态图标
        
        [Header("===== 状态图标 =====")]
        
        [Tooltip("增益图标基础路径")]
        public string buffIconBase = "UI/Icons/Buff/";
        
        [Tooltip("减益图标基础路径")]
        public string debuffIconBase = "UI/Icons/Debuff/";
        
        [Tooltip("元素图标基础路径")]
        public string elementIconBase = "UI/Icons/Element/";
        
        // 元素图标快捷引用
        [Tooltip("火属性图标")]
        public string iconFire = "UI/Icons/Element/Fire";
        
        [Tooltip("冰属性图标")]
        public string iconIce = "UI/Icons/Element/Ice";
        
        [Tooltip("雷属性图标")]
        public string iconThunder = "UI/Icons/Element/Thunder";
        
        [Tooltip("光属性图标")]
        public string iconLight = "UI/Icons/Element/Light";
        
        [Tooltip("暗属性图标")]
        public string iconDark = "UI/Icons/Element/Dark";
        
        [Tooltip("风属性图标")]
        public string iconWind = "UI/Icons/Element/Wind";
        
        [Tooltip("土属性图标")]
        public string iconEarth = "UI/Icons/Element/Earth";
        
        [Tooltip("水属性图标")]
        public string iconWater = "UI/Icons/Element/Water";
        
        #endregion
        
        #region 稀有度图标
        
        [Header("===== 稀有度图标 =====")]
        
        [Tooltip("普通品质边框")]
        public string rarityCommon = "UI/Icons/Rarity/Common";
        
        [Tooltip("优秀品质边框")]
        public string rarityUncommon = "UI/Icons/Rarity/Uncommon";
        
        [Tooltip("稀有品质边框")]
        public string rarityRare = "UI/Icons/Rarity/Rare";
        
        [Tooltip("史诗品质边框")]
        public string rarityEpic = "UI/Icons/Rarity/Epic";
        
        [Tooltip("传说品质边框")]
        public string rarityLegendary = "UI/Icons/Rarity/Legendary";
        
        #endregion
        
        #region 武器图标
        
        [Header("===== 武器图标 =====")]
        
        [Tooltip("剑图标")]
        public string iconSword = "UI/Icons/Weapon/Sword";
        
        [Tooltip("法杖图标")]
        public string iconStaff = "UI/Icons/Weapon/Staff";
        
        [Tooltip("弓图标")]
        public string iconBow = "UI/Icons/Weapon/Bow";
        
        [Tooltip("匕首图标")]
        public string iconDagger = "UI/Icons/Weapon/Dagger";
        
        [Tooltip("锤子图标")]
        public string iconHammer = "UI/Icons/Weapon/Hammer";
        
        [Tooltip("拳套图标")]
        public string iconFist = "UI/Icons/Weapon/Fist";
        
        [Tooltip("盾牌图标")]
        public string iconShield = "UI/Icons/Weapon/Shield";
        
        #endregion
        
        #region 种族图标
        
        [Header("===== 种族图标 =====")]
        
        [Tooltip("人族图标")]
        public string iconHuman = "UI/Icons/Race/Human";
        
        [Tooltip("兽人族图标")]
        public string iconBeast = "UI/Icons/Race/Beast";
        
        [Tooltip("吸血鬼图标")]
        public string iconVampire = "UI/Icons/Race/Vampire";
        
        [Tooltip("天使族图标")]
        public string iconAngel = "UI/Icons/Race/Angel";
        
        [Tooltip("魔人族图标")]
        public string iconDemon = "UI/Icons/Race/Demon";
        
        #endregion
        
        #region UI面板背景
        
        [Header("===== UI面板背景 =====")]
        
        [Tooltip("主菜单背景")]
        public string mainMenuBg = "UI/Panels/MainMenuBackground";
        
        [Tooltip("角色面板背景")]
        public string characterPanelBg = "UI/Panels/CharacterPanelBackground";
        
        [Tooltip("背包面板背景")]
        public string inventoryPanelBg = "UI/Panels/InventoryPanelBackground";
        
        [Tooltip("技能面板背景")]
        public string skillPanelBg = "UI/Panels/SkillPanelBackground";
        
        [Tooltip("设置面板背景")]
        public string settingsPanelBg = "UI/Panels/SettingsPanelBackground";
        
        #endregion
        
        #region 提示和反馈
        
        [Header("===== 提示和反馈 =====")]
        
        [Tooltip("成功提示")]
        public string successToast = "UI/Feedback/SuccessToast";
        
        [Tooltip("错误提示")]
        public string errorToast = "UI/Feedback/ErrorToast";
        
        [Tooltip("警告提示")]
        public string warningToast = "UI/Feedback/WarningToast";
        
        [Tooltip("加载中动画")]
        public string loadingSpinner = "UI/Feedback/LoadingSpinner";
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 获取元素图标路径
        /// </summary>
        public string GetElementIconPath(JRPG.Battle.Element element)
        {
            switch (element)
            {
                case JRPG.Battle.Element.Fire: return iconFire;
                case JRPG.Battle.Element.Ice: return iconIce;
                case JRPG.Battle.Element.Thunder: return iconThunder;
                case JRPG.Battle.Element.Light: return iconLight;
                case JRPG.Battle.Element.Dark: return iconDark;
                case JRPG.Battle.Element.Wind: return iconWind;
                case JRPG.Battle.Element.Earth: return iconEarth;
                case JRPG.Battle.Element.Water: return iconWater;
                default: return placeholderIcon;
            }
        }
        
        /// <summary>
        /// 获取稀有度图标路径
        /// </summary>
        public string GetRarityIconPath(JRPG.Battle.Rarity rarity)
        {
            switch (rarity)
            {
                case JRPG.Battle.Rarity.Common: return rarityCommon;
                case JRPG.Battle.Rarity.Uncommon: return rarityUncommon;
                case JRPG.Battle.Rarity.Rare: return rarityRare;
                case JRPG.Battle.Rarity.Epic: return rarityEpic;
                case JRPG.Battle.Rarity.Legendary: return rarityLegendary;
                default: return rarityCommon;
            }
        }
        
        /// <summary>
        /// 获取武器图标路径
        /// </summary>
        public string GetWeaponIconPath(JRPG.Battle.WeaponType weaponType)
        {
            switch (weaponType)
            {
                case JRPG.Battle.WeaponType.Sword: return iconSword;
                case JRPG.Battle.WeaponType.Staff: return iconStaff;
                case JRPG.Battle.WeaponType.Bow: return iconBow;
                case JRPG.Battle.WeaponType.Dagger: return iconDagger;
                case JRPG.Battle.WeaponType.Hammer: return iconHammer;
                case JRPG.Battle.WeaponType.Fist: return iconFist;
                case JRPG.Battle.WeaponType.Shield: return iconShield;
                default: return placeholderIcon;
            }
        }
        
        /// <summary>
        /// 获取种族图标路径
        /// </summary>
        public string GetRaceIconPath(JRPG.Battle.Race race)
        {
            switch (race)
            {
                case JRPG.Battle.Race.Human: return iconHuman;
                case JRPG.Battle.Race.Beast: return iconBeast;
                case JRPG.Battle.Race.Vampire: return iconVampire;
                case JRPG.Battle.Race.Angel: return iconAngel;
                case JRPG.Battle.Race.Demon: return iconDemon;
                default: return placeholderIcon;
            }
        }
        
        /// <summary>
        /// 异步加载Sprite，带占位符支持
        /// </summary>
        public void LoadSpriteAsync(string path, System.Action<Sprite> onLoaded)
        {
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
                    Resources.LoadAsync<Sprite>(placeholderIcon).completed += (placeholderOp) =>
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
