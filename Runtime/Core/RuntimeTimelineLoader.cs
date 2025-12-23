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
    /// 支持同时加载和播放多个 Timeline 实例
    /// </summary>
    public class RuntimeTimelineLoader : MonoBehaviour
    {
        /// <summary>
        /// Timeline 会话类，封装单个 Timeline 实例的所有状态
        /// </summary>
        public class TimelineSession
        {
            public int SessionId { get; private set; }
            public GameObject LoadedPrefab { get; set; }
            public PlayableDirector MainDirector { get; set; }
            public TimelineAsset AssembledTimeline { get; set; }
            public bool IsCompleted { get; set; }

            // 每个会话独立的事件
            public event Action<GameObject> OnPrefabLoaded;
            public event Action<TimelineAsset> OnTimelineAssembled;
            public event Action OnPlaybackCompleted;
            
            // 保存委托引用用于取消订阅
            private Action<PlayableDirector> stoppedCallback;

            public TimelineSession(int id)
            {
                SessionId = id;
            }

            public void InvokePrefabLoaded() => OnPrefabLoaded?.Invoke(LoadedPrefab);
            public void InvokeTimelineAssembled() => OnTimelineAssembled?.Invoke(AssembledTimeline);
            public void InvokePlaybackCompleted()
            {
                if (!IsCompleted)
                {
                    IsCompleted = true;
                    OnPlaybackCompleted?.Invoke();
                }
            }
            
            public void SubscribeToDirector()
            {
                if (MainDirector != null)
                {
                    stoppedCallback = (director) => InvokePlaybackCompleted();
                    MainDirector.stopped += stoppedCallback;
                }
            }
            
            public void UnsubscribeFromDirector()
            {
                if (MainDirector != null && stoppedCallback != null)
                {
                    MainDirector.stopped -= stoppedCallback;
                    stoppedCallback = null;
                }
            }

            public void Cleanup()
            {
                UnsubscribeFromDirector();
                
                if (MainDirector != null)
                {
                    MainDirector.Stop();
                }

                if (AssembledTimeline != null)
                {
                    Destroy(AssembledTimeline);
                    AssembledTimeline = null;
                }

                if (LoadedPrefab != null)
                {
                    Destroy(LoadedPrefab);
                    LoadedPrefab = null;
                }

                MainDirector = null;
                OnPrefabLoaded = null;
                OnTimelineAssembled = null;
                OnPlaybackCompleted = null;
            }
        }

        private struct TimelineInfo
        {
            public GameObject stageObject;
            public PlayableDirector director;
            public TimelineAsset timeline;
        }

        [Header("加载设置")]
        [Tooltip("是否在加载后自动播放")]
        public bool autoPlay = true;

        // 管理所有活跃的会话
        private List<TimelineSession> activeSessions = new List<TimelineSession>();
        private int nextSessionId = 1;

        /// <summary>
        /// 使用已加载的 Prefab 资源并根据阶段列表组装 Timeline
        /// 创建一个新的 Timeline 会话
        /// </summary>
        /// <param name="prefabAsset">Prefab 资源对象</param>
        /// <param name="stageIndices">阶段索引列表</param>
        /// <returns>创建的 Timeline 会话，如果失败返回 null</returns>
        public TimelineSession LoadAndAssemble(GameObject prefabAsset, int[] stageIndices, Action OnPlaybackCompleted )
        {
            if (prefabAsset == null)
            {
                Debug.LogError("[RuntimeTimelineLoader] Prefab 资源不能为空");
                return null;
            }

            if (stageIndices == null || stageIndices.Length == 0)
            {
                Debug.LogError("[RuntimeTimelineLoader] 阶段列表不能为空");
                return null;
            }

            // 创建新会话
            TimelineSession session = new TimelineSession(nextSessionId++);
            
            session.OnPlaybackCompleted += OnPlaybackCompleted;
            // 设置 Prefab 的父节点
            session.LoadedPrefab = prefabAsset;
            session.LoadedPrefab.transform.SetParent(transform);
            session.LoadedPrefab.name = $"{prefabAsset.name}_{session.SessionId}";
            
            session.InvokePrefabLoaded();
            Debug.Log($"[RuntimeTimelineLoader] 会话 {session.SessionId}: 成功加载 Prefab: {prefabAsset.name}");

            // 组装 Timeline
            bool assembled = AssembleTimeline(session, stageIndices);
            if (!assembled)
            {
                session.Cleanup();
                return null;
            }

            // 添加到活跃会话列表
            activeSessions.Add(session);

            // 自动播放
            if (autoPlay && session.MainDirector != null)
            {
                Play(session);
            }

            return session;
        }

        /// <summary>
        /// 根据阶段列表组装 Timeline
        /// </summary>
        private bool AssembleTimeline(TimelineSession session, int[] stageIndices)
        {
            if (session.LoadedPrefab == null)
            {
                Debug.LogError($"[RuntimeTimelineLoader] 会话 {session.SessionId}: 没有已加载的 Prefab");
                return false;
            }

            // 收集所有 Stage 的 Timeline
            Dictionary<int, TimelineInfo> stageTimelines = new Dictionary<int, TimelineInfo>();
            
            foreach (Transform child in session.LoadedPrefab.transform)
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
                        }
                    }
                }
            }

            if (stageTimelines.Count == 0)
            {
                Debug.LogError($"[RuntimeTimelineLoader] 会话 {session.SessionId}: 未找到任何 Stage Timeline");
                return false;
            }

            // 创建组装后的主 Timeline
            session.AssembledTimeline = ScriptableObject.CreateInstance<TimelineAsset>();
            session.AssembledTimeline.name = $"{session.LoadedPrefab.name}_Assembled";

            // 创建主 PlayableDirector
            session.MainDirector = session.LoadedPrefab.GetComponent<PlayableDirector>();
            if (session.MainDirector == null)
            {
                session.MainDirector = session.LoadedPrefab.AddComponent<PlayableDirector>();
            }
            session.MainDirector.playOnAwake = false;
            
            // 订阅播放完成事件
            session.SubscribeToDirector();
            
            // 为每个 Stage 创建独立的 ControlTrack
            double currentTime = 0;
            int trackIndex = 0;

            foreach (int stageIndex in stageIndices)
            {
                if (!stageTimelines.TryGetValue(stageIndex, out TimelineInfo stageInfo))
                {
                    Debug.LogWarning($"[RuntimeTimelineLoader] 会话 {session.SessionId}: 未找到 Stage_{stageIndex}，将跳过");
                    continue;
                }

                // 为当前 Stage 创建独立的 ControlTrack
                ControlTrack controlTrack = session.AssembledTimeline.CreateTrack<ControlTrack>(null, $"Control_Stage_{stageIndex}");
                
                // 获取完整时长
                double stageDuration = stageInfo.timeline.duration;
                
                // 计算 AnimationTrack 的时长（用于拼接计算）
                float animationDuration = 0f;
                foreach (var track in stageInfo.timeline.GetOutputTracks())
                {
                    if (track.GetType() == typeof(AnimationTrack))
                    {
                        foreach (var trackClip in track.GetClips())
                        {
                            float clipEnd = (float)(trackClip.start + trackClip.duration);
                            if (clipEnd > animationDuration)
                            {
                                animationDuration = clipEnd;
                            }
                        }
                    }
                }
                
                // 如果没有 AnimationTrack，使用完整时长
                if (animationDuration <= 0)
                {
                    animationDuration = (float)stageDuration;
                }
                
                // 在独立的 ControlTrack 上创建 ControlPlayableAsset Clip
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
                    session.MainDirector.SetReferenceValue(controlAsset.sourceGameObject.exposedName, stageInfo.stageObject);
                    
                    // 配置控制选项
                    controlAsset.updateDirector = true;
                    controlAsset.updateParticle = false;
                    controlAsset.updateITimeControl = false;
                    controlAsset.searchHierarchy = false;
                    controlAsset.active = true;
                    controlAsset.postPlayback = ActivationControlPlayable.PostPlaybackState.Revert;
                }

                Debug.Log($"[RuntimeTimelineLoader] 会话 {session.SessionId}: Track {trackIndex} - Stage_{stageIndex}, 开始: {currentTime:F2}s, 动画时长: {animationDuration:F2}s, 总时长: {stageDuration:F2}s");
                
                // 使用 AnimationTrack 时长来计算下一个阶段的开始位置
                currentTime += animationDuration;
                trackIndex++;
            }

            // 设置 PlayableDirector
            session.MainDirector.playableAsset = session.AssembledTimeline;

            // 禁用原始 Stage 的 PlayableDirector 自动播放
            foreach (var stageInfo in stageTimelines.Values)
            {
                if (stageInfo.director != null)
                {
                    stageInfo.director.playOnAwake = false;
                }
            }

            session.InvokeTimelineAssembled();
            Debug.Log($"[RuntimeTimelineLoader] 会话 {session.SessionId}: Timeline 组装完成，总时长: {session.AssembledTimeline.duration:F2}s");

            return true;
        }

        /// <summary>
        /// 播放指定会话的 Timeline
        /// </summary>
        public void Play(TimelineSession session)
        {
            if (session != null && session.MainDirector != null)
            {
                session.MainDirector.time = 0;
                session.MainDirector.Play();
                session.IsCompleted = false;
            }
        }

        /// <summary>
        /// 暂停指定会话
        /// </summary>
        public void Pause(TimelineSession session)
        {
            if (session != null && session.MainDirector != null)
            {
                session.MainDirector.Pause();
            }
        }

        /// <summary>
        /// 停止指定会话
        /// </summary>
        public void Stop(TimelineSession session)
        {
            if (session != null && session.MainDirector != null)
            {
                session.MainDirector.Stop();
                session.MainDirector.time = 0;
            }
        }

        /// <summary>
        /// 清理指定会话
        /// </summary>
        public void Cleanup(TimelineSession session)
        {
            if (session != null)
            {
                session.Cleanup();
                activeSessions.Remove(session);
            }
        }

        /// <summary>
        /// 清理所有会话
        /// </summary>
        public void CleanupAll()
        {
            // 倒序遍历以安全删除
            for (int i = activeSessions.Count - 1; i >= 0; i--)
            {
                activeSessions[i].Cleanup();
            }
            activeSessions.Clear();
        }

        private void OnDestroy()
        {
            CleanupAll();
        }
    }
}
