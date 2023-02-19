using UnityEngine;

namespace NekoNeko.Avatar
{
    /// <summary>
    /// Default values for avatar movement.
    /// </summary>
    [CreateAssetMenu(menuName = "Avatar/Movement Config")]
    public class AvatarMovementConfig : ScriptableObject
    {
        [field: SerializeField] public float RunSpeed { get; private set; } = 3f;
        [field: SerializeField] public float SprintSpeed { get; private set; } = 4.5f;
        [field: SerializeField] public float CrouchMoveSpeed { get; private set; } = 4.5f;
    }
}