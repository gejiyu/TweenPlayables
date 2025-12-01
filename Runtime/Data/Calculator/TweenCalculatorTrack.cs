#if UNITY_EDITOR
using System.ComponentModel;
#endif
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace TweenPlayables
{
    [TrackBindingType(typeof(TimelineDataManager))]
    [TrackClipType(typeof(TweenCalculatorClip))]
    [TrackColor(0.4f, 0.4f, 0.5f)]
#if UNITY_EDITOR
    [DisplayName("Tween Playables/Data/Calculator")]
#endif
    public sealed class TweenCalculatorTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<TweenCalculatorBehaviour>.Create(graph, inputCount);
        }
    }
}
