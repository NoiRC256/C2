using System.Linq;
using UnityEngine;
using NekoLib.Pool;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NekoNeko
{
    public class MonoEffect : MonoBehaviour
    {
        [Tooltip("If true, destroys the instance or returns it to a pool. Otherwise, deactivates the gameobject.")]
        [SerializeField] public bool DestroyOnEnd = true;
        [Tooltip("If true, begins playing on start.")]
        [SerializeField] public bool PlayOnStart = true;
        [Tooltip("If true, restarts playing when the duration reaches its end.")]
        [SerializeField] public bool RestartOnEnd = false;
        [SerializeField] private float _duration;
        [SerializeField] private ParticleSystem[] _particleSystems;
        [SerializeField] private Animation _animation;
        [SerializeField] private AudioSource _audioSource;

        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }
        public float SpeedMultiplier {
            get => _speedMultiplier;
            set {
                if (value <= 0f) value = 1f;
                if (value != _speedMultiplier)
                {
                    _speedMultiplier = value;
                    //ApplySpeedMultipler(_speedMultiplier);
                }
            }
        }
        public float ActualDuration => _duration / SpeedMultiplier;

        private IObjectPool _pool;
        private float _aliveCounter;
        [SerializeField][Min(1f)] private float _speedMultiplier = 1f;

        #region MonoBehaviour
        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            if (PlayOnStart) Play();
        }

        private void Update()
        {
            if (!IsPlaying || IsPaused) return;

            float deltaTime = Time.deltaTime * _speedMultiplier;

            Simulate(deltaTime, true, restart: false, fixedTimeStep: true);

            _aliveCounter -= deltaTime;
            if (_aliveCounter <= 0f)
            {
                if (RestartOnEnd)
                {
                    _aliveCounter = _duration;
                    Play();
                    return;
                }
                else Stop();
            }
        }
        #endregion

        public void Init(IObjectPool pool = null)
        {
            _pool = pool;
            _aliveCounter = _duration;
            //ApplySpeedMultipler(_speedMultiplier);
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                ParticleSystem.MainModule main = _particleSystems[i].main;
                main.playOnAwake = false;
            }
        }

        public void Clear()
        {
            IsPlaying = false;
            IsPaused = false;
        }

        public void Play()
        {
            this.gameObject.SetActive(true);
            IsPlaying = true;
            Simulate(0f, true, restart: true, fixedTimeStep: true);
            //for (int i = 0; i < _particleSystems.Length; i++)
            //{
            //    _particleSystems[i].Play(true);
            //}
            if (_animation)
            {
                _animation.enabled = true;
                _animation.Play();
            }
            if (_audioSource) _audioSource.Play();
        }

        public void Pause()
        {
            IsPaused = true;
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                _particleSystems[i].Pause(true);
            }
            if (_animation) _animation.enabled = false;
            if (_audioSource) _audioSource.Pause();
        }

        public void Resume()
        {
            IsPaused = false;
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                _particleSystems[i].Play(true);
            }
            if (_animation) _animation.enabled = true;
            if (_audioSource) _audioSource.Play();
        }

        public void Simulate(float time, bool withChildren = true,
            bool restart = true, bool fixedTimeStep = true)
        {
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                _particleSystems[i].Simulate(time, withChildren, restart, fixedTimeStep);
            }
        }

        public void Stop()
        {
            IsPlaying = false;
            if (DestroyOnEnd)
            {
                if (_pool != null) ReturnToPool();
                else Destroy();
            }
            else
            {
                Clear();
                this.gameObject.SetActive(false);
            }
        }

        public bool ReturnToPool()
        {
            OnReturnToPool();
            return _pool.Release(this);
        }

        public void OnReturnToPool()
        {
            Clear();
            this.gameObject.SetActive(false);
        }

        public void Destroy()
        {
            GameObject.Destroy(this.gameObject);
        }

        //private void ApplySpeedMultipler(float speedMultiplier)
        //{
        //    for (int i = 0; i < _particleSystems.Length; i++)
        //    {
        //        ParticleSystem.MainModule main = _particleSystems[i].main;
        //        main.simulationSpeed = speedMultiplier;
        //    }
        //}

        public float GetMaxDuration()
        {
            return Mathf.Max(GetParticleSystemsMaxDuration(), GetAnimationDuration(), GetAudioDuration()) / SpeedMultiplier;
        }

        public float GetParticleSystemsMaxDuration()
        {
            if (_particleSystems.Length == 0) return 0f;
            return _particleSystems.Max(x => x.main.duration) / SpeedMultiplier;
        }

        public float GetAnimationDuration()
        {
            if (_animation == null) return 0f;
            return _animation.clip.length / SpeedMultiplier;
        }

        public float GetAudioDuration()
        {
            if (_audioSource == null) return 0f;
            return _audioSource.clip.length / SpeedMultiplier;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MonoEffect))]
    public class MonoEffectEditor : Editor
    {
        private SerializedProperty _duration;
        private void OnEnable()
        {
            _duration = serializedObject.FindProperty("_duration");
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            MonoEffect monoEffect = (MonoEffect)target;
            EditorGUILayout.LabelField("Duration Settings", EditorStyles.boldLabel);
            if (GUILayout.Button("Sync To Particle System Duration"))
            {
                _duration.floatValue = monoEffect.GetParticleSystemsMaxDuration();
                serializedObject.ApplyModifiedProperties();
            }
            if (GUILayout.Button("Sync To Animation Duration"))
            {
                _duration.floatValue = monoEffect.GetAnimationDuration();
                serializedObject.ApplyModifiedProperties();
            }
            if (GUILayout.Button("Sync To Audio Duration"))
            {
                _duration.floatValue = monoEffect.GetAudioDuration();
                serializedObject.ApplyModifiedProperties();
            }
            if (GUILayout.Button("Auto Duration"))
            {
                _duration.floatValue = monoEffect.GetMaxDuration();
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif
}