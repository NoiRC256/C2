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
            _locomotion.InputMove(0f, Vector3.zero);
            //_locomotion.RootMotionInputMove(Vector3.zero);
            _locomotion.RunStopL.Events.OnEnd = End;
            _locomotion.RunStopR.Events.OnEnd = End;

            if (_data.ForwardFoot == 0f)
            {
                _data.ForwardFoot = 1f;
                _state = _avatar.Animancer.Play(_locomotion.RunStopL);
            }
            else
            {
                _data.ForwardFoot = 0f;
                _state = _avatar.Animancer.Play(_locomotion.RunStopR);
            }
        }

        public override void OnCheckTransitions()
        {
            if (_input.Move.IsPressed())
            {
                _stateMachine.TrySetState(_avatar.StateIdleToMove);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            //float speedFactor = (_data.MovementConfig.RunSpeed / _locomotion.RunReferenceSpeed) * _data.MoveSpeedMultiplier.Value;
            //_state.Speed = speedFactor;
            //_locomotion.MoveDeltaPosition(_locomotion.RootMotion.DeltaPosition);
        }

        private void End()
        {
            _stateMachine.TrySetState(_avatar.StateIdle);
        }
    }
}