using UnityEngine;
using NekoLib;
using NekoLib.Pool;

namespace NekoNeko
{
    /// <summary>
    /// Entry point of the game.
    /// Initializes and maintains core game services.
    /// <para>Wrapper class for <see cref="NekoLib"/>.<see cref="NekoLib.GameEngine"/>.</para>
    /// <para>To get the registered game services, it is recommended to use <see cref="NekoLib"/>.<see cref="NekoLib.GameEngine"/>
    /// for reusability.</para>
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public static class GameEntry
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            // Global object pool manager.
            NekoLib.GameEngine.Register<IObjectPoolManager, ObjectPoolManager>();

            Debug.Log("Game Entry Initialized");

            var gameConfig = new GameConfig();
            Debug.Log(gameConfig.frameRate);
        }
    }
}