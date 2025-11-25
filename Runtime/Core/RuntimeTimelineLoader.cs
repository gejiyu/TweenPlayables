using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TweenPlayables
{
    /// <summary>
    /// 运行时 Timeline 加载器
    /// 用于加载导出的 Prefab 并根据阶段列表组装 Timeline
    /// </summary>
    public class RuntimeTimelineLoader : MonoBehaviour
    {
        [Header("加载设置")]
        [Tooltip("Prefab 完整路径（相对于 Assets 文件夹，不含扩展名）")]
        public string prefabPath = "AssetsRes/FishRes/FishRes_8888/Prefabs";
        
        [Tooltip("是否在加载后自动播放")]
        public bool autoPlay = true;
        
        [Tooltip("组装后的 Timeline 播放速度")]
        public float playbackSpeed = 1f;

        [Header("运行时状态")]
        [SerializeField] private GameObject loadedPrefab;
        [SerializeField] private PlayableDirector mainDirector;
        [SerializeField] private TimelineAsset assembledTimeline;

        /// <summary>
        /// 加载完成事件
        /// </summary>
        public event Action<GameObject> OnPrefabLoaded;
        
        /// <summary>
        /// Timeline 组装完成事件
        /// </summary>
        public event Action<TimelineAsset> OnTimelineAssembled;
        
        /// <summary>
        /// 播放完成事件
        /// </summary>
        public event Action OnPlaybackCompleted;

        /// <summary>
        /// 加载 Prefab 并根据阶段列表组装 Timeline
        /// </summary>
        /// <param name="prefabName">Prefab 名称（不含扩展名）</param>
        /// <param name="stageIndices">阶段索引列表，例如 [0, 1, 2] 表示按顺序播放 Stage_0, Stage_1, Stage_2</param>
        /// <returns>是否加载成功</returns>
        public bool LoadAndAssemble(string prefabName, List<int> stageIndices)
        {
            if (string.IsNullOrEmpty(prefabName))
            {
                Debug.LogError("[RuntimeTimelineLoader] Prefab 名称不能为空");
                return false;
            }

            if (stageIndices == null || stageIndices.Count == 0)
            {
                Debug.LogError("[RuntimeTimelineLoader] 阶段列表不能为空");
                return false;
            }

            // 清理之前的资源
            Cleanup();

            // 加载 Prefab
            GameObject prefabAsset = LoadPrefabAsset(prefabName);
            if (prefabAsset == null)
            {
                Debug.LogError($"[RuntimeTimelineLoader] 无法加载 Prefab: {prefabName}");
                return false;
            }

            // 实例化 Prefab
            loadedPrefab = Instantiate(prefabAsset, transform);
            loadedPrefab.name = prefabName;
            
            OnPrefabLoaded?.Invoke(loadedPrefab);
            Debug.Log($"[RuntimeTimelineLoader] 成功加载 Prefab: {prefabName}");

            // 组装 Timeline
            bool assembled = AssembleTimeline(stageIndices);
            if (!assembled)
            {
                return false;
            }

            // 自动播放
            if (autoPlay && mainDirector != null)
            {
                Play();
            }

            return true;
        }

        /// <summary>
        /// 加载 Prefab 资源
        /// </summary>
        private GameObject LoadPrefabAsset(string prefabName)
        {
            GameObject prefabAsset = null;
            
#if UNITY_EDITOR
            // 编辑器模式：使用 AssetDatabase 加载
            string assetPath = $"Assets/{prefabPath}/{prefabName}.prefab";
            prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            
            if (prefabAsset == null)
            {
                Debug.LogError($"[RuntimeTimelineLoader] 编辑器模式加载失败: {assetPath}");
            }
#else
            // 运行时模式：使用 YEngine.AssetSystem 或 Resources 加载
            // 尝试使用 YEngine.AssetSystem
            try
            {
                var assetManagerType = System.Type.GetType("YEngine.AssetSystem.GAssetManager, YEngine.AssetSystem");
                if (assetManagerType != null)
                {
                    // 需要外部传入 GAssetManager 实例或使用单例
                    Debug.Log("[RuntimeTimelineLoader] 请通过 SetAssetLoader 设置资源加载器");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[RuntimeTimelineLoader] YEngine.AssetSystem 不可用: {e.Message}");
            }

            // 回退到 Resources 加载
            if (prefabAsset == null)
            {
                prefabAsset = Resources.Load<GameObject>(prefabName);
            }
#endif
            
            return prefabAsset;
        }

        /// <summary>
        /// 自定义资源加载委托
        /// </summary>
        public static Func<string, GameObject> CustomPrefabLoader { get; set; }

        /// <summary>
        /// 根据阶段列表组装 Timeline
        /// 使用 ControlTrack 嵌套子 Timeline
        /// </summary>
        private bool AssembleTimeline(List<int> stageIndices)
        {
            if (loadedPrefab == null)
            {
                Debug.LogError("[RuntimeTimelineLoader] 没有已加载的 Prefab");
                return false;
            }

            // 收集所有 Stage 的 Timeline
            Dictionary<int, TimelineInfo> stageTimelines = new Dictionary<int, TimelineInfo>();
            
            foreach (Transform child in loadedPrefab.transform)
            {
                if (child.name.StartsWith("Stage_"))
                {
                    string indexStr = child.name.Substring("Stage_".Length);
                    if (int.TryParse(indexStr, out int stageIndex))
                    {
                        PlayableDirector director = child.GetComponent<PlayableDirector>();
                        if (director != null && director.playableAsset is TimelineAsset timeline)
                        {
                            stageTimelines[stageIndex] = new TimelineInfo
                            {
                                stageObject = child.gameObject,
                                director = director,
                                timeline = timeline
                            };
                            Debug.Log($"[RuntimeTimelineLoader] 找到 Stage_{stageIndex}，Timeline: {timeline.name}");
                        }
                    }
                }
            }

            if (stageTimelines.Count == 0)
            {
                Debug.LogError("[RuntimeTimelineLoader] 未找到任何 Stage Timeline");
                return false;
            }

            // 验证所有请求的阶段都存在
            foreach (int index in stageIndices)
            {
                if (!stageTimelines.ContainsKey(index))
                {
                    Debug.LogWarning($"[RuntimeTimelineLoader] 未找到 Stage_{index}，将跳过");
                }
            }

            // 创建组装后的主 Timeline
            assembledTimeline = ScriptableObject.CreateInstance<TimelineAsset>();
            assembledTimeline.name = $"{loadedPrefab.name}_Assembled";

            // 创建主 PlayableDirector
            mainDirector = loadedPrefab.GetComponent<PlayableDirector>();
            if (mainDirector == null)
            {
                mainDirector = loadedPrefab.AddComponent<PlayableDirector>();
            }

            // 创建 ControlTrack 用于嵌套子 Timeline
            ControlTrack controlTrack = assembledTimeline.CreateTrack<ControlTrack>(null, "StageControl");

            // 按顺序添加每个 Stage 作为 ControlPlayableAsset
            double currentTime = 0;

            foreach (int stageIndex in stageIndices)
            {
                if (!stageTimelines.TryGetValue(stageIndex, out TimelineInfo stageInfo))
                {
                    continue;
                }

                double stageDuration = stageInfo.timeline.duration;
                
                Debug.Log($"[RuntimeTimelineLoader] 添加 Stage_{stageIndex}，时长: {stageDuration:F2}s，起始时间: {currentTime:F2}s");

                // 创建 ControlPlayableAsset Clip
                TimelineClip clip = controlTrack.CreateClip<ControlPlayableAsset>();
                clip.displayName = $"Stage_{stageIndex}";
                clip.start = currentTime;
                clip.duration = stageDuration;

                // 配置 ControlPlayableAsset
                ControlPlayableAsset controlAsset = clip.asset as ControlPlayableAsset;
                if (controlAsset != null)
                {
                    // 设置源 GameObject（包含 PlayableDirector）
                    controlAsset.sourceGameObject.exposedName = UnityEditor.GUID.Generate().ToString();
                    mainDirector.SetReferenceValue(controlAsset.sourceGameObject.exposedName, stageInfo.stageObject);
                    
                    // 配置控制选项
                    controlAsset.updateDirector = true;
                    controlAsset.updateParticle = false;
                    controlAsset.updateITimeControl = false;
                    controlAsset.searchHierarchy = false;
                    controlAsset.active = true;
                    controlAsset.postPlayback = ActivationControlPlayable.PostPlaybackState.Revert;
                }

                currentTime += stageDuration;
            }

            // 设置 PlayableDirector
            mainDirector.playableAsset = assembledTimeline;

            // 禁用原始 Stage 的 PlayableDirector 自动播放
            foreach (var stageInfo in stageTimelines.Values)
            {
                if (stageInfo.director != null)
                {
                    stageInfo.director.playOnAwake = false;
                }
            }

            OnTimelineAssembled?.Invoke(assembledTimeline);
            Debug.Log($"[RuntimeTimelineLoader] Timeline 组装完成，总时长: {assembledTimeline.duration:F2}s");

            return true;
        }

        /// <summary>
        /// 播放组装后的 Timeline
        /// </summary>
        public void Play()
        {
            if (mainDirector == null)
            {
                Debug.LogError("[RuntimeTimelineLoader] 没有可播放的 Timeline");
                return;
            }

            mainDirector.time = 0;
            mainDirector.Play();
            Debug.Log("[RuntimeTimelineLoader] 开始播放 Timeline");
        }

        /// <summary>
        /// 暂停播放
        /// </summary>
        public void Pause()
        {
            if (mainDirector != null)
            {
                mainDirector.Pause();
            }
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop()
        {
            if (mainDirector != null)
            {
                mainDirector.Stop();
                mainDirector.time = 0;
            }
        }

        /// <summary>
        /// 跳转到指定时间
        /// </summary>
        public void Seek(double time)
        {
            if (mainDirector != null)
            {
                mainDirector.time = time;
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Cleanup()
        {
            if (mainDirector != null)
            {
                mainDirector.Stop();
            }

            if (assembledTimeline != null)
            {
                Destroy(assembledTimeline);
                assembledTimeline = null;
            }

            if (loadedPrefab != null)
            {
                Destroy(loadedPrefab);
                loadedPrefab = null;
            }

            mainDirector = null;
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void Update()
        {
            // 检查播放完成
            if (mainDirector != null && 
                mainDirector.state == PlayState.Playing &&
                mainDirector.time >= mainDirector.duration)
            {
                OnPlaybackCompleted?.Invoke();
            }
        }

        /// <summary>
        /// Stage Timeline 信息
        /// </summary>
        private class TimelineInfo
        {
            public GameObject stageObject;
            public PlayableDirector director;
            public TimelineAsset timeline;
        }

        #region 静态工厂方法

        /// <summary>
        /// 创建 RuntimeTimelineLoader 并加载 Prefab
        /// </summary>
        /// <param name="prefabName">Prefab 名称</param>
        /// <param name="stageIndices">阶段索引列表</param>
        /// <param name="parent">父对象（可选）</param>
        /// <returns>RuntimeTimelineLoader 实例</returns>
        public static RuntimeTimelineLoader Create(string prefabName, List<int> stageIndices, Transform parent = null)
        {
            GameObject loaderObject = new GameObject($"TimelineLoader_{prefabName}");
            if (parent != null)
            {
                loaderObject.transform.SetParent(parent);
            }

            RuntimeTimelineLoader loader = loaderObject.AddComponent<RuntimeTimelineLoader>();
            loader.LoadAndAssemble(prefabName, stageIndices);

            return loader;
        }

        /// <summary>
        /// 快速加载并播放
        /// </summary>
        /// <param name="prefabName">Prefab 名称</param>
        /// <param name="stageIndices">阶段索引列表</param>
        /// <param name="parent">父对象（可选）</param>
        /// <returns>RuntimeTimelineLoader 实例</returns>
        public static RuntimeTimelineLoader LoadAndPlay(string prefabName, List<int> stageIndices, Transform parent = null)
        {
            var loader = Create(prefabName, stageIndices, parent);
            loader.autoPlay = true;
            return loader;
        }

        #endregion
    }
}
