using NekoLib.Movement;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace NekoNeko.Avatar
{
    public class AvatarLocomotion : MonoBehaviour
    {
        [SerializeField] private CharacterMover _mover;
        [SerializeField] private Transform _directionTr;
        [SerializeField] private Transform _dummyRoot;
        [field: SerializeField] public MonoRootMotion RootMotion { get; private set; }
        [field: SerializeField] public float DefaultFacingSmoothDuration { get; set; } = 0.15f;

        public AvatarData Data { get; set; }
        public AvatarInput Input { get; set; }
        public FacingHandler FacingHandler { get; set; } = new FacingHandler();

        private LocomotionStateMachine _stateMachine;

        private void Awake()
        {
            if (_directionTr == null) _directionTr = this.transform;
        }

        private void Start()
        {
            Data.LastInputDirection = transform.forward;
            _stateMachine = new LocomotionStateMachine(this);
        }

        #region Logic

        public void OnUpdate(float deltaTime)
        {
            Data.HasMoveInput = Input.Move.IsPressed();
            _stateMachine.OnUpdate(deltaTime);
            FacingHandler.OnUpdate(deltaTime);
            SetFacing(FacingHandler.CurrentFacing);
        }

        public void Move(float speed, Vector3 direction)
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

        public void MoveDeltaPosition(Vector3 deltaPosition)
        {
            _mover.MoveDeltaPosition(deltaPosition);
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

        #region Facing

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

        #endregion
    }
}