namespace NekoNeko.Battle
{
    public enum BulletMovementType
    {
        /// <summary>
        /// Bullet that launches along a direction and is affected by gravity.
        /// </summary>
        Projectile,
        /// <summary>
        /// Bullet that instantaneously checks collision along a direction or towards a target position.
        /// </summary>
        Hitscan,
        /// <summary>
        /// Bullet that launches along a precalculated ballistic path.
        /// </summary>
        Ballistic,
        /// <summary>
        /// Bullet that launches along a precalculated bezier path.
        /// </summary>
        Bezier,
    }
}