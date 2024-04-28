namespace BossMod.Endwalker.Ultimate.DSW1;

class ShiningBladeKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.FaithUnmoving), 16)
{
    private WDir _dirToAdelphel = (module.Enemies(OID.SerAdelphel).FirstOrDefault()?.Position ?? module.Center) - module.Center; // we don't want to be knocked near adelphel
    private readonly IReadOnlyList<Actor> _tears = module.Enemies(OID.AetherialTear); // we don't want to be knocked into them

    private const float _tearRadius = 9; // TODO: verify

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var e in CalculateMovements(slot, actor))
        {
            if (!Module.InBounds(e.to))
                hints.Add("About to be knocked into wall!");
            if (_tears.InRadius(e.to, _tearRadius).Any())
                hints.Add("About to be knocked into tear!");
            if (_dirToAdelphel.Dot(e.to - Module.Center) > 0)
                hints.Add("Aim away from boss!");
        }
    }

    // TODO: consider moving that to a separate component?
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var p in _tears)
            Arena.ZoneCircle(p.Position, _tearRadius, ArenaColor.AOE);
    }
}

class ShiningBladeFlares(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.BrightFlare), "GTFO from explosion!")
{
    private readonly List<WDir> _flares = []; // [0] = initial boss offset from center, [2] = first charge offset, [5] = second charge offset, [7] = third charge offset, [10] = fourth charge offset == [0]

    private static readonly AOEShapeCircle _shape = new(9);

    public bool Done => NumCasts >= _flares.Count;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _flares.Skip(NumCasts).Take(7).Select(f => new AOEInstance(_shape, Module.Center + f)); // TODO: activation
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID != OID.SerAdelphel || id != 0x1E43 || _flares.Count > 0)
            return;

        // add first flare at boss position; we can't determine direction yet
        var bossOffset = actor.Position - Module.Center;
        if (!Utils.AlmostEqual(bossOffset.LengthSq(), Module.Bounds.Radius * Module.Bounds.Radius, 1))
            ReportError("Unexpected boss position");
        _flares.Add(bossOffset);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.ShiningBlade && _flares.Count <= 1)
        {
            var startOffset = caster.Position - Module.Center;
            var endOffset = spell.TargetXZ - Module.Center;
            _flares.Clear();
            _flares.Add(startOffset);
            AddShortFlares(startOffset, endOffset);
            AddLongFlares(endOffset, -endOffset);
            AddShortFlares(-endOffset, -startOffset);
            AddLongFlares(-startOffset, startOffset);
        }
    }

    private void AddShortFlares(WDir startOffset, WDir endOffset)
    {
        _flares.Add((startOffset + endOffset) / 2);
        _flares.Add(endOffset);
    }

    private void AddLongFlares(WDir startOffset, WDir endOffset)
    {
        var frac = 7.5f / 22;
        _flares.Add(startOffset * frac);
        _flares.Add(endOffset * frac);
        _flares.Add(endOffset);
    }
}

class ShiningBladeExecution(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.Execution))
{
    private Actor? _target;

    private const float _executionRadius = 5;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_target == actor)
        {
            if (Raid.WithoutSlot().InRadiusExcluding(_target, _executionRadius).Any())
                hints.Add("GTFO from raid!");
        }
        else if (_target != null)
        {
            if (actor.Position.InCircle(_target.Position, _executionRadius))
                hints.Add("GTFO from tank!");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return player == _target ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_target != null)
            Arena.AddCircle(_target.Position, _executionRadius, ArenaColor.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ShiningBlade:
                _target = WorldState.Actors.Find(Module.Enemies(OID.SerAdelphel).FirstOrDefault()?.TargetID ?? 0);
                break;
            case AID.Execution:
                _target = null;
                ++NumCasts;
                break;
        }
    }
}
