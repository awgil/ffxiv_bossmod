namespace BossMod.Endwalker.Alliance.A33Oschon;

class TheArrow : Components.BaitAwayCast
{
    public TheArrow() : base(ActionID.MakeSpell(AID.TheArrow), new AOEShapeCircle(6), true) { }
}

class TheArrowP2 : Components.BaitAwayCast
{
    public TheArrowP2() : base(ActionID.MakeSpell(AID.TheArrowP2), new AOEShapeCircle(10), true) { }
}

class FlintedFoehnStack : Components.StackWithCastTargets
{
    public FlintedFoehnStack() : base(ActionID.MakeSpell(AID.FlintedFoehnStack), 6) { }
}

[ModuleInfo(PrimaryActorOID = (uint)OID.OschonP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11300)]
public class A33Oschon : BossModule
{
    private readonly IReadOnlyList<Actor> _oschonP2;

    public Actor? OschonP1() => !PrimaryActor.IsTargetable ? null : PrimaryActor;
    public Actor? OschonP2() => _oschonP2.FirstOrDefault();

    public A33Oschon(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(0, 750), 25, 25)) 
    {
        _oschonP2 = Enemies(OID.OschonP2);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        switch (StateMachine.ActivePhaseIndex)
        {
            case 0:
                Arena.Actor(OschonP1(), ArenaColor.Enemy);
                break;
            case 1:
                Arena.Actor(OschonP2(), ArenaColor.Enemy);
                break;
        }
    }
}