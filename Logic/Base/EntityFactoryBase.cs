using NekoLib;
using NekoLib.Pool;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace NekoNeko
{
    public class EntityFactoryBase
    {
        private static IObjectPoolManager _objectPoolManager;

        protected static IObjectPoolManager ObjectPoolManager {
            get {
                if(_objectPoolManager == null)
                {
                    _objectPoolManager = NekoLib.GameEngine.Get<IObjectPoolManager>();
                }
                return _objectPoolManager;
            }
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void DomainReload()
        {
            _objectPoolManager = null;
        }
#endif
    }
}
