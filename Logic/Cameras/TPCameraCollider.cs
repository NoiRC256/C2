using UnityEngine;

namespace NekoNeko.Cameras
{
    /// <summary>
    /// Plugin for <see cref="TPCamera"/>. Provides third-person camera collision support.
    /// </summary>
    public class TPCameraCollider : TPCameraPluginBase
    {
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _aimPointPositionOffset = 0f;
        [SerializeField] private float _camPositionOffset = 0f;
        [SerializeField] private float _probeRadius = 1f;
        [SerializeField] private float _restoreSpeed = 5f;
        [SerializeField] private bool _debugMode;

        private float _camDistance;
        private Vector3[] _adjustedCamClipPoints;
        private Vector3[] _desiredCamClipPoints;
        private bool _hasHit = false;
        private RaycastHit _hitInfo;

        public override void Init(TPCamera controller)
        {
            base.Init(controller);

            _camDistance = controller.CamDistance;
            _adjustedCamClipPoints = new Vector3[5];
            _desiredCamClipPoints = new Vector3[5];
        }
        public override void OnPostUpdate(float deltaTime)
        {
            base.OnPostUpdate(deltaTime);

            RestoreCamDistance(deltaTime);
            UpdateCamClipPoints(_tpCamera.CamPosition, _tpCamera.CamForward, _tpCamera.CamRotation, ref _desiredCamClipPoints);

            Vector3 direction = -_tpCamera.CamForward;
            Vector3 origin = _tpCamera.CamAimPointPosition + _aimPointPositionOffset * direction;
            Vector3 target = _tpCamera.CamPosition;
            float maxDistance = Mathf.Abs(_camDistance) - _camPositionOffset;

#if UNITY_EDITOR
            if (_debugMode)
            {
                Debug.DrawLine(origin, target, Color.magenta);
            }
#endif

            _hasHit = ProbeAndApply(origin, direction, maxDistance);
            _tpCamera.CamDistance = _camDistance;
        }

        private void UpdateCamClipPoints(Vector3 camPosition, Vector3 camForward, Quaternion rotation, ref Vector3[] clipPoints)
        {
            Camera cam = _tpCamera.Cam;
            if (cam == null) return;

            clipPoints = new Vector3[5];

            float z = cam.nearClipPlane;
            float x = Mathf.Tan(cam.fieldOfView / 3.41f) * z;
            float y = x / cam.aspect;

            // Top left.
            clipPoints[0] = (rotation * new Vector3(-x, y, z)) + camPosition;
            // Top right.
            clipPoints[1] = (rotation * new Vector3(x, y, z)) + camPosition;
            // Bottom left.
            clipPoints[2] = (rotation * new Vector3(-x, -y, z)) + camPosition;
            // Bottom right.
            clipPoints[3] = (rotation * new Vector3(x, -y, z)) + camPosition;
            // Middle.
            clipPoints[4] = camPosition - camForward;
        }

        private bool ProbeAndApply(Vector3 origin, Vector3 direction, float distance)
        {
            if (Physics.Raycast(origin, direction, out _hitInfo,
               maxDistance: distance, layerMask: _layerMask))
            {
                Vector3 localHitPoint = _tpCamera.PivotTr.InverseTransformPoint(_hitInfo.point);
                _camDistance = -localHitPoint.z;
                return true;
            }
            return false;
        }

        private void RestoreCamDistance(float deltaTime)
        {
            _camDistance = Mathf.Lerp(_camDistance, _tpCamera.CamBaseDistance, _restoreSpeed * deltaTime);
        }
    }
}
