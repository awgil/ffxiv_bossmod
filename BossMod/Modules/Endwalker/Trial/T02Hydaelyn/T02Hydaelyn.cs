namespace BossMod.Endwalker.Trial.T02Hydaelyn;

class MousasScorn : Components.CastSharedTankbuster
{
    public MousasScorn() : base(ActionID.MakeSpell(AID.MousasScorn), 4) { }
}

class MousasScornHint : Components.SingleTargetCast
{
    public MousasScornHint() : base(ActionID.MakeSpell(AID.MousasScorn), "Shared Tankbuster") { }
}

class HerossSundering : Components.BaitAwayCast
{
    public HerossSundering() : base(ActionID.MakeSpell(AID.HerossSundering), new AOEShapeCone(40, 45.Degrees())) { }
}

class HerossSunderingHint : Components.SingleTargetCast
{
    public HerossSunderingHint() : base(ActionID.MakeSpell(AID.HerossSundering), "Tankbuster cleave") { }
}

class HerossRadiance : Components.RaidwideCast
{
    public HerossRadiance() : base(ActionID.MakeSpell(AID.HerossRadiance)) { }
}

class MagossRadiance : Components.RaidwideCast
{
    public MagossRadiance() : base(ActionID.MakeSpell(AID.MagossRadiance)) { }
}

class RadiantHalo : Components.RaidwideCast
{
    public RadiantHalo() : base(ActionID.MakeSpell(AID.RadiantHalo)) { }
}

class CrystallineStoneIII : Components.StackWithCastTargets
{
    public CrystallineStoneIII() : base(ActionID.MakeSpell(AID.CrystallineStoneIII2), 6, 5) { }
}

class CrystallineBlizzardIII : Components.SpreadFromCastTargets
{
    public CrystallineBlizzardIII() : base(ActionID.MakeSpell(AID.CrystallineBlizzardIII2), 5) { }
}

class Beacon : Components.ChargeAOEs
{
    public Beacon() : base(ActionID.MakeSpell(AID.Beacon), 3) { }
}

class Beacon2 : Components.SelfTargetedAOEs
{
    public Beacon2() : base(ActionID.MakeSpell(AID.Beacon2), new AOEShapeRect(45, 3), 10) { }
}

class HydaelynsRay : Components.SelfTargetedAOEs
{
    public HydaelynsRay() : base(ActionID.MakeSpell(AID.HydaelynsRay), new AOEShapeRect(45, 15)) { }
}

class T02HydaelynStates : StateMachineBuilder
{
    public T02HydaelynStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ParhelicCircle>()
            .ActivateOnEnter<MousasScorn>()
            .ActivateOnEnter<Echoes>()
            .ActivateOnEnter<Beacon>()
            .ActivateOnEnter<Beacon2>()
            .ActivateOnEnter<CrystallineStoneIII>()
            .ActivateOnEnter<CrystallineBlizzardIII>()
            .ActivateOnEnter<MousasScornHint>()
            .ActivateOnEnter<HerossSundering>()
            .ActivateOnEnter<HerossSunderingHint>()
            .ActivateOnEnter<HerossRadiance>()
            .ActivateOnEnter<MagossRadiance>()
            .ActivateOnEnter<HydaelynsRay>()
            .ActivateOnEnter<RadiantHalo>()
            .ActivateOnEnter<Lightwave>()
            .ActivateOnEnter<WeaponTracker>()
            .ActivateOnEnter<Exodus>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 790, NameID = 10453)]
public class T02Hydaelyn : BossModule
{
    public T02Hydaelyn(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.CrystalOfLight))
            Arena.Actor(s, ArenaColor.Object);
    }
}
