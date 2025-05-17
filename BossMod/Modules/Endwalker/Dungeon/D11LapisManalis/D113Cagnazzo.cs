namespace BossMod.Endwalker.Dungeon.D11LapisManalis.D113Cagnazzo;

public enum OID : uint
{
    Boss = 0x3AE2, //R=8.0
    FearsomeFlotsam = 0x3AE3, //R=2.4
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    StygianDeluge = 31139, // Boss->self, 5.0s cast, range 80 circle
    Antediluvian = 31119, // Boss->self, 5.0s cast, single-target
    Antediluvian2 = 31120, // Helper->self, 6.5s cast, range 15 circle
    BodySlam = 31121, // Boss->location, 6.5s cast, single-target
    BodySlam2 = 31122, // Helper->self, 7.5s cast, range 60 circle, knockback 10, away from source
    BodySlam3 = 31123, // Helper->self, 7.5s cast, range 8 circle
    Teleport = 31131, // Boss->location, no cast, single-target, boss teleports 
    HydrobombTelegraph = 32695, // Helper->location, 2.0s cast, range 4 circle
    HydraulicRamTelegraph = 32693, // Helper->location, 2.0s cast, width 8 rect charge
    HydraulicRam = 32692, // Boss->self, 6.0s cast, single-target
    HydraulicRam2 = 32694, // Boss->location, no cast, width 8 rect charge
    Hydrobomb = 32696, // Helper->location, no cast, range 4 circle
    StartHydrofall = 31126, // Boss->self, no cast, single-target
    Hydrofall = 31375, // Boss->self, 5.0s cast, single-target
    Hydrofall2 = 31376, // Helper->players, 5.5s cast, range 6 circle
    CursedTide = 31130, // Boss->self, 5.0s cast, single-target
    StartLimitbreakPhase = 31132, // Boss->self, no cast, single-target
    NeapTide = 31134, // Helper->player, no cast, range 6 circle
    Hydrovent = 31136, // Helper->location, 5.0s cast, range 6 circle
    SpringTide = 31135, // Helper->players, no cast, range 6 circle
    Tsunami = 31137, // Helper->self, no cast, range 80 width 60 rect
    Voidcleaver = 31110, // Boss->self, 4.0s cast, single-target
    Voidcleaver2 = 31111, // Helper->self, no cast, range 100 circle
    VoidMiasma = 32691, // Helper->self, 3.0s cast, range 50 30-degree cone
    Lifescleaver = 31112, // Boss->self, 4.0s cast, single-target
    Lifescleaver2 = 31113, // Helper->self, 5.0s cast, range 50 30-degree cone
    VoidTorrent = 31118, // Boss->self/player, 5.0s cast, range 60 width 8 rect
}

public enum IconID : uint
{
    Stackmarker = 161, // player
    Spreadmarker = 139, // player
    Tankbuster = 230, // player
}

public enum TetherID : uint
{
    LimitBreakCharger = 3, // FearsomeFlotsam->Boss
    BaitAway = 1, // 3E97->player
}

public enum NPCYell : uint
{
    LimitBreakStart = 15175,
}

class VoidTorrent(BossModule module) : Components.BaitAwayCast(module, AID.VoidTorrent, new AOEShapeRect(60, 4));
class VoidTorrentHint(BossModule module) : Components.SingleTargetCast(module, AID.VoidTorrent, "Tankbuster cleave");
class Voidcleaver(BossModule module) : Components.RaidwideCast(module, AID.Voidcleaver);
class VoidMiasmaBait(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(50, 15.Degrees()), (uint)TetherID.BaitAway);
class VoidMiasma(BossModule module) : Components.StandardAOEs(module, AID.VoidMiasma, new AOEShapeCone(50, 15.Degrees()));
class Lifescleaver(BossModule module) : Components.StandardAOEs(module, AID.Lifescleaver2, new AOEShapeCone(50, 15.Degrees()));
class Tsunami(BossModule module) : Components.RaidwideAfterNPCYell(module, AID.Tsunami, (uint)NPCYell.LimitBreakStart, 4.5f);
class StygianDeluge(BossModule module) : Components.RaidwideCast(module, AID.StygianDeluge);
class Antediluvian(BossModule module) : Components.StandardAOEs(module, AID.Antediluvian2, new AOEShapeCircle(15));
class BodySlam(BossModule module) : Components.StandardAOEs(module, AID.BodySlam3, new AOEShapeCircle(8));
class BodySlamKB(BossModule module) : Components.KnockbackFromCastTarget(module, AID.BodySlam2, 10);

class HydraulicRam(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos source, AOEShape shape, Angle direction)> _casters = [];
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_casters.Count > 0)
            yield return new(_casters[0].shape, _casters[0].source, _casters[0].direction, _activation, ArenaColor.Danger);
        for (int i = 1; i < _casters.Count; ++i)
            yield return new(_casters[i].shape, _casters[i].source, _casters[i].direction, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HydraulicRamTelegraph)
        {
            var dir = spell.LocXZ - caster.Position;
            _casters.Add((caster.Position, new AOEShapeRect(dir.Length(), 4), Angle.FromDirection(dir)));
        }
        if ((AID)spell.Action.ID == AID.HydraulicRam)
            _activation = Module.CastFinishAt(spell, 1.5f); //since these are charges of different length with 0s cast time, the activation times are different for each and there are different patterns, so we just pretend that they all start after the telegraphs end
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_casters.Count > 0 && (AID)spell.Action.ID == AID.HydraulicRam2)
            _casters.RemoveAt(0);
    }
}

class Hydrobomb(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<WPos> _casters = [];
    private static readonly AOEShapeCircle circle = new(4);
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_casters.Count > 1)
            for (int i = 0; i < 2; ++i)
                yield return new(circle, _casters[i], default, _activation.AddSeconds(6 - _casters.Count / 2), ArenaColor.Danger);
        for (int i = 2; i < _casters.Count; ++i)
            yield return new(circle, _casters[i], default, _activation.AddSeconds(MathF.Ceiling(i / 2) + 6 - _casters.Count / 2));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HydrobombTelegraph)
            _casters.Add(spell.LocXZ);
        if ((AID)spell.Action.ID == AID.HydraulicRam)
            _activation = Module.CastFinishAt(spell, 2.2f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_casters.Count > 0 && (AID)spell.Action.ID == AID.Hydrobomb)
            _casters.RemoveAt(0);
    }
}

class Hydrovent(BossModule module) : Components.StandardAOEs(module, AID.Hydrovent, 6);

class NeapTide(BossModule module) : Components.UniformStackSpread(module, 0, 6, alwaysShowSpreads: true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Spreadmarker)
            AddSpread(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.NeapTide)
            Spreads.Clear();
    }
}

class Stackmarkers(BossModule module) : Components.UniformStackSpread(module, 6, 0) // Hydrofall and Springtide, both use the same icon
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Stackmarker)
            AddStack(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SpringTide or AID.Hydrofall2)
            Stacks.Clear();
    }
}

class D113CagnazzoStates : StateMachineBuilder
{
    public D113CagnazzoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Voidcleaver>()
            .ActivateOnEnter<Lifescleaver>()
            .ActivateOnEnter<VoidMiasma>()
            .ActivateOnEnter<VoidMiasmaBait>()
            .ActivateOnEnter<Antediluvian>()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<BodySlamKB>()
            .ActivateOnEnter<HydraulicRam>()
            .ActivateOnEnter<Hydrobomb>()
            .ActivateOnEnter<Stackmarkers>()
            .ActivateOnEnter<NeapTide>()
            .ActivateOnEnter<StygianDeluge>()
            .ActivateOnEnter<Hydrovent>()
            .ActivateOnEnter<VoidTorrent>()
            .ActivateOnEnter<VoidTorrentHint>()
            .ActivateOnEnter<Tsunami>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 896, NameID = 11995)]
public class D113Cagnazzo(WorldState ws, Actor primary) : BossModule(ws, primary, new(-250, 130), new ArenaBoundsSquare(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.FearsomeFlotsam), ArenaColor.Enemy);
    }
}
