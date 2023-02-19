using UnityEngine;

namespace NekoNeko.Battle
{
    public class ProjectileBulletMover : BulletMoverBase
    {
        private float _gravity;
        private float _gravitySpeedLimit;
        private bool _speedAffectsGravity;

        private Vector3 _vel;
        private float _gravitySpeed;

        public override void Init(BulletData bulletData, Vector3 origin, Vector3 direction, 
            Vector3 targetPosition = default, Transform targetTr = null)
        {
            base.Init(bulletData, origin, direction, targetPosition, targetTr);

            _gravity = bulletData.Gravity;
            _gravitySpeedLimit = bulletData.GravitySpeedLimit;
            _speedAffectsGravity = bulletData.SpeedAffectsGravity;
        }

        public override Vector3 CalcNextPosition(float deltaTime)
        {
            _vel = deltaTime * MoverData.Speed * MoverData.Direction;
            _vel.y = _gravitySpeed;
            return MoverData.Position + _vel;
        }

        public override void UpdateMotion(float deltaTime)
        {
            MoverData.Direction = (MoverData.NextPos - MoverData.Position).normalized;
            MoverData.Position = MoverData.NextPos;
            MoverData.Rotation = Quaternion.LookRotation(MoverData.Direction);

            _gravitySpeed += _gravity * deltaTime;
            if (_gravitySpeed < _gravitySpeedLimit) _gravitySpeed = _gravitySpeedLimit;
        }
    }
}