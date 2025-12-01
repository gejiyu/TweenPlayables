#if UNITY_EDITOR
using System.ComponentModel;
#endif
using UnityEngine;
using UnityEngine.Timeline;

namespace TweenPlayables
{
    [TrackClipType(typeof(TweenAnimationClip<TweenEventBehaviour>))]
#if UNITY_EDITOR
    [DisplayName("Tween Playables/General/Event")]
#endif
    public sealed class TweenEventTrack : TweenAnimationTrack<Transform, TweenEventMixerBehaviour, TweenEventBehaviour> { }
}
