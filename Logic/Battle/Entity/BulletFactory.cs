using UnityEngine;
using NekoLib.Pool;

namespace NekoNeko.Battle
{
    public class BulletFactory : EntityFactoryBase
    {
        /// <summary>
        /// Spawn and initialize an instance of a <see cref="Bullet"/> prefab.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="cfg"></param>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="layerMask"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Bullet Instantiate(Bullet prefab, BulletConfig cfg, Vector3 origin, Vector3 direction,
            LayerMask layerMask = default, IBattleActor source = null)
        {
            IObjectPool<Bullet> pool = ObjectPoolManager.GetPool<Bullet>(prefab);
            Bullet bullet = pool.Get();
            bullet.Init(cfg, origin, direction, layerMask, source, pool);
            return bullet;
        }
    }
}