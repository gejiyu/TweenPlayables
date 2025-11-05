using UnityEngine;

namespace TweenPlayables
{
    /// <summary>
    /// 空的 Mixer，所有逻辑已移至 TweenTransformBehaviour.ProcessFrame。
    /// </summary>
    public sealed class TweenTransformMixerBehaviour : TweenAnimationMixerBehaviour<Transform, TweenTransformBehaviour>
    {
        // 保留空实现以满足 Timeline Track 的要求
    }
}
