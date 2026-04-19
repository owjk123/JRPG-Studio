// SaveManager.cs - 存档管理器
// 管理游戏的保存和加载

using System;
using System.Collections.Generic;
using UnityEngine;
using JRPG.Resources;

namespace JRPG.Save
{
    /// <summary>
    /// 存档管理器单例类
    /// </summary>
    public class SaveManager
    {
        #region 单例
        
        private static SaveManager _instance;
        public static SaveManager Instance => _instance ??= new SaveManager();
        
        #endregion
        
        #region 常量
        
        /// <summary>
        /// 存档文件名前缀
        /// </summary>
        private const string SaveFilePrefix = "player_save_";
        
        /// <summary>
        /// 存档列表文件名
        /// </summary>
        private const string SaveListFileName = "save_slots.json";
        
        /// <summary>
        /// 最大存档槽位数量
        /// </summary>
        private const int MaxSaveSlots = 3;
        
        /// <summary>
        /// 自动保存间隔（秒）
        /// </summary>
        private const int AutoSaveInterval = 300; // 5分钟
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 保存开始事件
        /// </summary>
        public event Action<int> OnSaveStarted;
        
        /// <summary>
        /// 保存完成事件
        /// </summary>
        public event Action<int, bool> OnSaveCompleted;
        
        /// <summary>
        /// 加载开始事件
        /// </summary>
        public event Action<int> OnLoadStarted;
        
        /// <summary>
        /// 加载完成事件
        /// </summary>
        public event Action<int, bool> OnLoadCompleted;
        
        /// <summary>
        /// 存档列表更新事件
        /// </summary>
        public event Action OnSaveSlotsUpdated;
        
        #endregion
        
        #region 私有变量
        
        /// <summary>
        /// 当前存档槽位
        /// </summary>
        private int _currentSlot = -1;
        
        /// <summary>
        /// 当前玩家数据
        /// </summary>
        private PlayerData _currentPlayerData;
        
        /// <summary>
        /// 存档槽位列表
        /// </summary>
        private SaveSlotList _saveSlotList;
        
        /// <summary>
        /// 自动保存计时器
        /// </summary>
        private float _autoSaveTimer = 0;
        
        /// <summary>
        /// 是否启用自动保存
        /// </summary>
        private bool _autoSaveEnabled = true;
        
        #endregion
        
        #region 构造函数
        
        private SaveManager()
        {
            LoadSaveSlots();
        }
        
        #endregion
        
        #region 公开方法
        
        /// <summary>
        /// 获取当前玩家数据
        /// </summary>
        public PlayerData GetCurrentPlayerData()
        {
            if (_currentPlayerData == null)
            {
                // 如果没有当前数据，尝试加载默认槽位
                if (_currentSlot >= 0 && HasSave(_currentSlot))
                {
                    Load(_currentSlot);
                }
                else
                {
                    // 创建新玩家数据
                    _currentPlayerData = PlayerData.CreateNew("冒险者");
                }
            }
            
            return _currentPlayerData;
        }
        
        /// <summary>
        /// 保存到指定槽位
        /// </summary>
        public bool Save(int slotIndex, PlayerData data = null)
        {
            if (slotIndex < 0 || slotIndex >= MaxSaveSlots)
            {
                Debug.LogError($"无效的存档槽位: {slotIndex}");
                return false;
            }
            
            OnSaveStarted?.Invoke(slotIndex);
            
            try
            {
                // 使用指定数据或当前数据
                var saveData = data ?? _currentPlayerData;
                if (saveData == null)
                {
                    Debug.LogError("没有可保存的数据");
                    OnSaveCompleted?.Invoke(slotIndex, false);
                    return false;
                }
                
                // 更新时间
                saveData.UpdateLoginTime();
                
                // 序列化为JSON
                string json = JsonUtility.ToJson(saveData, false);
                
                // 写入文件
                string filePath = GetSaveFilePath(slotIndex);
                System.IO.File.WriteAllText(filePath, json);
                
                // 更新存档列表
                UpdateSaveSlot(slotIndex, saveData);
                SaveSaveSlots();
                
                Debug.Log($"存档保存成功: 槽位 {slotIndex}");
                OnSaveCompleted?.Invoke(slotIndex, true);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"存档保存失败: {e.Message}");
                OnSaveCompleted?.Invoke(slotIndex, false);
                return false;
            }
        }
        
        /// <summary>
        /// 从指定槽位加载
        /// </summary>
        public bool Load(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSaveSlots)
            {
                Debug.LogError($"无效的存档槽位: {slotIndex}");
                return false;
            }
            
            if (!HasSave(slotIndex))
            {
                Debug.LogWarning($"槽位 {slotIndex} 没有存档");
                return false;
            }
            
            OnLoadStarted?.Invoke(slotIndex);
            
            try
            {
                string filePath = GetSaveFilePath(slotIndex);
                string json = System.IO.File.ReadAllText(filePath);
                
                _currentPlayerData = JsonUtility.FromJson<PlayerData>(json);
                _currentSlot = slotIndex;
                
                Debug.Log($"存档加载成功: 槽位 {slotIndex}");
                OnLoadCompleted?.Invoke(slotIndex, true);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"存档加载失败: {e.Message}");
                OnLoadCompleted?.Invoke(slotIndex, false);
                return false;
            }
        }
        
        /// <summary>
        /// 快速保存（保存到当前槽位）
        /// </summary>
        public bool QuickSave()
        {
            if (_currentSlot < 0)
            {
                Debug.LogWarning("没有选择存档槽位");
                return false;
            }
            
            return Save(_currentSlot);
        }
        
        /// <summary>
        /// 快速加载（加载当前槽位）
        /// </summary>
        public bool QuickLoad()
        {
            if (_currentSlot < 0)
            {
                Debug.LogWarning("没有选择存档槽位");
                return false;
            }
            
            return Load(_currentSlot);
        }
        
        /// <summary>
        /// 检查指定槽位是否有存档
        /// </summary>
        public bool HasSave(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSaveSlots)
                return false;
            
            string filePath = GetSaveFilePath(slotIndex);
            return System.IO.File.Exists(filePath);
        }
        
        /// <summary>
        /// 获取存档槽位信息列表
        /// </summary>
        public SaveSlotList GetSaveSlots()
        {
            if (_saveSlotList == null)
            {
                LoadSaveSlots();
            }
            return _saveSlotList;
        }
        
        /// <summary>
        /// 获取指定槽位的存档信息
        /// </summary>
        public SaveSlotInfo GetSaveSlotInfo(int slotIndex)
        {
            var slots = GetSaveSlots();
            if (slotIndex < 0 || slotIndex >= slots.slots.Length)
                return null;
            
            // 更新存档状态
            slots.slots[slotIndex].hasSave = HasSave(slotIndex);
            
            return slots.slots[slotIndex];
        }
        
        /// <summary>
        /// 删除指定槽位的存档
        /// </summary>
        public bool DeleteSave(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSaveSlots)
                return false;
            
            try
            {
                string filePath = GetSaveFilePath(slotIndex);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                
                // 更新槽位信息
                _saveSlotList.slots[slotIndex] = new SaveSlotInfo { slotIndex = slotIndex };
                SaveSaveSlots();
                
                OnSaveSlotsUpdated?.Invoke();
                Debug.Log($"存档已删除: 槽位 {slotIndex}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"删除存档失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 选择存档槽位
        /// </summary>
        public void SelectSlot(int slotIndex)
        {
            _currentSlot = slotIndex;
            _saveSlotList.lastUsedSlot = slotIndex;
            SaveSaveSlots();
        }
        
        /// <summary>
        /// 获取当前槽位
        /// </summary>
        public int GetCurrentSlot()
        {
            return _currentSlot;
        }
        
        /// <summary>
        /// 创建新存档
        /// </summary>
        public bool CreateNewSave(string playerName)
        {
            if (_currentSlot < 0 || _currentSlot >= MaxSaveSlots)
            {
                Debug.LogError("请先选择存档槽位");
                return false;
            }
            
            var newPlayerData = PlayerData.CreateNew(playerName);
            return Save(_currentSlot, newPlayerData);
        }
        
        /// <summary>
        /// 启用/禁用自动保存
        /// </summary>
        public void SetAutoSave(bool enabled)
        {
            _autoSaveEnabled = enabled;
        }
        
        /// <summary>
        /// 更新（每帧调用）
        /// </summary>
        public void Update()
        {
            if (!_autoSaveEnabled || _currentPlayerData == null)
                return;
            
            _autoSaveTimer += Time.deltaTime;
            if (_autoSaveTimer >= AutoSaveInterval)
            {
                _autoSaveTimer = 0;
                QuickSave();
                Debug.Log("自动保存完成");
            }
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 获取存档文件路径
        /// </summary>
        private string GetSaveFilePath(int slotIndex)
        {
            string directory = Application.persistentDataPath + "/Saves/";
            
            // 确保目录存在
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            return directory + SaveFilePrefix + slotIndex + ".json";
        }
        
        /// <summary>
        /// 获取存档列表文件路径
        /// </summary>
        private string GetSaveListFilePath()
        {
            return Application.persistentDataPath + "/Saves/" + SaveListFileName;
        }
        
        /// <summary>
        /// 更新存档槽位信息
        /// </summary>
        private void UpdateSaveSlot(int slotIndex, PlayerData data)
        {
            if (_saveSlotList == null)
            {
                _saveSlotList = new SaveSlotList();
                for (int i = 0; i < MaxSaveSlots; i++)
                {
                    _saveSlotList.slots[i] = new SaveSlotInfo { slotIndex = i };
                }
            }
            
            _saveSlotList.slots[slotIndex] = new SaveSlotInfo
            {
                slotIndex = slotIndex,
                hasSave = true,
                playerName = data.playerName,
                playerLevel = data.playerLevel,
                saveTime = data.lastLoginTime,
                gameVersion = data.gameVersion
            };
            
            _saveSlotList.lastUsedSlot = slotIndex;
        }
        
        /// <summary>
        /// 加载存档列表
        /// </summary>
        private void LoadSaveSlots()
        {
            try
            {
                string filePath = GetSaveListFilePath();
                if (System.IO.File.Exists(filePath))
                {
                    string json = System.IO.File.ReadAllText(filePath);
                    _saveSlotList = JsonUtility.FromJson<SaveSlotList>(json);
                }
                
                if (_saveSlotList == null)
                {
                    _saveSlotList = new SaveSlotList();
                    for (int i = 0; i < MaxSaveSlots; i++)
                    {
                        _saveSlotList.slots[i] = new SaveSlotInfo { slotIndex = i };
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"加载存档列表失败: {e.Message}");
                _saveSlotList = new SaveSlotList();
            }
        }
        
        /// <summary>
        /// 保存存档列表
        /// </summary>
        private void SaveSaveSlots()
        {
            try
            {
                string filePath = GetSaveListFilePath();
                string directory = System.IO.Path.GetDirectoryName(filePath);
                
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                
                string json = JsonUtility.ToJson(_saveSlotList, true);
                System.IO.File.WriteAllText(filePath, json);
                
                OnSaveSlotsUpdated?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"保存存档列表失败: {e.Message}");
            }
        }
        
        #endregion
        
        #region 数据导入导出
        
        /// <summary>
        /// 导出存档（用于云存档或迁移）
        /// </summary>
        public string ExportSave()
        {
            if (_currentPlayerData == null)
                return null;
            
            return JsonUtility.ToJson(_currentPlayerData);
        }
        
        /// <summary>
        /// 导入存档
        /// </summary>
        public bool ImportSave(string jsonData)
        {
            try
            {
                _currentPlayerData = JsonUtility.FromJson<PlayerData>(jsonData);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"导入存档失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 备份当前存档
        /// </summary>
        public bool BackupCurrentSave()
        {
            if (_currentSlot < 0 || _currentPlayerData == null)
                return false;
            
            try
            {
                string sourcePath = GetSaveFilePath(_currentSlot);
                string backupPath = sourcePath + ".backup";
                
                if (System.IO.File.Exists(sourcePath))
                {
                    System.IO.File.Copy(sourcePath, backupPath, true);
                    Debug.Log($"存档备份成功: {backupPath}");
                    return true;
                }
                
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"存档备份失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 恢复备份
        /// </summary>
        public bool RestoreBackup()
        {
            if (_currentSlot < 0)
                return false;
            
            try
            {
                string sourcePath = GetSaveFilePath(_currentSlot);
                string backupPath = sourcePath + ".backup";
                
                if (System.IO.File.Exists(backupPath))
                {
                    System.IO.File.Copy(backupPath, sourcePath, true);
                    return Load(_currentSlot);
                }
                
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"恢复备份失败: {e.Message}");
                return false;
            }
        }
        
        #endregion
    }
}
