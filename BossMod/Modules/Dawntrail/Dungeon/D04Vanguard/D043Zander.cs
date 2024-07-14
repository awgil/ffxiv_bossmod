namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D043Zander;

public enum OID : uint
{
    Boss = 0x411E, // R2.1
    BossP2 = 0x41BA, // R2.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/BossP2->player, no cast, single-target

    BurstVisual1 = 39240, // Helper->self, 10.5s cast, range 20 width 40 rect
    BurstVisual2 = 39241, // Helper->self, 11.5s cast, range 20 width 40 rect
    Burst1 = 36575, // Helper->self, 10.0s cast, range 20 width 40 rect
    Burst2 = 36591, // Helper->self, 11.0s cast, range 20 width 40 rect

    Electrothermia = 36594, // Boss->self, 5.0s cast, range 60 circle, raidwide
    SaberRush = 36595, // Boss->player, 5.0s cast, single-target
    Screech = 36596, // BossP2->self, 5.0s cast, range 60 circle, raidwide
    ShadeShot = 36597, // BossP2->player, 5.0s cast, single-target

    SlitherbaneForeguardRect = 36589, // BossP2->self, 4.0s cast, range 20 width 4 rect
    SlitherbaneForeguardCone = 36592, // Helper->self, 4.5s cast, range 20 180-degree cone

    SlitherbaneRearguardRect = 36590, // Boss2->self, 4.0s cast, range 20 width 4 rect
    SlitherbaneRearguardCone = 36593, // Helper->self, 4.5s cast, range 20 180-degree cone

    SoulbaneSaber = 36574, // Boss->self, 3.0s cast, range 20 width 4 rect
    SoulbaneShock = 37922, // Helper->player, 5.0s cast, range 5 circle
    Syntheslean = 37198, // BossP2->self, 4.0s cast, range 19 90-degree cone

    SyntheslitherVisual1 = 36579, // BossP2->location, 4.0s cast, single-target
    SyntheslitherVisual2 = 36584, // BossP2->location, 4.0s cast, single-target
    Syntheslither1 = 36580, // Helper->self, 5.0s cast, range 19 90-degree cone
    Syntheslither2 = 36581, // Helper->self, 5.6s cast, range 19 90-degree cone
    Syntheslither3 = 36582, // Helper->self, 6.2s cast, range 19 90-degree cone
    Syntheslither4 = 36583, // Helper->self, 6.8s cast, range 19 90-degree cone
    Syntheslither5 = 36585, // Helper->self, 5.0s cast, range 19 90-degree cone
    Syntheslither6 = 36586, // Helper->self, 5.6s cast, range 19 90-degree cone
    Syntheslither7 = 36587, // Helper->self, 6.2s cast, range 19 90-degree cone
    Syntheslither8 = 36588, // Helper->self, 6.8s cast, range 19 90-degree cone

    PhaseChangeVisual1 = 36576, // Boss->self, no cast, single-target
    PhaseChangeVisual2 = 36577, // Boss->self, no cast, single-target
    PhaseChangeVisual3 = 36578 // Boss->self, no cast, single-target
}

class ElectrothermiaArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(17, 20);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Electrothermia && Module.Arena.Bounds == D043Zander.StartingBounds)
            _aoe = new(donut, Module.Center, default, spell.NPCFinishAt.AddSeconds(0.5f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x00)
        {
            Module.Arena.Bounds = D043Zander.DefaultBounds;
            _aoe = null;
        }
    }
}

class SlitherbaneBurstCombo(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly Angle offset = 180.Degrees();
    private static readonly AOEShapeCone cone = new(20, 90.Degrees());
    private static readonly AOEShapeRect rect = new(20, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return _aoes[0] with { Color = ArenaColor.Danger };
        if (_aoes.Count > 1)
            yield return _aoes[1] with { Risky = !_aoes[0].Rotation.AlmostEqual(_aoes[1].Rotation + offset, Helpers.RadianConversion) };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SlitherbaneRearguardCone:
            case AID.SlitherbaneForeguardCone:
                AddAOE(cone, caster.Position, spell.Rotation, spell.NPCFinishAt);
                break;
            case AID.Burst2:
                AddAOE(rect, caster.Position, spell.Rotation, spell.NPCFinishAt);
                break;
        }
    }

    private void AddAOE(AOEShape shape, WPos position, Angle rotation, DateTime activation)
    {
        _aoes.Add(new AOEInstance(shape, position, rotation, activation));
        _aoes.SortBy(x => x.Activation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SlitherbaneRearguardCone:
            case AID.SlitherbaneForeguardCone:
            case AID.Burst2:
                _aoes.RemoveAt(0);
                break;
        }
    }
}

class Electrothermia(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Electrothermia));
class Screech(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Screech));
class Burst1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Burst1), new AOEShapeRect(20, 20));
class SaberRush(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.SaberRush));
class ShadeShot(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.ShadeShot));
class SlitherbaneForeguardRect(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SlitherbaneForeguardRect), new AOEShapeRect(20, 2));
class SlitherbaneRearguardRect(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SlitherbaneRearguardRect), new AOEShapeRect(20, 2));
class SoulbaneSaber(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SoulbaneSaber), new AOEShapeRect(20, 2));
class SoulbaneShock(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SoulbaneShock), 5);
class Syntheslean(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Syntheslean), new AOEShapeCone(19, 45.Degrees()));
class Syntheslither1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Syntheslither1), new AOEShapeCone(19, 45.Degrees()));
class Syntheslither2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Syntheslither2), new AOEShapeCone(19, 45.Degrees()));
class Syntheslither3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Syntheslither3), new AOEShapeCone(19, 45.Degrees()));
class Syntheslither4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Syntheslither4), new AOEShapeCone(19, 45.Degrees()));
class Syntheslither5(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Syntheslither5), new AOEShapeCone(19, 45.Degrees()));
class Syntheslither6(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Syntheslither6), new AOEShapeCone(19, 45.Degrees()));
class Syntheslither7(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Syntheslither7), new AOEShapeCone(19, 45.Degrees()));
class Syntheslither8(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Syntheslither8), new AOEShapeCone(19, 45.Degrees()));

class D043ZanderStates : StateMachineBuilder
{
    public D043ZanderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectrothermiaArenaChange>()
            .ActivateOnEnter<Electrothermia>()
            .ActivateOnEnter<Screech>()
            .ActivateOnEnter<Burst1>()
            .ActivateOnEnter<SaberRush>()
            .ActivateOnEnter<ShadeShot>()
            .ActivateOnEnter<SlitherbaneForeguardRect>()
            .ActivateOnEnter<SlitherbaneRearguardRect>()
            .ActivateOnEnter<SlitherbaneBurstCombo>()
            .ActivateOnEnter<SoulbaneSaber>()
            .ActivateOnEnter<SoulbaneShock>()
            .ActivateOnEnter<Syntheslean>()
            .ActivateOnEnter<Syntheslither1>()
            .ActivateOnEnter<Syntheslither2>()
            .ActivateOnEnter<Syntheslither3>()
            .ActivateOnEnter<Syntheslither4>()
            .ActivateOnEnter<Syntheslither5>()
            .ActivateOnEnter<Syntheslither6>()
            .ActivateOnEnter<Syntheslither7>()
            .ActivateOnEnter<Syntheslither8>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12752, SortOrder = 6)]
public class D043Zander(WorldState ws, Actor primary) : BossModule(ws, primary, new(90, -430), StartingBounds)
{
    public static readonly ArenaBounds StartingBounds = new ArenaBoundsCircle(19.5f);
    public static readonly ArenaBounds DefaultBounds = new ArenaBoundsCircle(17);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossP2))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
