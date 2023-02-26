using Animancer.FSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NekoNeko.Avatar
{
    public class StateSprint : StateLocomotionBase
    {
        public StateSprint(AvatarController avatar, LocomotionStateConfig config) : base(avatar, config)
        {
        }

        public override void OnEnterState()
        {
            base.OnEnterState();

            _data.MovementState = AvatarData.MovementStateType.Sprint;
        }

        public override void OnCheckTransitions()
        {
            base.OnCheckTransitions();

            if (!_input.Sprint.IsPressed())
            {
                _stateMachine.TrySetState(_avatar.StateRun);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (_input.Walk.WasPressedThisFrame())
            {
                _data.WalkToggle = !_data.WalkToggle;
            }

            float speedFactor = (GetMoveSpeed() / GetMoveReferenceSpeed()) * _data.MoveSpeedMultiplier.Value;
            _state.Speed = speedFactor;
            _data.ForwardFoot = _movement.EvaluateFootCycle(_state.NormalizedTime, GetFootCycleConfig());
            _movement.InputMove(GetMoveSpeed(), _input.Move.ReadValue<Vector2>());
            _movement.FacingHandler.RotateTowards(_data.LastNonZeroInputDirection);
        }

        protected override float GetMoveSpeed()
        {
            return _data.MovementConfig.SprintSpeed;
        }
    }
}