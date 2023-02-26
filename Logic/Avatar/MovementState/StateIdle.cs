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
            _movement.InputMove(0f, Vector3.zero);
            _data.ForwardFoot = 1f;

            _avatar.Animancer.Play(_movement.Idle);

            _data.MovementState = AvatarData.MovementStateType.Idle;
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
            if (_input.Walk.WasPressedThisFrame())
            {
                _data.WalkToggle = !_data.WalkToggle;
            }
        }
    }
}