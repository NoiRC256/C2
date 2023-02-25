using UnityEngine;

namespace NekoNeko
{
    [DefaultExecutionOrder(-2)]
    [CreateAssetMenu(menuName = "Game Config")]
    public class GameConfig : ScriptableObject
    {
        private static GameConfig instance = null;

        [SerializeField] public int frameRate = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if(instance == null)
            {
                Debug.Log("Game Configuration Missing");
            }
            else
            {
                Debug.Log("Game Configuration Loaded");
            }
        }

        public GameConfig()
        {
            instance ??= this;
            Debug.Log("Game Configuration Created");
        }
    }
}