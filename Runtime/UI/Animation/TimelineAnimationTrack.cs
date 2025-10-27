using System;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

#if UNITY_EDITOR
using System.ComponentModel;
#endif

namespace TweenPlayables
{
    /// <summary>
    /// Timeline Animation track that inherits from UIAnimationTrack
    /// Maintains the same functionality as UIAnimationTrack but with Timeline prefix
    /// </summary>
    [Serializable]
    [TrackBindingType(typeof(Animator))]
    [TrackClipType(typeof(TimelineAnimationPlayableAsset))]
#if UNITY_EDITOR
    [DisplayName("Tween Playables/UI/Animation")]
#endif
    public sealed class TimelineAnimationTrack : AnimationTrack
    {

    }
}