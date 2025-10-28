using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TweenPlayables
{
    /// <summary>
    /// 动画条目，包含备注和动画名称
    /// </summary>
    [Serializable]
    public class AnimationEntry
    {
        [HorizontalGroup]
        [LabelText("备注信息")]
        [LabelWidth(60)]
        [ValidateInput("ValidateComment", "备注不能为空")]
        public string comment = "";
        
        [HorizontalGroup]
        [LabelText("动画名称")]
        [LabelWidth(60)]
        [ValidateInput("ValidateAnimationName", "动画名称不能为空")]
        public string animationName = "";

        // 验证方法
        private bool ValidateComment(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private bool ValidateAnimationName(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
        
        public AnimationEntry() { }
        
        public AnimationEntry(string comment, string animationName)
        {
            this.comment = comment;
            this.animationName = animationName;
        }
    }

    /// <summary>
    /// Timeline preset animation list configuration class for storing selectable animation state names with comments
    /// </summary>
    [CreateAssetMenu(fileName = "TimelinePresetAnimationList", menuName = "TweenPlayables/Timeline Preset Animation List")]
    [Serializable]
    [InfoBox("Timeline 动画预设列表配置\n用于存储可选择的动画状态名称和对应的备注信息")]
    public class TimelinePresetAnimationList : ScriptableObject
    {
        [Title("动画条目管理", "管理动画名称与备注的映射关系", TitleAlignments.Centered)]
        [TableList(ShowIndexLabels = true, DrawScrollView = true, MaxScrollViewHeight = 400)]
        [PropertySpace(SpaceBefore = 10)]
        [InfoBox("双击条目可以编辑，右键可以删除", InfoMessageType.Info)]
        [SerializeField]
        public List<AnimationEntry> animationEntries = new List<AnimationEntry>()
        {
        };

        [PropertySpace(SpaceBefore = 10)]
        [HorizontalGroup("Actions", Title = "快速操作")]
        [Button("添加新条目", ButtonSizes.Medium)]
        private void AddNewEntry()
        {
            animationEntries.Add(new AnimationEntry("新备注", "新动画名称"));
        }

        [PropertySpace(SpaceBefore = 10)]
        [HorizontalGroup("Actions")]
        [Button("清空列表", ButtonSizes.Medium)]
        private void ClearAllEntries()
        {
            if (UnityEditor.EditorUtility.DisplayDialog("确认清空", "确定要清空所有动画条目吗？", "确定", "取消"))
            {
                animationEntries.Clear();
            }
        }

        [PropertySpace(SpaceBefore = 10)]
        [HorizontalGroup("Actions")]
        [Button("排序条目", ButtonSizes.Medium)]
        private void SortEntries()
        {
            animationEntries.Sort((a, b) => string.Compare(a.comment, b.comment, StringComparison.OrdinalIgnoreCase));
        }

        [Title("预览信息")]
        [ShowInInspector, ReadOnly]
        [LabelText("条目数量")]
        private int EntryCount => animationEntries.Count;

        /// <summary>
        /// 获取所有备注信息（用于下拉框显示）
        /// </summary>
        public string[] GetComments()
        {
            return animationEntries.Select(entry => entry.comment).ToArray();
        }

        /// <summary>
        /// 获取所有动画名称
        /// </summary>
        public string[] GetAnimationNames()
        {
            return animationEntries.Select(entry => entry.animationName).ToArray();
        }

        /// <summary>
        /// 根据备注获取动画名称
        /// </summary>
        public string GetAnimationNameByComment(string comment)
        {
            var entry = animationEntries.FirstOrDefault(e => e.comment == comment);
            return entry?.animationName ?? "";
        }

        /// <summary>
        /// 根据动画名称获取备注
        /// </summary>
        public string GetCommentByAnimationName(string animationName)
        {
            var entry = animationEntries.FirstOrDefault(e => e.animationName == animationName);
            return entry?.comment ?? "";
        }

        /// <summary>
        /// 添加动画条目
        /// </summary>
        public void AddAnimationEntry(string comment, string animationName)
        {
            if (!string.IsNullOrEmpty(comment) && !string.IsNullOrEmpty(animationName))
            {
                // 检查是否已存在相同的备注
                if (!animationEntries.Any(e => e.comment == comment))
                {
                    animationEntries.Add(new AnimationEntry(comment, animationName));
                }
            }
        }

        /// <summary>
        /// 移除动画条目（通过备注）
        /// </summary>
        public void RemoveAnimationEntryByComment(string comment)
        {
            animationEntries.RemoveAll(e => e.comment == comment);
        }

        /// <summary>
        /// 移除动画条目（通过动画名称）
        /// </summary>
        public void RemoveAnimationEntryByName(string animationName)
        {
            animationEntries.RemoveAll(e => e.animationName == animationName);
        }

        /// <summary>
        /// 检查备注是否存在
        /// </summary>
        public bool ContainsComment(string comment)
        {
            return animationEntries.Any(e => e.comment == comment);
        }

        /// <summary>
        /// 检查动画名称是否存在
        /// </summary>
        public bool ContainsAnimationName(string animationName)
        {
            return animationEntries.Any(e => e.animationName == animationName);
        }
    }
}