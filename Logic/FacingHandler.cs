using UnityEngine;

namespace NekoNeko
{
    public class FacingHandler
    {
        public float CurrentFacing { get; private set; } = 0f;

        private float _targetFacing = 0f;
        private float _facingSmoothVelocity = 0f;

        public void SetTargetFacing(Vector3 direction)
        {
            _targetFacing = FacingAngleFromDirection(direction);
        }

        public void UpdateFacing(float deltaTime, float smoothDuration)
        {
            if (CurrentFacing == _targetFacing) return;
            CurrentFacing = Mathf.SmoothDampAngle(CurrentFacing, _targetFacing, ref _facingSmoothVelocity, smoothDuration);
        }

        public void UpdateFacing(float deltaTime, Vector3 direction, float smoothDuration)
        {
            SetTargetFacing(direction);
            UpdateFacing(deltaTime, smoothDuration);
        }

        public void UpdateFacing(float deltaTime, float targetFacing, float smoothDuration)
        {
            _targetFacing = targetFacing;
            UpdateFacing(deltaTime, smoothDuration);
        }

        public static float FacingAngleFromDirection(Vector3 direction)
        {
            return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        }
    }
}