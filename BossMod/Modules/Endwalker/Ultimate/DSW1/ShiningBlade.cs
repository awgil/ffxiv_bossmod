namespace BossMod.Endwalker.Ultimate.DSW1;

sealed class ShiningBladeKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.FaithUnmoving, 16f)
{
    private readonly WDir _dirToAdelphel = (module.Enemies((uint)OID.SerAdelphel).FirstOrDefault()?.Position ?? module.Center) - module.Center; // we don't want to be knocked near adelphel
    private readonly List<Actor> _tears = module.Enemies((uint)OID.AetherialTear); // we don't want to be knocked into them

    private const float _tearRadius = 9f; // TODO: verify

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var movements = CalculateMovements(slot, actor);
        var count = movements.Count;
        if (count == 0)
            return;
        for (var i = 0; i < count; ++i)
        {
            var e = movements[i];
            if (!Arena.InBounds(e.to))
                hints.Add("About to be knocked into wall!");
            if (_tears.InRadius(e.to, _tearRadius).Any())
                hints.Add("About to be knocked into tear!");
            if (_dirToAdelphel.Dot(e.to - Arena.Center) > 0)
                hints.Add("Aim away from boss!");
        }
    }

    // TODO: consider moving that to a separate component?
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var count = _tears.Count;
        if (count == 0)
            return;
        for (var i = 0; i < count; ++i)
            Arena.ZoneCircle(_tears[i].Position, _tearRadius, Colors.AOE);
    }
}

sealed class ShiningBladeFlares(BossModule module) : Components.GenericAOEs(module, (uint)AID.BrightFlare, "GTFO from explosion!")
{
    private readonly List<WDir> _flares = []; // [0] = initial boss offset from center, [2] = first charge offset, [5] = second charge offset, [7] = third charge offset, [10] = fourth charge offset == [0]

    private static readonly AOEShapeCircle _shape = new(9);

    public bool Done => NumCasts > 0 && _flares.Count == 0f;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _flares.Count;
        if (count == 0)
            return [];

        var max = count > 7 ? 7 : count;
        var aoes = new AOEInstance[max];

        for (var i = 0; i < max; ++i)
            aoes[i] = new(_shape, (Arena.Center + _flares[i]).Quantized()); // TODO: activation

        return aoes;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID != (uint)OID.SerAdelphel || id != 0x1E43 || _flares.Count > 0)
            return;

        // add first flare at boss position; we can't determine direction yet
        var bossOffset = actor.Position - Arena.Center;
        if (!Utils.AlmostEqual(bossOffset.LengthSq(), Arena.Bounds.Radius * Arena.Bounds.Radius, 1f))
            ReportError("Unexpected boss position");
        _flares.Add(bossOffset);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.ShiningBlade && _flares.Count <= 1)
        {
            var startOffset = caster.Position - Arena.Center;
            var endOffset = spell.TargetXZ - Arena.Center;
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
        _flares.Add((startOffset + endOffset) * 0.5f);
        _flares.Add(endOffset);
    }

    private void AddLongFlares(WDir startOffset, WDir endOffset)
    {
        const float frac = 7.5f / 22f;
        _flares.Add(startOffset * frac);
        _flares.Add(endOffset * frac);
        _flares.Add(endOffset);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _flares.RemoveAt(0);
    }
}

sealed class ShiningBladeExecution(BossModule module) : Components.CastCounter(module, (uint)AID.Execution)
{
    private Actor? _target;

    private const float _executionRadius = 5f;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_target == actor)
        {
            if (Raid.WithoutSlot(false, true, true).InRadiusExcluding(_target, _executionRadius).Any())
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
            Arena.ZoneCircleOutline(_target.Position, _executionRadius);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ShiningBlade:
                _target = WorldState.Actors.Find(Module.Enemies((uint)OID.SerAdelphel).FirstOrDefault()?.TargetID ?? 0);
                break;
            case (uint)AID.Execution:
                _target = null;
                ++NumCasts;
                break;
        }
    }
}
