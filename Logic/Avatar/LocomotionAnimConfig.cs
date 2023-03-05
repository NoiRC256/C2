using UnityEngine;

namespace NekoNeko.Avatar
{
    [CreateAssetMenu(menuName = "Avatar/Animation/Locomotion Animation Config")]
    public class LocomotionAnimConfig : AvatarStateConfig
    {
        [field: SerializeField] public float ReferenceSpeed { get; private set; } = 3f;
        [field: SerializeField] public FootCycleConfig FootCycle { get; private set; } = new FootCycleConfig();
    }
}