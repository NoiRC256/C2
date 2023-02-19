namespace NekoNeko.Battle
{
    /// <summary>
    /// Runtime data for bullets.
    /// </summary>
    public class BulletData
    {
        public IBattleActor Source { get; set; }

        public float DamageNear { get; set; }
        public float DamageNearDistance { get; set; }
        public float DamageFar { get; set; }
        public float DamageFarDistance { get; set; }
        public float SplashDamage { get; set; }
        public float SplashRadius { get; set; }
        public float Duration { get; set; }
        public float Range { get; set; }
        public float Thickness { get; set; }

        public float Speed { get; set; }
        public float Gravity { get; set; }
        public float GravitySpeedLimit { get; set; }
        public bool SpeedAffectsGravity { get; private set; }
        public float SeekRange { get; set; }
        public float SeekRate { get; set; }

        public MonoEffect HitEffect { get; private set; }
        public bool HitEffectOverridePrefab { get; private set; }
        public MonoEffect DestroyEffect { get; private set; }
        public bool DestroyEffectOverridePrefab { get; private set; }

        public BulletData(BulletConfig cfg, IBattleActor source = null)
        {
            Init(cfg, source);
        }

        public void Init(BulletConfig cfg, IBattleActor source = null)
        {
            Source = source;
            CopyFrom(cfg);
        }

        public void CopyFrom(BulletConfig cfg)
        {
            DamageNear = cfg.DamageNear;
            DamageNearDistance = cfg.DamageNearDistance;
            DamageFar = cfg.DamageFar;
            DamageFarDistance = cfg.DamageFarDistance;
            SplashDamage = cfg.SplashDamage;
            SplashRadius = cfg.SplashRadius;
            Duration = cfg.Duration;
            Range = cfg.Range;
            Thickness = cfg.Thickness;

            Speed = cfg.Speed;
            Gravity = cfg.Gravity;
            GravitySpeedLimit = cfg.GravitySpeedLimit;
            SeekRange = cfg.SeekRange;
            SeekRate = cfg.SeekRate;

            HitEffect = cfg.HitEffect;
            HitEffectOverridePrefab = cfg.HitEffectOverridePrefab;
            DestroyEffect = cfg.DestroyEffect;
            DestroyEffectOverridePrefab = cfg.DestroyEffectOverridePrefab;
        }

        public void Clear()
        {
            Source = null;
            HitEffect = null;
            DestroyEffect = null;
        }
    }
}