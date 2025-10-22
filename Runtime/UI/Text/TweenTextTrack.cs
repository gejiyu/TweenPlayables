#if UNITY_EDITOR
using System.ComponentModel;
#endif
using UnityEngine.UI;
using UnityEngine.Timeline;

namespace TweenPlayables
{
    [TrackBindingType(typeof(UILabel))]
    [TrackClipType(typeof(TweenTextClip))]
#if UNITY_EDITOR
    [DisplayName("Tween Playables/UI/Text")]
#endif
    public sealed class TweenTextTrack : TweenAnimationTrack<UILabel, TweenTextMixerBehaviour, TweenTextBehaviour> { }
}