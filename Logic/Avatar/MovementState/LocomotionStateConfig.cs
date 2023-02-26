using UnityEngine;

namespace NekoNeko.Avatar
{
    [CreateAssetMenu(menuName = "Avatar/States/Locomotion State Config")]
    public class LocomotionStateConfig : AvatarStateConfig
    {
        [field: SerializeField] public float ReferenceSpeed { get; private set; } = 3f;
        [field: SerializeField] public float DefaultBlendParameter { get; private set; } = 0f;
        [field: SerializeField] public float BlendDuration { get; private set; } = 0f;
        [field: SerializeField] public FootCycleConfig FootCycle { get; private set; }
    }
}