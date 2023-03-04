using UnityEngine;
using Animancer;

namespace NekoNeko.Avatar
{
    /// <summary>
    /// Main avatar controller.
    /// </summary>
    public class AvatarController : MonoBehaviour
    {
        [field: SerializeField] public AvatarData Data { get; private set; }
        [field: SerializeField] public AvatarInput Input { get; private set; }
        [field: SerializeField] public AnimancerComponent Animancer { get; private set; }

        [field: SerializeField] public AvatarMovement Movement { get; private set; }
        [field: SerializeField] public AvatarAim Aim { get; private set; }

        [field: SerializeField] public AvatarStateBase.StateMachine StateMachine { get; private set; }
        [field: SerializeField] public LocomotionStateConfig StateWalkConfig { get; private set; }
        [field: SerializeField] public LocomotionStateConfig StateRunConfig { get; private set; }
        [field: SerializeField] public LocomotionStateConfig StateSprintConfig { get; private set; }

        public AvatarStateBase StateIdle { get; private set; }
        public AvatarStateBase StateIdleToMove { get; private set; }
        public AvatarStateBase StateWalk { get; private set; }
        public AvatarStateBase StateRun { get; private set; }
        public AvatarStateBase StateSprint { get; private set; }
        public AvatarStateBase StateMoveToIdle { get; private set; }


        private void Awake()
        {
            Movement.Data = Data;
            Movement.Input = Input;
            Aim.Data = Data;
            Aim.Input = Input;
            StateMachine = new AvatarStateBase.StateMachine();
        }

        private void Start()
        {
            StateIdle = new StateIdle(this);
            StateIdleToMove = new StateIdleToMove(this);
            StateWalk = new StateWalk(this, StateWalkConfig);
            StateRun = new StateRun(this, StateRunConfig);
            StateSprint = new StateSprint(this, StateSprintConfig);
            StateMoveToIdle = new StateMoveToIdle(this);
            StateMachine.DefaultState = StateIdle;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            StateMachine.CurrentState.OnCheckTransitions();
            StateMachine.CurrentState.OnUpdate(Time.deltaTime);
            Movement.OnUpdate(deltaTime);
            Aim.OnUpdate(deltaTime);
        }
    }
}