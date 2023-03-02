using UnityEngine;

namespace NekoNeko
{
    public abstract class ProcedureBase
    {
        public enum Type
        {
            None,
            LauncherGame,
            FrontendGame,
            BattleGame,
        }

        public ProcedureBase LastProcedure { get; set; }

    }
}