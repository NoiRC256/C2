using UnityEngine;

namespace NekoNeko.Battle
{
    public abstract class BulletMoverBase : IBulletMover
    {
        public virtual Vector3 Direction {
            get => MoverData.Direction;
            set => MoverData.Direction = value;
        }
        public virtual Vector3 Position {
            get => MoverData.Position;
            set => MoverData.Position = value;
        }
        public virtual Quaternion Rotation {
            get => MoverData.Rotation;
            set => MoverData.Rotation = value;
        }

        public virtual BulletMovementData MoverData { get; private set; }

        public virtual void Init(BulletData bulletData, Vector3 origin, Vector3 direction,
            Vector3 targetPosition = default, Transform targetTr = null)
        {
            if (MoverData == null) MoverData = new BulletMovementData();
            MoverData.Init(bulletData, origin, targetPosition, targetTr);
            MoverData.Direction = direction;
            MoverData.Speed = bulletData.Speed;
            MoverData.Position = origin;
            MoverData.Rotation = Quaternion.LookRotation(direction);
        }

        public abstract Vector3 CalcNextPosition(float deltaTime);

        public virtual Vector3 UpdateNextPosition(float deltaTime)
        {
            MoverData.NextPos = CalcNextPosition(deltaTime);
            return MoverData.NextPos;
        }

        public abstract void UpdateMotion(float deltaTime);
    }
}