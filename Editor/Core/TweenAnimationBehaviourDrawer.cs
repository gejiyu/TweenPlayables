using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Timeline;

namespace TweenPlayables.Editor
{
    public abstract class TweenAnimationBehaviourDrawer : PropertyDrawer
    {
        protected abstract IEnumerable<string> GetPropertyNames();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConditionalPlayableAssetInspectorHelper.DrawExecutionConditionGUI(property.serializedObject);
            position.y += 7f;

            // 1. 绘制原有的 Tween 参数
            foreach (var propertyName in GetPropertyNames())
            {
                var prop = property.FindPropertyRelative(propertyName);
                if (prop != null)
                {
                    GUIHelper.Field(ref position, prop);
                    position.y += 2f;
                }
            }
        }

        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = 7f;
            
            // 原有参数高度
            foreach (var propertyName in GetPropertyNames())
            {
                var prop = property.FindPropertyRelative(propertyName);
                if (prop != null)
                {
                    height += EditorGUI.GetPropertyHeight(prop);
                    height += EditorGUIUtility.standardVerticalSpacing; // GUIHelper.Field 添加的间距
                    height += 2f; // 额外的间距
                }
            }

            return height;
        }
    }
}