using UnityEngine.UI;

namespace TweenPlayables
{
    public sealed class TweenTextMixerBehaviour : TweenAnimationMixerBehaviour<UILabel, TweenTextBehaviour>
    {
        readonly IntValueMixer fontSizeMixer = new();

        string textValue = null;
        string textChangeValue = null;
        string bitmapFontChangeValue = null;

        public override void Blend(UILabel binding, TweenTextBehaviour behaviour, float weight, float progress)
        {
            fontSizeMixer.TryBlend(behaviour.FontSize, binding, progress, weight);

            if (behaviour.Text.IsActive)
            {
                textValue = behaviour.Text.Evaluate(binding, progress);
            }
        }

        public override void Apply(UILabel binding)
        {
            fontSizeMixer.TryApplyAndClear(binding, (x, binding) => binding.fontSize = x);

            // Apply Text Tween first, then Text Change
            if (textValue != null)
            {
                binding.text = textValue;
                textValue = null;
            }
        }
    }
}