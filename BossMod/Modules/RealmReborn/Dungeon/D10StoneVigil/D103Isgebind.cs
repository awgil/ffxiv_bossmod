namespace BossMod.RealmReborn.Dungeon.D10StoneVigil.D103Isgebind;

public enum OID : uint
{
    Boss = 0x5AF, // x1
    Helper = 0x233C, // x8
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast
    RimeWreath = 1025, // Boss->self, 3.0s cast, raidwide
    FrostBreath = 1022, // Boss->self, 1.0s cast, range 27 ?-degree cone cleave
    SheetOfIce = 1023, // Boss->location, 2.5s cast, range 5 aoe
    SheetOfIce2 = 1024, // Helper->location, 3.0s cast, range 5 aoe
    Cauterize = 1026, // Boss->self, 4.0s cast, range 48 width 20 rect aoe
    Touchdown = 1027, // Boss->self, no cast, range 5 aoe around center
}

class RimeWreath(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.RimeWreath));
class FrostBreath(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.FrostBreath), new AOEShapeCone(27, 60.Degrees())); // TODO: verify angle
class SheetOfIce(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.SheetOfIce), 5);
class SheetOfIce2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.SheetOfIce2), 5);
class Cauterize(BossModule module) : Components.SelfTargetedLegacyRotationAOEs(module, ActionID.MakeSpell(AID.Cauterize), new AOEShapeRect(48, 10));

class Touchdown(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.Touchdown))
{
    private readonly AOEShapeCircle _shape = new(5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // TODO: proper timings...
        if (!Module.PrimaryActor.IsTargetable && !Module.FindComponent<Cauterize>()!.ActiveCasters.Any())
            yield return new(_shape, Module.Center);
    }
}

class D103IsgebindStates : StateMachineBuilder
{
    public D103IsgebindStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RimeWreath>()
            .ActivateOnEnter<FrostBreath>()
            .ActivateOnEnter<SheetOfIce>()
            .ActivateOnEnter<SheetOfIce2>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<Touchdown>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 11, NameID = 1680)]
public class D103Isgebind(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -248), new ArenaBoundsSquare(20));
