namespace TweenPlayables
{
    /// <summary>
    /// 空的 Mixer，所有逻辑已移至 TweenTextBehaviour.ProcessFrame。
    /// </summary>
    public sealed class TweenTextMixerBehaviour : TweenAnimationMixerBehaviour<UILabel, TweenTextBehaviour>
    {
        // 保留空实现以满足 Timeline Track 的要求
    }
}