namespace BossMod.Dawntrail.Savage.RM12S2TheLindwurm;

class SnakingKick(BossModule module) : Components.StandardAOEs(module, AID.SnakingKick, new AOEShapeCone(40, 90.Degrees()));

class Replication1FirstBait(BossModule module) : BossComponent(module)
{
    record struct Clone(Actor Actor, DateTime Activation, BitMask Targets = default)
    {
        public BitMask Targets = Targets;
    }
    readonly List<Clone> Clones = [];

    public override void Update()
    {
        for (var i = 0; i < Clones.Count; i++)
            Clones.Ref(i).Targets = Raid.WithSlot().SortedByRange(Clones[i].Actor.Position).Take(2).Mask();
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Luzzelwurm && id == 0x11D5)
            Clones.Add(new(actor, WorldState.FutureTime(8.2f)));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (c, _, t) in Clones)
        {
            if (t[pcSlot])
            {
                Arena.ActorInsideBounds(c.Position, c.Rotation, ArenaColor.Enemy);
                foreach (var (_, target) in Raid.WithSlot().IncludedInMask(t))
                {
                    if (Arena.Config.ShowOutlinesAndShadows)
                        Arena.AddCircle(target.Position, 5, 0xFF000000, 2);
                    Arena.AddCircle(target.Position, 5, ArenaColor.Danger);
                }
            }
            else
                Arena.ActorInsideBounds(c.Position, c.Rotation, ArenaColor.Object);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TopTierSlamCast or AID.MightyMagicCast)
            Clones.Clear();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Clones.Count == 0)
            return;

        var baiting = 0;
        var overlap = false;
        var stack = false;
        foreach (var (c, _, t) in Clones)
        {
            if (t[slot])
            {
                baiting++;
                overlap |= Raid.WithSlot().IncludedInMask(t).InRadiusExcluding(actor, 5).Any();
                stack |= Raid.WithSlot().ExcludedFromMask(t).InRadius(actor.Position, 5).Any();
            }
        }

        if (baiting == 0)
            hints.Add("Bait a clone!");
        else if (baiting > 1)
            hints.Add("Too many baits on you!");

        if (overlap)
            hints.Add("GTFO from other bait!");

        if (!stack)
            hints.Add("Stack with buddy!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (_, a, t) in Clones)
            hints.AddPredictedDamage(t, a, AIHints.PredictedDamageType.Shared);
    }
}

class WingedScourge(BossModule module) : Components.StandardAOEs(module, AID.WingedScourge, new AOEShapeCone(50, 15.Degrees()));

class MightyMagicTopTierSlamFirstBait(BossModule module) : Components.UniformStackSpread(module, 5, 5, maxStackSize: 2, alwaysShowSpreads: true)
{
    record struct Caster(Actor Actor, bool Stack, DateTime Activation);

    readonly List<Caster> Casters = [];

    public int NumFire;
    public int NumDark;

    public override void Update()
    {
        Spreads.Clear();
        Stacks.Clear();

        foreach (var (c, s, a) in Casters)
        {
            foreach (var target in Raid.WithoutSlot().SortedByRange(c.Position).Take(s ? 1 : 2))
            {
                if (s)
                    AddStack(target, a);
                else
                    AddSpread(target, a);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TopTierSlamCast:
                Casters.Add(new(caster, true, Module.CastFinishAt(spell, 1)));
                break;
            case AID.MightyMagicCast:
                Casters.Add(new(caster, false, Module.CastFinishAt(spell, 1)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TopTierSlamStack:
                Casters.RemoveAll(c => c.Stack);
                NumFire++;
                break;
            case AID.MightyMagicSpread:
                Casters.RemoveAll(c => !c.Stack);
                NumDark++;
                break;
        }
    }

    // copied from base, modified to only show Spread hint if another baiter is close to player, spreads are expected to hit partners
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!EnableHints)
            return;

        var isSpreadTarget = false;
        var isStackTarget = false;
        if (Spreads.FindIndex(s => s.Target == actor) is var iSpread && iSpread >= 0)
        {
            isSpreadTarget = true;
            hints.Add("Spread!", Raid.WithoutSlot().InRadiusExcluding(actor, Spreads[iSpread].Radius).Any(s => IsSpreadTarget(s) || IsStackTarget(s)));
        }
        else if (Stacks.FindIndex(s => s.Target == actor) is var iStack && iStack >= 0)
        {
            isStackTarget = true;
            var stack = Stacks[iStack];
            int numStacked = 1; // always stacked with self
            bool stackedWithOtherStackOrAvoid = false;
            foreach (var (j, other) in Raid.WithSlot().InRadiusExcluding(actor, stack.Radius))
            {
                ++numStacked;
                stackedWithOtherStackOrAvoid |= stack.ForbiddenPlayers[j] || IsStackTarget(other);
            }
            hints.Add("Stack!", stackedWithOtherStackOrAvoid || numStacked < stack.MinSize || numStacked > stack.MaxSize);
        }
        else
        {
            int numParticipatingStacks = 0;
            int numUnsatisfiedStacks = 0;
            foreach (var s in ActiveStacks.Where(s => !s.ForbiddenPlayers[slot]))
            {
                if (actor.Position.InCircle(s.Target.Position, s.Radius))
                    ++numParticipatingStacks;
                else if (Raid.WithoutSlot().InRadiusExcluding(s.Target, s.Radius).Count() + 1 < s.MinSize)
                    ++numUnsatisfiedStacks;
            }

            if (numParticipatingStacks > 1)
                hints.Add("Stack!");
            else if (numParticipatingStacks == 1)
                hints.Add("Stack!", false);
            else if (numUnsatisfiedStacks > 0)
                hints.Add("Stack!");
            // else: don't show anything, all potential stacks are already satisfied without a player
            //hints.Add("Stack!", ActiveStacks.Count(s => !s.ForbiddenPlayers[slot] && actor.Position.InCircle(s.Target.Position, s.Radius)) != 1);
        }

        if ((isSpreadTarget || isStackTarget) && ActiveSpreads.Any(s => s.Target != actor && actor.Position.InCircle(s.Target.Position, s.Radius)))
        {
            hints.Add("GTFO from spreads!");
        }
        else if (ActiveStacks.Any(s => s.Target != actor && s.ForbiddenPlayers[slot] && actor.Position.InCircle(s.Target.Position, s.Radius)))
        {
            hints.Add("GTFO from forbidden stacks!");
        }
    }
}

class Replication1SecondBait(BossModule module) : BossComponent(module)
{
    public enum Assignment
    {
        None,
        Fire,
        Dark
    }

    readonly Assignment[] _assignments = new Assignment[8];

    public record struct Clone(Actor Actor, Assignment Element);
    public readonly List<Clone> Clones = [];

    int _numFire;
    int _numDark;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_assignments[slot] != default)
            hints.Add($"Assignment: {_assignments[slot]}", false);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TopTierSlamCast:
                Clones.Add(new(caster, Assignment.Fire));
                break;
            case AID.MightyMagicCast:
                Clones.Add(new(caster, Assignment.Dark));
                break;
            case AID.WingedScourgeCastVertical:
            case AID.WingedScourgeCastHorizontal:
                Clones.Add(new(caster, Assignment.None));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TopTierSlamStack)
        {
            _numFire++;
            Assign();
        }
        if ((AID)spell.Action.ID == AID.MightyMagicSpread)
        {
            _numDark++;
            Assign();
        }
    }

    public override void OnActorPlayActionTimelineSync(Actor actor, List<(ulong InstanceID, ushort ID)> events)
    {
        if (Clones.FirstOrNull(c => c.Actor == actor) is { } clone)
        {
            Clones.Remove(clone);
            foreach (var (iid, id) in events)
            {
                if (id == 0x11D3 && WorldState.Actors.Find(iid) is { } child)
                    Clones.Add(new(child, clone.Element));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (c, a) in Clones)
        {
            if (a == Assignment.None)
                continue;

            var color = _assignments[pcSlot] != default
                ? a == _assignments[pcSlot] ? ArenaColor.Object : ArenaColor.PlayerGeneric
                : a == Assignment.Fire ? ArenaColor.Object : ArenaColor.Vulnerable;
            Arena.ActorInsideBounds(c.Position, c.Rotation, color);
        }
    }

    void Assign()
    {
        if (_numFire != 1 || _numDark != 2)
            return;

        foreach (var (i, player) in Raid.WithSlot())
            _assignments[i] = player.FindStatus(SID._Gen_DarkResistanceDownII, DateTime.MinValue) == null ? Assignment.Dark : Assignment.Fire;
    }
}

class WingedScourgeSecond(BossModule module) : Components.GenericAOEs(module)
{
    Angle Orientation;
    bool Recorded;
    bool Draw;

    readonly List<Actor> Clones = [];
    readonly List<(WPos Source, DateTime Activation)> Sources = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Draw)
            foreach (var (src, a) in Sources)
            {
                yield return new(new AOEShapeCone(50, 15.Degrees()), src, Orientation, a);
                yield return new(new AOEShapeCone(50, 15.Degrees()), src, Orientation + 180.Degrees(), a);
            }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WingedScourgeCastVertical or AID.WingedScourgeCastHorizontal && !Recorded)
        {
            Clones.Add(caster);
            Orientation = (AID)spell.Action.ID == AID.WingedScourgeCastVertical ? default : 90.Degrees();
        }
    }

    public override void OnActorPlayActionTimelineSync(Actor actor, List<(ulong InstanceID, ushort ID)> events)
    {
        if (Clones.Remove(actor))
        {
            Recorded = true;
            foreach (var (iid, id) in events)
            {
                if (id == 0x11D3 && WorldState.Actors.Find(iid) is { } clone)
                    Clones.Add(clone);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Dash && Clones.Contains(caster))
        {
            Draw = true;
            Sources.Add((spell.TargetXZ, WorldState.FutureTime(7.2f)));
        }
    }
}

class MightyMagicTopTierSlamSecondBait(BossModule module) : Components.UniformStackSpread(module, 5, 5, maxStackSize: 2, alwaysShowSpreads: true)
{
    readonly Replication1SecondBait _rep1Bait = module.FindComponent<Replication1SecondBait>()!;

    record struct Source(WPos Position, bool Stack, DateTime Activation);
    readonly List<Source> Sources = [];

    public int NumFire;
    public int NumDark;

    BitMask _fireVuln;
    BitMask _darkVuln;

    public override void Update()
    {
        Spreads.Clear();
        Stacks.Clear();

        foreach (var (c, s, a) in Sources)
        {
            foreach (var target in Raid.WithoutSlot().SortedByRange(c).Take(s ? 1 : 2))
            {
                if (s)
                    AddStack(target, a, _fireVuln);
                else
                    AddSpread(target, a);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_fireVuln[slot] && IsStackTarget(actor))
            hints.Add("Avoid baiting fire!");

        if (_darkVuln[slot] && IsSpreadTarget(actor))
            hints.Add("Avoid baiting dark!");
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID._Gen_FireResistanceDownII:
                _fireVuln.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case SID._Gen_DarkResistanceDownII:
                _darkVuln.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Dash:
                if (_rep1Bait.Clones.FirstOrNull(c => c.Actor == caster && c.Element != default) is { } clone)
                    Sources.Add(new(spell.TargetXZ, clone.Element == Replication1SecondBait.Assignment.Fire, WorldState.FutureTime(10))); // FIXME: time is wrong
                break;
            case AID.TopTierSlamStack:
                NumFire++;
                Sources.RemoveAll(s => s.Stack);
                break;
            case AID.MightyMagicSpread:
                NumDark++;
                Sources.RemoveAll(s => !s.Stack);
                break;
        }
    }
}

class DoubleSobatBuster(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(40, 90.Degrees()), (uint)IconID._Gen_Icon_sharelaser2tank5sec_c0k1)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DoubleSobatBuster1 or AID.DoubleSobatBuster2 or AID.DoubleSobatBuster3 or AID.DoubleSobatBuster4)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

class DoubleSobatRepeat(BossModule module) : Components.StandardAOEs(module, AID.DoubleSobatRepeat, new AOEShapeCone(40, 90.Degrees()));

class EsotericFinisher : Components.GenericBaitAway
{
    public EsotericFinisher(BossModule module) : base(module, AID.EsotericFinisher, centerAtTarget: true)
    {
        CurrentBaits.AddRange(Raid.WithoutSlot().OrderByDescending(r => r.Role == Role.Tank).Take(2).Select(r => new Bait(module.PrimaryActor, r, new AOEShapeCircle(10), WorldState.FutureTime(10)))); // FIXME time is wrong
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}
