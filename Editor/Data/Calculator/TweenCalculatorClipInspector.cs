using UnityEngine;
using UnityEditor;
using UnityEditor.Timeline;
using TweenPlayables;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine.Timeline;

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
            
            // Try to get TimelineDataManager from track binding
            TimelineDataManager dataManager = null;
            
            // Get the TimelineEditor's inspected asset
            var timelineAsset = TimelineEditor.inspectedAsset;
            if (timelineAsset != null)
            {
                // Find the track that contains this clip
                foreach (var track in timelineAsset.GetOutputTracks())
                {
                    if (track is TweenCalculatorTrack calculatorTrack)
                    {
                        foreach (var clip in calculatorTrack.GetClips())
                        {
                            if (clip.asset == target)
                            {
                                // Found our clip, now get the binding
                                var director = TimelineEditor.inspectedDirector;
                                if (director != null)
                                {
                                    dataManager = director.GetGenericBinding(calculatorTrack) as TimelineDataManager;
                                }
                                break;
                            }
                        }
                        if (dataManager != null) break;
                    }
                }
            }
            
            // Draw calculator configuration using helper
            CalculatorInspectorHelper.DrawCalculatorGUI(serializedObject, dataManager);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
