using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using TweenPlayables;
using UnityEditor.Timeline; // For StringDataSourceInspectorHelper
using Sirenix.OdinInspector.Editor; // If we want to keep consistency, though standard Editor is fine if we override OnInspectorGUI completely
using UnityEngine.Timeline;
using System.Collections.Generic;
using System.Text;

namespace TweenPlayables.Editor
{
    [CustomEditor(typeof(TweenStringConcatClip))]
    public class TweenStringConcatClipInspector : UnityEditor.Editor
    {
        private ReorderableList m_ItemsList;
        private SerializedProperty m_NodeProperty;
        private SerializedProperty m_ItemsProperty;
        private SerializedProperty m_IsActiveProperty;
        private SerializedProperty m_ResultDataKeyProperty;

        private void OnEnable()
        {
            m_NodeProperty = serializedObject.FindProperty("node");
            if (m_NodeProperty != null)
            {
                m_ItemsProperty = m_NodeProperty.FindPropertyRelative("items");
                m_IsActiveProperty = m_NodeProperty.FindPropertyRelative("isActive");
                m_ResultDataKeyProperty = m_NodeProperty.FindPropertyRelative("resultDataKey");

                m_ItemsList = new ReorderableList(serializedObject, m_ItemsProperty, true, true, true, true);
                
                m_ItemsList.drawHeaderCallback = (Rect rect) => {
                    EditorGUI.LabelField(rect, "String Items");
                };

                m_ItemsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    if (index >= m_ItemsProperty.arraySize) return;
                    
                    var element = m_ItemsProperty.GetArrayElementAtIndex(index);
                    // Add a little padding
                    rect.y += 2;
                    rect.height -= 4;
                    
                    StringDataSourceInspectorHelper.DrawStringDataSourceGUI(rect, element, new GUIContent($"Item {index}"));
                };

                m_ItemsList.elementHeightCallback = (int index) => {
                    if (index >= m_ItemsProperty.arraySize) return 0;
                    
                    var element = m_ItemsProperty.GetArrayElementAtIndex(index);
                    return StringDataSourceInspectorHelper.GetStringDataSourceHeight(element) + 4; // +4 for padding
                };
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (m_NodeProperty != null)
            {
                EditorGUILayout.PropertyField(m_IsActiveProperty);
                
                EditorGUILayout.Space();
                if (m_ItemsList != null)
                {
                    m_ItemsList.DoLayoutList();
                }
                
                EditorGUILayout.Space();
                
                // Try to get TimelineDataManager from track binding
                TimelineDataManager dataManager = GetTimelineDataManager();
                
                DrawResultKeyDropdown(m_ResultDataKeyProperty, dataManager);

                // Draw Preview
                DrawPreviewResult(dataManager);
            }
            else
            {
                base.OnInspectorGUI();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPreviewResult(TimelineDataManager dataManager)
        {
            var clip = target as TweenStringConcatClip;
            if (clip == null || clip.node.items == null) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Result Preview", EditorStyles.boldLabel);

            StringBuilder sb = new StringBuilder();
            
            foreach (var item in clip.node.items)
            {
                string val = StringDataSourceAccessor.GetString(item, dataManager);
                
                // If value is empty, try to provide a helpful placeholder for the preview
                if (string.IsNullOrEmpty(val))
                {
                    if (item.dataType == DataSourceType.DataManager && !string.IsNullOrEmpty(item.dataKey))
                    {
                        // Check if key exists in DataManager
                        bool keyExists = false;
                        if (dataManager != null)
                        {
                            var field = typeof(TimelineDataManager).GetField("dataStorage", 
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (field != null)
                            {
                                var dataStorage = field.GetValue(dataManager) as Dictionary<string, object>;
                                if (dataStorage != null && dataStorage.ContainsKey(item.dataKey))
                                {
                                    keyExists = true;
                                }
                            }
                        }
                        
                        if (!keyExists)
                        {
                            val = $"<{item.dataKey}>";
                        }
                    }
                    else if (item.dataType == DataSourceType.Protobuf)
                    {
                        val = $"<Protobuf:{item.protobufField}>";
                    }
                }
                
                sb.Append(val);
            }

            // Use a TextArea to display the result so it can handle long strings
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.fontSize = 12;
            style.wordWrap = true;
            style.richText = true;
            
            GUILayout.Label(sb.ToString(), style);
        }

        private TimelineDataManager GetTimelineDataManager()
        {
            var timelineAsset = TimelineEditor.inspectedAsset;
            if (timelineAsset != null)
            {
                foreach (var track in timelineAsset.GetOutputTracks())
                {
                    if (track is TweenStringConcatTrack concatTrack)
                    {
                        foreach (var clip in concatTrack.GetClips())
                        {
                            if (clip.asset == target)
                            {
                                var director = TimelineEditor.inspectedDirector;
                                if (director != null)
                                {
                                    return director.GetGenericBinding(concatTrack) as TimelineDataManager;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void DrawResultKeyDropdown(SerializedProperty dataKeyProperty, TimelineDataManager dataManager)
        {
            if (dataManager != null)
            {
                var availableKeys = GetAvailableDataKeys(dataManager);
                
                // Add "New Key..." option at the beginning
                var displayKeys = new string[availableKeys.Length + 1];
                displayKeys[0] = "<New Key...>";
                System.Array.Copy(availableKeys, 0, displayKeys, 1, availableKeys.Length);
                
                var currentKey = dataKeyProperty.stringValue;
                var currentIndex = System.Array.IndexOf(availableKeys, currentKey);
                
                // If current key is not found in available keys, it's a new key
                var displayIndex = currentIndex >= 0 ? currentIndex + 1 : 0;
                
                EditorGUILayout.BeginHorizontal();
                var newDisplayIndex = EditorGUILayout.Popup("Result Data Key", displayIndex, displayKeys);
                
                // Refresh button
                if (GUILayout.Button("↻", GUILayout.Width(25)))
                {
                    EditorUtility.SetDirty(dataManager);
                }
                EditorGUILayout.EndHorizontal();
                
                // Handle selection change
                if (newDisplayIndex != displayIndex)
                {
                    if (newDisplayIndex == 0)
                    {
                        // User selected "New Key..." - clear the current value to allow manual input
                        if (string.IsNullOrEmpty(currentKey) || currentIndex >= 0)
                        {
                            dataKeyProperty.stringValue = "";
                        }
                    }
                    else if (newDisplayIndex > 0 && newDisplayIndex <= availableKeys.Length)
                    {
                        // User selected an existing key
                        dataKeyProperty.stringValue = availableKeys[newDisplayIndex - 1];
                    }
                }
                
                // If "New Key..." is selected (displayIndex == 0), show text field
                if (newDisplayIndex == 0)
                {
                    EditorGUI.indentLevel++;
                    dataKeyProperty.stringValue = EditorGUILayout.DelayedTextField("New Key Name", dataKeyProperty.stringValue);
                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                EditorGUILayout.PropertyField(dataKeyProperty, new GUIContent("Result Data Key"));
                EditorGUILayout.HelpBox("No TimelineDataManager found in scene.", MessageType.Warning);
            }
        }

        private string[] GetAvailableDataKeys(TimelineDataManager dataManager)
        {
            if (dataManager == null)
                return new string[0];
            
            var field = typeof(TimelineDataManager).GetField("dataStorage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                var dataStorage = field.GetValue(dataManager) as Dictionary<string, object>;
                if (dataStorage != null)
                {
                    var keys = new List<string>(dataStorage.Keys);
                    keys.Sort();
                    return keys.ToArray();
                }
            }
            
            return new string[0];
        }
    }
}
