using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace TweenPlayables
{
    /// <summary>
    /// 运行时 Timeline 加载器
    /// 用于加载导出的 Prefab 并根据阶段列表组装 Timeline
    /// </summary>
    public class RuntimeTimelineLoader : MonoBehaviour
    {
        [Header("加载设置")]
        [Tooltip("是否在加载后自动播放")]
        public bool autoPlay = true;

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
        /// 使用已加载的 Prefab 资源并根据阶段列表组装 Timeline
        /// </summary>
        /// <param name="prefabAsset">Prefab 资源对象</param>
        /// <param name="stageIndices">阶段索引列表</param>
        /// <returns>是否加载成功</returns>
        public bool LoadAndAssemble(GameObject prefabAsset, int[] stageIndices)
        {
            if (prefabAsset == null)
            {
                Debug.LogError("[RuntimeTimelineLoader] Prefab 资源不能为空");
                return false;
            }

            if (stageIndices == null || stageIndices.Length == 0)
            {
                Debug.LogError("[RuntimeTimelineLoader] 阶段列表不能为空");
                return false;
            }

            // 清理之前的资源
            Cleanup();

            // 实例化 Prefab
            loadedPrefab = Instantiate(prefabAsset, transform);
            loadedPrefab.name = prefabAsset.name;
            
            OnPrefabLoaded?.Invoke(loadedPrefab);
            Debug.Log($"[RuntimeTimelineLoader] 成功加载 Prefab: {prefabAsset.name}");

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
        /// 根据阶段列表组装 Timeline
        /// 使用 ControlTrack 嵌套子 Timeline
        /// </summary>
        private bool AssembleTimeline(int[] stageIndices)
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
            mainDirector.playOnAwake = false;
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
                    controlAsset.sourceGameObject.exposedName = System.Guid.NewGuid().ToString();
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
        /// 创建 RuntimeTimelineLoader 并使用已加载的 Prefab
        /// </summary>
        /// <param name="prefabAsset">Prefab 资源</param>
        /// <param name="stageIndices">阶段索引列表</param>
        /// <param name="parent">父对象（可选）</param>
        /// <returns>RuntimeTimelineLoader 实例</returns>
        public static RuntimeTimelineLoader Create(GameObject prefabAsset, int[] stageIndices, Transform parent = null)
        {
            if (prefabAsset == null)
            {
                Debug.LogError("[RuntimeTimelineLoader] Create 失败: Prefab 资源为空");
                return null;
            }

            GameObject loaderObject = new GameObject($"TimelineLoader_{prefabAsset.name}");
            if (parent != null)
            {
                loaderObject.transform.SetParent(parent);
            }

            RuntimeTimelineLoader loader = loaderObject.AddComponent<RuntimeTimelineLoader>();
            loader.LoadAndAssemble(prefabAsset, stageIndices);

            return loader;
        }

        #endregion
    }
}
