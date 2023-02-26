using Animancer;
using UnityEngine;

namespace NekoNeko.Avatar
{
    public class StateIdleToMove : AvatarStateBase
    {
        AnimancerState _state;
        private float _counter;

        public StateIdleToMove(AvatarController avatar) : base(avatar)
        {
        }

        public override void OnEnterState()
        {
            if (_data.WalkToggle)
            {
                _stateMachine.TrySetState(_avatar.StateWalk);
                return;
            }

            _movement.RunStartL.Events.OnEnd = End;
            _movement.RunStartR.Events.OnEnd = End;

            if (_data.ForwardFoot == 0f) _state = _avatar.Animancer.Play(_movement.RunStartL);
            else _state = _avatar.Animancer.Play(_movement.RunStartR);

            _counter = 0f;

            _data.MovementState = AvatarData.MovementStateType.Run;
        }

        public override void OnCheckTransitions()
        {
            _counter += Time.deltaTime;

            if (_input.Walk.WasPressedThisFrame())
            {
                _data.WalkToggle = true;
                _stateMachine.TrySetState(_avatar.StateWalk);
                return;
            }

            if (!_input.Move.IsPressed() && _counter >= _movement.IdleToMoveMinExitTime)
            {
                _stateMachine.TrySetState(_avatar.StateMoveToIdle);
                return;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            float speedFactor = (_data.MovementConfig.RunSpeed / _movement.RunReferenceSpeed) * _data.MoveSpeedMultiplier.Value;
            _state.Speed = speedFactor;
            _movement.RootMotionInputMove(_movement.RootMotion.Velocity, _input.Move.ReadValue<Vector2>());
            _movement.FacingHandler.RotateTowards(_data.LastNonZeroInputDirection);
        }

        private void End()
        {
            _stateMachine.TrySetState(_avatar.StateRun);
        }

    }
}