using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Timeline;

namespace TweenPlayables.Editor
{
    /// <summary>
    /// Timeline editor for TimelineAnimationTrack
    /// </summary>
    [CustomTimelineEditor(typeof(TimelineAnimationTrack))]
    public sealed class TimelineAnimationTrackEditor : TweenAnimationTrackEditor
    {
        public override Color TrackColor => Styles.TimelineAnimationColor;
        public override Texture2D TrackIcon => Styles.TimelineAnimationIcon;
        public override string DefaultTrackName => "Timeline Animation Track";
    }

    /// <summary>
    /// Timeline editor for TimelineAnimationPlayableAsset
    /// </summary>
    [CustomTimelineEditor(typeof(TimelineAnimationPlayableAsset))]
    public sealed class TimelineAnimationClipEditor : TweenAnimationClipEditor
    {
        public override string DefaultClipName => "Timeline Animation";
        public override Color ClipColor => Styles.TimelineAnimationColor;
        public override Texture2D ClipIcon => Styles.TimelineAnimationIcon;
    }


    
    /// <summary>
    /// Custom inspector for TimelineAnimationPlayableAsset
    /// </summary>
    [CustomEditor(typeof(TimelineAnimationPlayableAsset))]
    public sealed class TimelineAnimationPlayableAssetEditor : UnityEditor.Editor
    {
        SerializedProperty clipProperty;
        SerializedProperty applyFootIKProperty;
        SerializedProperty loopProperty;
        
        private string[] availableAnimationNames;
        private int selectedIndex = 0;
        private TimelinePresetAnimationList timelinePresetAnimationList;
        private string currentPlayName = "";
        
        void OnEnable()
        {
            clipProperty = serializedObject.FindProperty("m_Clip");
            applyFootIKProperty = serializedObject.FindProperty("m_ApplyFootIK");
            loopProperty = serializedObject.FindProperty("m_Loop");
            
            // 初始化动画名称列表
            LoadAnimationNameList();
        }
        
        void LoadAnimationNameList()
        {
            // 尝试加载项目中的TimelinePresetAnimationList资源
            string[] guids = AssetDatabase.FindAssets("t:TimelinePresetAnimationList");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                timelinePresetAnimationList = AssetDatabase.LoadAssetAtPath<TimelinePresetAnimationList>(path);
            }
            
            if (timelinePresetAnimationList != null)
            {
                // 从资源文件中获取备注信息列表（用于下拉框显示）
                var commentsList = new List<string> { "None" };
                commentsList.AddRange(timelinePresetAnimationList.GetComments());
                availableAnimationNames = commentsList.ToArray();
            }
            else
            {
                // 如果没有找到资源文件，使用默认列表
                availableAnimationNames = new string[] 
                { 
                    "None"
                };
            }
            
            // 查找当前选中的索引
            UpdateSelectedIndexFromClip();
        }
        
        /// <summary>
        /// 根据当前Animation Clip或currentPlayName更新选中的索引
        /// </summary>
        void UpdateSelectedIndexFromClip()
        {
            selectedIndex = 0; // 默认选择 "None"
            
            // 优先检查Animation Clip
            if (clipProperty != null && clipProperty.objectReferenceValue is AnimationClip clip)
            {
                string clipName = clip.name;
                if (timelinePresetAnimationList != null)
                {
                    string comment = timelinePresetAnimationList.GetCommentByAnimationName(clipName);
                    if (!string.IsNullOrEmpty(comment))
                    {
                        selectedIndex = System.Array.IndexOf(availableAnimationNames, comment);
                        currentPlayName = clipName; // 同步更新currentPlayName
                    }
                }
            }
            // 如果没有Animation Clip，则根据currentPlayName查找
            else if (!string.IsNullOrEmpty(currentPlayName) && timelinePresetAnimationList != null)
            {
                string currentComment = timelinePresetAnimationList.GetCommentByAnimationName(currentPlayName);
                if (!string.IsNullOrEmpty(currentComment))
                {
                    selectedIndex = System.Array.IndexOf(availableAnimationNames, currentComment);
                }
            }
            
            if (selectedIndex < 0) selectedIndex = 0;
        }
        
        /// <summary>
        /// 根据动画名称查找项目中的 AnimationClip
        /// </summary>
        AnimationClip FindAnimationClipByName(string animationName)
        {
            if (string.IsNullOrEmpty(animationName))
                return null;
            
            // 在项目中搜索 AnimationClip 资源
            string[] guids = AssetDatabase.FindAssets($"{animationName} t:AnimationClip");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                
                if (clip != null && clip.name.Equals(animationName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return clip;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 创建TimelinePresetAnimationList资源文件
        /// </summary>
        void CreateAnimationNameListAsset()
        {
            // 创建新的TimelinePresetAnimationList资源
            TimelinePresetAnimationList newList = CreateInstance<TimelinePresetAnimationList>();
            
            string path = "Assets/Editor/TimelinePresetAnimationList.asset";
            if (!System.IO.Directory.Exists("Assets/Editor"))
            {
                System.IO.Directory.CreateDirectory("Assets/Editor");
            }
            
            AssetDatabase.CreateAsset(newList, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // 重新加载列表
            LoadAnimationNameList();
            
            // 选择新创建的资源
            Selection.activeObject = newList;
            EditorGUIUtility.PingObject(newList);
            
            Debug.Log($"已创建TimelinePresetAnimationList资源: {path}");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Timeline Animation Asset", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Always show these fields first
            EditorGUILayout.PropertyField(clipProperty, new GUIContent("Animation Clip", "Unity AnimationClip to play. Leave empty to use Play Name."));
            
            if (applyFootIKProperty != null)
                EditorGUILayout.PropertyField(applyFootIKProperty, new GUIContent("Apply Foot IK"));
            
            if (loopProperty != null)
                EditorGUILayout.PropertyField(loopProperty, new GUIContent("Loop"));
            

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("预设动画", EditorStyles.boldLabel);
            
            // 更新选中索引（基于当前Animation Clip）
            UpdateSelectedIndexFromClip();
            
            // 显示动画名称下拉框
            if (availableAnimationNames != null && availableAnimationNames.Length > 0)
            {
                EditorGUI.BeginChangeCheck();
                
                // 创建下拉框
                selectedIndex = EditorGUILayout.Popup("Play Name", selectedIndex, availableAnimationNames);
                
                if (EditorGUI.EndChangeCheck())
                {
                    // 当选择改变时，更新当前动画名称
                    if (selectedIndex >= 0 && selectedIndex < availableAnimationNames.Length)
                    {
                        string selectedComment = availableAnimationNames[selectedIndex];
                        if (selectedComment == "None")
                        {
                            // 清除动画名称和Animation Clip
                            currentPlayName = "";
                            if (clipProperty != null)
                                clipProperty.objectReferenceValue = null;
                        }
                        else
                        {
                            // 根据选择的备注获取实际的动画名称
                            string actualAnimationName = timelinePresetAnimationList.GetAnimationNameByComment(selectedComment);
                            currentPlayName = actualAnimationName;
                            
                            // 根据动画名称查找对应的 AnimationClip
                            AnimationClip foundClip = FindAnimationClipByName(actualAnimationName);
                            if (foundClip != null && clipProperty != null)
                            {
                                clipProperty.objectReferenceValue = foundClip;
                            }
                        }
                    }
                }
                
                // 显示当前选择的动画信息
                EditorGUILayout.LabelField("Current Animation:", currentPlayName);
            }
            else
            {
                // 备用方案：如果没有可用的动画名称列表，显示文本框
                currentPlayName = EditorGUILayout.TextField(new GUIContent("Play Name", "Animation state name to play when no AnimationClip is assigned."), currentPlayName);
            }
            
            // 动画名称列表管理
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("动画列表管理", EditorStyles.boldLabel);
            
            if (timelinePresetAnimationList != null)
            {
                EditorGUILayout.LabelField($"当前使用: {timelinePresetAnimationList.name}", EditorStyles.miniLabel);
                
                if (GUILayout.Button("编辑动画列表"))
                {
                    Selection.activeObject = timelinePresetAnimationList;
                    EditorGUIUtility.PingObject(timelinePresetAnimationList);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("未找到TimelinePresetAnimationList资源，将使用默认动画列表", MessageType.Info);
                
                if (GUILayout.Button("创建TimelinePresetAnimationList资源"))
                {
                    CreateAnimationNameListAsset();
                }
            }

            
            serializedObject.ApplyModifiedProperties();
        }
        

    }
}