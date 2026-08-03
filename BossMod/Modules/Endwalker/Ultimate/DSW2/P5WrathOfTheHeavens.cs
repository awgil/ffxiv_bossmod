namespace BossMod.Endwalker.Ultimate.DSW2;

sealed class P5WrathOfTheHeavensSkywardLeap(BossModule module) : Components.UniformStackSpread(module, default, 24f, raidwideOnResolve: false)
{
    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (IsSpreadTarget(actor) && SafeSpot() is var safespot && safespot != default)
            movementHints.Add(actor.Position, safespot, Colors.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (IsSpreadTarget(pc) && SafeSpot() is var safespot && safespot != default)
            Arena.ZoneCircleOutline(safespot, 1f, Colors.Safe);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.SkywardLeapP5)
            AddSpread(actor, WorldState.FutureTime(6.4d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SkywardLeapP5)
            Spreads.Clear();
    }

    // note: this assumes LPDU strat
    private WPos SafeSpot()
    {
        var relNorth = Module.Enemies((uint)OID.Vedrfolnir).FirstOrDefault();
        if (relNorth == null)
            return default;
        var dirToNorth = Angle.FromDirection(relNorth.Position - Arena.Center);
        return Arena.Center + 20f * (dirToNorth + 60f.Degrees()).ToDirection();
    }
}

sealed class P5WrathOfTheHeavensSpiralPierce(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(50f, 8f), (uint)TetherID.SpiralPierce, (uint)AID.SpiralPierce)
{
    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (SafeSpot(actor) is var safespot && safespot != default)
            movementHints.Add(actor.Position, safespot, Colors.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (SafeSpot(pc) is var safespot && safespot != default)
            Arena.ZoneCircleOutline(safespot, 1, Colors.Safe);
    }

    private WPos SafeSpot(Actor actor)
    {
        // stay as close to twisting dive as possible, while stretching the tether through the center
        var bait = ActiveBaitsOn(actor).FirstOrDefault();
        if (bait.Source == null)
            return default;
        WDir toMidpoint = default;
        foreach (var b in CurrentBaits)
            toMidpoint += b.Source.Position - Arena.Center;
        var relSouthDir = Angle.FromDirection(-toMidpoint);
        var offset = toMidpoint.OrthoL().Dot(bait.Source.Position - Arena.Center) > 0f ? 20f.Degrees() : -20f.Degrees();
        return Arena.Center + 20f * (relSouthDir + offset).ToDirection();
    }
}

sealed class P5WrathOfTheHeavensChainLightning(BossModule module) : Components.UniformStackSpread(module, default, 5f)
{
    public BitMask Targets;

    public void ShowSpreads(double delay) => AddSpreads(Raid.WithSlot(true, true, true).IncludedInMask(Targets).Actors(), WorldState.FutureTime(delay));

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Spreads.Count == 0 && Targets[slot])
            hints.Add("Prepare for lightning!", false);
        base.AddHints(slot, actor, hints);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => Targets[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    // note: this happens about a second before statuses appear
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ChainLightning:
                foreach (var t in spell.Targets)
                    Targets.Set(Raid.FindSlot(t.ID));
                break;
            case (uint)AID.ChainLightningAOE:
                Targets.Reset();
                Spreads.Clear();
                break;
        }
    }
}

sealed class P5WrathOfTheHeavensTwister(BossModule module) : Components.GenericAOEs(module, default, "GTFO from twister!")
{
    private readonly WPos[] _predicted = GetPositions(module);
    private readonly List<Actor> _voidzones = module.Enemies((uint)OID.VoidzoneTwister);

    private static readonly AOEShapeCircle _shape = new(2f); // TODO: verify radius

    public bool Active => _voidzones.Count > 0;

    private static WPos[] GetPositions(BossModule module)
    {
        var party = module.Raid.WithoutSlot(false, true, true);
        var len = party.Length;
        var pos = new WPos[len];
        for (var i = 0; i < len; ++i)
        {
            pos[i] = party[i].Position;
        }
        return pos;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var lenP = _predicted.Length;
        var countV = _voidzones.Count;
        var aoes = new AOEInstance[lenP + countV];
        for (var i = 0; i < lenP; ++i)
        {
            aoes[i] = new(_shape, _predicted[i]); // TODO: activation
        }
        for (var i = 0; i < countV; ++i)
        {
            aoes[lenP + i] = new(_shape, _voidzones[i].Position);
        }
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.VoidzoneTwister)
        {
            Array.Clear(_predicted);
        }
    }
}

// note: we're not really showing baits here, it's more misleading than helpful...
sealed class P5WrathOfTheHeavensCauterizeBait(BossModule module) : BossComponent(module)
{
    private Actor? _target;
    private readonly DSW2 bossmodule = (DSW2)module;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_target != actor)
            return;
        hints.Add("Prepare for divebomb!", false);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (_target != actor)
            return;
        movementHints.Add(actor.Position, SafeSpot(), Colors.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_target == pc)
            Arena.ZoneCircleOutline(SafeSpot(), 1f, Colors.Safe);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Cauterize)
            _target = actor;
    }

    private WPos SafeSpot()
    {
        if (bossmodule._SerCharibert is not Actor charibert)
        {
            return default;
        }
        return Arena.Center + 20f * (charibert.Position - Arena.Center).Normalized();
    }
}

sealed class P5WrathOfTheHeavensAscalonsMercyRevealed(BossModule module) : Components.BaitAwayEveryone(module, module.Enemies((uint)OID.BossP5).FirstOrDefault(), new AOEShapeCone(50f, 15f.Degrees()), (uint)AID.AscalonsMercyRevealedAOE);

// TODO: detect baiter
sealed class P5WrathOfTheHeavensLiquidHeaven(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.LiquidHeaven, m => m.Enemies((uint)OID.VoidzoneLiquidHeaven).Where(z => z.EventState != 7), 1.1f);

// TODO: detect baiter
sealed class P5WrathOfTheHeavensAltarFlare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AltarFlareAOE, 8f);

sealed class P5WrathOfTheHeavensEmptyDimension(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EmptyDimension, new AOEShapeDonut(6f, 70f))
{
    private WPos _predicted;

    public bool KnowPosition => _predicted != default;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Casters.Count == 0 && KnowPosition)
            Arena.ZoneCircleOutline(_predicted, 6f, Colors.Safe, 2f);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E43 && actor.OID == (uint)OID.SerGrinnaux)
            _predicted = actor.Position;
    }
}
