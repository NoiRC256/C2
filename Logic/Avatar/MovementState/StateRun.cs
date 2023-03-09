using UnityEngine;
using Animancer;

namespace NekoNeko.Avatar
{
    public class StateRun : StateLocomotionBase
    {
        protected override ITransition LocomotionAnim => _movement.AnimsetConfig.Run;
        protected override LocomotionAnimConfig LocomotionAnimConfig => _movement.AnimsetConfig.RunConfig;

        public StateRun(TPSAvatarController avatar) : base(avatar)
        {
        }

        public override void OnEnterState()
        {
            base.OnEnterState();
               
            _data.MovementState = AvatarData.MovementStateType.Run;
        }

        public override void OnCheckTransitions()
        {
            base.OnCheckTransitions();

            if (_input.Sprint.IsPressed())
            {
                _stateMachine.TrySetState(_avatar.StateSprint);
                return;
            }

            if (_input.Walk.WasPressedThisFrame())
            {
                _data.WalkToggle = true;
                _stateMachine.TrySetState(_avatar.StateWalk);
                return;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            _movement.InputMove(GetMoveSpeed(), _input.Move.ReadValue<Vector2>());
            _movement.FacingHandler.RotateTowards(_data.LastNonZeroInputDirection);
        }

        protected override float GetMoveSpeed()
        {
            return _data.MovementConfig.RunSpeed;
        }
    }
}