using UnityEngine;
using Animancer;

namespace NekoNeko.Avatar
{
    public class AvatarController : MonoBehaviour
    {
        [field: SerializeField] public AvatarData Data { get; private set; }
        [field: SerializeField] public AvatarInput Input { get; private set; }
        [field: SerializeField] public AnimancerComponent Animancer { get; private set; }

        [field: SerializeField] public AvatarLocomotion Locomotion { get; private set; }
        [field: SerializeField] public AvatarAim Aim { get; private set; }

        [field: SerializeField] public AvatarStateBase.StateMachine StateMachine { get; private set; }
        public AvatarStateBase StateIdle { get; private set; }
        public AvatarStateBase StateIdleToMove { get; private set; }
        public AvatarStateBase StateRun { get; private set; }
        public AvatarStateBase StateMoveToIdle { get; private set; }


        private void Awake()
        {
            Locomotion.Data = Data;
            Locomotion.Input = Input;
            Aim.Data = Data;
            Aim.Input = Input;
            StateMachine = new AvatarStateBase.StateMachine();
        }

        private void Start()
        {
            StateIdle = new StateIdle(this);
            StateIdleToMove = new StateIdleToMove(this);
            StateRun = new StateRun(this);
            StateMoveToIdle = new StateMoveToIdle(this);
            StateMachine.DefaultState = StateIdle;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            StateMachine.CurrentState.OnCheckTransitions();
            StateMachine.CurrentState.OnUpdate(Time.deltaTime);
            Locomotion.OnUpdate(deltaTime);
            Aim.OnUpdate(deltaTime);
        }
    }
}