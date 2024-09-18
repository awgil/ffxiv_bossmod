namespace BossMod.Shadowbringers.Dungeon.D11HeroesGauntlet.D113SpectralBerserker;
public enum OID : uint
{
    Boss = 0x2EFD, // R3.000, x? 
    Helper = 0x233C, // R0.500, x?, Helper type
    SteelarmCerigg = 0x2E80, // R0.500, x?
    GransonOfTheMournfulBlade = 0x2E81, // R0.500, x?
    LueReeqOfTheGildedBow = 0x2E82, // R0.500, x?
    GiottTheAleforged = 0x2E7F, // R0.500, x?
    Rubble = 0x2EFE, // R2.500, x?
    RageVoidzone = 0x1EA1A1,
}
public enum AID : uint
{
    Attack = 870, // 2F36/2EFD/2E7F/2E81->player/2E81/2F33/2F36, no cast, single-target
    AbsoluteBlizzardIV = 21062, // 2F38->2E82, 2.0s cast, single-target
    FastThrust = 21050, // 2F33->2E7F, no cast, single-target
    FullThrust = 21051, // 2F33->2E7F, no cast, single-target
    HanaArashi = 21057, // 2F36->2E81, no cast, single-target
    Disintegrate = 21040, // 2F32->2E80, no cast, single-target
    YamaArashi = 21056, // 2F36->2E81, no cast, single-target
    BeastlyFury = 21004, // 2EFD->self, 4.0s cast, range 50 circle
    AbsoluteFireIV = 21063, // 2F38->2E82, 2.0s cast, single-target
    TigerKick = 21039, // 2F32->2E80, no cast, single-target

    WildAnguish = 21000, // 2EFD->players, 5.0s cast, range 6 circle
    WildAnguish2 = 21001, // 2EFD->players, no cast, range 6 circle

    Bootshine = 21038, // 2F32->2E80, no cast, single-target

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
class BeastlyFury(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.BeastlyFury))
{
    private readonly List<AOEInstance> _arenaVoidZones = [];
    private static readonly AOEShapeRect rect = new(10, 10, 10);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_arenaVoidZones.Count > 0)
        {
            for (var i = 0; i < _arenaVoidZones.Count; i++)
            {
                yield return _arenaVoidZones[i] with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BeastlyFury)
        {
            _arenaVoidZones.Clear();
            Module.Arena.Bounds = D113SpectralBerserker.PostFury;
            int[] xOffsets = [730, 770];
            int[] zOffsets = [462, 502];

            foreach (var zOffset in zOffsets)
            {
                foreach (var xOffset in xOffsets)
                {
                    _arenaVoidZones.Add(new(rect, new(xOffset, zOffset)));
                }
            }
        }
        base.OnEventCast(caster, spell);
    }
}
class WildRageKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.WildRage), 16);
class WildRageImpact(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.WildRage2), 8);
class WildRageDonut(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WildRage3), new AOEShapeDonut(10, 50)); //Unneeded
class FallingRock(BossModule module) : Components.IconStackSpread(module, (uint)IconID.Stack, (uint)IconID.FallingRock, ActionID.MakeSpell(AID.WildAnguish2), ActionID.MakeSpell(AID.FallingRock), 6, 8, 1f, 2, 2, true)
{
    private IEnumerable<Actor> Rubbles => Module.Enemies(OID.Rubble).Where(e => e.IsTargetable);
    private IEnumerable<Actor> VulnDebuffTargets => WorldState.Party.WithoutSlot().Where(x => x.FindStatus(SID.MagicVulnUp) != null);
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WildAnguish or AID.WildAnguish2)
        {
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
            ++NumFinishedStacks;
        }
    }
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.Rubble)
        {
            Spreads.Clear();
            ++NumFinishedSpreads;
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.Rubble)
        {
            Stacks.Clear();
            Spreads.Clear();
        }
    }
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Spreads.FindIndex(s => s.Target == actor) is var iSpread && iSpread >= 0)
        {
            hints.Add("Spread!", Raid.WithoutSlot().InRadiusExcluding(actor, Spreads[iSpread].Radius).Any());
        }
        else if (Stacks.FindIndex(s => s.Target == actor) is var iStack && iStack >= 0)
        {
            var stack = Stacks[iStack];
            var numStacked = 1;
            var stackedWithOtherStackOrAvoid = false;
            foreach (var other in Rubbles)
            {
                ++numStacked;
                stackedWithOtherStackOrAvoid |= IsStackTarget(other);
            }
            hints.Add("Stack with Rubble!!", stackedWithOtherStackOrAvoid || numStacked < stack.MinSize || numStacked > stack.MaxSize);
        }
        if (ActiveSpreads.Any(s => s.Target != actor && actor.Position.InCircle(s.Target.Position, s.Radius)))
        {
            hints.Add("GTFO from spreads!");
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Stacks.Count > 0)
        {
            if (VulnDebuffTargets.Contains(actor))
            {
                foreach (var s in WorldState.Party.WithoutSlot().Where(s => s != actor))
                {
                    hints.AddForbiddenZone(ShapeDistance.Circle(s.Position, 6));
                }
            }
            else
            {
                var unsuitableRubbles = Rubbles
                .Where(rubble =>
                {
                    var playersInRadius = Raid.WithSlot().InRadius(rubble.Position, 2).Count();
                    var rubblesInRadius = Rubbles.InRadius(rubble.Position, 2).Count();
                    return !(playersInRadius + rubblesInRadius == 2 && playersInRadius == 1 && rubblesInRadius == 1);
                })
                .OrderBy(rubble => (rubble.Position - actor.Position).LengthSq())
                .FirstOrDefault();

                if (unsuitableRubbles != null)
                {
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(unsuitableRubbles.Position, 6));
                }
            }
        }

        foreach (var spreadFrom in ActiveSpreads.Where(s => s.Target != actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(spreadFrom.Target.Position, spreadFrom.Radius), spreadFrom.Activation);

        foreach (var avoid in ActiveStacks.Where(s => s.Target != actor && s.ForbiddenPlayers[slot]))
        {
            hints.AddForbiddenZone(ShapeDistance.Circle(avoid.Target.Position, avoid.Radius), avoid.Activation.AddSeconds(4));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!AlwaysShowSpreads && Spreads.FindIndex(s => s.Target == pc) is var iSpread && iSpread >= 0)
        {
            Arena.AddCircle(pc.Position, Spreads[iSpread].Radius, ArenaColor.Danger);
        }
        else
        {
            foreach (var s in ActiveStacks)
            {
                if (VulnDebuffTargets.Contains(pc))
                {
                    Arena.AddCircleFilled(s.Target.Position, s.Radius, ArenaColor.Danger);
                }
                else
                {
                    if (Arena.Config.ShowOutlinesAndShadows)
                        Arena.AddCircle(s.Target.Position, s.Radius, 0xFF000000, 2);
                    Arena.AddCircle(s.Target.Position, s.Radius, ArenaColor.Safe);
                }
            }
            foreach (var s in ActiveSpreads)
            {
                if (Arena.Config.ShowOutlinesAndShadows)
                    Arena.AddCircle(s.Target.Position, s.Radius, 0xFF000000, 2);
                Arena.AddCircle(s.Target.Position, s.Radius, ArenaColor.Danger);
            }
        }
        Arena.Actors(Rubbles, ArenaColor.Object, true);
    }
}
class Jump(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Jump));
class WildRampage(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WildRampage2), new AOEShapeRect(50, 25, 3)); //Ended up not using, maybe someone can union it I cba anymore.
class WildRageVoidZone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _craters = [];
    private readonly List<Actor> VoidzoneStore = [];
    private static readonly AOEShapeCircle circle = new(8);
    private bool safe;
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_craters.Count > 0 && !safe)
        {
            for (var i = 0; i < _craters.Count; i++)
            {
                yield return _craters[i] with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state != 0x00010002 || (OID)actor.OID != OID.RageVoidzone)
            return;
        _craters.Add(new(circle, actor.Position));
        VoidzoneStore.Add(actor);
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((OID)caster.OID is OID.Boss && (AID)spell.Action.ID is AID.WildRage)
        {
            safe = false;
            foreach (var z in VoidzoneStore)
            {
                _craters.Add(new(circle, z.Position));
            }
        }
        if ((OID)caster.OID is OID.Helper && (AID)spell.Action.ID is AID.WildRampage2)
        {
            safe = true;
            _craters.Clear();
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((OID)caster.OID is OID.Boss && (AID)spell.Action.ID is AID.WildRage)
        {
            safe = false;
            foreach (var z in VoidzoneStore)
            {
                _craters.Add(new(circle, z.Position));
            }
        }
        if ((OID)caster.OID is OID.Helper && (AID)spell.Action.ID is AID.WildRampage2)
        {
            safe = false;
            foreach (var z in VoidzoneStore)
            {
                _craters.Add(new(circle, z.Position));
            }
        }
    }
}
class WildRampageTower(BossModule module) : BossComponent(module)
{
    public List<Components.GenericTowers.Tower> _towers = [];
    public float Radius = 8f;
    private readonly List<Actor> VoidzoneStore = [];
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state != 0x00010002 && (OID)actor.OID != OID.RageVoidzone)
            return;
        VoidzoneStore.Add(actor);
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((OID)caster.OID is OID.Helper && (AID)spell.Action.ID is AID.WildRampage2)
        {
            foreach (var z in VoidzoneStore)
            {
                _towers.Add(new(z.Position, Radius));
            }
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((OID)caster.OID is OID.Helper && (AID)spell.Action.ID is AID.WildRampage2)
        {
            _towers.Clear();
        }
    }
    public static void DrawTower(MiniArena arena, WPos pos, float radius, bool safe)
    {
        if (arena.Config.ShowOutlinesAndShadows)
            arena.AddCircle(pos, radius, 0xFF000000, 3);
        arena.AddCircle(pos, radius, safe ? ArenaColor.Safe : ArenaColor.Danger, 2);
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in _towers)
            DrawTower(Arena, t.Position, t.Radius, !t.ForbiddenSoakers[pcSlot]);
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_towers.Count == 0)
            return;
        var bestTower = _towers[0];
        var maxSoakers = -1;
        foreach (var t in _towers)
        {
            var curSoakers = Raid.WithSlot().InRadius(t.Position, 6).Count();
            if (curSoakers > maxSoakers)
            {
                maxSoakers = curSoakers;
                bestTower = t;
            }
        }
        hints.AddForbiddenZone(new AOEShapeDonut(6, 50), bestTower.Position);
    }
}
class RagingSlice(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.RagingSliceInitial), new AOEShapeRect(50, 3));
class RagingSliceFollowup(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.RagingSliceFollowup), new AOEShapeRect(50, 3));

class D113SpectralBerserkerStates : StateMachineBuilder
{
    public D113SpectralBerserkerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BeastlyFury>()
            .ActivateOnEnter<WildRageKnockback>()
            .ActivateOnEnter<WildRageImpact>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<Jump>()
            .ActivateOnEnter<WildRageVoidZone>()
            .ActivateOnEnter<WildRampageTower>()
            .ActivateOnEnter<RagingSlice>()
            .ActivateOnEnter<RagingSliceFollowup>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 737, NameID = 9511)]
public class D113SpectralBerserker(WorldState ws, Actor primary) : BossModule(ws, primary, new(750f, 482f), DefaultBounds)
{
    public static readonly ArenaBoundsSquare DefaultBounds = new(22);
    public static readonly ArenaBoundsSquare PostFury = new(20);
}
