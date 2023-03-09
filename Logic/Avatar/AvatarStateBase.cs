using Animancer.FSM;

namespace NekoNeko.Avatar
{
    public abstract class AvatarStateBase : IState
    {
        public class StateMachine : StateMachine<AvatarStateBase>.WithDefault {}

        protected AvatarStateBase.StateMachine _stateMachine;
        protected TPSAvatarController _avatar;

        protected AvatarData _data;
        protected AvatarInput _input;
        protected AvatarMovement _movement;
        protected AvatarAim _aim;
        private TPSAvatarController avatar;

        public AvatarStateBase(TPSAvatarController avatar)
        {
            _stateMachine = avatar.StateMachine;
            _avatar = avatar;
            _data = avatar.Data;
            _input = avatar.Input;
            _movement = avatar.Movement;
            _aim = avatar.Aim;
        }

        public virtual bool CanEnterState => true;
        public virtual bool CanExitState {
            get {
                var nextState = this.GetNextState();
                if (nextState == this) return CanInterruptSelf;
                else if (Priority == AvatarStatePriority.Low) return true;
                else return nextState.Priority > Priority;
            }
        }
        public virtual AvatarStatePriority Priority => AvatarStatePriority.Low;
        public virtual bool CanInterruptSelf => false;


        public virtual void OnEnterState()
        {

        }

        public virtual void OnExitState()
        {

        }

        /// <summary>
        /// Evaluate transition conditions and calls state machine to initiate transitions.
        /// </summary>
        public virtual void OnCheckTransitions()
        {

        }

        /// <summary>
        /// Update tick.
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void OnUpdate(float deltaTime)
        {

        }
    }
}