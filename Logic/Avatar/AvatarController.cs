using UnityEngine;

namespace NekoNeko.Avatar
{
    public class AvatarController : MonoBehaviour
    {
        [SerializeField] private AvatarData _data;
        [SerializeField] private AvatarInput _input;

        [SerializeField] private AvatarLocomotion _locomotion;
        [SerializeField] private AvatarAim _aim;
        [SerializeField] private AvatarAnimation _animation;

        private void Awake()
        {
            _locomotion.Data = _data;
            _locomotion.Input = _input;
            _aim.Data = _data;
            _aim.Input = _input;
            _animation.Data = _data;
            _animation.Input = _input;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            _locomotion.OnUpdate(deltaTime);
            _aim.OnUpdate(deltaTime);
            _animation.OnUpdate(deltaTime);
        }
    }
}