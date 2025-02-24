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
    _AutoAttack_Attack = 870, // Boss/Unknown->player, no cast, single-target
    _Ability_NursedGrudge = 19309, // Boss->self, no cast, single-target
    _Ability_Scrutiny = 20005, // Boss->self, 13.0s cast, single-target
    _Weaponskill_FetidFang = 19305, // Boss->player, 4.0s cast, single-target
    _Weaponskill_FetidFang1 = 19314, // Unknown->player, 4.0s cast, single-target
    _Weaponskill_Reflection = 19311, // Boss->self, 1.2s cast, range 40 45-degree cone
    _Weaponskill_Explosion = 19310, // SinisterBubble->self, 14.0s cast, range 8 circle
    _Weaponskill_LuminousRay = 20006, // Boss->self, 5.0s cast, range 50 width 8 rect
    _Weaponskill_LuminousRay1 = 20007, // Unknown->self, 5.0s cast, range 50 width 8 rect
    _Weaponskill_Inscrutability = 19306, // Boss->self, 4.0s cast, range 40 circle
    _Weaponskill_Inscrutability1 = 19315, // Unknown->self, 4.0s cast, range 40 circle
    EctoplasmMark1 = 19319, // Helper->player, no cast, single-target
    EctoplasmMark2 = 19312, // Helper->player, no cast, single-target
    _Ability_EctoplasmicRay = 19321, // Boss->self, 5.0s cast, single-target
    _Ability_EctoplasmicRay1 = 19322, // Unknown->self, 5.0s cast, single-target
    EctoplasmicRay1 = 19320, // Unknown->self, no cast, range 50 width 8 rect
    EctoplasmicRay2 = 19313, // Boss->self, no cast, range 50 width 8 rect
    _Weaponskill_PlainWeirdness = 20043, // Unknown->self, 3.0s cast, single-target
    _Weaponskill_Clearout = 19307, // Boss->self, 3.0s cast, range 9 120-degree cone
    _Weaponskill_Clearout1 = 19316, // Unknown->self, 3.0s cast, range 9 120-degree cone
    _Weaponskill_Setback = 19308, // Boss->self, 3.0s cast, range 9 120-degree cone
    _Weaponskill_Setback1 = 19317, // Unknown->self, 3.0s cast, range 9 120-degree cone
}

class Clearout(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Clearout), new AOEShapeCone(9, 60.Degrees()));
class Clearout1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Clearout1), new AOEShapeCone(9, 60.Degrees()));
class Setback(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Setback), new AOEShapeCone(9, 60.Degrees()));
class Setback1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Setback1), new AOEShapeCone(9, 60.Degrees()));
class FetidFang(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID._Weaponskill_FetidFang));
class FetidFang1(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID._Weaponskill_FetidFang1));
class LuminousRay(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_LuminousRay), new AOEShapeRect(50, 4));
class LuminousRay1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_LuminousRay1), new AOEShapeRect(50, 4));
class Inscrutability(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_Inscrutability));
class Inscrutability1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_Inscrutability1));
class Explosion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Explosion), new AOEShapeCircle(8));
class EctoplasmicRay(BossModule module) : Components.SimpleLineStack(module, 4, 50, ActionID.MakeSpell(AID.EctoplasmMark1), ActionID.MakeSpell(AID.EctoplasmicRay1), 5.2f);
class EctoplasmicRay2(BossModule module) : Components.SimpleLineStack(module, 4, 50, ActionID.MakeSpell(AID.EctoplasmMark2), ActionID.MakeSpell(AID.EctoplasmicRay2), 5.2f);

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
        if ((AID)spell.Action.ID == AID._Weaponskill_Reflection)
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

