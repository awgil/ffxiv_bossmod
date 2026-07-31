namespace BossMod.Stormblood.Dungeon.D09DrownedCityOfSkalla.D091Kelpie;

public enum OID : uint
{
    Boss = 0x1FAB, // R5.400, x1
    Helper = 0x18D6, // R0.500, x4, Helper type
    Hydrosphere = 0x2051, // R1.200, x0 (spawn during fight), Helper type
    BloodyPuddle = 0x1EA7D5, // event object
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Torpedo = 9807, // Boss->player, 2.0s cast, single-target
    RisingSeas = 9808, // Boss->self, 3.0s cast, range 50+R circle
    HydroPull = 9809, // Boss->self, 7.0s cast, ???
    HydroPush = 9810, // Boss->self, 7.0s cast, range 50+R circle
    Gallop = 9811, // Boss->location, no cast, ???
    BloodyPuddleCast = 9812, // Boss->self, 5.0s cast, single-target
    BloodyPuddle = 9813, // Helper->self, no cast, range 8 circle
    BubbleBurst = 9755, // Hydrosphere->self, 1.0s cast, range 6 circle
}

public enum SID : uint
{
    Dropsy = 283, // none->player, extra=0x0
    WaterResistanceDown = 431, // Hydrosphere->player, extra=0x1
}

public enum IconID : uint
{
    BigSpread = 43, // player->self
    Target = 1, // player->self
}

public enum TetherID : uint
{
    Water = 3, // Hydrosphere->player
}

class Torpedo(BossModule module) : Components.SingleTargetCast(module, AID.Torpedo);
class RisingSeas(BossModule module) : Components.RaidwideCast(module, AID.RisingSeas);
class HydroPull(BossModule module) : Components.KnockbackFromCastTarget(module, AID.HydroPull, 20, kind: Kind.TowardsOrigin)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor).Where(s => !IsImmune(slot, s.Activation)))
        {
            var orig = src.Origin;
            var ctr = Arena.Center;
            var puddles = Module.Enemies(OID.BloodyPuddle).Select(e => e.Position).ToList();
            hints.AddForbiddenZone(p =>
            {
                var dir = orig - p;
                var proj = p + dir.Normalized() * Math.Min(20, dir.Length());
                return !proj.AlmostEqual(ctr, 15) || puddles.Any(p => proj.InCircle(p, 8));
            }, src.Activation);
        }
    }
}
class HydroPush(BossModule module) : Components.KnockbackFromCastTarget(module, AID.HydroPush, 20, kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor).Where(s => !IsImmune(slot, s.Activation)))
        {
            var safeCenter = Arena.Center - src.Direction.ToDirection() * src.Distance;
            hints.AddForbiddenZone(ShapeContains.InvertedRect(safeCenter, new WDir(0, 1), 15, 15, 15), src.Activation);
        }
    }
}
class BloodyPuddleSpread(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.BigSpread, AID.BloodyPuddle, 8, 5.1f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (IsSpreadTarget(actor))
            hints.AddForbiddenZone(ShapeContains.Rect(Arena.Center, 0.Degrees(), 11, 11, 11), Spreads[0].Activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == SpreadAction)
            Spreads.Clear();
    }
}
class BloodyPuddlePuddle(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 8, AID.BloodyPuddle, m => m.Enemies(OID.BloodyPuddle).Where(e => e.EventState != 7), 1);

class BubbleBurst(BossModule module) : Components.GenericAOEs(module, AID.BubbleBurst)
{
    readonly List<(Actor Source, Actor Target, DateTime Activation)> _sources = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _sources.Select(s => new AOEInstance(new AOEShapeCircle(6), s.Source.Position, default, s.Activation));

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (from, to, _) in _sources)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddLine(from.Position, to.Position, 0xFF000000, 2);
            Arena.AddLine(from.Position, to.Position, ArenaColor.Danger);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Water)
            _sources.Add((source, WorldState.Actors.Find(tether.Target)!, WorldState.FutureTime(11)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _sources.RemoveAll(s => s.Source == caster);
        }
    }
}

class D091KelpieStates : StateMachineBuilder
{
    public D091KelpieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Torpedo>()
            .ActivateOnEnter<RisingSeas>()
            .ActivateOnEnter<HydroPush>()
            .ActivateOnEnter<HydroPull>()
            .ActivateOnEnter<BubbleBurst>()
            .ActivateOnEnter<BloodyPuddleSpread>()
            .ActivateOnEnter<BloodyPuddlePuddle>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 279, NameID = 6907)]
public class D091Kelpie(WorldState ws, Actor primary) : BossModule(ws, primary, new(-220, 4), new ArenaBoundsSquare(15));

