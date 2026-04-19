// GameManager.cs - 游戏核心管理器
// 这是JRPG项目的核心单例管理器，负责游戏状态和系统协调

using UnityEngine;
using System;

namespace JRPG.Core
{
    /// <summary>
    /// 游戏核心管理器 - 负责游戏状态和系统协调
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton
        
        public static GameManager Instance { get; private set; }
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 游戏状态变化事件
        /// </summary>
        public event Action<GameState, GameState> OnGameStateChanged;
        
        #endregion
        
        #region Fields
        
        [Header("游戏设置")]
        [SerializeField] private int _targetFrameRate = 60;
        [SerializeField] private bool _enableDebugLog = true;
        
        private GameState _currentState = GameState.None;
        
        // 子系统引用
        private BattleManager _battleManager;
        private CharacterManager _characterManager;
        private UIManager _uiManager;
        private AudioManager _audioManager;
        private SaveManager _saveManager;
        
        #endregion
        
        #region Properties
        
        public GameState CurrentState => _currentState;
        public BattleManager Battle => _battleManager;
        public CharacterManager Characters => _characterManager;
        public UIManager UI => _uiManager;
        public AudioManager Audio => _audioManager;
        public SaveManager Save => _saveManager;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // 单例初始化
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 初始化子系统
            InitializeSubSystems();
            
            // 设置目标帧率
            Application.targetFrameRate = _targetFrameRate;
        }
        
        private void Start()
        {
            // 初始状态
            ChangeState(GameState.MainMenu);
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 改变游戏状态
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (_currentState == newState) return;
            
            var oldState = _currentState;
            _currentState = newState;
            
            HandleStateChange(oldState, newState);
            
            OnGameStateChanged?.Invoke(oldState, newState);
            
            if (_enableDebugLog)
            {
                Debug.Log($"[GameManager] 状态变化: {oldState} -> {newState}");
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private void InitializeSubSystems()
        {
            // 获取或添加子系统
            _battleManager = GetComponentInChildren<BattleManager>();
            _characterManager = GetComponentInChildren<CharacterManager>();
            _uiManager = GetComponentInChildren<UIManager>();
            _audioManager = GetComponentInChildren<AudioManager>();
            _saveManager = GetComponentInChildren<SaveManager>();
            
            // 初始化各系统
            _saveManager?.Initialize();
            _audioManager?.Initialize();
            _uiManager?.Initialize();
            _characterManager?.Initialize();
            _battleManager?.Initialize();
        }
        
        private void HandleStateChange(GameState oldState, GameState newState)
        {
            switch (newState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    break;
                    
                case GameState.Exploration:
                    Time.timeScale = 1f;
                    break;
                    
                case GameState.Battle:
                    Time.timeScale = 1f;
                    break;
                    
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                    
                case GameState.Cutscene:
                    Time.timeScale = 1f;
                    break;
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    public enum GameState
    {
        None,
        Loading,
        MainMenu,
        Exploration,
        Battle,
        Cutscene,
        Paused
    }
}
