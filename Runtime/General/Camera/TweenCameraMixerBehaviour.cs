using UnityEngine;

namespace TweenPlayables
{
    /// <summary>
    /// 空的 Mixer，所有逻辑已移至 TweenCameraBehaviour.ProcessFrame。
    /// </summary>
    public sealed class TweenCameraMixerBehaviour : TweenAnimationMixerBehaviour<Camera, TweenCameraBehaviour>
    {
        // 保留空实现以满足 Timeline Track 的要求
    }
}