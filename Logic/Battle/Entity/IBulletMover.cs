using UnityEngine;

namespace NekoNeko.Battle
{
    public interface IBulletMover
    {
        Vector3 Direction { get; set; }
        Vector3 Position { get; set; }
        Quaternion Rotation { get; set; }

        void Init(BulletData bulletData, Vector3 origin, Vector3 direction,
            Vector3 targetPosition = default, Transform targetTr = null);

        Vector3 CalcNextPosition(float deltaTime);

        Vector3 UpdateNextPosition(float deltaTime);

        void UpdateMotion(float deltaTime);
    }
}