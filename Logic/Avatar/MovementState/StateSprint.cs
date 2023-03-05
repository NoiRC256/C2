using UnityEngine;
using Animancer;

namespace NekoNeko.Avatar
{
    public class StateSprint : StateLocomotionBase
    {
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

            _state.Speed = (GetMoveSpeed() / GetMoveReferenceSpeed()) * _data.MoveSpeedMultiplier.Value;
            _data.ForwardFoot = _movement.EvaluateFootCycle(_state.NormalizedTime, GetFootCycleConfig());
            _movement.InputMove(GetMoveSpeed(), _input.Move.ReadValue<Vector2>());
            _movement.FacingHandler.RotateTowards(_data.LastNonZeroInputDirection);
        }

        protected override LocomotionAnimConfig GetLocomotionAnimConfig() => _movement.AnimationConfig.SprintConfig;

        protected override ITransition GetLocomotionAnimation()
        {
            return _movement.AnimationConfig.Sprint;
        }

        protected override float GetMoveSpeed()
        {
            return _data.MovementConfig.SprintSpeed;
        }
    }
}