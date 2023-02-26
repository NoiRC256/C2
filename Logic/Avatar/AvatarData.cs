using UnityEngine;
using NekoLib.Stats;

namespace NekoNeko.Avatar
{
    public class AvatarData : MonoBehaviour
    {
        public enum MovementStateType
        {
            None,
            Idle,
            Walk,
            Run,
            Sprint,
            RootMotion,
        }

        public enum AimStateType
        {
            None,
            Hip,
            Shoulder,
            Sight,
        }

        [field: SerializeField] public AvatarMovementConfig MovementConfig { get; private set; }

        #region State Properties

        public MovementStateType MovementState { get; set; }
        public AimStateType AimState { get; set; }
        public Stat MoveSpeedMultiplier { get; private set; }
        public float LastMoveSpeed {
            get => _lastMoveSpeed;
            set {
                _lastMoveSpeed = value;
                if (value > 0.01f) LastNonZeroMoveSpeed = value;
            }
        }
        private float _lastMoveSpeed;
        public float LastNonZeroMoveSpeed { get; private set; }
        public float MoveDirectionDot { get; private set; }
        public Vector3 MoveDirectionCross { get; private set; }

        public float ForwardFoot { get; set; }
        public float LocomotionBlend { get; set; }
        public float LocomotionTargetBlend { get; set; }

        #endregion

        #region Input Properties

        public bool HasMovementInput { get; set; }
        public bool WalkToggle { get; set; }
        public float LastInputSpeed {
            get => _lastInputSpeed;
            set {
                _lastInputSpeed = value;
                if (value > 0.01f) LastNonZeroInputSpeed = value;
            }
        }
        private float _lastInputSpeed;
        public float LastNonZeroInputSpeed { get; private set; }
        public Vector3 LastInputDirection {
            get => _lastInputDirection;
            set {
                UpdateLastInputDirection(value);
            }
        }
        private Vector3 _lastInputDirection;
        public Vector3 LastNonZeroInputDirection { get; private set; }

        #endregion

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
            MoveDirectionDot = Vector3.Dot(direction, _lastInputDirection);
            MoveDirectionCross = Vector3.Cross(direction, _lastInputDirection);
        }
    }
}