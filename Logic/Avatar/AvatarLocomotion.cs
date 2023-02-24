using Animancer;
using NekoLib.Movement;
using System;
using UnityEngine;

namespace NekoNeko.Avatar
{
    public class AvatarLocomotion : MonoBehaviour
    {
        #region Exposed Fields

        [SerializeField] private CharacterMover _mover;
        [SerializeField] private Transform _directionTr;
        [SerializeField] private Transform _dummyRoot;

        [field: SerializeField] public MonoRootMotion RootMotion { get; private set; }
        [field: SerializeField] public float DefaultFacingSmoothDuration { get; set; } = 0.15f;

        [field: Header("Animations")]
        [field: SerializeField] public ClipTransitionAsset.UnShared Idle { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared Walk { get; private set; }
        [field: SerializeField] public FootCycleConfig WalkFootCycle { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared WalkStopL { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared WalkStopR { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared RunStartL { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared RunStartR { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared Run { get; private set; }
        [field: SerializeField] public FootCycleConfig RunFootCycle { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared RunStopL { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared RunStopR { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared Sprint { get; private set; }
        [field: SerializeField] public FootCycleConfig SprintFootCycle { get; private set; }
        [field: SerializeField] public ClipTransitionAsset.UnShared SprintStopR { get; private set; }

        #endregion

        public AvatarData Data { get; set; }
        public AvatarInput Input { get; set; }
        public FacingHandler FacingHandler { get; private set; } = new FacingHandler();
        public VelocitySource _rootMotionVelocity = new VelocitySource(true);

        #region MonoBehaviour

        private void Awake()
        {
            if (_directionTr == null) _directionTr = this.transform;
        }

        private void Start()
        {
            Data.LastInputDirection = transform.forward;
            _mover.AddVelocitySource(_rootMotionVelocity);
        }

        #endregion

        #region Logic

        public void OnUpdate(float deltaTime)
        {
            Data.HasMovementInput = Input.Move.IsPressed();
            FacingHandler.OnUpdate(deltaTime);
            SetFacing(FacingHandler.CurrentFacing);
        }

        /// <summary>
        /// Move without recording the speed and direction.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="direction"></param>
        public void InputMoveNoCache(float speed, Vector3 direction)
        {
            _mover.InputMove(speed, direction);
        }

        public void InputMove(float speed, Vector3 direction)
        {
            Data.LastInputSpeed = speed;
            Data.LastInputDirection = direction;
            _mover.InputMove(speed, direction);
        }

        public void InputMove(float speed, Vector2 input)
        {
            Vector3 inputDirection = DirectionFromInput(input, _directionTr);
            InputMove(speed, inputDirection);
        }

        public void InputMove(float speed, Vector2 input, Transform directionTr)
        {
            Vector3 inputDirection = DirectionFromInput(input, directionTr);
            InputMove(speed, inputDirection);
        }

        public void RootMotionMove(Vector3 velocity)
        {
            _rootMotionVelocity.Velocity = velocity;
        }

        public void RootMotionInputMove(Vector3 velocity)
        {
            _mover.SetActiveVelocity(velocity);
        }

        public void RootMotionInputMove(Vector3 velocity, Vector2 input)
        {
            if(input == Vector2.zero)
            {
                RootMotionInputMove(velocity.magnitude * Data.LastNonZeroInputDirection);
                return;
            } 

            Vector3 inputDirection = DirectionFromInput(input, _directionTr);
            Data.LastInputDirection = inputDirection;
            RootMotionInputMove(velocity.magnitude * inputDirection);
        }

        public void MoveDeltaPosition(Vector3 deltaPosition, bool alignToGround = true, bool restrictToGround = false)
        {
            _mover.MoveDeltaPosition(deltaPosition, alignToGround, restrictToGround);
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