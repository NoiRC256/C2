using UnityEngine;

namespace NekoNeko
{
    /// <summary>
    /// Relays animator root motion.
    /// </summary>
    public class MonoRootMotion : MonoBehaviour
    {
        [SerializeField] private bool _applyBuiltInRootMotion = false;

        private Animator _animator;

        public bool ApplyBuiltinRootMotion {
            get => _applyBuiltInRootMotion;
            set => _applyBuiltInRootMotion = value;
        }
        public Vector3 DeltaPosition => _animator.deltaPosition;
        public Quaternion DeltaRotation => _animator.deltaRotation;
        public Vector3 Velocity => _animator.velocity;

        private void OnValidate()
        {
            TryGetComponent(out _animator);
        }

        private void Awake()
        {
            OnValidate();
        }

        private void OnAnimatorMove()
        {
            if(_applyBuiltInRootMotion) _animator.ApplyBuiltinRootMotion();
        }
    }
}