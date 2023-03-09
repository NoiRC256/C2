using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NekoNeko.Avatar
{
    [CreateAssetMenu(menuName = "Avatar/Animation/Animation Config")]
    public class AvatarAnimsetConfig : ScriptableObject
    {
        [field: Header("Animations")]
        [field: SerializeField] public ClipTransitionAsset.UnShared Idle { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared Walk { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared WalkStopL { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared WalkStopR { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared RunStartL { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared RunStartR { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared Run { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared RunStopL { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared RunStopR { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared Sprint { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared SprintStopR { get; private set; }
        [field: SerializeField] public LinearMixerTransitionAsset.UnShared MovementMixer { get; private set; }

        [field: Header("Animation Parameters")]
        [field: SerializeField] public LocomotionAnimConfig WalkConfig { get; private set; }
        [field: SerializeField] public LocomotionAnimConfig RunConfig { get; private set; }
        [field: SerializeField] public LocomotionAnimConfig SprintConfig { get; private set; }
        [field: SerializeField] public float RunStartMaxExitTime { get; private set; } = 1f;
    }
}