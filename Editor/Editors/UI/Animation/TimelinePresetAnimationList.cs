using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TweenPlayables
{
    /// <summary>
    /// 动画条目，包含备注和动画名称
    /// </summary>
    [Serializable]
    public class AnimationEntry
    {
        [Header("备注信息")]
        public string comment = "";
        
        [Header("动画名称")]
        public string animationName = "";
        
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
    public class TimelinePresetAnimationList : ScriptableObject
    {
        [Header("动画条目列表（备注 -> 动画名称）")]
        [SerializeField]
        public List<AnimationEntry> animationEntries = new List<AnimationEntry>()
        {
        };

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