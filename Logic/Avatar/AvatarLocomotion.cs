using NekoLib.Movement;
using UnityEngine;

namespace NekoNeko.Avatar
{
    public class AvatarLocomotion : MonoBehaviour
    {
        [SerializeField] private CharacterMover _mover;
        [SerializeField] private Transform _directionTr;
        [SerializeField] private Transform _dummyRoot;

        public AvatarData Data { get; set; }
        public AvatarInput Input { get; set; }

        private void Awake()
        {
            if (_directionTr == null) _directionTr = this.transform;
        }

        public void OnUpdate(float deltaTime)
        {
            Vector2 moveInput = Input.Move.ReadValue<Vector2>();
            //Debug.Log(moveInput);
            float speed = Data.MovementConfig.RunSpeed * Data.MoveSpeedMultiplier.Value;
            Move(speed, moveInput);
        }

        public void Move(float speed, Vector2 input)
        {
            _mover.InputMove(speed, DirectionFromInput(input, _directionTr));
        }

        public void Move(float speed, Vector2 input, Transform directionTr)
        {
            _mover.InputMove(speed, DirectionFromInput(input, directionTr));
        }

        private Vector3 DirectionFromInput(Vector2 input, Transform directionTr)
        {
            Vector3 direction = input.x * Vector3.ProjectOnPlane(directionTr.right, Vector3.up);
            direction += input.y * Vector3.ProjectOnPlane(directionTr.forward, Vector3.up);
            return direction.normalized;
        }
    }
}