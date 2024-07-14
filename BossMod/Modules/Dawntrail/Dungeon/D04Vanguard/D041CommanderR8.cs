namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D041CommanderR8;

public enum OID : uint
{
    Boss = 0x411D, // R3.24
    VanguardSentryR8 = 0x41BC, // R3.24
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 36403, // Boss->player, no cast, single-target

    Electrowave = 36571, // Boss->self, 5.0s cast, range 60 circle

    EnhancedMobility1 = 39140, // Boss->location, 10.0s cast, range 14 width 6 rect, in, start -60°
    EnhancedMobility2 = 39141, // Boss->location, 10.0s cast, range 14 width 6 rect, out, start 60°
    EnhancedMobility3 = 36559, // Boss->location, 10.0s cast, range 14 width 6 rect, out, start -60°
    EnhancedMobility4 = 36560, // Boss->location, 10.0s cast, range 14 width 6 rect, in, start 60°

    EnhancedMobility5 = 36563, // Helper->self, 10.5s cast, range 10 width 14 rect, sword right
    EnhancedMobility6 = 36564, // Helper->self, 10.5s cast, range 10 width 14 rect, sword left
    EnhancedMobility7 = 37184, // Helper->self, 10.5s cast, range 20 width 14 rect, sword right
    EnhancedMobility8 = 37191, // Helper->self, 10.5s cast, range 20 width 14 rect, sword left

    RapidRotaryVisual1 = 36561, // Boss->self, no cast, single-target
    RapidRotaryVisual2 = 36562, // Boss->self, no cast, single-target
    RapidRotaryVisual3 = 39142, // Boss->self, no cast, single-target
    RapidRotaryVisual4 = 39143, // Boss->self, no cast, single-target

    RapidRotaryCone = 36566, // Helper->self, no cast, range 14 120-degree cone
    RapidRotaryDonutSegmentBig = 36567, // Helper->self, no cast, range 17-28 donut, 120° donut segment
    RapidRotaryDonutSegmentSmall = 36565, // Helper->self, no cast, range 11-17 donut, 120° donut segment

    Dispatch = 36568, // Boss->self, 4.0s cast, single-target
    Rush = 36569, // VanguardSentryR8->location, 6.0s cast, width 5 rect charge
    AerialOffensive = 36570, // VanguardSentryR8->location, 9.0s cast, range 4 circle, visual starts at radius 4, final size 14

    ElectrosurgeVisual = 36572, // Boss->self, 4.0+1.0s cast, single-target
    Electrosurge = 36573 // Helper->player, 5.0s cast, range 5 circle, spread
}

class ElectrowaveArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(D041CommanderR8.ArenaCenter, 20)], [new Square(D041CommanderR8.ArenaCenter, 17)]);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Electrowave && Module.Arena.Bounds == D041CommanderR8.StartingBounds)
            _aoe = new(square, Module.Center, default, spell.NPCFinishAt.AddSeconds(0.4f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x0A)
        {
            Module.Arena.Bounds = D041CommanderR8.DefaultBounds;
            _aoe = null;
        }
    }
}

class EnhancedMobilitySword(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly Angle a90 = 90.Degrees();
    private static readonly AOEShapeRect rect = new(14, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.EnhancedMobility5:
            case AID.EnhancedMobility7:
                AddAOE(spell, -a90);
                break;
            case AID.EnhancedMobility6:
            case AID.EnhancedMobility8:
                AddAOE(spell, a90);
                break;
        }
    }

    private void AddAOE(ActorCastInfo spell, Angle offset)
    {
        _aoe = new AOEInstance(rect, Module.Center + 5 * spell.Rotation.ToDirection(), spell.Rotation + offset, spell.NPCFinishAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.EnhancedMobility5 or AID.EnhancedMobility6 or AID.EnhancedMobility7 or AID.EnhancedMobility8)
            _aoe = null;
    }
}

class RapidRotary(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly Angle a60 = 60.Degrees();
    private static readonly Angle a120 = 120.Degrees();
    private const float ActivationDelay = 1.8f;
    private const float ActivationDelayIncrement = 0.3f;
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeDonutSector donutSectorSmall = new(11, 17, a60);
    private static readonly AOEShapeDonutSector donutSectorBig = new(17, 28, a60);
    private static readonly AOEShapeCone cone = new(14, a60);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < Math.Clamp(_aoes.Count, 0, 7); i++)
        {
            var aoe = _aoes[i];
            yield return new AOEInstance(aoe.Shape, aoe.Origin, aoe.Rotation, aoe.Activation, i < 2 ? ArenaColor.Danger : ArenaColor.AOE);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.EnhancedMobility1:
                AddAOEs(donutSectorSmall, donutSectorBig, -a60, spell.NPCFinishAt);
                break;
            case AID.EnhancedMobility2:
                AddAOEs(donutSectorSmall, cone, a60, spell.NPCFinishAt);
                break;
            case AID.EnhancedMobility3:
                AddAOEs(donutSectorSmall, cone, -a60, spell.NPCFinishAt);
                break;
            case AID.EnhancedMobility4:
                AddAOEs(donutSectorSmall, donutSectorBig, a60, spell.NPCFinishAt);
                break;
        }
    }

    private void AddAOEs(AOEShape shape1, AOEShape shape2, Angle initialAngle, DateTime finishAt)
    {
        for (var i = 0; i < 3; i++)
        {
            var angle = initialAngle - i * a120;
            _aoes.Add(new AOEInstance(shape1, Module.Center, angle, finishAt.AddSeconds(ActivationDelay + i * ActivationDelayIncrement)));
            _aoes.Add(new AOEInstance(shape2, Module.Center, angle, finishAt.AddSeconds(ActivationDelay + i * ActivationDelayIncrement)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.RapidRotaryCone or AID.RapidRotaryDonutSegmentBig or AID.RapidRotaryDonutSegmentSmall)
            _aoes.RemoveAt(0);
    }
}

class Electrowave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Electrowave));

class EnhancedMobility1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EnhancedMobility1), new AOEShapeRect(14, 3));
class EnhancedMobility2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EnhancedMobility2), new AOEShapeRect(14, 3));
class EnhancedMobility3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EnhancedMobility3), new AOEShapeRect(14, 3));
class EnhancedMobility4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EnhancedMobility4), new AOEShapeRect(14, 3));

class Rush(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.Rush), 2.5f);
class AerialOffensive(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.AerialOffensive), 14, maxCasts: 4);
class Electrosurge(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Electrosurge), 5);

class D041CommanderR8States : StateMachineBuilder
{
    public D041CommanderR8States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectrowaveArenaChange>()
            .ActivateOnEnter<Electrowave>()
            .ActivateOnEnter<RapidRotary>()
            .ActivateOnEnter<EnhancedMobilitySword>()
            .ActivateOnEnter<EnhancedMobility1>()
            .ActivateOnEnter<EnhancedMobility2>()
            .ActivateOnEnter<EnhancedMobility3>()
            .ActivateOnEnter<EnhancedMobility4>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<AerialOffensive>()
            .ActivateOnEnter<Electrosurge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12750, SortOrder = 2)]
public class D041CommanderR8(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-100, 207);
    public static readonly ArenaBounds StartingBounds = new ArenaBoundsSquare(19.5f);
    public static readonly ArenaBounds DefaultBounds = new ArenaBoundsSquare(17);
}
