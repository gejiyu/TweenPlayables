using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace TweenPlayables
{
    [Serializable]
    public sealed class TweenTextBehaviour : TweenAnimationBehaviour<UILabel>
    {
        [SerializeField] IntTweenParameter fontSize;
        [SerializeField] StringTweenParameter text;
        [SerializeField] StringChangeParameter textChange;
        [SerializeField] StringChangeParameter bitmapFontChange;

        public ReadOnlyTweenParameter<int> FontSize => fontSize;
        public ReadOnlyTweenParameter<string> Text => text;
        public ReadOnlyChangeParameter<string> TextChange => textChange;
        public ReadOnlyChangeParameter<string> BitmapFontChange => bitmapFontChange;


        public override void OnTweenInitialize(UILabel playerData)
        {
            fontSize.SetInitialValue(playerData, playerData.fontSize);
            text.SetInitialValue(playerData, playerData.text);
            textChange.SetInitialValue(playerData, playerData.text);

            string fontName = "";
            if (playerData.bitmapFont != null)
            {
                INGUIFont uif = playerData.bitmapFont;
                while (uif.replacement != null)
                {
                    uif = uif.replacement;
                }
                if (uif is UnityEngine.Object obj)
                {
                    fontName = obj.name;
                }
            }

            bitmapFontChange.SetInitialValue(playerData, fontName);
        }

        public override void OnTweenStarted(UILabel binding, TweenAnimationBehaviour<UILabel> behaviour, Playable playable, FrameData info)
        {
            if (textChange.scrambleMode == ChangeScrambleMode.Start)
                binding.text = textChange.ChangeValue;
            if (bitmapFontChange.scrambleMode == ChangeScrambleMode.Start)
            {
                // 依赖GameFramework 加载
                // var fontAsset = GLoadResManager.Instance:LoadRes(bitmapFontChange.ChangeValue);
                // binding.bitmapFont = fontAsset as INGUIFont;
                // GLoadResManager.Instance:ReleaseResource(bitmapFontChange.ChangeValue);
            }
        }

        public override void OnTweenFinished(UILabel binding, TweenAnimationBehaviour<UILabel> behaviour, Playable playable, FrameData info)
        {
            if (textChange.scrambleMode == ChangeScrambleMode.End)
                binding.text = textChange.ChangeValue;
            if (bitmapFontChange.scrambleMode == ChangeScrambleMode.End)
            {   
                // 依赖GameFramework 加载
                // var fontAsset = GLoadResManager.Instance:LoadRes(bitmapFontChange.ChangeValue);
                // binding.bitmapFont = fontAsset as INGUIFont;
                // GLoadResManager.Instance:ReleaseResource(bitmapFontChange.ChangeValue);
            }
        }
    }
}