namespace BossMod.Shadowbringers.Dungeon.D11HeroesGauntlet.D113SpectralBerserker;

public enum OID : uint
{
    Boss = 0x2EFD, // R3.000, x? 
    Helper = 0x233C, // R0.500, x?, Helper type
    Rubble = 0x2EFE, // R2.500, x?
    RageVoidzone = 0x1EA1A1,
}
public enum AID : uint
{
    BeastlyFury = 21004, // 2EFD->self, 4.0s cast, range 50 circle
    WildAnguish = 21000, // 2EFD->players, 5.0s cast, range 6 circle
    WildAnguish2 = 21001, // 2EFD->players, no cast, range 6 circle

    WildRage = 20994, // 2EFD->location, 5.0s cast, range 8 circle
    WildRage2 = 20995, // 233C->location, 5.7s cast, range 8 circle
    WildRage3 = 20996, // 233C->self, 5.7s cast, range -50 donut

    FallingRock = 20997, // 2EFE->self, no cast, range 8 circle
    Jump = 21047, // 2F33->2E7F, no cast, single-target

    WildRampage = 20998, // 2EFD->self, 5.0s cast, single-target
    WildRampage2 = 20999, // 233C->self, 5.5s cast, range 50 width 50 rect

    RagingSliceInitial = 21002, // 2EFD->self, 3.7s cast, range 50 width 6 rect
    RagingSliceFollowup = 21003, // 2EFD->self, 2.5s cast, range 50 width 6 rect
}
public enum SID : uint
{
    MagicVulnUp = 1138,
}

public enum IconID : uint
{
    Stack = 93,
    FallingRock = 229,
}

class BeastlyFury(BossModule module) : Components.RaidwideCast(module, AID.BeastlyFury);

class Bounds : Components.GenericAOEs
{
    public DateTime? Activation { get; private set; }
    public RelSimplifiedComplexPolygon ArenaPoly { get; init; }
    public RelSimplifiedComplexPolygon ArenaInversePoly { get; init; } // used for forbidden zone

    public Bounds(BossModule module) : base(module, AID.BeastlyFury)
    {
        var boundsLarge = new RelSimplifiedComplexPolygon(CurveApprox.Rect(new(22, 0), new(0, 22)));

        var boundsSmall = new RelSimplifiedComplexPolygon(CurveApprox.Rect(new(20, 0), new(0, 20)));
        var hole = CurveApprox.Rect(new(10, 0), new(0, 10));
        foreach (var corner in CurveApprox.Rect(new(20, 0), new(0, 20)))
            boundsSmall = Arena.Bounds.Clipper.Difference(new(boundsSmall), new(hole.Select(d => d + corner)));

        ArenaPoly = boundsSmall;
        ArenaInversePoly = Arena.Bounds.Clipper.Difference(new(boundsLarge), new(ArenaPoly));
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Activation is { } a)
            yield return new(new AOEShapeCustom(ArenaInversePoly), Arena.Center, default, a);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (NumCasts > 0)
            return;

        if (spell.Action == WatchedAction)
            Activation = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && NumCasts == 0)
        {
            NumCasts++;
            Activation = default;

            Arena.Bounds = new ArenaBoundsCustom(20, ArenaPoly);
        }
    }
}

class WildRageKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.WildRage, 16)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
            if (!IsImmune(slot, src.Activation))
            {
                var safeX = src.Origin.X < 750 ? 746 : 754;
                hints.AddForbiddenZone(p => !p.InCircle(new(safeX, 479), 1) && !p.InCircle(new(safeX, 485), 1), src.Activation);
            }
    }
}
class WildRageImpact(BossModule module) : Components.StandardAOEs(module, AID.WildRage2, 8);

class RagingSlice(BossModule module) : Components.GroupedAOEs(module, [AID.RagingSliceInitial, AID.RagingSliceFollowup], new AOEShapeRect(50, 3));

class WildAnguish2(BossModule module) : BossComponent(module)
{
    private readonly Actor?[] _rubbles = new Actor?[4];
    private bool _rubble;

    public readonly List<(Actor Target, Actor? Rubble)> Stacks = [];
    private DateTime _activation;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Rubble)
        {
            var (slot, _) = Raid.WithSlot().Where(p => p.Item2.Class != Class.None).Closest(actor.Position);
            _rubbles[slot] = actor;
            _rubble = true;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Stack && _rubble)
        {
            if (_activation == default)
                _activation = WorldState.FutureTime(5.3f);
            var playerSlot = Raid.FindSlot(actor.InstanceID);
            Stacks.Add((actor, _rubbles[playerSlot]));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (player, _) in Stacks)
            Arena.AddCircle(player.Position, 6, player == pc ? ArenaColor.Safe : ArenaColor.Danger);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var (player, rock) in Stacks)
        {
            if (player == actor)
            {
                hints.Add("Stack with rock!", !player.Position.InCircle(rock?.Position ?? default, 6));

                if (Raid.WithoutSlot(excludeNPCs: true).InRadius(player.Position, 6).Count() > 1)
                    hints.Add("Bait away from raid!");
            }
            else if (player.Position.InCircle(actor.Position, 6))
                hints.Add("GTFO from stack!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var myStack = Stacks.FindIndex(s => s.Target == actor);
        if (myStack >= 0 && Stacks[myStack].Rubble is { } rock)
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(rock.Position, 6), _activation);

        if (Stacks.Count > 0)
            foreach (var other in Raid.WithoutSlot(excludeNPCs: true).Exclude(actor))
                hints.AddForbiddenZone(ShapeContains.Circle(other.Position, 6), _activation);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var theirs = Stacks.FindIndex(s => s.Target == player || s.Rubble == player);
        var ours = Stacks.FindIndex(s => s.Target == pc || s.Rubble == pc);

        if (theirs >= 0 && ours >= 0 && theirs != ours)
            return PlayerPriority.Danger;

        return PlayerPriority.Normal;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WildAnguish or AID.WildAnguish2)
        {
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
            if (Stacks.Count == 0)
            {
                _rubble = false;
                _activation = default;
            }
        }
    }
}

class FallingRock(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.FallingRock, AID.FallingRock, 8, 5.1f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == SpreadAction)
            Spreads.Clear();
    }
}

class WildAnguishStack(BossModule module) : Components.StackWithCastTargets(module, AID.WildAnguish, 6)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (WorldState.Actors.All(x => x.OID != (uint)OID.Rubble))
            base.OnCastStarted(caster, spell);
    }
}

class WildHole(BossModule module) : Components.CastCounter(module, AID.WildRampage2)
{
    private readonly List<WPos> _zones = [];

    public DateTime UnsafeAt;

    public bool Safe => UnsafeAt > WorldState.CurrentTime;
    public float HoleSize => Safe ? 7f : 8f; // 7.5 is too big to reliably dodge the rage and 7 is too small to stay out from the berzerk.

    public override void OnActorEAnim(Actor actor, uint state)
    {
        // two event objects get spawned at the exact same location for each hole, idk why
        if ((OID)actor.OID == OID.RageVoidzone && state == 0x00010002 && !_zones.Any(z => z.AlmostEqual(actor.Position, 1)))
            _zones.Add(actor.Position);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var z in _zones)
            Arena.ZoneCircle(z, HoleSize, UnsafeAt > WorldState.CurrentTime ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_zones.Count > 0)
        {
            var safe = UnsafeAt > WorldState.CurrentTime;
            hints.AddForbiddenZone(p =>
            {
                var inHole = _zones.Any(z => p.InCircle(z, HoleSize));
                return safe ? !inHole : inHole;
            }, UnsafeAt);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            UnsafeAt = Module.CastFinishAt(spell);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var inHole = _zones.Any(z => actor.Position.InCircle(z, HoleSize));
        if (Safe)
            hints.Add("Hide from AOE!", !inHole);
        else if (inHole)
            hints.Add("GTFO from voidzone!");
    }
}

class D113SpectralBerserkerStates : StateMachineBuilder
{
    public D113SpectralBerserkerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Bounds>()
            .ActivateOnEnter<BeastlyFury>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<WildAnguish2>()
            .ActivateOnEnter<WildAnguishStack>()
            .ActivateOnEnter<WildRageKnockback>()
            .ActivateOnEnter<WildHole>()
            .ActivateOnEnter<WildRageImpact>()
            .ActivateOnEnter<RagingSlice>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 737, NameID = 9511)]
public class D113SpectralBerserker(WorldState ws, Actor primary) : BossModule(ws, primary, new(750, 482), new ArenaBoundsSquare(22));
