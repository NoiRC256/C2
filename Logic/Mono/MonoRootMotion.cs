using UnityEngine;

namespace NekoNeko
{
    /// <summary>
    /// Intercept and transfer animator root motion to specified transforms.
    /// </summary>
    public class MonoRootMotion : MonoBehaviour
    {
        [SerializeField] private bool _interceptRootMotion = true;

        private Animator _animator;
        public bool InterceptRootMotion {
            get => _interceptRootMotion;
            set => _interceptRootMotion = value;
        }
        public Vector3 RootMotionDeltaPosition => _animator.deltaPosition;
        public Quaternion RootMotionDeltaRotation => _animator.deltaRotation;

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
            if (_animator.hasRootMotion)
            {
                _animator.ApplyBuiltinRootMotion();
                Vector3 deltaPosition = _animator.deltaPosition;
                Quaternion deltaRotation = _animator.deltaRotation;
                if(_interceptRootMotion)
                {
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                }
            }
        }
    }
}