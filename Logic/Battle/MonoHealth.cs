using NekoLib.Events;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NekoNeko.Battle
{
    public class MonoHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _maxHp;
        [SerializeField] private List<MonoHitbox> _hitBoxes = new List<MonoHitbox>();
        [SerializeField] private UnityEvent _onHpDepleted;
        [SerializeField] private UnityEvent _onHpFilled;
        [SerializeField] private UnityEvent<float> _onHpChanged;

        private float _hp;
        public float Hp {
            get => _hp;
            set {
                _hp = value;
                if (_hp >= _maxHp)
                {
                    _hp = _maxHp;
                    _onHpFilled?.Invoke();
                }
                else if (_hp <= 0f)
                {
                    _hp = 0f;
                    _onHpDepleted?.Invoke();
                }
                _onHpChanged?.Invoke(_hp);
            }
        }

        private void Awake()
        {
            _hp = _maxHp;
        }

        private void OnEnable()
        {
            for (int i = 0; i < _hitBoxes.Count; i++)
            {
                _hitBoxes[i].ReceivedAttack += ReceiveAttack;
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < _hitBoxes.Count; i++)
            {
                _hitBoxes[i].ReceivedAttack -= ReceiveAttack;
            }
        }

        public void ReceiveAttack(AttackData attackData)
        {
            float damage = attackData.Damage;
            Hp -= damage;
        }
    }
}