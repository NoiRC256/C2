using UnityEngine;

namespace NekoNeko.Avatar
{
    public class StateIdle : AvatarStateBase
    {
        public StateIdle(AvatarController avatar) : base(avatar)
        {
        }

        public override void OnEnterState()
        {
            _locomotion.InputMove(0f, Vector3.zero);
            _data.ForwardFoot = 1f;

            _avatar.Animancer.Play(_locomotion.Idle);
        }

        public override void OnCheckTransitions()
        {
            if (_input.Move.IsPressed())
            {
                _stateMachine.TrySetState(_avatar.StateIdleToMove);
            }
        }
    }
}