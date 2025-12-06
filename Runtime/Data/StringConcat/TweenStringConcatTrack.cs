#if UNITY_EDITOR
using System.ComponentModel;
#endif
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace TweenPlayables
{
    /// <summary>
    /// Timeline 字符串拼接轨道
    /// </summary>
    [TrackBindingType(typeof(TimelineDataManager))]
    [TrackClipType(typeof(TweenStringConcatClip))]
    [TrackColor(0.1f, 0.5f, 0.9f)]
#if UNITY_EDITOR
    [DisplayName("Tween Playables/Data/String Concat")]
#endif
    public sealed class TweenStringConcatTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<TweenStringConcatBehaviour>.Create(graph, inputCount);
        }
    }
}
