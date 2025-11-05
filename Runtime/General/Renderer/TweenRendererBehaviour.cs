using System;
using UnityEngine;
using UnityEngine.Playables;

namespace TweenPlayables
{
    [Serializable]
    public sealed class TweenRendererBehaviour : TweenAnimationBehaviour<Renderer>
    {
        [SerializeField] ColorTweenParameter color;
        [SerializeField] Vector2TweenParameter textureOffset;
        [SerializeField] Vector2TweenParameter textureScale;

        // 缓存的材质实例，用于修改而不影响 sharedMaterial
        [NonSerialized] private Material materialInstance;

        public ReadOnlyTweenParameter<Color> Color => color;
        public ReadOnlyTweenParameter<Vector2> TextureOffset => textureOffset;
        public ReadOnlyTweenParameter<Vector2> TextureScale => textureScale;

        public override void OnTweenInitialize(Renderer playerData)
        {
            // 创建材质实例
            if (materialInstance == null)
            {
                materialInstance = new Material(playerData.sharedMaterial);
                playerData.material = materialInstance;
            }
            
            color.SetInitialValue(playerData, materialInstance.color);
            textureOffset.SetInitialValue(playerData, materialInstance.mainTextureOffset);
            textureScale.SetInitialValue(playerData, materialInstance.mainTextureScale);
        }

        public override void OnGraphStop(Playable playable)
        {
            // 销毁材质实例
            if (materialInstance != null)
            {
                UnityEngine.Object.DestroyImmediate(materialInstance);
                materialInstance = null;
            }
            base.OnGraphStop(playable);
        }

        public override void ApplyProgress(Renderer binding, float progress)
        {
            if (materialInstance == null) return;
            
            if (color.IsActive)
                materialInstance.color = color.Evaluate(binding, progress);
            if (textureOffset.IsActive)
                materialInstance.mainTextureOffset = textureOffset.Evaluate(binding, progress);
            if (textureScale.IsActive)
                materialInstance.mainTextureScale = textureScale.Evaluate(binding, progress);
        }

        public override void ApplyFinalState(Renderer binding)
        {
            if (materialInstance == null) return;
            
            if (color.IsActive)
                materialInstance.color = color.Evaluate(binding, 1f);
            if (textureOffset.IsActive)
                materialInstance.mainTextureOffset = textureOffset.Evaluate(binding, 1f);
            if (textureScale.IsActive)
                materialInstance.mainTextureScale = textureScale.Evaluate(binding, 1f);
        }
    }
}