#if UNITY_EDITOR
using System.ComponentModel;
#endif
using UnityEngine;
using UnityEngine.Timeline;

namespace TweenPlayables
{
    [TrackBindingType(typeof(UISprite))]
    [TrackClipType(typeof(TweenImageClip))]
#if UNITY_EDITOR
    [DisplayName("Tween Playables/UI/Image")]
#endif
    public sealed class TweenImageTrack : TweenAnimationTrack<UISprite, TweenImageMixerBehaviour, TweenImageBehaviour> { }
}