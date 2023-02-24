using UnityEngine;

namespace NekoNeko.Avatar
{
    public class StateRun : AvatarStateBase
    {
        private Animancer.AnimancerState _runAnimState;

        public StateRun(AvatarController avatar) : base(avatar)
        {
        }

        public override void OnEnterState()
        {
            _runAnimState = _avatar.Animancer.Play(_locomotion.Run);
            if (_data.ForwardFoot == 1f) _runAnimState.NormalizedTime += _locomotion.RunFootCycle.Duration;
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
            _locomotion.InputMove(GetMoveSpeed(), _input.Move.ReadValue<Vector2>());
            _locomotion.FacingHandler.RotateTowards(_data.LastNonZeroInputDirection);

            _data.ForwardFoot = _locomotion.EvaluateFootCycle(_runAnimState.NormalizedTime, _locomotion.RunFootCycle);
        }

        private float GetMoveSpeed()
        {
            return _data.MovementConfig.RunSpeed;
        }
    }
}