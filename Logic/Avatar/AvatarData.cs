using UnityEngine;
using NekoLib.Stats;

namespace NekoNeko.Avatar
{
    public class AvatarData : MonoBehaviour
    {
        [field: SerializeField] public AvatarMovementConfig MovementConfig { get; private set; }

        public AvatarLocomotionState LocomotionState { get; set; }
        public AvatarAimState AimState { get; set; }
        public Stat MoveSpeedMultiplier { get; private set; }

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            MoveSpeedMultiplier = new Stat(1f);
        }
    }
}