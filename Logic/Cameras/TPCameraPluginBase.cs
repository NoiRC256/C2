using UnityEngine;

namespace NekoNeko.Cameras
{
    public abstract class TPCameraPluginBase : MonoBehaviour
    {
        protected TPCamera _tpCamera;
        protected bool _isInitialized = false;

        public virtual void Init(TPCamera controller)
        {
            _tpCamera = controller;
            _isInitialized = true;
        }

        public virtual void OnPreUpdate(float deltaTime)
        {
            if (!_isInitialized) return;
        }

        public virtual void OnPostUpdate(float deltaTime)
        {
            if (!_isInitialized) return;
        }
    }
}
