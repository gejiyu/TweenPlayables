using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace TweenPlayables
{
    [Serializable]
    public struct StringConcatNode
    {
        public bool isActive;
        public List<StringDataSource> items;
        public string resultDataKey;
    }

    [Serializable]
    public sealed class TweenStringConcatClip : PlayableAsset, ITimelineClipAsset
    {
        public StringConcatNode node = new StringConcatNode 
        { 
            isActive = true,
            items = new List<StringDataSource>()
        };

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<TweenStringConcatBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();

            behaviour.SetNode(node);
            
            return playable;
        }
    }
}
