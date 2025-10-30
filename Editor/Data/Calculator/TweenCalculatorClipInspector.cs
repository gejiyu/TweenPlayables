using UnityEngine;
using UnityEditor;
using UnityEditor.Timeline;
using TweenPlayables;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

namespace TweenPlayables.Editor
{
    [CustomEditor(typeof(TweenCalculatorClip))]
    public class TweenCalculatorClipInspector : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Draw a nice header
            SirenixEditorGUI.Title("Calculator Configuration", "Configure calculation operations for Timeline", TextAlignment.Center, true);
            
            EditorGUILayout.Space(5);
            
            // Draw calculator configuration using helper
            CalculatorInspectorHelper.DrawCalculatorGUI(serializedObject);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
