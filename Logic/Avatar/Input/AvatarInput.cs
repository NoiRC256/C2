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
        public InputAction Fire { get; private set; }
        public InputAction Sprint { get; private set; }
        public InputAction Walk { get; private set; }
        public InputAction Interact { get; private set; }

        private void Awake()
        {
            Move = _playerInput.actions[_config.Move];
            Look = _playerInput.actions[_config.Look];
            Fire = _playerInput.actions[_config.Fire];
            Sprint = _playerInput.actions[_config.Sprint];
            Walk = _playerInput.actions[_config.Walk];
            Interact = _playerInput.actions[_config.Interact];
        }
    }
}