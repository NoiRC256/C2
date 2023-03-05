using Animancer;
using UnityEngine;

namespace NekoNeko.Avatar
{
    public class StateIdleToMove : AvatarStateBase
    {
        AnimancerState _state;
        private float _counter = 0f;

        public StateIdleToMove(TPSAvatarController avatar) : base(avatar)
        {
        }

        public override void OnEnterState()
        {
            if (_data.WalkToggle)
            {
                _stateMachine.TrySetState(_avatar.StateWalk);
                return;
            }

            _movement.AnimationConfig.RunStartL.Events.OnEnd = End;
            _movement.AnimationConfig.RunStartR.Events.OnEnd = End;

            if (_data.ForwardFoot == 0f) _state = _avatar.Animancer.Play(_movement.AnimationConfig.RunStartL);
            else _state = _avatar.Animancer.Play(_movement.AnimationConfig.RunStartR);

            _counter = 0f;

            _data.MovementState = AvatarData.MovementStateType.Run;
        }

        public override void OnExitState()
        {
            base.OnExitState();
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

            if (_state.NormalizedTime >= _movement.AnimationConfig.RunStartMaxExitTime)
            {
                _stateMachine.TrySetState(_avatar.StateRun);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            _state.Speed = (_data.MovementConfig.RunSpeed / _movement.AnimationConfig.RunConfig.ReferenceSpeed) * _data.MoveSpeedMultiplier.Value;
            _movement.RootMotionInputMove(_input.Move.ReadValue<Vector2>());
            _movement.FacingHandler.RotateTowards(_data.LastNonZeroInputDirection);
        }

        private void End()
        {
            _stateMachine.TrySetState(_avatar.StateRun);
        }

    }
}