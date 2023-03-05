using Animancer;
using UnityEngine;

namespace NekoNeko.Avatar
{
    public abstract class StateLocomotionBase : AvatarStateBase
    {
        protected Animancer.AnimancerState _state;

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
            get => _movement.AnimationConfig.MovementMixer.State.Parameter;
            set => _movement.AnimationConfig.MovementMixer.State.Parameter = value;
        }

        protected bool _isBlending;
        protected float _blendElapsedTime;

        protected StateLocomotionBase(TPSAvatarController avatar) : base(avatar)
        {
        }

        public override void OnEnterState()
        {
            if (_stateMachine.PreviousState is StateLocomotionBase)
            {
                float normalizedTime = _avatar.Animancer.States.Current.NormalizedTime;
                _state = _avatar.Animancer.Play(GetLocomotionAnimation());
                _state.NormalizedTime += normalizedTime;
            }
            else
            {
                _state = _avatar.Animancer.Play(GetLocomotionAnimation());
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

        protected abstract LocomotionAnimConfig GetLocomotionAnimConfig();

        protected abstract ITransition GetLocomotionAnimation();

        protected abstract float GetMoveSpeed();

        protected virtual float GetMoveReferenceSpeed() => GetLocomotionAnimConfig().ReferenceSpeed;

        protected virtual FootCycleConfig GetFootCycleConfig() => GetLocomotionAnimConfig().FootCycle;
    }
}
