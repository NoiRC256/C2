using Animancer;
using UnityEngine;

namespace NekoNeko.Avatar
{
    public class StateMoveToIdle : AvatarStateBase
    {
        private AnimancerState _state;

        public StateMoveToIdle(AvatarController avatar) : base(avatar)
        {
        }

        public override void OnEnterState()
        {
            _movement.InputMove(0f, Vector3.zero);

            switch (_data.MovementState)
            {
                case AvatarData.MovementStateType.Walk:
                    _movement.WalkStopL.Events.OnEnd = End;
                    _movement.WalkStopR.Events.OnEnd = End;
                    _state = PlayMoveStopClip(_movement.WalkStopL, _movement.WalkStopR);
                    break;
                case AvatarData.MovementStateType.Run:
                    _movement.RunStopL.Events.OnEnd = End;
                    _movement.RunStopR.Events.OnEnd = End;
                    _state = PlayMoveStopClip(_movement.RunStopL, _movement.RunStopR);
                    break;
                case AvatarData.MovementStateType.Sprint:
                    _data.ForwardFoot = 0f;
                    _state = _avatar.Animancer.Play(_movement.SprintStopR);
                    break;
            }
        }

        public override void OnExitState()
        {
            base.OnExitState();
        }

        public override void OnCheckTransitions()
        {
            if (_input.Move.IsPressed())
            {
                _stateMachine.TrySetState(_avatar.StateIdleToMove);
                return;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            if (_input.Walk.WasPressedThisFrame())
            {
                _data.WalkToggle = !_data.WalkToggle;
            }

            switch (_data.MovementState)
            {
                case AvatarData.MovementStateType.Walk:
                    _state.Speed = (_data.MovementConfig.RunSpeed / _avatar.StateRunConfig.ReferenceSpeed) * _data.MoveSpeedMultiplier.Value;
                    _movement.RootMotionInputMove();
                    break;
                case AvatarData.MovementStateType.Run:
                    break;
                case AvatarData.MovementStateType.Sprint:
                    _state.Speed = (_data.MovementConfig.SprintSpeed / _avatar.StateSprintConfig.ReferenceSpeed) * _data.MoveSpeedMultiplier.Value;
                    _movement.RootMotionInputMove();
                    break;
            }
        }

        private AnimancerState PlayMoveStopClip(ITransition left, ITransition right)
        {
            if (_data.ForwardFoot == 0f)
            {
                _data.ForwardFoot = 1f;
                return _avatar.Animancer.Play(left);
            }
            else
            {
                _data.ForwardFoot = 0f;
                return _avatar.Animancer.Play(right);
            }
        }

        private void End()
        {
            _stateMachine.TrySetState(_avatar.StateIdle);
        }
    }
}