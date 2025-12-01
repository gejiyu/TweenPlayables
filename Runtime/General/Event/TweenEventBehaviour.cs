using System;
using UnityEngine;
using UnityEngine.Playables;

namespace TweenPlayables
{
    [Serializable]
    public sealed class TweenEventBehaviour : TweenAnimationBehaviour<Transform>
    {
        [SerializeField] StringChangeParameter eventName;

        public ReadOnlyChangeParameter<string> EventName => eventName;

        public override void OnTweenInitialize(Transform playerData)
        {
            eventName.SetInitialValue(playerData, "");
        }

        public override void OnTweenStarted(Transform binding, TweenAnimationBehaviour<Transform> behaviour, Playable playable, FrameData info)
        {
            if (eventName.IsActive && eventName.scrambleMode == ChangeScrambleMode.Start)
            {
                string names = eventName.ChangeValue;
                if (!string.IsNullOrEmpty(names))
                {
                    TriggerEvents(names);
                }
            }
        }

        public override void OnTweenFinished(Transform binding, TweenAnimationBehaviour<Transform> behaviour, Playable playable, FrameData info)
        {
            if (eventName.IsActive && eventName.scrambleMode == ChangeScrambleMode.End)
            {
                string names = eventName.ChangeValue;
                if (!string.IsNullOrEmpty(names))
                {
                    TriggerEvents(names);
                }
            }
        }

        private void TriggerEvents(string eventNames)
        {
            string[] events = eventNames.Split(new char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string evt in events)
            {
                string trimmedEvent = evt.Trim();
                if (!string.IsNullOrEmpty(trimmedEvent))
                {
                    GEventCtrl.TriggerEvent(trimmedEvent);
                    Debug.Log($"[TweenEvent] 触发事件: {trimmedEvent}");
                }
            }
        }
    }
}
