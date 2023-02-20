using NekoLib.Movement;
using TMPro;
using UnityEngine;

namespace NekoNeko.Avatar
{
    public class AvatarLocomotion : MonoBehaviour
    {
        [SerializeField] private CharacterMover _mover;
        [SerializeField] private Transform _directionTr;
        [SerializeField] private Transform _dummyRoot;
        [field: SerializeField] public float DefaultFacingSmoothDuration { get; set; } = 0.15f;

        public AvatarData Data { get; set; }
        public AvatarInput Input { get; set; }
        public Vector3 LastInputDirection {
            get => _lastInputDirection;
            private set { if (value != Vector3.zero) _lastInputDirection = value; }
        }

        private FacingHandler _facingHandler = new FacingHandler();
        private Vector3 _lastInputDirection;

        private void Awake()
        {
            if (_directionTr == null) _directionTr = this.transform;
            LastInputDirection = transform.forward;
        }

        public void OnUpdate(float deltaTime)
        {
            Vector2 moveInput = Input.Move.ReadValue<Vector2>();
            float speed = Data.MovementConfig.RunSpeed * Data.MoveSpeedMultiplier.Value;
            Move(speed, moveInput);

            SetTargetFacing(LastInputDirection);
            UpdateFacing(deltaTime);
            SetFacing(_facingHandler.CurrentFacing);
        }

        public void Move(float speed, Vector2 input)
        {
            Vector3 inputDirection = DirectionFromInput(input, _directionTr);
            LastInputDirection = inputDirection;
            _mover.InputMove(speed, inputDirection);
        }

        public void Move(float speed, Vector2 input, Transform directionTr)
        {
            Vector3 inputDirection = DirectionFromInput(input, directionTr);
            LastInputDirection = inputDirection;
            _mover.InputMove(speed, inputDirection);
        }

        #region Facing

        public void SetTargetFacing(Vector3 direction) 
            => _facingHandler.SetTargetFacing(direction);

        private void UpdateFacing(float deltaTime)
        {
            _facingHandler.UpdateFacing(deltaTime, smoothDuration: DefaultFacingSmoothDuration);
        }

        public void UpdateFacing(float deltaTime, float smoothDuration) 
            => _facingHandler.UpdateFacing(deltaTime, smoothDuration);

        public void UpdateFacing(float deltaTime, Vector3 direction, float smoothDuration) 
            => _facingHandler.UpdateFacing(deltaTime, direction, smoothDuration);

        public void UpdateFacing(float deltaTime, float targetFacing, float smoothDuration)
            => _facingHandler.UpdateFacing(deltaTime, targetFacing, smoothDuration);

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