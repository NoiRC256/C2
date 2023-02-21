using UnityEngine;

namespace NekoNeko.Cameras
{
    /// <summary>
    /// Camera controller.
    /// </summary>

    [DisallowMultipleComponent]
    public class TPCamera : MonoBehaviour
    {
        #region Exposed Fields

        [SerializeField] protected Transform _pivotTr;
        [SerializeField] protected Transform _camTr;

        [field: Header("Orbit")]
        [field: SerializeField] public float HorizontalSensitivity { get; set; } = 10f;
        [field: SerializeField] public float VerticalSensitivity { get; set; } = 10f;

        [field: Tooltip("Vertical angle limit at the top")]
        [field: SerializeField][field: Range(-89.9f, 89.9f)] public float TopAngleLimit { get; set; } = -89.9f;

        [field: Tooltip("Vertical angle limit at the bottom")]
        [field: SerializeField][field: Range(-89.9f, 89.9f)] public float BottomAngleLimit { get; set; } = 89.9f;
        [field: Tooltip("Desired camera distance")]
        [field: SerializeField] public float CamBaseDistance { get; set; } = 5f;

        [Header("Optional")]
        [SerializeField] protected TPCameraPluginBase[] _plugins;
        [field: SerializeField] public Camera Cam { get; private set; }

        #endregion

        #region Properties

        public Transform PivotTr => _pivotTr;
        public Vector3 CamPosition => _camTr.position;
        public Quaternion CamRotation => _camTr.rotation;
        public Vector3 CamForward => _camTr.forward;
        public Vector3 CamAimPointPosition { get; private set; }
        public Vector3 CamBaseLocalPosition { get; set; }
        public bool HasPivotTr => _pivotTr != null;
        public bool HasCamTr => _camTr != null;

        /// <summary>
        /// Current camera distance.
        /// </summary>
        public float CamDistance {
            get => _camDistance;
            set {
                _camDistance = value;
                Vector3 camLocalPosition = GetCamLocalPosition();
                camLocalPosition.z = -CamDistance;
                SetCamLocalPosition(camLocalPosition);
            }
        }

        /// <summary>
        /// Camera rotation input.
        /// </summary>
        public Vector2 Input { get; set; }
        /// <summary>
        /// The last calculated input rotation euler angles.
        /// </summary>
        public Vector3 InputEulerAngles { get; private set; }

        #endregion

        private float _camDistance;

        #region MonoBehaviour

        private void Reset()
        {
            // Check duplicate.
            TPCameraCollider[] tmp = GetComponents<TPCameraCollider>();
            if (tmp.Length > 0 && (tmp[0] != this || tmp.Length > 1))
            {
                Debug.LogWarning("There can only be one TPSCameraController component on a gameobject.");
                Component.DestroyImmediate(this);
            }
        }

        protected virtual void Awake()
        {
            if(_pivotTr == null) _pivotTr = this.transform;
            if (_camTr == null) _camTr = _pivotTr;
            if(Cam == null) Cam = Camera.main;
            CamDistance = CamBaseDistance;

            for (int i = 0; i < _plugins.Length; i++)
            {
                _plugins[i].Init(this);
            }
        }

        protected virtual void LateUpdate()
        {
            OnUpdate(Time.deltaTime);
        }

        #endregion

        #region Logic

        public void Activate()
        {
            this.gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            this.gameObject.SetActive(false);
        }

        public void OnUpdate(float deltaTime)
        {
            ExecutePluginsPreUpdate(deltaTime);

            // Apply input rotation.
            InputEulerAngles = CalcInputRotation(deltaTime);
            SetPivotEulerAngles(GetPivotEulerAngles() + InputEulerAngles);

            ExecutePluginsPostUpdate(deltaTime);

            OnUpdateEnd();
        }

        // Run plugins pre update.
        private void ExecutePluginsPreUpdate(float deltaTime)
        {
            for (int i = 0; i < _plugins.Length; i++)
            {
                _plugins[i].OnPreUpdate(deltaTime);
            }
        }

        // Run plugins post update.
        private void ExecutePluginsPostUpdate(float deltaTime)
        {
            for (int i = 0; i < _plugins.Length; i++)
            {
                _plugins[i].OnPostUpdate(deltaTime);
            }
        }

        private void OnUpdateEnd()
        {
            // Clean up.
            CamAimPointPosition = _camTr.position + _camTr.forward * -_camTr.localPosition.z;
            Input = Vector2.zero;
            InputEulerAngles = Vector3.zero;
        }

        private Vector3 CalcInputRotation(float deltaTime)
        {
            float verticalInput = Input.x * VerticalSensitivity;
            float horizontalInput = Input.y * HorizontalSensitivity;
            return new Vector3(-horizontalInput * deltaTime, verticalInput * deltaTime, 0f);
        }

        #endregion

        #region Helpers

        public Vector3 RestrictRotation(Vector3 eulerAngles)
        {
            eulerAngles.x = ClampAngle(eulerAngles.x, TopAngleLimit, BottomAngleLimit);
            return eulerAngles;
        }

        /// <summary>
        /// Normalize the provided angle to [-180, 180] and clamp it by the provided min and max.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public float ClampAngle(float angle, float min, float max)
        {
            return Mathf.Clamp(NormalizeAngle(angle), min, max);
        }

        public float NormalizeAngle(float angle)
        {
            angle %= 360;
            angle = angle > 180 ? angle - 360 : angle;
            return angle;
        }

        public Vector3 GetPivotEulerAngles()
        {
            return _pivotTr.localEulerAngles;
        }

        public void SetPivotEulerAngles(Vector3 eulerAngles)
        {
            _pivotTr.localEulerAngles = RestrictRotation(eulerAngles);
        }

        public Vector3 GetPivotLocalPosition()
        {
            return _pivotTr.localPosition;
        }

        public void SetPivotLocalPosition(Vector3 localPosition)
        {
            _pivotTr.localPosition = localPosition;
        }

        public Vector3 GetCamLocalPosition()
        {
            return _camTr.localPosition;
        }

        public void SetCamLocalPosition(Vector3 localPosition)
        {
            _camTr.localPosition = localPosition;
        }

        #endregion
    }
}
