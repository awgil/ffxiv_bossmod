namespace BossMod.Endwalker.Dungeon.D11LapisManalis.D110AlbusGriffin;

public enum OID : uint
{
    Boss = 0x3E9F, //R=4.6
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    WindsOfWinter = 32785, // Boss->self, 5,0s cast, range 40 circle
    Freefall = 32786, // Boss->location, 3,5s cast, range 8 circle
    GoldenTalons = 32787, // Boss->self, 4,5s cast, range 8 90-degree cone
};

class WindsOfWinter : Components.RaidwideCast
{
    public WindsOfWinter() : base(ActionID.MakeSpell(AID.WindsOfWinter), "Stun Albus Griffin, Raidwide") { }
}

class GoldenTalons : Components.SelfTargetedAOEs
{
    public GoldenTalons() : base(ActionID.MakeSpell(AID.GoldenTalons), new AOEShapeCone(8, 45.Degrees())) { }
}

class Freefall : Components.LocationTargetedAOEs
{
    public Freefall() : base(ActionID.MakeSpell(AID.Freefall), 8) { }
}

class D110AlbusGriffinStates : StateMachineBuilder
{
    public D110AlbusGriffinStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Freefall>()
            .ActivateOnEnter<WindsOfWinter>()
            .ActivateOnEnter<GoldenTalons>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 896, NameID = 12245)]
public class D110AlbusGriffin : BossModule
{
    public D110AlbusGriffin(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(47, -570.5f), 8.5f, 11.5f)) { }
}
