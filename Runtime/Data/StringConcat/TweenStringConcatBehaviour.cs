using System;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace TweenPlayables
{
    /// <summary>
    /// Timeline 字符串拼接行为
    /// </summary>
    [Serializable]
    public class TweenStringConcatBehaviour : PlayableBehaviour
    {
        private bool initialized;
        private TimelineDataManager binding;
        private StringConcatNode node;

        public void SetNode(StringConcatNode node)
        {
            this.node = node;
        }

        public override void OnGraphStop(Playable playable)
        {
            initialized = false;
            binding = null;
        }

        public override void OnPlayableCreate(Playable playable)
        {
            initialized = false;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (!node.isActive)
                return;

            var dataManager = info.output.GetUserData() as TimelineDataManager;
            
            if (!initialized && dataManager != null)
            {
                Initialize(dataManager);
            }
            
            if (info.effectivePlayState == PlayState.Playing && binding != null)
            {
                PerformConcatenation(binding);
            }
        }

        private void Initialize(TimelineDataManager dataManager)
        {
            if (dataManager == null || initialized) 
                return;

            binding = dataManager;
            initialized = true;
        }

        private void PerformConcatenation(TimelineDataManager dataManager)
        {
            if (dataManager == null || node.items == null) return;

            StringBuilder sb = new StringBuilder();

            foreach (var item in node.items)
            {
                string val = StringDataSourceAccessor.GetString(item, dataManager);
                sb.Append(val);
            }

            string result = sb.ToString();

            if (!string.IsNullOrEmpty(node.resultDataKey))
            {
                dataManager.Set(node.resultDataKey, result);
            }
        }
    }
}
