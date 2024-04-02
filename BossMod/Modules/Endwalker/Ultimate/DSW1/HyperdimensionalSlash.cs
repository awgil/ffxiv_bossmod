namespace BossMod.Endwalker.Ultimate.DSW1;

class HyperdimensionalSlash : BossComponent
{
    public int NumCasts { get; private set; }
    private BitMask _laserTargets;
    private Angle _coneDir;
    private List<(WPos Pos, Actor? Source)> _tears = new();
    private BitMask _riskyTears;

    private static readonly float _linkRadius = 9; // TODO: verify
    private static readonly AOEShapeRect _aoeLaser = new(70, 4);
    private static readonly AOEShapeCone _aoeCone = new(40, 60.Degrees());

    public override void Update(BossModule module)
    {
        _tears.Clear();
        foreach (var tear in module.Enemies(OID.AetherialTear))
            _tears.Add((tear.Position, null));
        foreach (var target in module.Raid.WithSlot(true).IncludedInMask(_laserTargets).Actors())
            _tears.Add((TearPosition(module, target), target));

        _riskyTears.Reset();
        for (int i = 0; i < _tears.Count; ++i)
        {
            for (int j = i + 1; j < _tears.Count; ++j)
            {
                if (_tears[i].Pos.InCircle(_tears[j].Pos, _linkRadius))
                {
                    _riskyTears.Set(i);
                    _riskyTears.Set(j);
                }
            }
        }

        // TODO: proper targeting (seems to be predefined, charibert's target for first?..)
        var coneTarget = module.Raid.WithSlot().ExcludedFromMask(_laserTargets).Actors().Closest(module.Bounds.Center);
        _coneDir = coneTarget != null ? Angle.FromDirection(coneTarget.Position - module.Bounds.Center) : 0.Degrees();
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_laserTargets.None())
            return;

        int tearIndex = _tears.FindIndex(t => t.Source == actor);
        hints.Add(tearIndex >= 0 ? "Next: laser" : "Next: cone", false);
        if (tearIndex >= 0)
        {
            // make sure actor's tear placement is good
            if (_riskyTears[tearIndex])
                hints.Add("Aim away from other tears!");
            if (actor.Position.InCircle(_tears[tearIndex].Pos, _linkRadius))
                hints.Add("Stay closer to center!");
        }

        // make sure actor is not clipped by any lasers
        var otherLasers = _laserTargets;
        otherLasers.Clear(slot);
        if (module.Raid.WithSlot().IncludedInMask(otherLasers).WhereActor(target => _aoeLaser.Check(actor.Position, module.Bounds.Center, Angle.FromDirection(target.Position - module.Bounds.Center))).Any())
            hints.Add("GTFO from laser aoe!");

        // make sure actor is either not hit by cone (if is target of a laser) or is hit by a cone (otherwise)
        bool hitByCone = _aoeCone.Check(actor.Position, module.Bounds.Center, _coneDir);
        if (tearIndex >= 0 && hitByCone)
            hints.Add("GTFO from cone aoe!");
        else if (tearIndex < 0 && !hitByCone)
            hints.Add("Stack with others!");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_laserTargets.None())
            return;

        foreach (var t in module.Raid.WithSlot().IncludedInMask(_laserTargets).Actors())
            _aoeLaser.Draw(arena, module.Bounds.Center, Angle.FromDirection(t.Position - module.Bounds.Center));
        _aoeCone.Draw(arena, module.Bounds.Center, _coneDir);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        for (int i = 0; i < _tears.Count; ++i)
            arena.AddCircle(_tears[i].Pos, _linkRadius, _riskyTears[i] ? ArenaColor.Danger : ArenaColor.Safe);

        if (_laserTargets[pcSlot])
            arena.AddLine(module.Bounds.Center, TearPosition(module, pc), ArenaColor.Danger);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.HyperdimensionalSlashAOERest)
        {
            _laserTargets.Reset();
            ++NumCasts;
        }
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if ((IconID)iconID == IconID.HyperdimensionalSlash)
        {
            _laserTargets.Set(module.Raid.FindSlot(actor.InstanceID));
        }
    }

    private WPos TearPosition(BossModule module, Actor target)
    {
        return module.Bounds.ClampToBounds(module.Bounds.Center + 50 * (target.Position - module.Bounds.Center).Normalized());
    }
}
