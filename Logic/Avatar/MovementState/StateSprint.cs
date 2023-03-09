using UnityEngine;
using Animancer;

namespace NekoNeko.Avatar
{
    public class StateSprint : StateLocomotionBase
    {
        protected override ITransition LocomotionAnim => _movement.AnimsetConfig.Sprint;
        protected override LocomotionAnimConfig LocomotionAnimConfig => _movement.AnimsetConfig.SprintConfig;

        public StateSprint(TPSAvatarController avatar) : base(avatar)
        {
        }

        public override void OnEnterState()
        {
            base.OnEnterState();

            _data.MovementState = AvatarData.MovementStateType.Sprint;
        }

        public override void OnCheckTransitions()
        {
            base.OnCheckTransitions();

            if (!_input.Sprint.IsPressed())
            {
                _stateMachine.TrySetState(_avatar.StateRun);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (_input.Walk.WasPressedThisFrame())
            {
                _data.WalkToggle = !_data.WalkToggle;
            }

            _movement.InputMove(GetMoveSpeed(), _input.Move.ReadValue<Vector2>());
            _movement.FacingHandler.RotateTowards(_data.LastNonZeroInputDirection);
        }

        protected override float GetMoveSpeed()
        {
            return _data.MovementConfig.SprintSpeed;
        }
    }
}