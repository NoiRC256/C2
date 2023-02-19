using NekoLib;
using NekoLib.Pool;

namespace NekoNeko.Battle
{
    public class AttackData : IPoolable
    {
        private NekoLib.Pool.IObjectPool _pool;

        public IBattleActor Source { get; set; }
        public AttackType AttackType { get; set; }
        public float Damage { get; set; }
        public bool ShowDamageText { get; set; }
        public bool IsCrit { get; set; }

        public static AttackData Create(IBattleActor source = null, AttackType attackType = AttackType.Unknown, float damage = 0f,
            bool showDamageText = true)
        {
            var pool = NekoLib.GameEngine.Get<IObjectPoolManager>().GetPool<AttackData>();
            AttackData attackData = pool.Get();
            attackData.Init(source, attackType, damage, showDamageText, pool);
            return attackData;
        }

        public static AttackData Create(AttackData from)
        {
            AttackData to = AttackData.Create();
            to.AttackType = from.AttackType;
            to.Damage = from.Damage;
            to.IsCrit = from.IsCrit;
            return to;
        }

        public void Init(IBattleActor source = null, AttackType attackType = AttackType.Unknown, float damage = 0f,
            bool showDamageText = true, IObjectPool pool = null)
        {
            _pool = pool;
            Source = source;
            AttackType = attackType;
            Damage = damage;
            ShowDamageText = showDamageText;
        }

        public void Copy(AttackData from)
        {
            AttackType = from.AttackType;
            Damage = from.Damage;
            IsCrit = from.IsCrit;
        }

        public void Clear()
        {

        }

        public void OnTakeFromPool()
        {
        }

        public void OnReturnToPool()
        {
            Clear();
        }

        public void Destroy()
        {
            _pool.Release(this);
        }
    }
}