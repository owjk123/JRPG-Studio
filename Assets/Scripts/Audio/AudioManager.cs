using UnityEngine;
using System.Collections.Generic;

namespace JRPGStudio.Audio
{
    /// <summary>
    /// 音频管理器
    /// 负责背景音乐和音效的播放控制
    /// </summary>
    public class AudioManager : MonoBehaviour, ISystem
    {
        public static AudioManager Instance { get; private set; }

        [Header("音频源")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambientSource;

        [Header("音量设置")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float bgmVolume = 0.8f;
        [Range(0f, 1f)] public float sfxVolume = 1f;
        [Range(0f, 1f)] public float ambientVolume = 0.5f;

        // 音频资源缓存
        private Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();

        // 当前播放的BGM
        private string currentBGM = "";
        private float bgmFadeTime = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
        }

        private void InitializeAudioSources()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            if (ambientSource == null)
            {
                ambientSource = gameObject.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }

            UpdateVolumes();
        }

        #region BGM控制

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        public void PlayBGM(string bgmPath, bool fadeIn = true)
        {
            if (currentBGM == bgmPath) return;

            AudioClip clip = LoadAudioClip(bgmPath);
            if (clip == null)
            {
                Debug.LogWarning($"BGM not found: {bgmPath}");
                return;
            }

            currentBGM = bgmPath;

            if (fadeIn && bgmSource.isPlaying)
            {
                StartCoroutine(CrossFadeBGM(clip));
            }
            else
            {
                bgmSource.clip = clip;
                bgmSource.Play();
            }
        }

        /// <summary>
        /// 停止BGM
        /// </summary>
        public void StopBGM(bool fadeOut = true)
        {
            if (fadeOut)
            {
                StartCoroutine(FadeOutBGM());
            }
            else
            {
                bgmSource.Stop();
                currentBGM = "";
            }
        }

        /// <summary>
        /// 暂停BGM
        /// </summary>
        public void PauseBGM()
        {
            bgmSource.Pause();
        }

        /// <summary>
        /// 恢复BGM
        /// </summary>
        public void ResumeBGM()
        {
            bgmSource.UnPause();
        }

        #endregion

        #region SFX控制

        /// <summary>
        /// 播放音效
        /// </summary>
        public void PlaySFX(string sfxPath, float volumeScale = 1f)
        {
            AudioClip clip = LoadAudioClip(sfxPath);
            if (clip == null)
            {
                Debug.LogWarning($"SFX not found: {sfxPath}");
                return;
            }

            sfxSource.PlayOneShot(clip, volumeScale);
        }

        /// <summary>
        /// 播放音效（指定位置）
        /// </summary>
        public void PlaySFXAtPosition(string sfxPath, Vector3 position, float volumeScale = 1f)
        {
            AudioClip clip = LoadAudioClip(sfxPath);
            if (clip == null) return;

            AudioSource.PlayClipAtPoint(clip, position, sfxVolume * masterVolume * volumeScale);
        }

        #endregion

        #region 环境音

        /// <summary>
        /// 播放环境音
        /// </summary>
        public void PlayAmbient(string ambientPath)
        {
            AudioClip clip = LoadAudioClip(ambientPath);
            if (clip == null) return;

            ambientSource.clip = clip;
            ambientSource.Play();
        }

        /// <summary>
        /// 停止环境音
        /// </summary>
        public void StopAmbient()
        {
            ambientSource.Stop();
        }

        #endregion

        #region 音量控制

        /// <summary>
        /// 更新所有音量
        /// </summary>
        public void UpdateVolumes()
        {
            bgmSource.volume = bgmVolume * masterVolume;
            sfxSource.volume = sfxVolume * masterVolume;
            ambientSource.volume = ambientVolume * masterVolume;
        }

        /// <summary>
        /// 设置主音量
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
            SaveVolumeSettings();
        }

        /// <summary>
        /// 设置BGM音量
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
            SaveVolumeSettings();
        }

        /// <summary>
        /// 设置SFX音量
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
            SaveVolumeSettings();
        }

        #endregion

        #region 辅助方法

        private AudioClip LoadAudioClip(string path)
        {
            if (audioCache.TryGetValue(path, out AudioClip cachedClip))
            {
                return cachedClip;
            }

            AudioClip clip = Resources.Load<AudioClip>(path);
            if (clip != null)
            {
                audioCache[path] = clip;
            }
            return clip;
        }

        private System.Collections.IEnumerator CrossFadeBGM(AudioClip newClip)
        {
            float startVolume = bgmSource.volume;
            float timer = 0f;

            // 淡出
            while (timer < bgmFadeTime / 2f)
            {
                timer += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / (bgmFadeTime / 2f));
                yield return null;
            }

            // 切换
            bgmSource.clip = newClip;
            bgmSource.Play();

            // 淡入
            timer = 0f;
            while (timer < bgmFadeTime / 2f)
            {
                timer += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(0f, bgmVolume * masterVolume, timer / (bgmFadeTime / 2f));
                yield return null;
            }

            bgmSource.volume = bgmVolume * masterVolume;
        }

        private System.Collections.IEnumerator FadeOutBGM()
        {
            float startVolume = bgmSource.volume;
            float timer = 0f;

            while (timer < bgmFadeTime)
            {
                timer += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / bgmFadeTime);
                yield return null;
            }

            bgmSource.Stop();
            currentBGM = "";
            bgmSource.volume = bgmVolume * masterVolume;
        }

        private void SaveVolumeSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.SetFloat("AmbientVolume", ambientVolume);
            PlayerPrefs.Save();
        }

        private void LoadVolumeSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            ambientVolume = PlayerPrefs.GetFloat("AmbientVolume", 0.5f);
            UpdateVolumes();
        }

        #endregion

        #region ISystem实现

        public void Initialize()
        {
            LoadVolumeSettings();
            Debug.Log("AudioManager initialized");
        }

        public void Shutdown()
        {
            SaveVolumeSettings();
            Debug.Log("AudioManager shutdown");
        }

        #endregion
    }

    /// <summary>
    /// 系统接口
    /// </summary>
    public interface ISystem
    {
        void Initialize();
        void Shutdown();
    }
}
