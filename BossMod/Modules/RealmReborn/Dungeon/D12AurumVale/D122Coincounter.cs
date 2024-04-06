namespace BossMod.RealmReborn.Dungeon.D12AurumVale.D122Coincounter;

public enum OID : uint
{
    Boss = 0x5BE, // x1
};

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    TenTonzeSwipe = 1034, // Boss->self, no cast, range 6+R ?-degree cone cleave
    HundredTonzeSwipe = 1035, // Boss->self, 3.0s cast, range 6+R 120-degree cone aoe
    HundredTonzeSwing = 628, // Boss->self, 4.0s cast, range 8+R circle aoe
    Glower = 629, // Boss->self, 3.0s cast, range 17+R width 7 rect aoe
    AnimalInstinct = 630, // Boss->self, no cast, single-target
    EyeOfTheBeholder = 631, // Boss->self, 2.5s cast, range 8-15+R donut 270-degree cone aoe
};

class TenTonzeSwipe : Components.Cleave
{
    public TenTonzeSwipe() : base(ActionID.MakeSpell(AID.TenTonzeSwipe), new AOEShapeCone(10, 60.Degrees())) { } // TODO: verify angle
}

class HundredTonzeSwipe : Components.SelfTargetedAOEs
{
    public HundredTonzeSwipe() : base(ActionID.MakeSpell(AID.HundredTonzeSwipe), new AOEShapeCone(10, 60.Degrees())) { }
}

class HundredTonzeSwing : Components.SelfTargetedAOEs
{
    public HundredTonzeSwing() : base(ActionID.MakeSpell(AID.HundredTonzeSwing), new AOEShapeCircle(12)) { }
}

class Glower : Components.SelfTargetedAOEs
{
    public Glower() : base(ActionID.MakeSpell(AID.Glower), new AOEShapeRect(21, 3.5f)) { }
}

class EyeOfTheBeholder : Components.SelfTargetedAOEs
{
    public EyeOfTheBeholder() : base(ActionID.MakeSpell(AID.EyeOfTheBeholder), new AOEShapeDonutSector(8, 19, 135.Degrees())) { }
}

class D122CoincounterStates : StateMachineBuilder
{
    public D122CoincounterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TenTonzeSwipe>()
            .ActivateOnEnter<HundredTonzeSwipe>()
            .ActivateOnEnter<HundredTonzeSwing>()
            .ActivateOnEnter<Glower>()
            .ActivateOnEnter<EyeOfTheBeholder>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 5, NameID = 1533)]
public class D122Coincounter : BossModule
{
    public D122Coincounter(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-150, -150), 20)) { }
}
