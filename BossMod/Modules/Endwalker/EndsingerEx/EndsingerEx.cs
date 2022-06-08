namespace BossMod.Endwalker.EndsingerEx
{
    // raidwide is slightly delayed
    class Elegeia : CommonComponents.CastCounter
    {
        public Elegeia() : base(ActionID.MakeSpell(AID.Elegeia)) { }
    }

    class Telomania : CommonComponents.CastCounter
    {
        public Telomania() : base(ActionID.MakeSpell(AID.TelomaniaLast)) { }
    }

    class UltimateFate : CommonComponents.CastCounter
    {
        public UltimateFate() : base(ActionID.MakeSpell(AID.EnrageAOE)) { }
    }

    // TODO: proper tankbuster component...
    class Hubris : CommonComponents.CastCounter
    {
        public Hubris() : base(ActionID.MakeSpell(AID.HubrisAOE)) { }
    }

    // TODO: proper stacks component
    class Eironeia : CommonComponents.CastCounter
    {
        public Eironeia() : base(ActionID.MakeSpell(AID.EironeiaAOE)) { }
    }

    public class EndsingerEx : BossModule
    {
        public EndsingerEx(BossModuleManager manager, Actor primary)
            : base(manager, primary)
        {
            Arena.IsCircle = true;
            InitStates(new EndsingerExStates(this).Build());
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }
    }
}
