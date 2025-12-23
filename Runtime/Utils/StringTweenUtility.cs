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
                // 尝试解析为 ulong (uint64) 以支持大数值
                if (ulong.TryParse(startValue, out ulong startULong) && ulong.TryParse(endValue, out ulong endULong))
                {
                    ulong currentValue;
                    if (endULong >= startULong)
                    {
                        // 正向插值
                        currentValue = startULong + (ulong)((endULong - startULong) * t);
                    }
                    else
                    {
                        // 反向插值
                        currentValue = startULong - (ulong)((startULong - endULong) * t);
                    }
                    return currentValue.ToString();
                }
            }

            return null;
        }
    }
}