namespace BossMod.Dawntrail.Ultimate.FRU;

class P2MirrorMirrorReflectedScytheKickBlue(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.ReflectedScytheKickBlue))
{
    private WDir _blueMirror;
    private AOEInstance? _aoe;

    private static readonly AOEShapeDonut _shape = new(4, 20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoe == null && Module.Enemies(OID.BossP2).FirstOrDefault() is var boss && boss != null && boss.TargetID == actor.InstanceID)
        {
            // main tank should drag the boss away
            // note: before mirror appears, we want to stay near center (to minimize movement no matter where mirror appears), so this works fine if blue mirror is zero
            // TODO: verify distance calculation - we want boss to be at least 4m away from center
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.Center - 16 * _blueMirror, 1), DateTime.MaxValue);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_blueMirror != default)
        {
            Arena.Actor(Module.Center + 20 * _blueMirror, Angle.FromDirection(-_blueMirror), ArenaColor.Object);
            if (_aoe == null)
            {
                // draw hint for melees
                Arena.AddCircle(Module.Center - 11 * _blueMirror, 1, ArenaColor.Safe);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ScytheKick && _blueMirror != default)
            _aoe = new(_shape, Module.Center + 20 * _blueMirror, default, Module.CastFinishAt(spell));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is >= 1 and <= 8 && state == 0x00020001)
            _blueMirror = (225 - index * 45).Degrees().ToDirection();
    }
}

class P2MirrorMirrorReflectedScytheKickRed(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ReflectedScytheKickRed), new AOEShapeDonut(4, 20))
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Casters, ArenaColor.Object, true);
    }
}

class P2MirrorMirrorHouseOfLight(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.HouseOfLight))
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private Angle? _blueMirror;
    private int _numRedMirrors;
    private readonly List<(Actor source, DateTime activation)> _sources = [];

    private static readonly AOEShapeCone _shape = new(60, 15.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        foreach (var s in _sources.Take(2))
            foreach (var p in Raid.WithoutSlot().SortedByRange(s.source.Position).Take(4))
                CurrentBaits.Add(new(s.source, p, _shape, s.activation));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var group = (NumCasts == 0 ? _config.P2MirrorMirror1SpreadSpots : _config.P2MirrorMirror2SpreadSpots)[assignment];
        if (_sources.Count < 2 || _blueMirror == null || group < 0)
            return; // inactive or no valid assignments

        var origin = _sources[group < 4 ? 0 : 1];
        Angle dir;
        if (NumCasts == 0)
        {
            dir = _blueMirror.Value + (group & 3) switch
            {
                0 => -135.Degrees(),
                1 => 135.Degrees(),
                2 => -90.Degrees(),
                3 => 90.Degrees(),
                _ => default
            };
        }
        else
        {
            var offset = origin.source.Position - Module.Center;
            var altSourceToTheRight = offset.OrthoL().Dot(_sources[group < 4 ? 1 : 0].source.Position - Module.Center) < 0;
            dir = Angle.FromDirection(offset) + group switch
            {
                0 => (altSourceToTheRight ? 90 : -90).Degrees(),
                1 => (altSourceToTheRight ? -90 : 90).Degrees(),
                2 => 180.Degrees(),
                3 => (altSourceToTheRight ? 135 : -135).Degrees(),
                4 => -90.Degrees(),
                5 => 90.Degrees(),
                6 => (altSourceToTheRight ? 180 : -135).Degrees(),
                7 => (altSourceToTheRight ? 135 : 180).Degrees(),
                _ => default
            };

            // special logic for current tank: if mechanic will take a while to resolve, and boss is far enough away from the destination, and normal destination is on the same side as the boss, drag towards other side first
            // this guarantees uptime for OT
            //if (origin.activation > WorldState.FutureTime(3) && Module.Enemies(OID.BossP2).FirstOrDefault() is var boss && boss != null && boss.TargetID == actor.InstanceID)
            //{
            //    var dirVec = dir.ToDirection();
            //    if (dirVec.Dot(boss.Position - origin.source.Position) > 2.5f && (origin.source.Position - 3 * dirVec - boss.Position).Length() > boss.HitboxRadius + 3.5f)
            //        dir += 180.Degrees();
            //}
        }
        hints.AddForbiddenZone(ShapeDistance.InvertedCone(origin.source.Position, 4, dir, 15.Degrees()), origin.activation);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is >= 1 and <= 8 && state == 0x00020001)
        {
            _blueMirror = (225 - index * 45).Degrees();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ScytheKick:
                var activation = Module.CastFinishAt(spell, 0.7f);
                _sources.Add((caster, activation));
                var mirror = _blueMirror != null ? Module.Enemies(OID.FrozenMirror).Closest(Module.Center + 20 * _blueMirror.Value.ToDirection()) : null;
                if (mirror != null)
                    _sources.Add((mirror, activation));
                break;
            case AID.ReflectedScytheKickRed:
                _sources.Add((caster, Module.CastFinishAt(spell, 0.6f)));
                if (++_numRedMirrors == 2 && _blueMirror != null && _sources.Count >= 2)
                {
                    // order two red mirrors so that first one is closer to boss and second one closer to blue mirror; if both are same distance, select CW ones (arbitrary)
                    var d1 = (Angle.FromDirection(_sources[^2].source.Position - Module.Center) - _blueMirror.Value).Normalized();
                    var d2 = (Angle.FromDirection(_sources[^1].source.Position - Module.Center) - _blueMirror.Value).Normalized();
                    var d1abs = d1.Abs();
                    var d2abs = d2.Abs();
                    var swap = d2abs.AlmostEqual(d1abs, 0.1f)
                        ? d2.Rad > 0 // swap if currently second one is CCW from blue mirror
                        : d2abs.Rad > d1abs.Rad; // swap if currently second one is further from the blue mirror
                    if (swap)
                        (_sources[^1], _sources[^2]) = (_sources[^2], _sources[^1]);
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            var deadline = WorldState.FutureTime(2);
            var firstInBunch = _sources.RemoveAll(s => s.activation < deadline) > 0;
            if (firstInBunch)
                ++NumCasts;
        }
    }
}

class P2MirrorMirrorBanish(BossModule module) : P2Banish(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var prepos = PrepositionLocation(assignment);
        if (prepos != null)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(prepos.Value, 1), DateTime.MaxValue);
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }

    private WPos? PrepositionLocation(PartyRolesConfig.Assignment assignment)
    {
        // TODO: consider a different strategy for melee (left if more left)
        if (Stacks.Count > 0 && Stacks[0].Activation > WorldState.FutureTime(2.5f))
        {
            // preposition for stacks
            var boss = Module.Enemies(OID.BossP2).FirstOrDefault();
            return assignment switch
            {
                PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.M1 => boss != null ? boss.Position + 6 * boss.Rotation.ToDirection().OrthoL() : null,
                PartyRolesConfig.Assignment.OT or PartyRolesConfig.Assignment.M2 => boss != null ? boss.Position + 6 * boss.Rotation.ToDirection().OrthoR() : null,
                _ => null // TODO: implement positioning for ranged
            };
        }
        else if (Spreads.Count > 0 && Spreads[0].Activation > WorldState.FutureTime(2.5f))
        {
            // preposition for spreads
            var boss = Module.Enemies(OID.BossP2).FirstOrDefault();
            return assignment switch
            {
                PartyRolesConfig.Assignment.MT => boss != null ? boss.Position + 6 * (boss.Rotation + 45.Degrees()).ToDirection() : null,
                PartyRolesConfig.Assignment.OT => boss != null ? boss.Position + 6 * (boss.Rotation - 45.Degrees()).ToDirection() : null,
                PartyRolesConfig.Assignment.M1 => boss != null ? boss.Position + 6 * (boss.Rotation + 135.Degrees()).ToDirection() : null,
                PartyRolesConfig.Assignment.M2 => boss != null ? boss.Position + 6 * (boss.Rotation - 135.Degrees()).ToDirection() : null,
                _ => null // TODO: implement positioning for ranged
            };
        }
        return null;
    }
}
