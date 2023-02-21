using UnityEngine;
using NekoLib.Stats;
using static UnityEngine.Rendering.DebugUI;

namespace NekoNeko.Avatar
{
    public class AvatarData : MonoBehaviour
    {
        [field: SerializeField] public AvatarMovementConfig MovementConfig { get; private set; }

        public AvatarLocomotionState LocomotionState { get; set; }
        public AvatarAimState AimState { get; set; }
        public Stat MoveSpeedMultiplier { get; private set; }
        public float LastMoveSpeed {
            get => _lastMoveSpeed;
            set {
                _lastMoveSpeed = value;
                if (value > 0.01f) LastNonZeroMoveSpeed = value;
            }
        }
        public float LastNonZeroMoveSpeed { get; private set; }

        public bool HasMoveInput { get; set; }
        public bool WalkToggle { get; set; }
        public float LastInputSpeed {
            get => _lastInputSpeed;
            set {
                _lastInputSpeed = value;
                if (value > 0.01f) LastNonZeroInputSpeed = value;
            }
        }
        public float LastNonZeroInputSpeed { get; private set; }
        public Vector3 LastInputDirection {
            get => _lastInputDirection;
            set {
                UpdateLastInputDirection(value);
            }
        }
        public Vector3 LastNonZeroInputDirection { get; private set; }
        public float InputDirectionDot { get; private set; }
        public Vector3 InputDirectionCross { get; private set; }

        private float _lastMoveSpeed;
        private float _lastInputSpeed;
        private Vector3 _lastInputDirection;

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            MoveSpeedMultiplier = new Stat(1f);
        }

        private void UpdateLastInputDirection(Vector3 direction)
        {
            _lastInputDirection = direction;
            if (direction != Vector3.zero) LastNonZeroInputDirection = direction;
            InputDirectionDot = Vector3.Dot(direction, _lastInputDirection);
            InputDirectionCross = Vector3.Cross(direction, _lastInputDirection);
        }
    }
}