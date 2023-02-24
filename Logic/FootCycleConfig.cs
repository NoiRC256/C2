using UnityEngine;

namespace NekoNeko
{
    [CreateAssetMenu(menuName = "Avatar/Foot Cycle Config")]
    public class FootCycleConfig : ScriptableObject
    {
        [field: SerializeField] public float LeftFootTime = 0f;
        [field: SerializeField] public float RightFootTime = 0.5f;
        [field: SerializeField] public bool IsLeftFootFirst = true;
        public float Duration => Mathf.Abs(RightFootTime - LeftFootTime);
    }
}