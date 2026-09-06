namespace BossMod.Stormblood.Ultimate.UCOB;

class P3BlackfireTrio : Components.CastCounter
{
    private Actor? _nael;

    public DateTime BaitAt;
    public Angle RelativeNorth { get; private set; }

    public P3BlackfireTrio(BossModule module) : base(module, AID.BlackfireTrio)
    {
        BaitAt = WorldState.FutureTime(8.5f);
    }

    public bool Active => _nael != null;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_nael != null)
            Arena.ActorInsideBounds(_nael.Position, _nael.Rotation, ArenaColor.Object);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.NaelDeusDarnus && id == 0x1E43 && NumCasts > 0)
        {
            _nael = actor;
            RelativeNorth = (actor.Position - Arena.Center).ToAngle();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MegaflareDive)
            BaitAt = default;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (BaitAt != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 1), BaitAt);
    }
}

class P3ThermionicBeam : Components.UniformStackSpread
{
    public P3ThermionicBeam(BossModule module) : base(module, 4, 0, 8)
    {
        var target = Raid.Player(); // note: target is random
        if (target != null)
            AddStack(target, WorldState.FutureTime(5.3f)); // assume it is activated right when downtime starts
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ThermionicBeam)
            Stacks.Clear();
    }

    // we don't make any effort to stack with the party, since the standard BFT dodge should force everyone to stand together; only add damage prediction
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Stacks)
            hints.AddPredictedDamage(Raid.WithSlot().InRadius(s.Target.Position, StackRadius).Mask(), s.Activation);
    }
}

class P3BlackfireLiquidHell(BossModule module) : LiquidHell(module)
{
    bool _arenaSplitHints = true;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Module.FindComponent<P3BlackfireTrio>() is not { } bft || !_arenaSplitHints)
            return;

        var numSources = NumSources + _predictedByEvent.Count;

        if (numSources < 5)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center, bft.RelativeNorth, 60, 0, 1));
        else if (numSources == 5)
        {
            var relN = bft.RelativeNorth.ToDirection();
            hints.AddForbiddenZone(ShapeDistance.HalfPlane(Arena.Center, actor.Class.IsDD() ? relN.OrthoL() : relN.OrthoR()));
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.MegaflareStack)
            _arenaSplitHints = false;
    }
}

class P3MegaflareTower(BossModule module) : Components.CastTowers(module, AID.MegaflareTower, 3)
{
    BitMask _stackTargets;
    bool _assigned;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction && Towers.Count == 4 && Module.FindComponent<P3BlackfireTrio>() is { } bft)
        {
            var dirN = bft.RelativeNorth.ToDirection();

            for (var i = 0; i < Towers.Count; i++)
            {
                var toTower = Towers[i].Position - Arena.Center;
                var towerN = dirN.Dot(toTower) > 0;
                var towerE = dirN.OrthoR().Dot(toTower) > 0;

                if (towerE)
                {
                    var allowedRole = towerN ? Role.Tank : Role.Healer;

                    Towers.Ref(i).ForbiddenSoakers = Raid.WithSlot(includeDead: true).WhereActor(a => a.Role != allowedRole).Mask();
                }
                else
                    Towers.Ref(i).ForbiddenSoakers = Raid.WithSlot(includeDead: true).WhereActor(a => a.Class.IsSupport()).Mask();
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_stackTargets.Any())
            base.AddAIHints(slot, actor, assignment, hints);

        // before stack markers appear, everyone should head for arena center, relative south of puddles
        else if (Module.FindComponent<P3BlackfireTrio>() is { } bft)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center, bft.RelativeNorth, 3, 3, 30), DateTime.MaxValue);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MegaflareStack)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            _stackTargets.Set(slot);
            foreach (ref var t in Towers.AsSpan())
                t.ForbiddenSoakers.Set(slot);
            AssignTowers();
        }
    }

    void AssignTowers()
    {
        if (_assigned)
            return;

        if (Module.FindComponent<P3BlackfireTrio>() is not { } bft)
            return;

        if (_stackTargets.NumSetBits() != 4)
            return;

        var relativeN = bft.RelativeNorth.ToDirection();

        var towersOrdered = Towers.Index()
            .Where(t => (t.Item.Position - Arena.Center).Dot(relativeN.OrthoL()) > 0)
            .OrderByDescending(t => (t.Item.Position - Arena.Center).Dot(relativeN))
            .ToList();

        var slotsOrdered = Service.Config.Get<PartyRolesConfig>().SlotsPerAssignment(Raid).Skip(4).Where(t => !_stackTargets[t]);

        foreach (var ((i, t), s) in towersOrdered.Zip(slotsOrdered))
            Towers.Ref(i).ForbiddenSoakers = ~BitMask.Build(s);

        _assigned = true;
    }
}

class P3MegaflareStack(BossModule module) : Components.UniformStackSpread(module, 5, 0, 4, 4)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MegaflareStack)
        {
            if (Stacks.Count == 0)
                AddStack(actor, WorldState.FutureTime(5), new(0xff));
            Stacks.Ref(0).ForbiddenPlayers.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MegaflareStack)
            Stacks.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.FindComponent<P3BlackfireTrio>() is { } bft && Stacks.Count > 0 && !Stacks[0].ForbiddenPlayers[slot])
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center + (bft.RelativeNorth + 180.Degrees()).ToDirection() * 8, 2.5f), Stacks[0].Activation);
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}
