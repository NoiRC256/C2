using UnityEngine;
using UnityEngine.InputSystem;

namespace NekoNeko.Cameras
{
    [DisallowMultipleComponent]
    public class TPCameraPlayerInput : TPCameraPluginBase
    {
        [SerializeField] public PlayerInput _playerInput;
    }
}