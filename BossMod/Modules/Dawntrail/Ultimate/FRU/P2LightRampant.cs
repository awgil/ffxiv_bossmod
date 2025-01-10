namespace BossMod.Dawntrail.Ultimate.FRU;

class P2LightRampant(BossModule module) : BossComponent(module)
{
    private readonly Actor?[] _tetherTargets = new Actor?[PartyState.MaxPartySize];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        for (int i = 0; i < _tetherTargets.Length; ++i)
        {
            var source = Raid[i];
            var target = _tetherTargets[i];
            if (source != null && target != null)
                Arena.AddLine(source.Position, target.Position, (source.Position - target.Position).LengthSq() < 625 ? ArenaColor.Danger : ArenaColor.Safe);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.LightRampantChains or TetherID.LightRampantCurse && Raid.FindSlot(source.InstanceID) is var slot && slot >= 0)
            _tetherTargets[slot] = WorldState.Actors.Find(tether.Target);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.LightRampantChains or TetherID.LightRampantCurse && Raid.FindSlot(source.InstanceID) is var slot && slot >= 0)
            _tetherTargets[slot] = null;
    }
}

class P2LuminousHammer(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.LuminousHammer, ActionID.MakeSpell(AID.LuminousHammer), 7.1f, true)
{
    public readonly int[] BaitsPerPlayer = new int[PartyState.MaxPartySize];
    public readonly WDir[] PrevBaitOffset = new WDir[PartyState.MaxPartySize];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // there are dedicated components for hints

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && Raid.FindSlot(spell.MainTargetID) is var slot && slot >= 0)
        {
            ++NumCasts;
            PrevBaitOffset[slot] = (Raid[slot]?.Position ?? Module.Center) - Module.Center;
            if (++BaitsPerPlayer[slot] == 5)
                CurrentBaits.RemoveAll(b => b.Target == Raid[slot]); // last bait
        }
    }
}

class P2BrightHunger1(BossModule module) : Components.GenericTowers(module, ActionID.MakeSpell(AID.BrightHunger))
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private BitMask _forbidden;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // there are dedicated components for hints

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.LuminousHammer)
        {
            _forbidden.Set(Raid.FindSlot(actor.InstanceID));
            RebuildTowers();
        }
    }

    private void RebuildTowers()
    {
        List<(int slot, int prio)> conga = [];
        foreach (var (slot, group) in _config.P2LightRampantAssignment.Resolve(Raid))
            if (!_forbidden[slot])
                conga.Add((slot, group));
        conga.SortBy(kv => kv.prio);
        if (conga.Count == 6)
        {
            var firstSouth = conga.FindIndex(kv => kv.prio >= 4);
            if (firstSouth == 2)
            {
                // rotate SW->NW
                (conga[2], conga[1]) = (conga[1], conga[2]);
                (conga[1], conga[0]) = (conga[0], conga[1]);
            }
            else if (firstSouth == 4)
            {
                // rotate NE->SE
                (conga[3], conga[4]) = (conga[4], conga[3]);
                (conga[4], conga[5]) = (conga[5], conga[4]);
            }
            // swap SE & SW to make order CW from NW
            (conga[3], conga[5]) = (conga[5], conga[3]);
            // finally, swap N & S and NW & NE to convert prepositions to tower positions
            (conga[0], conga[2]) = (conga[2], conga[0]);
            (conga[1], conga[4]) = (conga[4], conga[1]);
        }
        else
        {
            // bad assignments, assume there are none set
            conga.Clear();
        }

        Towers.Clear();
        for (int i = 0; i < 6; ++i)
        {
            var dir = (240 - i * 60).Degrees();
            var forbidden = conga.Count == 6 ? BitMask.Build(conga[i].slot) ^ new BitMask(0xFF) : _forbidden;
            Towers.Add(new(Module.Center + 16 * dir.ToDirection(), 4, 1, 1, forbidden, WorldState.FutureTime(10.3f)));
        }
    }
}

// note: we can start showing aoes ~3s earlier if we check spawns, but it's not really needed
class P2HolyLightBurst(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HolyLightBurst), new AOEShapeCircle(11), 3)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // there are dedicated components for hints
}

class P2PowerfulLight(BossModule module) : Components.UniformStackSpread(module, 5, 0, 4, 4)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // there are dedicated components for hints

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.WeightOfLight)
            AddStack(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.PowerfulLight)
            Stacks.Clear();
    }
}

class P2BrightHunger2(BossModule module) : Components.GenericTowers(module, ActionID.MakeSpell(AID.BrightHunger))
{
    private BitMask _forbidden;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // there are dedicated components for hints

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Lightsteeped && status.Extra >= 3)
            _forbidden.Set(Raid.FindSlot(actor.InstanceID));
    }

    // TODO: better criteria for activating a tower...
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Towers.Count == 0 && (AID)spell.Action.ID == AID.HolyLightBurst)
            Towers.Add(new(Module.Center, 4, 1, 8, _forbidden, WorldState.FutureTime(6.5f)));
    }
}

class P2HouseOfLightBoss(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.HouseOfLightBossAOE), false)
{
    private static readonly AOEShapeCone _shape = new(60, 30.Degrees()); // TODO: verify angle

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HouseOfLightBoss)
            foreach (var p in Raid.WithoutSlot(true))
                CurrentBaits.Add(new(caster, p, _shape, Module.CastFinishAt(spell, 0.9f)));
    }
}

// movement to soak towers and bait first 3 puddles (third puddle is baited right before towers resolve)
class P2LightRampantAITowers(BossModule module) : BossComponent(module)
{
    private readonly P2LuminousHammer? _puddles = module.FindComponent<P2LuminousHammer>();
    private readonly P2BrightHunger1? _towers = module.FindComponent<P2BrightHunger1>();

    private const float BaitOffset = 8;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_puddles == null || _towers == null)
            return;

        var bait = _puddles.ActiveBaitsOn(actor).FirstOrDefault();
        if (bait.Target != null)
        {
            if (_puddles.BaitsPerPlayer[slot] == 0)
            {
                // position for first bait
                var partner = _puddles.ActiveBaitsNotOn(actor).FirstOrDefault().Target;
                if (partner == null)
                    return; // we can't resolve the hint without knowing the partner

                // logic:
                // - if actor and partner are north and south, stay on current side
                // - if both are on the same side, the 'more clockwise' one (NE/SW) moves to the opposite side
                // TODO: last rule is fuzzy in practice, see if we can adjust better
                var north = actor.Position.Z < Module.Center.Z;
                if (north == (partner.Position.Z < Module.Center.Z))
                {
                    // same side, see if we need to swap
                    var moreRight = actor.Position.X > partner.Position.X;
                    var moreCW = north == moreRight;
                    north ^= moreCW;
                }

                var preposSpot = Module.Center + new WDir(0, north ? -BaitOffset : BaitOffset);
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(preposSpot, 1), bait.Activation);
            }
            else
            {
                // each next bait is just previous position rotated CW by 45 degrees
                // note that this is only really relevant for second and third puddles - after that towers resolve and we use different component
                //var nextSpot = Module.Center + BaitOffset * _puddles.PrevBaitOffset[slot].Normalized().Rotate(-45.Degrees());
                //hints.AddForbiddenZone(ShapeDistance.InvertedCircle(nextSpot, 3));
                var shape = ShapeDistance.DonutSector(Module.Center, BaitOffset - 1, BaitOffset + 2, Angle.FromDirection(_puddles.PrevBaitOffset[slot]) - 45.Degrees(), 30.Degrees());
                hints.AddForbiddenZone(p => -shape(p), DateTime.MaxValue);
            }
        }
        else
        {
            // if we have one tower assigned, stay inside it, somewhat closer to the edge
            var assignedTowerIndex = _towers.Towers.FindIndex(t => !t.ForbiddenSoakers[slot]);
            if (assignedTowerIndex >= 0 && _towers.Towers.FindIndex(assignedTowerIndex + 1, t => !t.ForbiddenSoakers[slot]) < 0)
            {
                ref var t = ref _towers.Towers.Ref(assignedTowerIndex);
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.Center + (t.Position - Module.Center) * 1.125f, 2), t.Activation); // center is at R16, x1.125 == R18
            }
            // else: we either have no towers assigned (== doing puddles), or have multiple assigned (== assignments failed), so do nothing
        }
    }
}

// movement to preposition for resolving stacks
class P2LightRampantAIStack(BossModule module) : BossComponent(module)
{
    private readonly P2LuminousHammer? _puddles = module.FindComponent<P2LuminousHammer>();
    private readonly P2PowerfulLight? _stack = module.FindComponent<P2PowerfulLight>();
    private readonly P2HolyLightBurst? _orbs = module.FindComponent<P2HolyLightBurst>();

    public const float Radius = 18;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_puddles == null || _stack == null || _orbs == null)
            return;

        // initially we don't know whether first orbs will cover N/S, puddle baiter is still relatively far away and has two baits left
        // when orbs start, baiter has just finished his puddles; he can still be relatively far away from N/S center, so we might need to wait for him
        var northCamp = IsNorthCamp(actor);
        var startingDir = (northCamp ? 180 : 0).Degrees();
        var startingPos = Module.Center + new WDir(0, northCamp ? -Radius : Radius);
        if (_puddles.ActiveBaits.Any())
        {
            // just move to starting position, until all puddles are resolved
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(startingPos, 1), DateTime.MaxValue);
            return;
        }

        var isStackTarget = _stack.IsStackTarget(actor);
        var haveOrbs = _orbs.Casters.Count > 0;
        var centerDangerous = haveOrbs && _orbs.ActiveCasters.Any(c => actor.Position.Z - Module.Center.Z is var off && (northCamp ? off < -15 : off > 15));
        var idealDestDir = startingDir - (centerDangerous ? 40 : 20).Degrees(); // alt: haveOrbs ? 20 : 30 (but i don't think it's how people really move...)
        var idealPos = Module.Center + Radius * idealDestDir.ToDirection();

        if (isStackTarget)
        {
            // as a stack target, our responsibility is to wait for everyone to stack up, then carefully move towards ideal dir
            // note that we need to be careful to avoid oscillations
            var toIdeal = idealPos - actor.Position;
            foreach (var partner in Raid.WithoutSlot().Exclude(actor).Where(p => IsNorthCamp(p) == northCamp))
            {
                var toPartner = partner.Position - actor.Position;
                var distSq = toPartner.LengthSq();
                if (distSq > 9 && toIdeal.Dot(toPartner) < 0)
                {
                    // partner is far enough away, and moving towards ideal pos will not bring us closer => just stay where we are
                    return;
                }
            }
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(idealPos, 1), DateTime.MaxValue);
        }
        else if (_stack.Stacks.FirstOrDefault(s => IsNorthCamp(s.Target) == northCamp).Target is var stackTarget && stackTarget != null)
        {
            // otherwise we just want to stay close to the stack target, slightly offset to the ideal position
            var dirToIdeal = idealPos - stackTarget.Position;
            var dest = dirToIdeal.LengthSq() <= 4 ? idealPos : stackTarget.Position + 2 * dirToIdeal.Normalized();
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(dest, 1), DateTime.MaxValue);
        }
    }

    private bool IsNorthCamp(Actor actor) => (_puddles?.ActiveBaitsOn(actor).Any() ?? false) ? actor.Position.X < Module.Center.X : actor.Position.Z < Module.Center.Z;
}

// movement to dodge orbs after resolving stack
class P2LightRampantAIOrbs(BossModule module) : BossComponent(module)
{
    private readonly P2HolyLightBurst? _orbs = module.FindComponent<P2HolyLightBurst>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_orbs == null || _orbs.Casters.Count == 0)
            return;

        if (_orbs.NumCasts == 0)
        {
            // dodge first orbs, while staying near edge
            hints.AddForbiddenZone(ShapeDistance.Circle(Module.Center, 16));
        }
        else
        {
            // dodge second orbs, while trying to come closer to the center
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.Center, 6), DateTime.MaxValue);
        }

        // actual orb aoes
        foreach (var c in _orbs.ActiveCasters)
            hints.AddForbiddenZone(_orbs.Shape.Distance(c.Position, default), Module.CastFinishAt(c.CastInfo, -1));
    }
}

// movement to soak central tower (if needed) and preposition for banish
class P2LightRampantAIResolve(BossModule module) : BossComponent(module)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private readonly P2BrightHunger2? _tower = module.FindComponent<P2BrightHunger2>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_tower == null || _tower.Towers.Count == 0)
            return; // no towers

        ref var t = ref _tower.Towers.Ref(0);
        hints.AddForbiddenZone(t.ForbiddenSoakers[slot] ? ShapeDistance.Circle(t.Position, t.Radius) : ShapeDistance.InvertedCircle(t.Position, t.Radius), t.Activation);

        var clockspot = _config.P2Banish2SpreadSpots[assignment];
        if (clockspot >= 0)
        {
            var assignedDirection = (180 - 45 * clockspot).Degrees();
            hints.AddForbiddenZone(ShapeDistance.InvertedCone(t.Position, 50, assignedDirection, 30.Degrees()), DateTime.MaxValue);
        }
    }
}
