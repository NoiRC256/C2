using Animancer.FSM;

namespace NekoNeko.Avatar
{
    public abstract class AvatarStateBase : IState
    {
        public class StateMachine : StateMachine<AvatarStateBase>.WithDefault { }

        protected AvatarStateBase.StateMachine _stateMachine;
        protected AvatarController _avatar;

        protected AvatarData _data;
        protected AvatarInput _input;
        protected AvatarMovement _movement;
        protected AvatarAim _aim;
        private AvatarController avatar;

        public AvatarStateBase(AvatarController avatar)
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

        public virtual void OnCheckTransitions()
        {

        }

        public virtual void OnUpdate(float deltaTime)
        {

        }
    }
}