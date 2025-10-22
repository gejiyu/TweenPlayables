using System;
using Random = UnityEngine.Random;

namespace TweenPlayables
{
    public static class StringTweenUtility
    {
        public static string TweenText(string startValue, string endValue, float t, ScrambleMode scrambleMode = ScrambleMode.None, string customScrambleChars = null)
        {
            // 将字符串转换为整数进行线性插值
            if (scrambleMode == ScrambleMode.None)
            {
                if (int.TryParse(startValue, out int startInt) && int.TryParse(endValue, out int endInt))
                {
                    int currentValue = (int)(startInt + (endInt - startInt) * t);
                    return currentValue.ToString();
                }
            }
            
            // 如果转换失败，返回空字符串或原始值
            return startValue;
        }
    }
}