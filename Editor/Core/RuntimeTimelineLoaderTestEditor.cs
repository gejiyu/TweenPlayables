using UnityEngine;
using UnityEditor;

namespace TweenPlayables.Editor
{
    [CustomEditor(typeof(RuntimeTimelineLoaderTest))]
    public class RuntimeTimelineLoaderTestEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            RuntimeTimelineLoaderTest test = (RuntimeTimelineLoaderTest)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("运行时操作", EditorStyles.boldLabel);

            // 只在运行时启用按钮
            GUI.enabled = Application.isPlaying;

            EditorGUILayout.BeginHorizontal();
            
            // 加载并播放按钮
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("▶ 加载并播放", GUILayout.Height(30)))
            {
                test.LoadAndPlay();
            }

            // 停止按钮
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("■ 停止", GUILayout.Height(30)))
            {
                test.StopPlayback();
            }

            // 清理按钮
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("✕ 清理", GUILayout.Height(30)))
            {
                test.Cleanup();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("请先进入播放模式后再使用按钮", MessageType.Info);
            }
        }
    }
}
