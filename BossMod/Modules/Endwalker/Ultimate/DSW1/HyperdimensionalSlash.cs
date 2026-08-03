namespace BossMod.Endwalker.Ultimate.DSW1;

sealed class HyperdimensionalSlash(BossModule module) : BossComponent(module)
{
    public int NumCasts;
    private BitMask _laserTargets;
    private Angle _coneDir;
    private readonly List<(WPos Pos, Actor? Source)> _tears = [];
    private BitMask _riskyTears;

    private const float _linkRadius = 9f; // TODO: verify
    private static readonly AOEShapeRect _aoeLaser = new(70f, 4f);
    private static readonly AOEShapeCone _aoeCone = new(40f, 60f.Degrees());

    public override void Update()
    {
        _tears.Clear();
        foreach (var tear in Module.Enemies((uint)OID.AetherialTear))
            _tears.Add((tear.Position, null));
        foreach (var target in Raid.WithSlot(true, true, true).IncludedInMask(_laserTargets).Actors())
            _tears.Add((TearPosition(target), target));

        _riskyTears.Reset();
        for (var i = 0; i < _tears.Count; ++i)
        {
            for (var j = i + 1; j < _tears.Count; ++j)
            {
                if (_tears[i].Pos.InCircle(_tears[j].Pos, _linkRadius))
                {
                    _riskyTears.Set(i);
                    _riskyTears.Set(j);
                }
            }
        }

        // TODO: proper targeting (seems to be predefined, charibert's target for first?..)
        var coneTarget = Raid.WithSlot(false, true, true).ExcludedFromMask(_laserTargets).Actors().Closest(Arena.Center);
        _coneDir = coneTarget != null ? Angle.FromDirection(coneTarget.Position - Arena.Center) : default;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_laserTargets.None())
            return;

        var tearIndex = _tears.FindIndex(t => t.Source == actor);
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
        if (Raid.WithSlot(false, true, true).IncludedInMask(otherLasers).WhereActor(target => _aoeLaser.Check(actor.Position, Arena.Center, Angle.FromDirection(target.Position - Arena.Center))).Any())
            hints.Add("GTFO from laser aoe!");

        // make sure actor is either not hit by cone (if is target of a laser) or is hit by a cone (otherwise)
        var hitByCone = _aoeCone.Check(actor.Position, Arena.Center, _coneDir);
        if (tearIndex >= 0 && hitByCone)
            hints.Add("GTFO from cone aoe!");
        else if (tearIndex < 0 && !hitByCone)
            hints.Add("Stack with others!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_laserTargets.None())
            return;

        foreach (var t in Raid.WithSlot(false, true, true).IncludedInMask(_laserTargets).Actors())
            _aoeLaser.Draw(Arena, Arena.Center, Angle.FromDirection(t.Position - Arena.Center));
        _aoeCone.Draw(Arena, Arena.Center, _coneDir);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        for (var i = 0; i < _tears.Count; ++i)
            Arena.ZoneCircleOutline(_tears[i].Pos, _linkRadius, _riskyTears[i] ? default : Colors.Safe);

        if (_laserTargets[pcSlot])
            Arena.AddLine(Arena.Center, TearPosition(pc));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.HyperdimensionalSlashAOERest)
        {
            _laserTargets.Reset();
            ++NumCasts;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.HyperdimensionalSlash)
        {
            _laserTargets.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    private WPos TearPosition(Actor target) => Arena.Center + Arena.Bounds.ClampToBounds(50f * (target.Position - Arena.Center).Normalized());
}
