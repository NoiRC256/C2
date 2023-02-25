using Animancer.FSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NekoNeko.Avatar
{
    public class StateSprint : StateRun
    {
        public StateSprint(AvatarController avatar) : base(avatar)
        {
        }

        public override void OnEnterState()
        {
            base.OnEnterState();

            _locomotion.MovementMixer.State.Parameter = 2f;
            _data.LocomotionState = AvatarData.LocomotionStateType.Sprint;
        }

        public override void OnCheckTransitions()
        {
            if (!_input.Move.IsPressed())
            {
                _stateMachine.TrySetState(_avatar.StateMoveToIdle);
                return;
            }

            if (!_input.Sprint.IsPressed())
            {
                _stateMachine.TrySetState(_avatar.StateRun);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
        }

        protected override float GetMoveSpeed()
        {
            return _data.MovementConfig.SprintSpeed;
        }

        protected override float GetMoveReferenceSpeed()
        {
            return _locomotion.SprintReferenceSpeed;
        }

        protected override FootCycleConfig GetFootCycleConfig()
        {
            return _locomotion.SprintFootCycle;
        }
    }
}