using NekoNeko.Avatar;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NekoNeko.Avatar
{
    internal abstract class LocomotionState
    {
        protected AvatarLocomotion _locomotion;
        public LocomotionStateMachine StateMachine { get; private set; }
        public AvatarData Data { get; private set; }
        public AvatarInput Input { get; private set; }


        public LocomotionState(AvatarLocomotion locomotion, LocomotionStateMachine stateMachine)
        {
            _locomotion = locomotion;
            StateMachine = stateMachine;
            Data = _locomotion.Data;
            Input = _locomotion.Input;
        }

        public virtual void OnEnter() { }
        public virtual void OnPollTransitions() { }
        public virtual void OnUpdate(float deltaTime) { }
        public virtual void OnExit() { }
    }

    internal class StateIdle : LocomotionState
    {
        public StateIdle(AvatarLocomotion locomotion, LocomotionStateMachine stateMachine) : base(locomotion, stateMachine)
        {
        }

        public override void OnEnter()
        {
            Data.LocomotionState = AvatarLocomotionState.Idle;
            Input.Move.started += OnMoveInput;
            Input.Walk.started += OnWalkInput;
            _locomotion.InputMove(0f, Vector2.zero);
        }

        public override void OnExit()
        {
            Input.Move.started -= OnMoveInput;
            Input.Walk.started -= OnWalkInput;
        }

        public override void OnUpdate(float deltaTime)
        {
            _locomotion.Move(_locomotion.RootMotion.Velocity.magnitude, _locomotion.RootMotion.Velocity.normalized);
        }

        private void OnMoveInput(InputAction.CallbackContext ctx)
        {
            if(Data.WalkToggle) StateMachine.ChangeState(StateMachine.StateWalk);
            else StateMachine.ChangeState(StateMachine.StateRun);
        }

        public void OnWalkInput(InputAction.CallbackContext ctx)
        {
            Data.WalkToggle = !Data.WalkToggle;
        }
    }

    internal class StateRun : LocomotionState
    {
        public StateRun(AvatarLocomotion locomotion, LocomotionStateMachine stateMachine) : base(locomotion, stateMachine)
        {
        }

        public override void OnEnter()
        {
            Data.LocomotionState = AvatarLocomotionState.Run;
            Input.Walk.started += OnWalkInput;
        }

        public override void OnExit()
        {
            Input.Walk.started -= OnWalkInput;
        }

        public override void OnPollTransitions()
        {
            if (Input.Move.ReadValue<Vector2>() == Vector2.zero)
            {
                StateMachine.ChangeState(StateMachine.StateIdle);
                return;
            }
            if (Input.Sprint.IsPressed())
            {
                StateMachine.ChangeState(StateMachine.StateSprint);
                return;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            float moveSpeed = GetMoveSpeed();
            Data.LastMoveSpeed = moveSpeed;
            _locomotion.InputMove(GetInputSpeed(moveSpeed), Input.Move.ReadValue<Vector2>());
            _locomotion.FacingHandler.RotateTowards(Data.LastNonZeroInputDirection);
        }

        protected virtual float GetMoveSpeed()
        {
            return Data.MovementConfig.RunSpeed;
        }

        protected virtual float GetInputSpeed(float moveSpeed)
        {
            return moveSpeed * Data.MoveSpeedMultiplier.Value;
        }

        public void OnWalkInput(InputAction.CallbackContext ctx)
        {
            Data.WalkToggle = true;
            StateMachine.ChangeState(StateMachine.StateWalk);
        }
    }

    internal class StateSprint : StateRun
    {
        public StateSprint(AvatarLocomotion locomotion, LocomotionStateMachine stateMachine) : base(locomotion, stateMachine)
        {
        }

        public override void OnEnter()
        {
            Data.LocomotionState = AvatarLocomotionState.Sprint;
            Data.WalkToggle = false;
        }

        public override void OnExit()
        {
            
        }

        public override void OnPollTransitions()
        {
            if (Input.Move.ReadValue<Vector2>() == Vector2.zero)
            {
                StateMachine.ChangeState(StateMachine.StateIdle);
                return;
            }
            if (!Input.Sprint.IsPressed())
            {
                StateMachine.ChangeState(StateMachine.StateRun);
                return;
            }
        }

        protected override float GetMoveSpeed()
        {
            return Data.MovementConfig.SprintSpeed;
        }
    }

    internal class StateSprintTurn : LocomotionState
    {
        private float _duration = 0.5f;
        private float _elapsedTime = 0f;

        public StateSprintTurn(AvatarLocomotion locomotion, LocomotionStateMachine stateMachine) : base(locomotion, stateMachine)
        {
        }

        public override void OnEnter()
        {
            Data.LocomotionState = AvatarLocomotionState.RootMotion;
            _elapsedTime = 0f;
        }

        public override void OnExit()
        {
        }

        public override void OnPollTransitions()
        {
            if (_elapsedTime >= _duration)
            {
                StateMachine.ChangeState(StateMachine.StateSprint);
                return;
            }
        }

        public override void OnUpdate(float deltaTime)
        {

            _elapsedTime += deltaTime;
        }
    }

    internal class StateWalk : LocomotionState
    {
        public StateWalk(AvatarLocomotion locomotion, LocomotionStateMachine stateMachine) : base(locomotion, stateMachine)
        {
        }

        public override void OnEnter()
        {
            Data.LocomotionState = AvatarLocomotionState.Walk;
            Input.Walk.started += OnWalkInput;
        }

        public override void OnExit()
        {
            Input.Walk.started -= OnWalkInput;
        }

        public override void OnPollTransitions()
        {
            if (Input.Move.ReadValue<Vector2>() == Vector2.zero)
            {
                StateMachine.ChangeState(StateMachine.StateIdle);
                return;
            }
            if (Input.Sprint.IsPressed())
            {
                StateMachine.ChangeState(StateMachine.StateSprint);
                return;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            _locomotion.InputMove(_locomotion.RootMotion.Velocity.magnitude, Input.Move.ReadValue<Vector2>());
            _locomotion.FacingHandler.RotateTowards(Data.LastInputDirection);
        }

        public void OnWalkInput(InputAction.CallbackContext ctx)
        {
            Data.WalkToggle = false;
            StateMachine.ChangeState(StateMachine.StateRun);
        }
    }

}