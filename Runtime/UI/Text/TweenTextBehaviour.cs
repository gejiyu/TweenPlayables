using System;
using UnityEngine;
using UnityEngine.UI;

namespace TweenPlayables
{
    [Serializable]
    public sealed class TweenTextBehaviour : TweenAnimationBehaviour<UILabel>
    {
        [SerializeField] IntTweenParameter fontSize;
        [SerializeField] StringTweenParameter text;
        [SerializeField] StringChangeParameter textChange;

        public ReadOnlyTweenParameter<int> FontSize => fontSize;
        public ReadOnlyTweenParameter<string> Text => text;
        public ReadOnlyChangeParameter<string> TextChange => textChange;

        public override void OnTweenInitialize(UILabel playerData)
        {
            fontSize.SetInitialValue(playerData, playerData.fontSize);
            text.SetInitialValue(playerData, playerData.text);
        }
    }
}