using UnityEngine;

namespace NekoNeko.Avatar
{
    public abstract class StateLocomotionBase : AvatarStateBase
    {
        protected Animancer.AnimancerState _state;

        protected LocomotionStateConfig Config { get; private set; }

        // Mixer.
        protected float CurrentBlend { 
            get => _data.LocomotionBlend; 
            set => _data.LocomotionBlend = value; 
        }
        protected float TargetBlend {
            get => _data.LocomotionTargetBlend;
            set => _data.LocomotionTargetBlend = value;
        }
        protected float BlendParameter {
            get => _movement.MovementMixer.State.Parameter;
            set => _movement.MovementMixer.State.Parameter = value;
        }

        protected bool _isBlending;
        protected float _blendElapsedTime;

        public StateLocomotionBase(AvatarController avatar, LocomotionStateConfig config) : base(avatar)
        {
            Config = config;
        }

        public override void OnEnterState()
        {
            if (_stateMachine.PreviousState is StateLocomotionBase == false)
            {
                _state = _avatar.Animancer.Play(_movement.MovementMixer);
                if (_data.ForwardFoot == 1f) _state.NormalizedTime += _movement.RunFootCycle.Duration;
                BlendParameter = CurrentBlend = TargetBlend = Config.DefaultBlendParameter;
            }
            else
            {
                _state = _avatar.Animancer.States.Current;
                StateLocomotionBase previousState = (StateLocomotionBase)_stateMachine.PreviousState;
                float parameter = GetDefaultBlendParameter(previousState);
                CurrentBlend = parameter;
                BlendParameterTo(Config.DefaultBlendParameter);
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
            UpdateParameterBlend(deltaTime);
        }

        public virtual float GetDefaultBlendParameter(StateLocomotionBase locomotionState)
        {
            return locomotionState.Config.DefaultBlendParameter;
        }

        protected virtual void BlendParameterTo(float value)
        {
            if (Config.BlendDuration <= 0f)
            {
                BlendParameter = value;
                return;
            }
            TargetBlend = value;
            _isBlending = true;
            _blendElapsedTime = 0f;
        }

        protected virtual void UpdateParameterBlend(float deltaTime)
        {
            if (!_isBlending) return;
            _blendElapsedTime += deltaTime;
            float blendSpeed = Mathf.Abs(TargetBlend - CurrentBlend) / (Config.BlendDuration - _blendElapsedTime);
            CurrentBlend = Mathf.MoveTowards(CurrentBlend, TargetBlend, blendSpeed * deltaTime);
            BlendParameter = CurrentBlend;
            if(CurrentBlend == TargetBlend)
            {
                _isBlending = false;
            }
        }

        protected abstract float GetMoveSpeed();

        protected virtual float GetMoveReferenceSpeed() => Config.ReferenceSpeed;

        protected virtual FootCycleConfig GetFootCycleConfig() => Config.FootCycle;
    }
}
