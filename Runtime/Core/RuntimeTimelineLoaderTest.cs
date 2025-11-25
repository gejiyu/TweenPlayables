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
        [SerializeField] private RuntimeTimelineLoader currentLoader;
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

            // 清理之前的 Loader
            if (currentLoader != null)
            {
                currentLoader.Cleanup();
                Destroy(currentLoader.gameObject);
                currentLoader = null;
            }

            List<int> stageIndices = ParseStageIndices();
            Debug.Log($"[RuntimeTimelineLoaderTest] 加载 Prefab: {prefabName}, 阶段: [{string.Join(", ", stageIndices)}]");

            // 创建 Loader
            currentLoader = RuntimeTimelineLoader.Create(prefabName, stageIndices, transform);
            
            if (currentLoader != null)
            {
                currentLoader.prefabPath = prefabPath;
                currentLoader.OnPlaybackCompleted += OnPlaybackCompleted;
                isPlaying = true;
            }
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void StopPlayback()
        {
            if (currentLoader != null)
            {
                currentLoader.Stop();
                isPlaying = false;
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Cleanup()
        {
            if (currentLoader != null)
            {
                currentLoader.Cleanup();
                Destroy(currentLoader.gameObject);
                currentLoader = null;
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
