using System.Diagnostics;

namespace NekoNeko.Avatar
{
    internal class LocomotionStateMachine
    {
        public AvatarLocomotion _locomotion;
        public LocomotionState CurrentState { get; private set; }
        public StateIdle StateIdle;
        public StateRun StateRun;
        public StateSprint StateSprint;
        public StateSprintTurn StateSprintTurn;
        public StateWalk StateWalk;

        public LocomotionStateMachine(AvatarLocomotion locomotion)
        {
            _locomotion = locomotion;
            Init();
        }

        public void Init()
        {
            StateIdle = new StateIdle(_locomotion, this);
            StateRun = new StateRun(_locomotion, this);
            StateSprint = new StateSprint(_locomotion, this);
            StateSprintTurn = new StateSprintTurn(_locomotion, this);
            StateWalk = new StateWalk(_locomotion, this);
            ChangeState(StateIdle);
        }

        public void OnUpdate(float deltaTime)
        {
            CurrentState.OnPollTransitions();
            CurrentState.OnUpdate(deltaTime);
        }

        public void ChangeState(LocomotionState state)
        {
            CurrentState?.OnExit();
            state.OnEnter();
            CurrentState = state;
        }
    }
}