using UnityEngine;

namespace NekoNeko.Avatar
{
    public class StateWalk : AvatarStateBase
    {
        protected Animancer.AnimancerState _state;

        public StateWalk(AvatarController avatar) : base(avatar)
        {
        }

        public override void OnEnterState()
        {
            if (!_avatar.Animancer.IsPlaying(_locomotion.MovementMixer))
            {
                _state = _avatar.Animancer.Play(_locomotion.MovementMixer);
            }
            else _state = _avatar.Animancer.States.Current;

            if (_stateMachine.PreviousState is StateRun == false)
            {
                if (_data.ForwardFoot == 1f) _state.NormalizedTime += _locomotion.RunFootCycle.Duration;
            }

            _locomotion.MovementMixer.State.Parameter = 0f;
            _data.LocomotionState = AvatarData.LocomotionStateType.Walk;
        }

        public override void OnCheckTransitions()
        {
            if (!_input.Move.IsPressed())
            {
                _stateMachine.TrySetState(_avatar.StateMoveToIdle);
                return;
            }

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
            //float speedFactor = (_data.MovementConfig.WalkSpeed / _locomotion.WalkReferenceSpeed) * _data.MoveSpeedMultiplier.Value;
            //_state.Speed = speedFactor;
            //_locomotion.RootMotionInputMove(_locomotion.RootMotion.Velocity, _input.Move.ReadValue<Vector2>());
            //_locomotion.FacingHandler.RotateTowards(_data.LastNonZeroInputDirection);

            _locomotion.MoveDeltaPosition(_locomotion.RootMotion.DeltaPosition);
        }
    }
}