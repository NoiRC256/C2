using UnityEngine;

namespace NekoNeko
{
    [CreateAssetMenu(menuName = "Game Settings")]
    public class SettingsConfig : ScriptableObject
    {
        [SerializeField] public int frameRate = 60;
        [SerializeField] public float horizontalSensitivity = 5f;
        [SerializeField] public float verticalSensitivity = 5f;
        [SerializeField] public float aimSensitivity = 5f;
        [SerializeField] public float aimSensitivityScopeMultiplier2 = 0.5f;
        [SerializeField] public float aimSensitivityScopeMultiplier3 = 0.25f;
        [SerializeField] public float aimSensitivityScopeMultiplier4 = 0.125f;
    }
}