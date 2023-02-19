namespace NekoNeko.Battle
{
    public class BattleUtil
    {
        public static float RpmToDeltaTime(float rpm)
        {
            if (rpm <= 0f) return 0f;
            return 60f / rpm;
        }

        public static float DeltaTimeToRpm(float deltaTime)
        {
            if (deltaTime <= 0f) return 0f;
            return 60f / deltaTime;
        }
    }
}