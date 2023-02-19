using UnityEngine;

namespace NekoNeko.Cameras
{
    public class TPCameraRecoilProvider : TPCameraPluginBase
    {

        [field: Header("Recoil")]

        [field: Tooltip("Default time in seconds for recoil kicks to complete. " +
            "\nCan be overridden when initiating a recoil from method.")]
        [field: SerializeField] public float DefaultKickDuration { get; set; } = 0.04f;

        [field: Tooltip("Default speed at which the camera restores its rotation from recoil." +
            "\nCan be overridden when initiating a recoil from method.")]
        [field: SerializeField] public float DefaultRestoreSpeed { get; set; } = 25f;

        [field: Tooltip("Default delay in seconds for recoil restore to start after recoil kick completes." +
            "\nCan be overridden when initiating a recoil from method.")]
        [field: SerializeField] public float DefaultRestoreDelay { get; set; } = 0.12f;
        [field: SerializeField] public bool AllowVerticalRestore { get; set; } = true;
        [field: SerializeField] public bool AlloHorizontalRestore { get; set; } = true;

        private bool _isRecoiling = false;
        private bool _shouldRecoilKick = false;
        private bool _shouldRecoilRestore = false;
        private bool _shouldRecoilRestoreVertical = false;
        private bool _shouldRecoilRestoreHorizontal = false;
        private float _recoilKickElapsedTime = 0f;
        private float _recoilRestoreDelayElapsedTime = 0f;

        private float _recoilKickDuration = 0f;
        private float _recoilRestoreSpeed = 0f;
        private float _recoilRestoreDelay = 0f;

        private Vector3 _recoilStartRotation = Vector3.zero;
        private Vector3 _recoilTargetRotation = Vector3.zero;
        private Vector3 _recoilRestoreRotation = Vector3.zero;

        public override void OnPostUpdate(float deltaTime)
        {
            base.OnPostUpdate(deltaTime);

            // Apply recoil rotation.
            Vector3 pivotEulerAngles = _tpCamera.GetPivotEulerAngles();
            pivotEulerAngles = UpdateRecoil(pivotEulerAngles, deltaTime);
            if (_isRecoiling)
            {
                _recoilTargetRotation += _tpCamera.InputEulerAngles;
                if (_tpCamera.Input != Vector2.zero) RefreshRecoilRestoreRotation();
            }
            _tpCamera.SetPivotEulerAngles(pivotEulerAngles);
        }

        #region Recoil

        /// <summary>
        /// Perform recoil using the provided vertical recoil and horizontal recoil strengths.
        /// </summary>
        /// <param name="verticalRecoil"></param>
        /// <param name="horizontalRecoil"></param>
        public void DoRecoil(float verticalRecoil, float horizontalRecoil, bool verticalRestore = false, bool horizontalRestore = false)
        {
            if (!_isRecoiling) RefreshRecoilRestoreRotation();

            Vector3 curRotation = _tpCamera.GetPivotEulerAngles();
            _recoilTargetRotation.x = curRotation.x + verticalRecoil;
            _recoilTargetRotation.y = curRotation.y + horizontalRecoil;

            _isRecoiling = true;
            _shouldRecoilKick = true;
            _shouldRecoilRestore = false;
            if (AllowVerticalRestore) _shouldRecoilRestoreVertical = verticalRestore;
            if (AlloHorizontalRestore) _shouldRecoilRestoreHorizontal = horizontalRestore;
            _recoilKickElapsedTime = 0f;
            _recoilRestoreDelayElapsedTime = 0f;

            _recoilKickDuration = DefaultKickDuration;
            _recoilRestoreSpeed = DefaultRestoreSpeed;
            _recoilRestoreDelay = DefaultRestoreDelay;

            _recoilStartRotation = curRotation;
        }

        /// <summary>
        /// <inheritdoc cref="DoRecoil(float, float, bool, bool)"/>
        /// </summary>
        /// <param name="verticalRecoil"></param>
        /// <param name="horizontalRecoil"></param>
        /// <param name="kickDuration"></param>
        /// <param name="restoreSpeed"></param>
        /// <param name="restoreDelay"></param>
        /// <param name="verticalRestore"></param>
        /// <param name="horizontalRestore"></param>
        public void DoRecoil(float verticalRecoil, float horizontalRecoil,
            float kickDuration = -1f, float restoreSpeed = -1f, float restoreDelay = -1f,
            bool verticalRestore = false, bool horizontalRestore = false)
        {
            DoRecoil(verticalRecoil, horizontalRecoil, verticalRestore, horizontalRestore);

            if (kickDuration >= 0f) _recoilKickDuration = kickDuration;
            if (restoreSpeed >= 0f) _recoilRestoreSpeed = restoreSpeed;
            if (restoreDelay >= 0f) _recoilRestoreDelay = restoreDelay;
        }

        public void RefreshRecoilRestoreRotation()
        {
            _recoilRestoreRotation = _tpCamera.GetPivotEulerAngles();
        }

        /// <summary>
        /// Update recoil rotation and apply to the provided rotation.
        /// </summary>
        /// <param name="eulerAngles"></param>
        /// <param name="deltaTime"></param>
        private Vector3 UpdateRecoil(Vector3 eulerAngles, float deltaTime)
        {
            if (!_isRecoiling) return eulerAngles;
            if (_shouldRecoilKick)
            {
                UpdateRecoilKick(ref eulerAngles, deltaTime);
            }
            else
            {
                if (_recoilRestoreDelay <= 0f || _recoilRestoreDelayElapsedTime > _recoilRestoreDelay)
                {
                    UpdateRecoilRestore(ref eulerAngles, deltaTime);
                }
                else _recoilRestoreDelayElapsedTime += deltaTime;
            }
            return eulerAngles;
        }

        private void UpdateRecoilKick(ref Vector3 eulerAngles, float deltaTime)
        {
            if (_recoilKickDuration <= 0f)
            {
                eulerAngles.x = _recoilTargetRotation.x;
                eulerAngles.y = _recoilTargetRotation.y;
                return;
            }

            if (_recoilKickElapsedTime >= _recoilKickDuration)
            {
                _shouldRecoilKick = false;
                return;
            }

            _recoilKickElapsedTime += deltaTime;
            float lerpFactor = _recoilKickElapsedTime / _recoilKickDuration;
            float verticalRecoilRotation = Mathf.LerpAngle(_recoilStartRotation.x, _recoilTargetRotation.x, lerpFactor);
            float horizontalRecoilRotation = Mathf.LerpAngle(_recoilStartRotation.y, _recoilTargetRotation.y, lerpFactor);
            eulerAngles.x = verticalRecoilRotation;
            eulerAngles.y = horizontalRecoilRotation;
            eulerAngles = _tpCamera.RestrictRotation(eulerAngles);
        }

        private void UpdateRecoilRestore(ref Vector3 eulerAngles, float deltaTime)
        {
            if (_recoilRestoreSpeed <= 0f)
            {
                EndRecoil();
                return;
            }

            bool verticalRestoreed = true;
            bool horizontalRestoreed = true;

            if (_shouldRecoilRestoreVertical)
            {
                // Restore vertical rotation over time.
                eulerAngles.x = Mathf.MoveTowardsAngle(eulerAngles.x, _recoilRestoreRotation.x, _recoilRestoreSpeed * deltaTime);
                if (eulerAngles.x != _recoilRestoreRotation.x) verticalRestoreed = false;
            }

            if (_shouldRecoilRestoreHorizontal)
            {
                // Restore vertical rotation over time.
                eulerAngles.y = Mathf.MoveTowardsAngle(eulerAngles.y, _recoilRestoreRotation.y, _recoilRestoreSpeed * deltaTime);
                if (eulerAngles.y != _recoilRestoreRotation.y) horizontalRestoreed = false;
            }

            eulerAngles = _tpCamera.RestrictRotation(eulerAngles);

            if (verticalRestoreed && horizontalRestoreed)
            {
                EndRecoil();
            }
        }

        private void EndRecoil()
        {
            _shouldRecoilKick = false;
            _shouldRecoilRestore = false;
            _shouldRecoilRestoreVertical = false;
            _shouldRecoilRestoreHorizontal = false;
            _isRecoiling = false;
        }

        #endregion
    }
}