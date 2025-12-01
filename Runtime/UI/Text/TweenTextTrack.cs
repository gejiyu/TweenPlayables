#if UNITY_EDITOR
using System.ComponentModel;
#endif
using UnityEngine;
using UnityEngine.Timeline;

namespace TweenPlayables
{
    [TrackBindingType(typeof(UILabel))]
    [TrackClipType(typeof(TweenAnimationClip<TweenTextBehaviour>))]
#if UNITY_EDITOR
    [DisplayName("Tween Playables/UI/Text")]
#endif
    public sealed class TweenTextTrack : TweenAnimationTrack<UILabel, TweenTextMixerBehaviour, TweenTextBehaviour> { }
}