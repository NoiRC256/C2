using NekoLib.Pool;

namespace NekoNeko
{
    public class MonoEffectFactory : EntityFactoryBase
    {
        /// <summary>
        /// Spawn and initialize an instance of a <see cref="MonoEffect"/> prefab.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static MonoEffect Instantiate(MonoEffect prefab)
        {
            IObjectPool<MonoEffect> pool = ObjectPoolManager.GetPool<MonoEffect>(prefab);
            MonoEffect monoEffect = pool.Get();
            monoEffect.Init(pool);
            return monoEffect;
        }
    }
}
