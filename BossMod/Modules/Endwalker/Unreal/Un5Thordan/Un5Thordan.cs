namespace BossMod.Endwalker.Unreal.Un5Thordan;

[ConfigDisplay(Order = 0x350, Parent = typeof(EndwalkerConfig))]
public class Un5ThordanConfig : CooldownPlanningConfigNode
{
    public Un5ThordanConfig() : base(90) { }
}

[ModuleInfo(PrimaryActorOID = (uint)OID.Thordan)]
public class Un5Thordan : BossModule
{
    
    public Un5Thordan(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 0), 20))
    {
    }
}

