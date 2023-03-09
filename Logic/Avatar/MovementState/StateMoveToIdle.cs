using Animancer;
using UnityEngine;

namespace NekoNeko.Avatar
{
    public class StateMoveToIdle : AvatarStateBase
    {
        private AnimancerState _state;

        public StateMoveToIdle(TPSAvatarController avatar) : base(avatar)
        {
        }

        public override void OnEnterState()
        {
            _movement.InputMove(0f, Vector3.zero);

            switch (_data.MovementState)
            {
                case AvatarData.MovementStateType.Walk:
                    _movement.AnimsetConfig.WalkStopL.Events.OnEnd = End;
                    _movement.AnimsetConfig.WalkStopR.Events.OnEnd = End;
                    _state = PlayMoveStopClip(_movement.AnimsetConfig.WalkStopL, _movement.AnimsetConfig.WalkStopR);
                    break;
                case AvatarData.MovementStateType.Run:
                    _movement.AnimsetConfig.RunStopL.Events.OnEnd = End;
                    _movement.AnimsetConfig.RunStopR.Events.OnEnd = End;
                    _state = PlayMoveStopClip(_movement.AnimsetConfig.RunStopL, _movement.AnimsetConfig.RunStopR);
                    break;
                case AvatarData.MovementStateType.Sprint:
                    _data.ForwardFoot = 0f;
                    _state = _avatar.Animancer.Play(_movement.AnimsetConfig.SprintStopR);
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
                    _state.Speed = (_data.MovementConfig.RunSpeed / _movement.AnimsetConfig.WalkConfig.ReferenceSpeed) * _data.MoveSpeedMultiplier.Value;
                    _movement.RootMotionInputMove();
                    break;
                case AvatarData.MovementStateType.Run:
                    break;
                case AvatarData.MovementStateType.Sprint:
                    _state.Speed = (_data.MovementConfig.SprintSpeed / _movement.AnimsetConfig.SprintConfig.ReferenceSpeed) * _data.MoveSpeedMultiplier.Value;
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