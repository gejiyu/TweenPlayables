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
        
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            var duration = playable.GetDuration();
            var count = playable.GetTime() + info.deltaTime;

            if ((info.effectivePlayState == PlayState.Paused && count > duration) || playable.GetGraph().GetRootPlayable(0).IsDone())
            {
                OnTweenFinished(binding, this, playable, info);
            }
        }

        internal void Initialize(TBinding playerData)
        {
            if (playerData == null) return;
            if (initialized) return;
            
            binding = playerData;
            OnTweenInitialize(playerData);
            initialized = true;
        }

        public virtual void OnTweenInitialize(TBinding playerData) { }
        public virtual void OnTweenStarted(TBinding binding, TweenAnimationBehaviour<TBinding> behaviour, Playable playable, FrameData info) { }
        public virtual void OnTweenFinished(TBinding binding, TweenAnimationBehaviour<TBinding> behaviour, Playable playable, FrameData info) { }
    }
}