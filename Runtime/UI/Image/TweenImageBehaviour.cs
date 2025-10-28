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

    }
}