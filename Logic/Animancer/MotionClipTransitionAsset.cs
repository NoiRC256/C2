using Animancer;
using System;
using UnityEngine;

namespace NekoNeko
{
    /// <inheritdoc/>
    /// https://kybernetik.com.au/animancer/api/Animancer/ClipTransitionAsset
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Motion Clip Transition", order = Strings.AssetMenuOrder + 1)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(ClipTransitionAsset))]
    public class MotionClipTransitionAsset : AnimancerTransitionAsset<MotionClipTransition>
    {
        /// <inheritdoc/>
        [Serializable]
        public new class UnShared :
            UnShared<MotionClipTransitionAsset, MotionClipTransition, ClipState>,
            ClipState.ITransition
        { }
    }

    [System.Serializable]
    public class MotionClipTransition : ClipTransition
    {
        [SerializeField] private bool _applyRootMotion = true;

        public override void Apply(AnimancerState state)
        {
            base.Apply(state);
            state.Root.Component.Animator.applyRootMotion = _applyRootMotion;
        }
    }
}
