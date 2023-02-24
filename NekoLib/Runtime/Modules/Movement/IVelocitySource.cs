using UnityEngine;

namespace NekoLib.Movement
{
    public interface IVelocitySource
    {
        bool AlignToGround { get; }

        Vector3 Evaluate();
    }
}