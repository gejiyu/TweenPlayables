using System;
using UnityEngine;
using UnityEngine.Playables;

namespace TweenPlayables
{
    [Serializable]
    public sealed class TweenEventBehaviour : TweenAnimationBehaviour<Transform>
    {
        [SerializeField] StringChangeParameter startEventName;
        [SerializeField] StringChangeParameter endEventName;

        public ReadOnlyChangeParameter<string> StartEventName => startEventName;
        public ReadOnlyChangeParameter<string> EndEventName => endEventName;

        public override void OnTweenInitialize(Transform playerData)
        {
            startEventName.SetInitialValue(playerData, "");
            endEventName.SetInitialValue(playerData, "");
        }

        public override void OnTweenStarted(Transform binding, TweenAnimationBehaviour<Transform> behaviour, Playable playable, FrameData info)
        {
            if (startEventName.IsActive && startEventName.scrambleMode == ChangeScrambleMode.Start)
            {
                string eventName = startEventName.ChangeValue;
                if (!string.IsNullOrEmpty(eventName))
                {
                    GEventCtrl.TriggerEvent(eventName);
                    Debug.Log($"[TweenEvent] 触发开始事件: {eventName} on {binding.name}");
                }
            }
        }

        public override void OnTweenFinished(Transform binding, TweenAnimationBehaviour<Transform> behaviour, Playable playable, FrameData info)
        {
            if (endEventName.IsActive && endEventName.scrambleMode == ChangeScrambleMode.End)
            {
                string eventName = endEventName.ChangeValue;
                if (!string.IsNullOrEmpty(eventName))
                {
                    GEventCtrl.TriggerEvent(eventName);
                    Debug.Log($"[TweenEvent] 触发结束事件: {eventName} on {binding.name}");
                }
            }
        }
    }
}
