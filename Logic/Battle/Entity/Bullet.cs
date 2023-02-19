using UnityEngine;
using NekoLib.Pool;

namespace NekoNeko.Battle
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] MonoEffect _hitEffect;
        [SerializeField] MonoEffect _destroyEffect;

        public BulletConfig DefaultCfg { get; private set; }
        public BulletData BulletData { get; private set; }
        public Vector3 Origin { get; private set; }
        public IBulletMover Mover { get; set; }

        private IObjectPool _pool;
        private readonly AttackData _attackData = new AttackData();
        private LayerMask _layerMask;
        private float _aliveCounter;
        private bool _isActive;

        private Vector3 _nextPos;

        #region MonoBehaviour
        private void Update()
        {
            OnUpdate(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            OnFixedUpdate(Time.deltaTime);
        }
        #endregion

        public void Init(BulletConfig cfg, Vector3 origin, Vector3 direction,
            LayerMask layerMask = default, IBattleActor source = null, IObjectPool pool = null)
        {
            // Init runtime data.
            DefaultCfg = Instantiate(cfg);
            if (BulletData == null) BulletData = new BulletData(DefaultCfg, source);
            else BulletData.Init(DefaultCfg, source);

            // Init core vars.
            Origin = origin;
            Mover = new ProjectileBulletMover();
            Mover.Init(BulletData, origin, direction);
            transform.position = Mover.Position;
            transform.rotation = Mover.Rotation;

            _pool = pool;
            _attackData.Init(source);
            _layerMask = layerMask;
            _aliveCounter = DefaultCfg.Duration;

            OnInit();
        }

        public void OnInit()
        {
            _isActive = true;
            this.gameObject.SetActive(true);
        }

        public void Clear()
        {
            DefaultCfg = null;
            BulletData.Clear();
            _attackData.Clear();
        }

        public void OnUpdate(float deltaTime)
        {

        }

        public void OnFixedUpdate(float deltaTime)
        {
            // Calculate next position.
            _nextPos = Mover.UpdateNextPosition(deltaTime);

            // Detect collision towards next position.
            CheckHit();

            // Update motion.
            if (!_isActive) return;
            Mover.UpdateMotion(deltaTime);
            transform.position = Mover.Position;
            transform.rotation = Mover.Rotation;

            _aliveCounter -= deltaTime;
            if (_aliveCounter <= 0f) End();
        }

        private void CheckHit()
        {
            RaycastHit hitInfo;
            bool hasHit = false;
            if (BulletData.Thickness > 0f)
            {
                hasHit = Physics.SphereCast(transform.position, BulletData.Thickness / 2f, _nextPos - transform.position, out hitInfo,
                    float.MaxValue, layerMask: _layerMask);
            }
            else
            {
                hasHit = Physics.Linecast(transform.position, _nextPos, out hitInfo,
                    layerMask: _layerMask);
            }

            if (hasHit)
            {
                Hit(hitInfo);
            }
        }

        protected void Hit(RaycastHit hitInfo)
        {
            float distance = Vector3.Distance(hitInfo.point, Origin);
            if (hitInfo.collider.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                AttackData attackResult = AttackData.Create(_attackData);
                attackResult.Damage = CalcDamage(distance);
                damageable.ReceiveAttack(attackResult);
            }
            End();
        }

        /// <summary>
        /// Returns linear interpolated damage at the specified distance.
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public float CalcDamage(float distance)
        {
            return Mathf.Lerp(BulletData.DamageNear, BulletData.DamageFar,
                Mathf.InverseLerp(BulletData.DamageNearDistance, BulletData.DamageFarDistance, distance));
        }

        private void End()
        {
            _isActive = false;
            if (_pool != null)
            {
                bool isReleased = ReturnToPool();
                if (!isReleased) Destroy();
            }
            else Destroy();
        }

        public bool ReturnToPool()
        {
            OnReturnToPool();
            return _pool.Release(this);
        }

        public void OnReturnToPool()
        {
            this.gameObject.SetActive(false);
            Clear();
        }

        public void Destroy()
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}