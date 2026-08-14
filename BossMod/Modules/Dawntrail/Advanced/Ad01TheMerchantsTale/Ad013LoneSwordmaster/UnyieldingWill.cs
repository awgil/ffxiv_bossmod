namespace BossMod.Dawntrail.Advanced.Ad01MerchantsTale.Ad013LoneSwordmaster;

sealed class UnyieldingWill(BossModule module) : Components.GenericBaitAway(module)
{
    // safe spot depends on player Malefic debuff
    // ForceOfWillTether follows player along axis that ForceOfWillSmall is facing
    // each player gets targetted? if so, display AOE for non-player rect
    // 2nd tether breaks if short enough; possible to stand on it if starting helper is in a safe direction
    // anywhere from 1-3 directions safe
    private readonly DebuffTracker _debuffs = module.FindComponent<DebuffTracker>()!;
    private readonly Dictionary<Actor, (Actor Source, Actor Intermediate)> _playerTethers = [];
    private readonly List<(Actor Source, Actor Target)> _tethers = [];
    public override void Update()
    {
        base.Update();
        var count = CurrentBaits.Count;
        if (count == 0)
            return;
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < count; ++i)
        {
            ref var b = ref baits[i];
            var length = (b.Target.Position - b.Source.Position).Length();
            if (b.Shape is AOEShapeRect rect && rect.LengthFront != length)
            {
                b.Shape = new AOEShapeRect(length, 2f);
            }
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        // only fires 1 if player standing on top of tether helper; fires 2 if away from tether helper
        // player bound until AOEs finish so safe to ignore 2
        if (spell.Action.ID == (uint)AID.UnyieldingWill1)
        {
            CurrentBaits.Clear();
            _playerTethers.Clear();
            _tethers.Clear();
        }
    }
    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        // how to AI handle case when tether helper spawns at player position so tether to player never happens?
        if (tether.ID == (uint)TetherID.UnyieldingWill)
        {
            var target = WorldState.Actors.Find(tether.Target);
            if (target == null)
                return;

            _tethers.Add((source, target));
            CurrentBaits.Add(new(source, target, new AOEShapeRect(default, 2f)));

            var isTargetPlayer = Raid.WithoutSlot().Contains(target);

            if (!isTargetPlayer)
                return;

            var tethers = CollectionsMarshal.AsSpan(_tethers);
            var len = tethers.Length;
            for (var i = 0; i < len; i++)
            {
                if (tethers[i].Target == source)
                {
                    _playerTethers[target] = (tethers[i].Source, tethers[i].Target);
                    break;
                }
            }
        }
    }
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        // add hint if player will be hit on unsafe side
        // add hint if player is in another player's baited path
        // can't control start position; only add bait away hint if another player between tether helper and target player
        // 2nd tether drops if player <= 2f from tether helper
        // does initial will have own rotation?
        if (ActiveBaits.Count == 0)
            return;

        var hasTether = _playerTethers.TryGetValue(actor, out var tethers);
        if (hasTether)
        {
            var inter = tethers.Intermediate;
            var angle = inter.AngleTo(actor);
            var angles = _debuffs.GetAngles(slot);
            var len = angles.Length;

            if (len > 0)
            {
                var safe = false;
                for (var i = 0; i < len; i++)
                {
                    if (angle.AlmostEqual(angles[i], 45f.Degrees().Rad))
                    {
                        safe = true;
                    }
                }

                if (!safe)
                {
                    hints.Add("Face safe side to AOE!");
                }
            }
        }

        // base avoid other player baited AOEs
        base.AddHints(slot, actor, hints);
        var count = CurrentBaits.Count;
        var id = actor.InstanceID;
        for (var i = 0; i < count; ++i)
        {
            var b = CurrentBaits[i];
            if (b.Target.InstanceID != id)
            {
                continue;
            }
            if (PlayersClippedBy(ref b).Count != 0)
            {
                hints.Add(BaitAwayHint);
                return;
            }
        }
    }
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (OnlyShowOutlines || IgnoreOtherBaits)
            return;

        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;
        var pcID = pc.InstanceID;
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            var hasPlayer = _playerTethers.TryGetValue(pc, out var playerTether);
            //if (!b.Source.IsDead && b.Target.InstanceID != pcID && (AlwaysDrawOtherBaits || IsClippedBy(pc, ref b)))
            if (!b.Source.IsDead && (AlwaysDrawOtherBaits || IsClippedBy(pc, ref b)) && (!hasPlayer || b.Target.InstanceID != pcID && b.Source != playerTether.Source))
            {
                b.Shape.Draw(Arena, BaitOrigin(ref b), b.Rotation);
            }
        }
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;
        var pcID = pc.InstanceID;
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            var hasPlayer = _playerTethers.TryGetValue(pc, out var playerTether);
            //if (!b.Source.IsDead && (OnlyShowOutlines || !OnlyShowOutlines && b.Target.InstanceID == pcID))
            if (!b.Source.IsDead && (OnlyShowOutlines || !OnlyShowOutlines && (b.Target.InstanceID == pcID || hasPlayer && b.Source == playerTether.Source)))
            {
                b.Shape.Outline(Arena, BaitOrigin(ref b), b.Rotation);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveBaits.Count == 0)
            return;

        var angles = _debuffs.GetAngles(slot);
        var unsafeAngles = _debuffs.GetUnsafeAngles(slot);
        var len = angles.Length;
        if (len == 0)
            return;

        var hasTether = _playerTethers.TryGetValue(actor, out var tethers);
        if (!hasTether)
            return;

        var source = tethers.Source;
        var inter = tethers.Intermediate;

        // if source is same direction as safe side, stand in that path; try standing close to avoid clipping other players
        var sourceSafe = true;
        for (var i = 0; i < len; i++)
        {
            if (source.Rotation.AlmostEqual(unsafeAngles[i], 0.02f))
            {
                sourceSafe = false;
                break;
            }
        }

        if (sourceSafe)
        {
            hints.AddForbiddenZone(new AOEShapeRect(40f, 2f, invertForbiddenZone: true), source.Position, source.Rotation);
            hints.GoalZones.Add(AIHints.GoalSingleTarget(source, 2f));
            return;
        }

        // add AOE from 1st bait as forbidden only if side is unsafe
        //base.AddAIHints(slot, actor, assignment, hints);

        // determine if source is N/S or E/W, need to move perpendicular
        // source sits directly on edge (20f from center), anywhere along that edge
        hints.AddForbiddenZone(new AOEShapeRect(40f, 2f), source.Position, source.Rotation);
        var isNorthSouth = source.Position.Z is -795f or -835f;
        var interAngle = _debuffs.GetUnyieldingAngle(slot, isNorthSouth);
        if (interAngle != null)
        {
            hints.AddForbiddenZone(new AOEShapeRect(40f, 2f), inter.Position, interAngle.Value);
        }
    }
}
