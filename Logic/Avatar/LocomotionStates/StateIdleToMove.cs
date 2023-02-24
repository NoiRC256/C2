using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NekoNeko.Avatar
{
    public class StateIdleToMove : AvatarStateBase
    {
        public StateIdleToMove(AvatarController avatar) : base(avatar)
        {
        }

        public override void OnEnterState()
        {
            _locomotion.RunStartL.Events.OnEnd = End;
            _locomotion.RunStartR.Events.OnEnd = End;

            if (_data.ForwardFoot == 0f) _avatar.Animancer.Play(_locomotion.RunStartL);
            else _avatar.Animancer.Play(_locomotion.RunStartR);
        }

        public override void OnCheckTransitions()
        {
            if (!_input.Move.IsPressed())
            {
                _stateMachine.TrySetState(_avatar.StateMoveToIdle);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            _locomotion.RootMotionInputMove(_locomotion.RootMotion.Velocity, _input.Move.ReadValue<Vector2>());
            _locomotion.FacingHandler.RotateTowards(_data.LastNonZeroInputDirection);
        }

        private void End()
        {
            _stateMachine.TrySetState(_avatar.StateRun);
        }

    }
}