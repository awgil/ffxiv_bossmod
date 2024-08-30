namespace BossMod.RealmReborn.Dungeon.D29TheKeeperoftheLake.D292MagitekGunship;

public enum OID : uint
{
    Boss = 0xE68, // R6.000, x1
    CohortEques = 0xE70, // R0.500, x0 (spawn during fight)
    CohortLaquearius = 0xE6F, // R0.500, x0 (spawn during fight)
    CohortSecutor = 0xE71, // R0.500, x0 (spawn during fight)
    CohortSignifer = 0xE72, // R0.500, x0 (spawn during fight)
    CohortVanguard = 0xE69, // R2.800, x0 (spawn during fight)
    GarleanFireLinger = 0x1E98CA, // R0.500, x0 (spawn during fight), EventObj type
    MagitekGunshipAlt = 0xE7B, // R0.500, x1
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/ThCohortLaquearius/ThCohortVanguard->player, no cast, single-target
    CarpetBombCast = 3391, // Boss->location, 1.0s cast, single-target // Done
    CarpetBombLinger = 3392, // MagitekGunshipAlt->location, 3.0s cast, range 5 circle // Done
    FlameThrower = 3389, // Boss->self, 3.5s cast, range 12+R 120-degree cone, cast to start flamethrower // Needs to be looked at
    FlameThrowerLinger = 3390, // MagitekGunshipAlt->self, no cast, range 17+R ?-degree cone, persistant void cone aoe // Needs to be looked at pt. 2
    GarleanFire = 3411, // MagitekGunshipAlt->location, no cast, range 8 circle, LocationTargetAOE // Done

    // Add's Attacks
    AutoAttackAdd1 = 871, // CohortEques->player, no cast, single-target // N/A
    AutoAttackAdd2 = 872, // CohortSecutor->player, no cast, single-target // N/a
    DrillCannons = 1433, // CohortVanguard->self, 2.5s cast, range 30+R width 5 rect // done
    Overcharge = 1435, // CohortVanguard->self, 2.5s cast, range 8+R 120-degree cone // done
    Thunder = 968, // CohortSignifer->player, 1.0s cast, single-target // N/A
}

class MagitekGunshipAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.CohortSignifer, (uint)OID.CohortSecutor, (uint)OID.CohortLaquearius, (uint)OID.CohortEques, (uint)OID.CohortVanguard]);
class CarpetBomb(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.CarpetBombLinger), 5, "Get out of the puddle, NOW!");
class GarleanFireVoid(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 8, ActionID.MakeSpell(AID.GarleanFire), m => m.Enemies(OID.GarleanFireLinger).Where(z => z.EventState != 7), 0.8f);
class AddDrillCannons(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DrillCannons), new AOEShapeRect(32.8f, 2.5f));
class AddOvercharge(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Overcharge), new AOEShapeCone(10.8f, 60.Degrees()));

class Flamethrower(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FlameThrower), new AOEShapeCone(18, 60.Degrees()));
class FlamethrowerAOE(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.FlameThrowerLinger))
{
    private AOEInstance? _persistentAOE;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_persistentAOE);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        // boss just finished casting regular flamethrower
        if ((AID)spell.Action.ID == AID.FlameThrower)
            _persistentAOE = new AOEInstance(new AOEShapeCone(18, 60.Degrees()), caster.Position, caster.Rotation);

        if ((AID)spell.Action.ID == AID.FlameThrowerLinger && NumCasts >= 5)
        {
            _persistentAOE = null;
            // reset for next usage of mechanic
            NumCasts = 0;
        }
    }
}

class D292MagitekGunshipStates : StateMachineBuilder
{
    public D292MagitekGunshipStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekGunshipAdds>()
            .ActivateOnEnter<CarpetBomb>()
            .ActivateOnEnter<GarleanFireVoid>()
            .ActivateOnEnter<AddDrillCannons>()
            .ActivateOnEnter<AddOvercharge>()
            .ActivateOnEnter<Flamethrower>()
            .ActivateOnEnter<FlamethrowerAOE>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman, Xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 32, NameID = 3373)]
public class D292MagitekGunship(WorldState ws, Actor primary) : BossModule(ws, primary, new(8.5f, -150), new ArenaBoundsCircle(19));
