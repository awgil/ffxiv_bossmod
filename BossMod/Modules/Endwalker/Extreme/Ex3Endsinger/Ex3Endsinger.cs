namespace BossMod.Endwalker.Extreme.Ex3Endsigner
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

    [ConfigDisplay(Order = 0x030, Parent = typeof(EndwalkerConfig))]
    public class Ex3EndsingerConfig : CooldownPlanningConfigNode { }

    [CooldownPlanning(typeof(Ex3EndsingerConfig))]
    public class Ex3Endsinger : BossModule
    {
        public Ex3Endsinger(WorldState ws, Actor primary)
            : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20))
        {
            StateMachine = new Ex3EndsingerStates(this).Build();
        }
    }
}
