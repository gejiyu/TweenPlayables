using System.Collections.Generic;
using UnityEngine;

namespace TweenPlayables
{
    /// <summary>
    /// RuntimeTimelineLoader 测试组件
    /// 在运行时点击按钮加载并播放 Timeline
    /// </summary>
    public class RuntimeTimelineLoaderTest : MonoBehaviour
    {
        [Header("加载参数")]
        [Tooltip("Prefab 路径（相对于 Assets 文件夹，不含扩展名）")]
        public string prefabPath = "AssetsRes/FishRes/FishRes_8888/Prefabs";
        
        [Tooltip("Prefab 名称（不含扩展名）")]
        public string prefabName = "Death_8888";
        
        [Tooltip("阶段索引列表，用逗号分隔（如：0,1,2）")]
        public string stageIndicesString = "0";

        [Header("运行时状态")]
        [SerializeField] private RuntimeTimelineLoader loader;
        [SerializeField] private RuntimeTimelineLoader.TimelineSession currentSession;
        [SerializeField] private bool isPlaying;

        private List<int> ParseStageIndices()
        {
            List<int> indices = new List<int>();
            
            if (string.IsNullOrEmpty(stageIndicesString))
            {
                indices.Add(0);
                return indices;
            }

            string[] parts = stageIndicesString.Split(',');
            foreach (string part in parts)
            {
                if (int.TryParse(part.Trim(), out int index))
                {
                    indices.Add(index);
                }
            }

            if (indices.Count == 0)
            {
                indices.Add(0);
            }

            return indices;
        }

        /// <summary>
        /// 加载并播放 Timeline
        /// </summary>
        public void LoadAndPlay()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[RuntimeTimelineLoaderTest] 只能在运行时使用");
                return;
            }

            // 清理之前的会话
            if (currentSession != null)
            {
                loader.Cleanup(currentSession);
                currentSession = null;
            }

            // 创建 Loader（如果不存在）
            if (loader == null)
            {
                GameObject loaderObject = new GameObject($"TimelineLoader_{prefabName}");
                loaderObject.transform.SetParent(transform);
                loader = loaderObject.AddComponent<RuntimeTimelineLoader>();
            }

            List<int> stageIndices = ParseStageIndices();
            Debug.Log($"[RuntimeTimelineLoaderTest] 加载 Prefab: {prefabName}, 阶段: [{string.Join(", ", stageIndices)}]");

            // 模拟外部加载
            GameObject prefab = null;
#if UNITY_EDITOR
            string assetPath = $"Assets/{prefabPath}/{prefabName}.prefab";
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
#else
            prefab = Resources.Load<GameObject>(prefabName);
#endif

            if (prefab == null)
            {
                Debug.LogError($"[RuntimeTimelineLoaderTest] 无法加载 Prefab: {prefabName}");
                return;
            }

            // 加载并组装 Timeline
            currentSession = loader.LoadAndAssemble(prefab, stageIndices.ToArray(), null);
            
            if (currentSession != null)
            {
                currentSession.OnPlaybackCompleted += OnPlaybackCompleted;
                isPlaying = true;
            }
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void StopPlayback()
        {
            if (loader != null && currentSession != null)
            {
                loader.Stop(currentSession);
                isPlaying = false;
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Cleanup()
        {
            if (loader != null)
            {
                if (currentSession != null)
                {
                    loader.Cleanup(currentSession);
                    currentSession = null;
                }
                loader.CleanupAll();
                Destroy(loader.gameObject);
                loader = null;
                isPlaying = false;
            }
        }

        private void OnPlaybackCompleted()
        {
            Debug.Log("[RuntimeTimelineLoaderTest] 播放完成");
            isPlaying = false;
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}
