using UnityEngine;

namespace NekoNeko
{
    public class FacingHandler
    {
        [SerializeField] public float DefaultSmoothDuration { get; set; } = 0.15f;

        public float CurrentFacing { get; private set; } = 0f;

        private float _targetFacing = 0f;
        private float _facingSmoothVelocity = 0f;
        private bool _shouldTurn = false;
        private float _smoothDuration;

        public void RefreshFacing(Quaternion rotation)
        {
            _targetFacing = rotation.eulerAngles.y;
            CurrentFacing = _targetFacing;
            _shouldTurn = false;
        }

        public void RotateTowards(Vector3 direction)
        {
            SetTargetFacing(direction);
            if (CurrentFacing == _targetFacing) return;
            _shouldTurn = true;
            _smoothDuration = DefaultSmoothDuration;
        }

        public void RotateTowards(Vector3 direction, float smoothDuration)
        {
            RotateTowards(direction);
            _smoothDuration = smoothDuration;
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_shouldTurn) return;
            if (_smoothDuration <= 0f)
            {
                CurrentFacing = _targetFacing;
                _shouldTurn = false;
                return;
            }

            CurrentFacing = Mathf.SmoothDampAngle(CurrentFacing, _targetFacing, ref _facingSmoothVelocity, _smoothDuration);

            if (CurrentFacing == _targetFacing)
            {
                _shouldTurn = false;
            }
        }

        private void SetTargetFacing(Vector3 direction)
        {
            _targetFacing = FacingAngleFromDirection(direction);
        }

        public static float FacingAngleFromDirection(Vector3 direction)
        {
            return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        }
    }
}