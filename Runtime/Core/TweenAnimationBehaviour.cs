using System;
using UnityEngine;
using UnityEngine.Playables;

namespace TweenPlayables
{
    [Serializable]
    public abstract class TweenAnimationBehaviour<TBinding> : PlayableBehaviour where TBinding : Component
    {
        bool initialized;
        protected TBinding binding;
        
        // 缓存 TimelineDataManager 的引用，供 StringDataSource 等使用
        [NonSerialized] protected UnityEngine.Timeline.TimelineDataManager cachedDataManager;

        public override void OnGraphStop(Playable playable)
        {
            initialized = false;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (info.effectivePlayState == PlayState.Playing && binding != null)
            {
                OnTweenStarted(binding, this, playable, info);
            }
        }
        
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            // 自动初始化
            if (!initialized && playerData is TBinding target)
            {
                Initialize(target);
            }
            
            if (binding == null) return;
            
            var duration = playable.GetDuration();
            if (duration <= 0) return;
            
            var progress = (float)(playable.GetTime() / duration);
            
            // 应用当前进度的状态
            ApplyProgress(binding, progress);
        }
        
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (binding == null) return;
            
            // Clip 暂停或 Timeline 完成时，应用最终状态
            if (info.effectivePlayState == PlayState.Paused || playable.GetGraph().GetRootPlayable(0).IsDone())
            {
                // 先应用最终状态（progress = 1）
                ApplyFinalState(binding);
                
                // 再调用完成回调
                OnTweenFinished(binding, this, playable, info);
            }
        }

        internal void Initialize(TBinding playerData)
        {
            if (playerData == null) return;
            if (initialized) return;
            
            binding = playerData;
            
            // 尝试查找并缓存 TimelineDataManager
            if (cachedDataManager == null)
            {
                cachedDataManager = UnityEngine.Object.FindObjectOfType<UnityEngine.Timeline.TimelineDataManager>();
            }
            
            OnTweenInitialize(playerData);
            initialized = true;
        }

        public virtual void OnTweenInitialize(TBinding playerData) { }
        public virtual void OnTweenStarted(TBinding binding, TweenAnimationBehaviour<TBinding> behaviour, Playable playable, FrameData info) { }
        public virtual void OnTweenFinished(TBinding binding, TweenAnimationBehaviour<TBinding> behaviour, Playable playable, FrameData info) { }
        
        /// <summary>
        /// 应用指定进度的状态。子类应重写此方法以实现动画混合逻辑。
        /// </summary>
        public virtual void ApplyProgress(TBinding binding, float progress) { }
        
        /// <summary>
        /// 应用 progress = 1 时的最终状态。子类应重写此方法以确保动画完成时应用精确的最终值。
        /// </summary>
        public virtual void ApplyFinalState(TBinding binding) { }
    }
}