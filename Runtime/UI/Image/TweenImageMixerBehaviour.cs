namespace TweenPlayables
{
    /// <summary>
    /// 空的 Mixer，所有逻辑已移至 TweenImageBehaviour.ProcessFrame。
    /// </summary>
    public sealed class TweenImageMixerBehaviour : TweenAnimationMixerBehaviour<UISprite, TweenImageBehaviour>
    {
        // 保留空实现以满足 Timeline Track 的要求
    }
}