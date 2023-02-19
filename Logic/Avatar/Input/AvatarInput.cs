using UnityEngine;
using UnityEngine.InputSystem;

namespace NekoNeko.Avatar
{
    public class AvatarInput : MonoBehaviour
    {
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private AvatarInputConfig _config;

        public InputAction Move { get; private set; }
        public InputAction Look { get; private set; }
        public InputAction Attack { get; private set; }
        public InputAction Interact { get; private set; }

        private void Start()
        {
            Move = _playerInput.actions[_config.Move];
            Look = _playerInput.actions[_config.Look];
            Attack = _playerInput.actions[_config.Fire];
            Interact = _playerInput.actions[_config.Interact];
        }
    }
}