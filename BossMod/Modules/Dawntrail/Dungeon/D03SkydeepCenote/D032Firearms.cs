namespace BossMod.Dawntrail.Dungeon.D03SkydeepCenote.D032Firearms;

public enum OID : uint
{
    Boss = 0x4184, // R4.62
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 36451, // Boss->location, no cast, single-target

    DynamicDominance = 36448, // Boss->self, 5.0s cast, range 70 circle
    MirrorManeuver = 39139, // Boss->self, 3.0s cast, single-target

    ThunderlightBurstVisual = 36443, // Boss->self, 8.0s cast, single-target
    ThunderlightBurstAOE = 36445, // Helper->self, 10.9s cast, range 35 circle

    ThunderlightBurst1 = 38581, // Helper->self, 8.2s cast, range 42 width 8 rect
    ThunderlightBurst2 = 38582, // Helper->self, 8.2s cast, range 49 width 8 rect
    ThunderlightBurst3 = 38583, // Helper->self, 8.2s cast, range 35 width 8 rect
    ThunderlightBurst4 = 38584, // Helper->self, 8.2s cast, range 36 width 8 rect

    AncientArtillery = 36442, // Boss->self, 3.0s cast, single-target
    EmergentArtillery = 39000, // Boss->self, 3.0s cast, single-target

    Artillery1 = 38660, // Helper->self, 8.5s cast, range 10 width 10 rect
    Artillery2 = 38661, // Helper->self, 8.5s cast, range 10 width 10 rect
    Artillery3 = 38662, // Helper->self, 8.5s cast, range 10 width 10 rect
    Artillery4 = 38663, // Helper->self, 8.5s cast, range 10 width 10 rect

    Pummel = 36447, // Boss->player, 5.0s cast, single-target

    ThunderlightFlurry = 36450 // Helper->player, 5.0s cast, range 6 circle
}

class DynamicDominanceArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(D032Firearms.ArenaCenter, 25)], [new Square(D032Firearms.ArenaCenter, 20)]);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DynamicDominance && Module.Arena.Bounds == D032Firearms.StartingBounds)
            _aoe = new(square, Module.Center, default, spell.NPCFinishAt.AddSeconds(0.6f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x14)
        {
            Module.Arena.Bounds = D032Firearms.DefaultBounds;
            _aoe = null;
        }
    }
}

class DynamicDominance(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DynamicDominance));

class ThunderlightBurstAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderlightBurstAOE), new AOEShapeCircle(35));
class ThunderlightBurst1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderlightBurst1), new AOEShapeRect(42, 4));
class ThunderlightBurst2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderlightBurst2), new AOEShapeRect(49, 4));
class ThunderlightBurst3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderlightBurst3), new AOEShapeRect(35, 4));
class ThunderlightBurst4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderlightBurst4), new AOEShapeRect(36, 4));

class Artillery1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Artillery1), new AOEShapeRect(5, 5, 5));
class Artillery2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Artillery2), new AOEShapeRect(5, 5, 5));
class Artillery3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Artillery3), new AOEShapeRect(5, 5, 5));
class Artillery4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Artillery4), new AOEShapeRect(5, 5, 5));

class Pummel(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Pummel));
class ThunderlightFlurry(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.ThunderlightFlurry), 6);

class D032FirearmsStates : StateMachineBuilder
{
    public D032FirearmsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DynamicDominanceArenaChange>()
            .ActivateOnEnter<DynamicDominance>()
            .ActivateOnEnter<ThunderlightBurstAOE>()
            .ActivateOnEnter<ThunderlightBurst1>()
            .ActivateOnEnter<ThunderlightBurst2>()
            .ActivateOnEnter<ThunderlightBurst3>()
            .ActivateOnEnter<ThunderlightBurst4>()
            .ActivateOnEnter<Artillery1>()
            .ActivateOnEnter<Artillery2>()
            .ActivateOnEnter<Artillery3>()
            .ActivateOnEnter<Artillery4>()
            .ActivateOnEnter<Pummel>()
            .ActivateOnEnter<ThunderlightFlurry>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 829, NameID = 12888)]
public class D032Firearms(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-85, -155);
    public static readonly ArenaBoundsSquare StartingBounds = new(24.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20);
}
