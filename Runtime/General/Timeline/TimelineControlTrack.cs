#if UNITY_EDITOR
using System.ComponentModel;
#endif
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace TweenPlayables
{
    [TrackBindingType(typeof(PlayableDirector))]
    [TrackClipType(typeof(TimelineControlClip))]
#if UNITY_EDITOR
    [DisplayName("Tween Playables/General/Control")]
#endif
    public sealed class TimelineControlTrack : TweenAnimationTrack<PlayableDirector, TweenTimelineControlMixerBehaviour, TweenTimelineControlBehaviour> { }
}