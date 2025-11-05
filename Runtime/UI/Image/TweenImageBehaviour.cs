using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace TweenPlayables
{
    [Serializable]
    public sealed class TweenImageBehaviour : TweenAnimationBehaviour<UISprite>
    {
        [SerializeField] ColorTweenParameter color;
        [SerializeField] FloatTweenParameter fillAmount;
        [SerializeField] StringChangeParameter spriteNameChange;

        public ReadOnlyTweenParameter<Color> Color => color;
        public ReadOnlyTweenParameter<float> FillAmount => fillAmount;
        public ReadOnlyChangeParameter<string> SpriteNameChange => spriteNameChange;

        public override void OnTweenInitialize(UISprite playerData)
        {
            color.SetInitialValue(playerData, playerData.color);
            fillAmount.SetInitialValue(playerData, playerData.fillAmount);
            spriteNameChange.SetInitialValue(playerData, playerData.spriteName);
        }

        public override void OnTweenStarted(UISprite binding, TweenAnimationBehaviour<UISprite> behaviour, Playable playable, FrameData info)
        {
            if (spriteNameChange.IsActive && spriteNameChange.scrambleMode == ChangeScrambleMode.Start)
                binding.spriteName = spriteNameChange.ChangeValue;
        }

        public override void OnTweenFinished(UISprite binding, TweenAnimationBehaviour<UISprite> behaviour, Playable playable, FrameData info)
        {
            if (spriteNameChange.IsActive && spriteNameChange.scrambleMode == ChangeScrambleMode.End)
                binding.spriteName = spriteNameChange.ChangeValue;
        }

        public override void ApplyProgress(UISprite binding, float progress)
        {
            // 应用当前进度的值
            if (color.IsActive)
                binding.color = color.Evaluate(binding, progress);
            if (fillAmount.IsActive)
                binding.fillAmount = fillAmount.Evaluate(binding, progress);
        }

        public override void ApplyFinalState(UISprite binding)
        {
            // 应用最终值 (progress = 1)
            if (color.IsActive)
                binding.color = color.Evaluate(binding, 1f);
            if (fillAmount.IsActive)
                binding.fillAmount = fillAmount.Evaluate(binding, 1f);
        }
    }
}