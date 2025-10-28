using System;
using Random = UnityEngine.Random;

namespace TweenPlayables
{
    public static class StringTweenUtility
    {
        public static string TweenText(string startValue, string endValue, float t, ScrambleMode scrambleMode = ScrambleMode.Tween, string customScrambleChars = null)
        {
            // 将字符串转换为整数进行线性插值
            if (scrambleMode == ScrambleMode.Tween)
            {
                if (int.TryParse(startValue, out int startInt) && int.TryParse(endValue, out int endInt))
                {
                    int currentValue = (int)(startInt + (endInt - startInt) * t);
                    return currentValue.ToString();
                }
            }

            return null;
        }
    }
}