using Animancer;
using UnityEngine;

namespace NekoNeko.Avatar
{
    public class StateWalk : StateLocomotionBase
    {
        public StateWalk(TPSAvatarController avatar) : base(avatar)
        {
        }

        public override void OnEnterState()
        {
            base.OnEnterState();

            _data.MovementState = AvatarData.MovementStateType.Walk;
        }

        public override void OnExitState()
        {
            base.OnExitState();
        }

        public override void OnCheckTransitions()
        {
            base.OnCheckTransitions();

            if (_input.Sprint.IsPressed())
            {
                _data.WalkToggle = false;
                _stateMachine.TrySetState(_avatar.StateSprint);
                return;
            }

            if (_input.Walk.WasPressedThisFrame())
            {
                _data.WalkToggle = false;
                _stateMachine.TrySetState(_avatar.StateRun);
                return;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            _state.Speed = (_data.MovementConfig.WalkSpeed / GetMoveReferenceSpeed()) * _data.MoveSpeedMultiplier.Value;
            _data.ForwardFoot = _movement.EvaluateFootCycle(_state.NormalizedTime, GetFootCycleConfig());
            _movement.RootMotionInputMove(_input.Move.ReadValue<Vector2>());
            _movement.FacingHandler.RotateTowards(_data.LastNonZeroInputDirection);
        }

        protected override LocomotionAnimConfig GetLocomotionAnimConfig() => _movement.AnimationConfig.WalkConfig;

        protected override ITransition GetLocomotionAnimation()
        {
            return _movement.AnimationConfig.Walk;
        }

        protected override float GetMoveSpeed()
        {
            return _data.MovementConfig.WalkSpeed;
        }
    }
}