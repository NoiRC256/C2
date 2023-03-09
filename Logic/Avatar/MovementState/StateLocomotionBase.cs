using Animancer;

namespace NekoNeko.Avatar
{
    /// <summary>
    /// Base state for locomotions like running, walking, sprinting.
    /// <para>Contains common logic for matching animation speed and updating foot cycle data.</para>
    /// </summary>
    public abstract class StateLocomotionBase : AvatarStateBase
    {
        protected Animancer.AnimancerState _state;
        protected abstract ITransition LocomotionAnim { get; }
        protected abstract LocomotionAnimConfig LocomotionAnimConfig { get; }

        protected StateLocomotionBase(TPSAvatarController avatar) : base(avatar)
        {
        }

        public override void OnEnterState()
        {
            // If previous state is also locomotion state, inherit the normalized time for locomotion cycle.
            if (_stateMachine.PreviousState is StateLocomotionBase)
            {
                float normalizedTime = _avatar.Animancer.States.Current.NormalizedTime;
                _state = _avatar.Animancer.Play(LocomotionAnim);
                _state.NormalizedTime += normalizedTime;
            }
            else
            {
                _state = _avatar.Animancer.Play(LocomotionAnim);
            }
        }

        public override void OnCheckTransitions()
        {
            if (!_input.Move.IsPressed())
            {
                _stateMachine.TrySetState(_avatar.StateMoveToIdle);
                return;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            UpdateStateSpeed();
            UpdateFootCycle();
        }

        protected virtual void UpdateStateSpeed()
        {
            _state.Speed = (GetMoveSpeed() / LocomotionAnimConfig.ReferenceSpeed) * _data.MoveSpeedMultiplier.Value;
        }

        protected virtual void UpdateFootCycle()
        {
            _data.ForwardFoot = _movement.EvaluateFootCycle(_state.NormalizedTime, LocomotionAnimConfig.FootCycle);
        }

        protected abstract float GetMoveSpeed();
    }
}
