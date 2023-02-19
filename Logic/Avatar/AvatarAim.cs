using NekoNeko.Cameras;
using UnityEngine;

namespace NekoNeko.Avatar
{
    public class AvatarAim : MonoBehaviour
    {
        [SerializeField] private TPCamera _tpCamera;
        [SerializeField] public float HorizontalSensitivity = 1f;
        [SerializeField] public float VerticalSensitivity = 1f;

        public AvatarData Data { get; set; }
        public AvatarInput Input { get; set; }

        private void Awake()
        {
        }

        public void OnUpdate(float deltaTime)
        {
            Vector2 _input = Input.Look.ReadValue<Vector2>();
            _input.x *= HorizontalSensitivity;
            _input.y *= VerticalSensitivity;
            _tpCamera.Input = _input;
        }
    }
}