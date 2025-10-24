using System;
using UnityEngine;

namespace TweenPlayables
{
    public static class ValueMixerExtensions
    {
        public static bool TryBlend<T>(this ValueMixer<T> mixer, ReadOnlyTweenParameter<T> parameter, object binding, float progress, float weight)
        {
            if (parameter.IsActive)
            {
                mixer.Blend(parameter.Evaluate(binding, progress), weight);
                return true;
            }

            return false;
        }

        public static bool TryApplyAndClear<T>(this ValueMixer<T> mixer, Action<T> action)
        {
            if (mixer.HasValue)
            {
                action(mixer.Value);
                mixer.Clear();
                return true;
            }

            return false;
        }

        public static bool TryApplyAndClear<T, TBinding>(this ValueMixer<T> mixer, TBinding target, Action<T, TBinding> action)
        {
            if (mixer.HasValue)
            {
                action(mixer.Value, target);
                mixer.Clear();
                return true;
            }

            return false;
        }

        public static bool TryBlend<T>(this ValueMixer<T> mixer, ReadOnlyChangeParameter<T> parameter, object binding, float progress)
        {
            if (parameter.IsActive)
            {
                T value = parameter.Evaluate(binding, progress);
                
                // 检查是否为无效值（针对不同类型的无效值判断）
                if (IsValidValue(value))
                {
                    mixer.Blend(value, 1);
                    return true;
                }
            }

            return false;
        }
        
        private static bool IsValidValue<T>(T value)
        {
            // 针对不同类型检查无效值
            if (value is Vector3 vec3)
            {
                // 检查各个分量是否为正无穷
                return !(float.IsPositiveInfinity(vec3.x) && 
                         float.IsPositiveInfinity(vec3.y) && 
                         float.IsPositiveInfinity(vec3.z));
            }
            else if (value is Vector2 vec2)
            {
                // 检查各个分量是否为正无穷
                return !(float.IsPositiveInfinity(vec2.x) && 
                         float.IsPositiveInfinity(vec2.y));
            }
            else if (value is float f)
            {
                return !float.IsPositiveInfinity(f);
            }
            else if (value is string str)
            {
                return str != null;
            }
            
            // 对于其他类型，默认认为有效
            return true;
        }
    }
}