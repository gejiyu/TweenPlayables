using UnityEngine;
using UnityEngine.Playables;

namespace TweenPlayables
{
    /// <summary>
    /// 空的 Mixer。所有逻辑（包括初始化）已移至 TweenAnimationBehaviour.ProcessFrame。
    /// </summary>
    public abstract class TweenAnimationMixerBehaviour<TBinding, TAnimationBehaviour> : PlayableBehaviour
        where TBinding : Component
        where TAnimationBehaviour : TweenAnimationBehaviour<TBinding>, new()
    {
        // 完全空实现，仅用于满足 Timeline Track 的泛型要求
    }
}
