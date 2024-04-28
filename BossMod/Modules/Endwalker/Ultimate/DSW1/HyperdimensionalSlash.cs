namespace BossMod.Endwalker.Ultimate.DSW1;

class HyperdimensionalSlash(BossModule module) : BossComponent(module)
{
    public int NumCasts { get; private set; }
    private BitMask _laserTargets;
    private Angle _coneDir;
    private readonly List<(WPos Pos, Actor? Source)> _tears = [];
    private BitMask _riskyTears;

    private const float _linkRadius = 9; // TODO: verify
    private static readonly AOEShapeRect _aoeLaser = new(70, 4);
    private static readonly AOEShapeCone _aoeCone = new(40, 60.Degrees());

    public override void Update()
    {
        _tears.Clear();
        foreach (var tear in Module.Enemies(OID.AetherialTear))
            _tears.Add((tear.Position, null));
        foreach (var target in Raid.WithSlot(true).IncludedInMask(_laserTargets).Actors())
            _tears.Add((TearPosition(target), target));

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
        var coneTarget = Raid.WithSlot().ExcludedFromMask(_laserTargets).Actors().Closest(Module.Center);
        _coneDir = coneTarget != null ? Angle.FromDirection(coneTarget.Position - Module.Center) : 0.Degrees();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
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
        if (Raid.WithSlot().IncludedInMask(otherLasers).WhereActor(target => _aoeLaser.Check(actor.Position, Module.Center, Angle.FromDirection(target.Position - Module.Center))).Any())
            hints.Add("GTFO from laser aoe!");

        // make sure actor is either not hit by cone (if is target of a laser) or is hit by a cone (otherwise)
        bool hitByCone = _aoeCone.Check(actor.Position, Module.Center, _coneDir);
        if (tearIndex >= 0 && hitByCone)
            hints.Add("GTFO from cone aoe!");
        else if (tearIndex < 0 && !hitByCone)
            hints.Add("Stack with others!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_laserTargets.None())
            return;

        foreach (var t in Raid.WithSlot().IncludedInMask(_laserTargets).Actors())
            _aoeLaser.Draw(Arena, Module.Center, Angle.FromDirection(t.Position - Module.Center));
        _aoeCone.Draw(Arena, Module.Center, _coneDir);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        for (int i = 0; i < _tears.Count; ++i)
            Arena.AddCircle(_tears[i].Pos, _linkRadius, _riskyTears[i] ? ArenaColor.Danger : ArenaColor.Safe);

        if (_laserTargets[pcSlot])
            Arena.AddLine(Module.Center, TearPosition(pc), ArenaColor.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.HyperdimensionalSlashAOERest)
        {
            _laserTargets.Reset();
            ++NumCasts;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if ((IconID)iconID == IconID.HyperdimensionalSlash)
        {
            _laserTargets.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    private WPos TearPosition(Actor target) => Module.Center + Module.Bounds.ClampToBounds(50 * (target.Position - Module.Center).Normalized());
}
