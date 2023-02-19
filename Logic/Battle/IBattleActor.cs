namespace NekoNeko.Battle
{
    public interface IBattleActor
    {
        public int BattleTeamId { get; }
        public AttackSourceType AttackSourceType { get; }
    }
}