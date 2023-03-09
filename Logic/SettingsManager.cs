using NekoLib;
using NekoNeko;
using UnityEngine;

public class SettingsManager : MonoSingleton<SettingsManager>
{
    [SerializeField] private SettingsConfig settingsConfig;

    protected override void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        ApplySettings(settingsConfig);
    } 

    public void ApplySettings(SettingsConfig config)
    {

    }
}
