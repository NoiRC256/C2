using UnityEngine;

namespace NekoLib.Movement
{
    public class VelocitySource : IVelocitySource
    {
        public bool AlignToGround { get; set; }
        public Vector3 Velocity { get; set; }

        public VelocitySource(bool alignToGround = false)
        {
            AlignToGround = alignToGround;
        }

        public Vector3 Evaluate()
        {
            Vector3 currentVelocity = Velocity;
            Velocity = Vector3.zero;
            return currentVelocity;
        }
    }
}