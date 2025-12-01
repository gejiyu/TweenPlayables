#if UNITY_EDITOR
using System.ComponentModel;
#endif
using UnityEngine;
using UnityEngine.Timeline;

namespace TweenPlayables
{
    [TrackBindingType(typeof(UnityEngine.UI.Image))]
    [TrackClipType(typeof(TweenAnimationClip<TweenImageBehaviour>))]
#if UNITY_EDITOR
    [DisplayName("Tween Playables/UI/Image")]
#endif
    public sealed class TweenImageTrack : TweenAnimationTrack<UnityEngine.UI.Image, TweenImageMixerBehaviour, TweenImageBehaviour> { }
}