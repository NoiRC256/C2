using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NekoLib
{
    /// <summary>
    /// Service locator.
    /// </summary>
    public static class GameEngine
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

#if UNITY_EDITOR
        // Domain reload for editor enter play mode options.
        [InitializeOnEnterPlayMode]
        private static void InitializeOnEnterPlayMode()
        {
            _services.Clear();
        }
#endif

        /// <summary>
        /// Register an object as a service.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="obj"></param>
        public static void Register<TService>(object obj) where TService : class
        {
            if (_services.ContainsKey(typeof(TService))) return;
            _services.Add(typeof(TService), obj);

            if (obj.GetType() == typeof(GameObject))
            {
                GameObject.DontDestroyOnLoad((GameObject)obj);
            }
            else if (typeof(Component).IsAssignableFrom(obj.GetType()))
            {
                GameObject.DontDestroyOnLoad((Component)obj);
            }
        }

        /// <summary>
        /// Register a Unity component type as a service.
        /// <para>This will instantiate a new <see cref="GameObject"/> containing a new component of the specified type.</para>
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TInstance"></typeparam>
        public static void Register<TService, TInstance>()
            where TService : class where TInstance : Component
        {
            var obj = new GameObject(typeof(TService).Name).AddComponent<TInstance>();
            Register<TService>(obj);
        }

        /// <summary>
        /// Get a service of the specified type.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public static TService Get<TService>() where TService : class
        {
            return (TService)Get(typeof(TService));
        }

        /// <summary>
        /// Get a service of the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Get(Type type)
        {
            if (_services.ContainsKey(type))
            {
                return _services[type];
            }
            return null;
        }
    }
}