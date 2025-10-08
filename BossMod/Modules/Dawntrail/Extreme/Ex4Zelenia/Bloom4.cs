namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class Bloom4Emblazon(BossModule module) : Emblazon(module)
{
    public bool RoseSouth => _tiles[10] && _tiles[13];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (Baiters[slot] && !actor.Position.InCone(Arena.Center, RoseSouth ? default : 180.Degrees(), 45.Degrees()))
            hints.Add("Go to safe quadrant!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Baiters[slot])
            hints.AddForbiddenZone(ShapeContains.InvertedCone(Arena.Center, 16, RoseSouth ? default : 180.Degrees(), 45.Degrees()), Activation);
    }
}

class Bloom4AlexandrianThunderIIIAOE(BossModule module) : Components.StandardAOEs(module, AID.AlexandrianThunderIIIGroundTarget, new AOEShapeCircle(4));

class Bloom4AlexandrianThunderIII(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.AlexandrianThunderIII, AID.AlexandrianThunderIII, 4, 5)
{
    private readonly Tiles _tiles = module.FindComponent<Tiles>()!;

    private bool SpreadSouth => _tiles[9] && _tiles[14];

    [Flags]
    enum Clip
    {
        None,
        Tile = 1,
        Player = 2
    }

    private static Func<WPos, Clip> ClipBaitersShape(bool spreadSouth, WPos center) => p =>
    {
        var signright = spreadSouth ? 1 : -1;
        var c = Clip.None;

        if (Intersect.CircleDonutSector(p, 4, center, 8, 16, new WDir(1, 0).Rotate(22.5f.Degrees() * signright), 22.5f.Degrees()))
            c |= Clip.Tile;
        else if (Intersect.CircleDonutSector(p, 4, center, 8, 16, new WDir(-1, 0).Rotate(-22.5f.Degrees() * signright), 22.5f.Degrees()))
            c |= Clip.Tile;

        if (Intersect.CircleCone(p, 4, new WPos(100, 100), 16, new WDir(0, spreadSouth ? -1 : 1), 45.Degrees()))
            c |= Clip.Player;
        return c;
    };

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (IsSpreadTarget(actor))
        {
            hints.AddForbiddenZone(_tiles.TileShape(), Spreads[0].Activation);
            var sh = ClipBaitersShape(SpreadSouth, Arena.Center);
            // hints.AddForbiddenZone(p => ClipBaitersShape(p) > 0, Spreads[0].Activation);
            hints.AddForbiddenZone(p => sh(p).HasFlag(Clip.Tile), Spreads[0].Activation);
            hints.AddForbiddenZone(p => sh(p).HasFlag(Clip.Player), Spreads[0].Activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (IsSpreadTarget(actor))
        {
            if (_tiles.InActiveTile(actor))
                hints.Add("GTFO from tile!");

            var clip = ClipBaitersShape(SpreadSouth, Arena.Center)(actor.Position);
            if (clip.HasFlag(Clip.Tile))
                hints.Add("Stay away from connected tiles!");

            if (clip.HasFlag(Clip.Player))
                hints.Add("Stay away from baiters!");
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        if (IsSpreadTarget(pc))
        {
            var clip = ClipBaitersShape(SpreadSouth, Arena.Center)(pc.Position);

            if (clip.HasFlag(Clip.Player))
                Arena.ZoneCone(Arena.Center, 0, 16, SpreadSouth ? 180.Degrees() : default, 45.Degrees(), ArenaColor.Danger);

            if (clip.HasFlag(Clip.Tile))
            {
                var rightSide = pc.Position.X > 100;
                var badTile = (SpreadSouth, rightSide) switch
                {
                    (true, true) => 9,
                    (false, true) => 10,
                    (true, false) => 14,
                    (false, false) => 13
                };
                Tiles.ZoneTile(Arena, badTile, ArenaColor.Danger);
            }
        }
    }
}

class EncirclingThorns(BossModule module) : Components.Chains(module, (uint)TetherID.Thorns, default, 20, activationDelay: 5.1f);

class BanishIII(BossModule module) : Components.CastCounter(module, AID.AlexandrianBanishIIIStack)
{
    private readonly Tiles _tiles = module.FindComponent<Tiles>()!;
    public Actor? Target { get; private set; }
    public DateTime Activation { get; private set; }

    private BitMask _stackTiles;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.AlexandrianBanishIII)
        {
            Target = actor;
            Activation = WorldState.FutureTime(4.8f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Target = null;
        }
    }

    public override void Update()
    {
        _stackTiles.Reset();
        var activeTiles = _tiles.Mask;
        if (Target != null)
        {
            foreach (var t in activeTiles.SetBits())
            {
                if (_stackTiles[t])
                    continue;

                var tangle = Tiles.GetTileOrientation(t);
                if (Intersect.CircleDonutSector(Target.Position, 4, Arena.Center, t < 8 ? 0 : 8, t < 8 ? 8 : 16, tangle.ToDirection(), 22.5f.Degrees()))
                    _stackTiles |= _tiles.GetConnectedTiles(t);
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        Tiles.ZoneTiles(Arena, _stackTiles, ArenaColor.SafeFromAOE);
        Tiles.ZoneTiles(Arena, _tiles.Mask & ~_stackTiles, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Target is { } t)
            Arena.AddCircle(t.Position, 4, ArenaColor.Safe);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Target == actor)
            hints.Add("Stack!", false);
        else if (Target != null)
        {
            var realStack = actor.Position.InCircle(Target.Position, 4);
            var tileStack = _stackTiles[Tiles.GetTile(actor)];
            hints.Add("Stack!", !realStack && !tileStack);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Target != null && Target != actor)
        {
            var tpos = Target.Position;
            var stackTiles = _stackTiles;
            hints.AddForbiddenZone(p => !p.InCircle(tpos, 4) && !stackTiles[Tiles.GetTile(p)], Activation);
        }
    }
}
