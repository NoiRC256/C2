using System;
using UnityEngine;

namespace NekoNeko.Battle
{
    /// <summary>
    /// Configuration for bullets.
    /// </summary>
    [CreateAssetMenu(menuName = "Battle/Bullet Config", fileName = "BulletCfg")]
    [Serializable]
    public class BulletConfig : ScriptableObject
    {
        [field: SerializeField] public int Id { get; private set; } = 0;
        [field: SerializeField] public float DamageNear { get; private set; } = 143f;
        [field: SerializeField][field: Min(0f)] public float DamageNearDistance { get; private set; } = 10f;
        [field: SerializeField] public float DamageFar { get; private set; } = 112f;
        [field: SerializeField][field: Min(0f)] public float DamageFarDistance { get; private set; } = 40f;
        [field: SerializeField] public float SplashDamage { get; private set; } = 0f;
        [field: SerializeField][field: Min(0f)] public float SplashRadius { get; private set; } = 0f;
        [field: SerializeField][field: Min(0f)] public float Duration { get; private set; } = 10f;
        [field: SerializeField][field: Min(0f)] public float Range { get; private set; } = 100f;
        [field: SerializeField][field: Min(0f)] public float Thickness { get; private set; } = 0f;

        [field: Header("Motion")]
        [field: SerializeField][field: Min(0f)] public float Speed { get; private set; } = 10f;
        [field: SerializeField] public float Gravity { get; private set; } = -1f;
        [field: SerializeField] public float GravitySpeedLimit { get; private set; } = -10f;
        [field: SerializeField][field: Min(0f)] public float SeekRange { get; private set; } = 0f;
        [field: SerializeField] public float SeekRate { get; private set; } = 0f;

        [field: Header("Effects")]
        [field: SerializeField] public MonoEffect HitEffect { get; private set; }
        [field: SerializeField] public bool HitEffectOverridePrefab { get; private set; } = true;
        [field: SerializeField] public MonoEffect DestroyEffect { get; private set; }
        [field: SerializeField] public bool DestroyEffectOverridePrefab { get; private set; } = true;
    }
}