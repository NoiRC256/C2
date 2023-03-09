using NekoLib.Movement;
using UnityEngine;

namespace NekoNeko.Avatar
{
    /// <summary>
    /// Provides configurations and methods for avatar movement.
    /// </summary>
    [System.Serializable]
    public class AvatarMovement
    {
        #region Exposed Fields

        [SerializeField] private CharacterMover _mover;
        [SerializeField] private Transform _dummyRoot;

        [field: SerializeField] public Transform DirectionTr { get; set; }
        [field: SerializeField] public MonoRootMotion RootMotion { get; private set; }
        [field: SerializeField] public float DefaultFacingSmoothDuration { get; set; } = 0.15f;
        [field: SerializeField] public float IdleToMoveMinExitTime { get; private set; } = 0.25f;
        [field: SerializeField] public AvatarAnimsetConfig AnimsetConfig { get; private set; }

        #endregion

        public AvatarData Data { get; set; }
        public AvatarInput Input { get; set; }
        public FacingHandler FacingHandler { get; private set; } = new FacingHandler();

        #region Logic

        public void OnUpdate(float deltaTime)
        {
            Data.HasMovementInput = Input.Move.IsPressed();
            FacingHandler.OnUpdate(deltaTime);
            SetFacing(FacingHandler.CurrentFacing);
        }

        public void InputMove(float speed, Vector3 direction)
        {
            Data.LastInputSpeed = speed;
            Data.LastInputDirection = direction;
            _mover.InputMove(speed, direction);
        }

        public void InputMove(float speed, Vector2 input)
        {
            Vector3 inputDirection = DirectionFromInput(input, DirectionTr);
            InputMove(speed, inputDirection);
        }

        public void InputMove(float speed, Vector2 input, Transform directionTr)
        {
            Vector3 inputDirection = DirectionFromInput(input, directionTr);
            InputMove(speed, inputDirection);
        }

        public void RootMotionRotate()
        {
            MoveDeltaRotation(RootMotion.DeltaRotation);
        }

        public void RootMotionInputMove()
        {
            _mover.InputMove(RootMotion.Velocity);
        }

        public void RootMotionInputMove(Vector2 input)
        {
            if (input == Vector2.zero)
            {
                RootMotionInputMove();
                return;
            }
            float inputSpeed = RootMotion.Velocity.magnitude;
            Vector3 inputDirection = DirectionFromInput(input, DirectionTr);
            Data.LastMoveSpeed = inputSpeed;
            Data.LastInputDirection = inputDirection;
            _mover.InputMove(inputSpeed * inputDirection);
        }

        public void MoveDeltaRotation(Quaternion deltaRotation)
        {
            Vector3 deltaEulerAngles = deltaRotation.eulerAngles;
            deltaEulerAngles.x = deltaEulerAngles.z = 0f;
            deltaRotation = Quaternion.Euler(deltaEulerAngles);
            _dummyRoot.rotation *= deltaRotation;

            FacingHandler.RefreshFacing(_dummyRoot.rotation);
        }

        #endregion

        #region Animation

        public float EvaluateFootCycle(float normalizedTime, FootCycleConfig footCycle)
        {
            return  GetForwardFootByTime(normalizedTime, footCycle.Duration, footCycle.IsLeftFootFirst);
        }

        #endregion

        #region Facing

        /// <summary>
        /// Directly set the facing rotation.
        /// </summary>
        /// <param name="facingAngle"></param>
        public void SetFacing(float facingAngle)
        {
            Vector3 eulerAngles = _dummyRoot.eulerAngles;
            eulerAngles.y = facingAngle;
            _dummyRoot.eulerAngles = eulerAngles;
        }

        #endregion

        #region Helpers

        private Vector3 DirectionFromInput(Vector2 input, Transform directionTr)
        {
            Vector3 direction = input.x * Vector3.ProjectOnPlane(directionTr.right, Vector3.up);
            direction += input.y * Vector3.ProjectOnPlane(directionTr.forward, Vector3.up);
            return direction.normalized;
        }

        public float GetForwardFootByTime(float normalizedTime, float footCycleLength, bool isLeftFootFirst = true)
        {
            float cycleCount = Mathf.Ceil(normalizedTime / footCycleLength);
            return GetForwardFootByCycle(cycleCount, isLeftFootFirst);
        }

        /// <summary>
        /// Returns foot id from cycle count.
        /// Left foot is 0, right foot is 1.
        /// </summary>
        /// <param name="cycleCount"></param>
        /// <returns></returns>
        public float GetForwardFootByCycle(float cycleCount, bool isLeftFootOddCycle = true)
        {
            if (isLeftFootOddCycle) return cycleCount % 2f > 0 ? 0f : 1f;
            else return cycleCount % 2f > 0 ? 1f : 0f;
        }

        #endregion
    }
}