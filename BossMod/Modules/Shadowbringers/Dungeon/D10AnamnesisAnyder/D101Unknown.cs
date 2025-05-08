namespace BossMod.Shadowbringers.Dungeon.D10AnamnesisAnyder.D101Unknown;

public enum OID : uint
{
    Boss = 0x2CD9, // R4.900, x1
    Unknown = 0x2CDA, // R4.900, x1
    Helper = 0x233C, // R0.500, x2, Helper type
    SinisterBubble = 0x2CDB, // R1.500, x12
}

public enum AID : uint
{
    FetidFang = 19305, // Boss->player, 4.0s cast, single-target
    FetidFang1 = 19314, // Unknown->player, 4.0s cast, single-target
    Reflection = 19311, // Boss->self, 1.2s cast, range 40 45-degree cone
    Explosion = 19310, // SinisterBubble->self, 14.0s cast, range 8 circle
    LuminousRay = 20006, // Boss->self, 5.0s cast, range 50 width 8 rect
    LuminousRay1 = 20007, // Unknown->self, 5.0s cast, range 50 width 8 rect
    Inscrutability = 19306, // Boss->self, 4.0s cast, range 40 circle
    Inscrutability1 = 19315, // Unknown->self, 4.0s cast, range 40 circle
    Clearout = 19307, // Boss->self, 3.0s cast, range 9 120-degree cone
    Clearout1 = 19316, // Unknown->self, 3.0s cast, range 9 120-degree cone
    Setback = 19308, // Boss->self, 3.0s cast, range 9 120-degree cone
    Setback1 = 19317, // Unknown->self, 3.0s cast, range 9 120-degree cone

    EctoplasmicMark1 = 19319, // Helper->player, no cast, single-target
    EctoplasmicMark2 = 19312, // Helper->player, no cast, single-target
    EctoplasmicRay1 = 19320, // Unknown->self, no cast, range 50 width 8 rect
    EctoplasmicRay2 = 19313, // Boss->self, no cast, range 50 width 8 rect
}

class Clearout(BossModule module) : Components.StandardAOEs(module, AID.Clearout, new AOEShapeCone(9, 60.Degrees()));
class Clearout1(BossModule module) : Components.StandardAOEs(module, AID.Clearout1, new AOEShapeCone(9, 60.Degrees()));
class Setback(BossModule module) : Components.StandardAOEs(module, AID.Setback, new AOEShapeCone(9, 60.Degrees()));
class Setback1(BossModule module) : Components.StandardAOEs(module, AID.Setback1, new AOEShapeCone(9, 60.Degrees()));
class FetidFang(BossModule module) : Components.SingleTargetCast(module, AID.FetidFang);
class FetidFang1(BossModule module) : Components.SingleTargetCast(module, AID.FetidFang1);
class LuminousRay(BossModule module) : Components.StandardAOEs(module, AID.LuminousRay, new AOEShapeRect(50, 4));
class LuminousRay1(BossModule module) : Components.StandardAOEs(module, AID.LuminousRay1, new AOEShapeRect(50, 4));
class Inscrutability(BossModule module) : Components.RaidwideCast(module, AID.Inscrutability);
class Inscrutability1(BossModule module) : Components.RaidwideCast(module, AID.Inscrutability1);
class Explosion(BossModule module) : Components.StandardAOEs(module, AID.Explosion, new AOEShapeCircle(8));
class EctoplasmicRay(BossModule module) : Components.SimpleLineStack(module, 4, 50, AID.EctoplasmicMark1, AID.EctoplasmicRay1, 5.2f);
class EctoplasmicRay2(BossModule module) : Components.SimpleLineStack(module, 4, 50, AID.EctoplasmicMark2, AID.EctoplasmicRay2, 5.2f);

class Reflection(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void Update()
    {
        if (Module.PrimaryActor.IsDeadOrDestroyed)
            _aoe = null;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        Angle? angle = state switch
        {
            0x00041000 => -135.Degrees(),
            0x00042000 => 135.Degrees(),
            0x00044000 => 0.Degrees(),
            _ => null
        };

        if (angle != null)
            _aoe = new AOEInstance(new AOEShapeCone(40, 22.5f.Degrees()), actor.Position, angle.Value, WorldState.FutureTime(14.4f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Reflection)
            _aoe = null;
    }
}

class UnknownStates : StateMachineBuilder
{
    public UnknownStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FetidFang>()
            .ActivateOnEnter<FetidFang1>()
            .ActivateOnEnter<LuminousRay>()
            .ActivateOnEnter<LuminousRay1>()
            .ActivateOnEnter<Inscrutability>()
            .ActivateOnEnter<Inscrutability1>()
            .ActivateOnEnter<Reflection>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<EctoplasmicRay>()
            .ActivateOnEnter<EctoplasmicRay2>()
            .ActivateOnEnter<Clearout>()
            .ActivateOnEnter<Clearout1>()
            .ActivateOnEnter<Setback>()
            .ActivateOnEnter<Setback1>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && Module.Enemies(OID.Unknown).All(s => s.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 714, NameID = 9261)]
public class Unknown(WorldState ws, Actor primary) : BossModule(ws, primary, new(-40, 290), new ArenaBoundsCircle(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Unknown), ArenaColor.Enemy);
    }
}
