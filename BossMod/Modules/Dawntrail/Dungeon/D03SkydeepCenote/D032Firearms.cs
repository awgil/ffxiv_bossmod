namespace BossMod.Dawntrail.Dungeon.D03SkydeepCenote.D032Firearms;

public enum OID : uint
{
    Boss = 0x4184, // R4.620, x1
    Helper = 0x233C, // R0.500, x20, Helper type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 36451, // Boss->location, no cast, single-target
    DynamicDominance = 36448, // Boss->self, 5.0s cast, range 70 circle, raidwide
    MirrorManeuver = 39139, // Boss->self, 3.0s cast, single-target, visual (mirrors & orbs)
    ThunderlightBurst = 36443, // Boss->self, 8.0s cast, single-target, visual (laser)
    ThunderlightBurstAOERect1 = 38581, // Helper->self, 8.2s cast, range 42 width 8 rect
    ThunderlightBurstAOERect2 = 38582, // Helper->self, 8.2s cast, range 49 width 8 rect
    ThunderlightBurstAOERect3 = 38583, // Helper->self, 8.2s cast, range 35 width 8 rect
    ThunderlightBurstAOERect4 = 38584, // Helper->self, 8.2s cast, range 36 width 8 rect
    ThunderlightBurstAOECircle = 36445, // Helper->self, 10.9s cast, range 35 circle
    AncientArtillery = 36442, // Boss->self, 3.0s cast, single-target, visual (show expanding square)
    EmergentArtillery = 39000, // Boss->self, 3.0s cast, single-target, visual (expand & explode square)
    ArtilleryAOE1 = 38660, // Helper->self, 8.5s cast, range 10 width 10 rect
    ArtilleryAOE2 = 38661, // Helper->self, 8.5s cast, range 10 width 10 rect
    ArtilleryAOE3 = 38662, // Helper->self, 8.5s cast, range 10 width 10 rect
    ArtilleryAOE4 = 38663, // Helper->self, 8.5s cast, range 10 width 10 rect
    Pummel = 36447, // Boss->player, 5.0s cast, single-target, tankbuster
    ThunderlightFlurry = 36450, // Helper->player, 5.0s cast, range 6 circle spread
}

public enum IconID : uint
{
    Pummel = 218, // player
    ThunderlightFlurry = 139, // player
}

class DynamicDominance(BossModule module) : Components.RaidwideCast(module, AID.DynamicDominance);
class ThunderlightBurstRect1(BossModule module) : Components.StandardAOEs(module, AID.ThunderlightBurstAOERect1, new AOEShapeRect(42, 4));
class ThunderlightBurstRect2(BossModule module) : Components.StandardAOEs(module, AID.ThunderlightBurstAOERect2, new AOEShapeRect(49, 4));
class ThunderlightBurstRect3(BossModule module) : Components.StandardAOEs(module, AID.ThunderlightBurstAOERect3, new AOEShapeRect(35, 4));
class ThunderlightBurstRect4(BossModule module) : Components.StandardAOEs(module, AID.ThunderlightBurstAOERect4, new AOEShapeRect(36, 4));
class ThunderlightBurstCircle(BossModule module) : Components.StandardAOEs(module, AID.ThunderlightBurstAOECircle, new AOEShapeCircle(35));

class Artillery(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeRect _shape = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArtilleryAOE1 or AID.ArtilleryAOE2 or AID.ArtilleryAOE3 or AID.ArtilleryAOE4)
            AOEs.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArtilleryAOE1 or AID.ArtilleryAOE2 or AID.ArtilleryAOE3 or AID.ArtilleryAOE4)
            AOEs.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
    }
}

class Pummel(BossModule module) : Components.SingleTargetCast(module, AID.Pummel);

class ThunderlightFlurry(BossModule module) : Components.SpreadFromCastTargets(module, AID.ThunderlightFlurry, 6)
{
    private readonly Artillery? _artillery = module.FindComponent<Artillery>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // start adding hints only after overlapping artillery is resolved
        if (_artillery == null || _artillery.AOEs.Count == 0)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class D032FirearmsStates : StateMachineBuilder
{
    public D032FirearmsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DynamicDominance>()
            .ActivateOnEnter<ThunderlightBurstRect1>()
            .ActivateOnEnter<ThunderlightBurstRect2>()
            .ActivateOnEnter<ThunderlightBurstRect3>()
            .ActivateOnEnter<ThunderlightBurstRect4>()
            .ActivateOnEnter<ThunderlightBurstCircle>()
            .ActivateOnEnter<Artillery>()
            .ActivateOnEnter<Pummel>()
            .ActivateOnEnter<ThunderlightFlurry>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 829, NameID = 12888)]
public class D032Firearms(WorldState ws, Actor primary) : BossModule(ws, primary, new(-85, -155), new ArenaBoundsSquare(20));
