using System;
using UnityEngine;
using UnityEngine.Events;

namespace NekoNeko.Battle
{
    public class MonoHitbox : MonoBehaviour, IDamageable
    {
        [SerializeField] private UnityEvent<AttackData> _onReceivedAttack;

        public event Action<AttackData> ReceivedAttack;

        public void ReceiveAttack(AttackData attackData)
        {
            ReceivedAttack?.Invoke(attackData);
            _onReceivedAttack?.Invoke(attackData);
        }
    }
}