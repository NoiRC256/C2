using UnityEngine;

namespace NekoNeko.Avatar
{
    public class StateRun : AvatarStateBase
    {
        protected Animancer.AnimancerState _state;

        public StateRun(AvatarController avatar) : base(avatar)
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

            _locomotion.MovementMixer.State.Parameter = 1f;
            _data.LocomotionState = AvatarData.LocomotionStateType.Run;
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
                _stateMachine.TrySetState(_avatar.StateSprint);
                return;
            }

            if (_data.WalkToggle)
            {
                _stateMachine.TrySetState(_avatar.StateWalk);
                return;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            _locomotion.InputMove(GetMoveSpeed(), _input.Move.ReadValue<Vector2>());
            _locomotion.FacingHandler.RotateTowards(_data.LastNonZeroInputDirection);

            float speedFactor = (GetMoveSpeed() / GetMoveReferenceSpeed()) * _data.MoveSpeedMultiplier.Value;
            _state.Speed = speedFactor;

            _data.ForwardFoot = _locomotion.EvaluateFootCycle(_state.NormalizedTime, GetFootCycleConfig());
        }

        protected virtual float GetMoveSpeed()
        {
            return _data.MovementConfig.RunSpeed;
        }

        protected virtual float GetMoveReferenceSpeed()
        {
            return _locomotion.RunReferenceSpeed;
        }

        protected virtual FootCycleConfig GetFootCycleConfig()
        {
            return _locomotion.RunFootCycle;
        }
    }
}