using UnityEngine;

namespace NekoNeko.Battle
{
    public class BulletMovementData
    {
        /// <summary>
        /// Origin position of the bullet.
        /// </summary>
        public Vector3 Origin { get; private set; }
        /// <summary>
        /// Current target position of the bullet.
        /// </summary>
        public virtual Vector3 TargetPosition { get; set; }
        /// <summary>
        /// Current target transform of the bullet.
        /// </summary>
        public virtual Transform TargetTr { get; set; }

        /// <summary>
        /// Current movement direction of the bullet.
        /// </summary>
        public Vector3 Direction { get; set; }
        /// <summary>
        /// Speed of the bullet.
        /// </summary>
        public float Speed { get; set; }
        /// <summary>
        /// Current position of the bullet.
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Current rotation of the bullet.
        /// </summary>
        public Quaternion Rotation { get; set; }
        /// <summary>
        /// Next calculated position of the bullet.
        /// </summary>
        public Vector3 NextPos { get; set; }

        public virtual void Init(BulletData bulletData, Vector3 origin,
            Vector3 targetPosition = default, Transform targetTr = null)
        {
            Clear();
            Origin = origin;
            TargetPosition = targetPosition;
            TargetTr = targetTr;
        }

        public void Clear()
        {
            TargetTr = null;
        }
    }
}