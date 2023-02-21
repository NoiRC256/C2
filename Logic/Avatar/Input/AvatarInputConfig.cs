using UnityEngine;

namespace NekoNeko.Avatar
{
    [CreateAssetMenu(menuName ="Avatar/Input Config", fileName = "AvatarInputConfig")]
    public class AvatarInputConfig : ScriptableObject
    {
        [field: SerializeField] public string Move { get; private set; } = "Move";
        [field: SerializeField] public string Look { get; private set; } = "Look";
        [field: SerializeField] public string Fire { get; private set; } = "Fire";
        [field: SerializeField] public string Sprint { get; private set; } = "Sprint";
        [field: SerializeField] public string Walk { get; private set; } = "Walk";
        [field: SerializeField] public string Interact { get; private set; } = "Interact";
    }
}