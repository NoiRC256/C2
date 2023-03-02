using NekoLib;

namespace NekoNeko
{
    public class GameManager : MonoSingleton<GameManager>
    {
        protected override void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}