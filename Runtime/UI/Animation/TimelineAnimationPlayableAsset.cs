using System;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace TweenPlayables
{
    /// <summary>
    /// Timeline Animation asset that inherits from Timeline's AnimationPlayableAsset
    /// Maintains the same functionality as UIAnimationPlayableAsset but with Timeline prefix
    /// </summary>
    [Serializable]
    public sealed class TimelineAnimationPlayableAsset : AnimationPlayableAsset
    {}
}