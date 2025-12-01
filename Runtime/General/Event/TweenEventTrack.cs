#if UNITY_EDITOR
using System.ComponentModel;
#endif
using UnityEngine;
using UnityEngine.Timeline;

namespace TweenPlayables
{
    [TrackClipType(typeof(TweenEventClip))]
    [TrackColor(0.6f, 0.15f, 0.15f)]
#if UNITY_EDITOR
    [DisplayName("Tween Playables/General/Event")]
#endif
    public sealed class TweenEventTrack : TweenAnimationTrack<Transform, TweenEventMixerBehaviour, TweenEventBehaviour> { }
}
