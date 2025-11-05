using UnityEngine.Playables;

namespace TweenPlayables
{
    /// <summary>
    /// 空的 Mixer，所有逻辑已移至 TweenTimelineControlBehaviour.ProcessFrame。
    /// </summary>
    public sealed class TweenTimelineControlMixerBehaviour : TweenAnimationMixerBehaviour<PlayableDirector, TweenTimelineControlBehaviour>
    {
        // 保留空实现以满足 Timeline Track 的要求
    }
}