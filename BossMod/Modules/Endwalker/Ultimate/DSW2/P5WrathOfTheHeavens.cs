namespace BossMod.Endwalker.Ultimate.DSW2;

class P5WrathOfTheHeavensSkywardLeap(BossModule module) : Components.UniformStackSpread(module, 0, 24, alwaysShowSpreads: true, raidwideOnResolve: false)
{
    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (IsSpreadTarget(actor) && SafeSpot() is var safespot && safespot != default)
            movementHints.Add(actor.Position, safespot, ArenaColor.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (IsSpreadTarget(pc) && SafeSpot() is var safespot && safespot != default)
            Arena.AddCircle(safespot, 1, ArenaColor.Safe);
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.SkywardLeapP5)
            AddSpread(actor, WorldState.FutureTime(6.4f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SkywardLeapP5)
            Spreads.Clear();
    }

    // note: this assumes LPDU strat
    private WPos SafeSpot()
    {
        var relNorth = Module.Enemies(OID.Vedrfolnir).FirstOrDefault();
        if (relNorth == null)
            return default;
        var dirToNorth = Angle.FromDirection(relNorth.Position - Module.Center);
        return Module.Center + 20 * (dirToNorth + 60.Degrees()).ToDirection();
    }
}

class P5WrathOfTheHeavensSpiralPierce(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(50, 8), (uint)TetherID.SpiralPierce, ActionID.MakeSpell(AID.SpiralPierce))
{
    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (SafeSpot(actor) is var safespot && safespot != default)
            movementHints.Add(actor.Position, safespot, ArenaColor.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (SafeSpot(pc) is var safespot && safespot != default)
            Arena.AddCircle(safespot, 1, ArenaColor.Safe);
    }

    private WPos SafeSpot(Actor actor)
    {
        // stay as close to twisting dive as possible, while stretching the tether through the center
        var bait = ActiveBaitsOn(actor).FirstOrDefault();
        if (bait.Source == null)
            return default;
        WDir toMidpoint = default;
        foreach (var b in CurrentBaits)
            toMidpoint += b.Source.Position - Module.Center;
        var relSouthDir = Angle.FromDirection(-toMidpoint);
        var offset = toMidpoint.OrthoL().Dot(bait.Source.Position - Module.Center) > 0 ? 20.Degrees() : -20.Degrees();
        return Module.Center + 20 * (relSouthDir + offset).ToDirection();
    }
}

class P5WrathOfTheHeavensChainLightning(BossModule module) : Components.UniformStackSpread(module, 0, 5, alwaysShowSpreads: true)
{
    public BitMask Targets;

    public void ShowSpreads(float delay) => AddSpreads(Raid.WithSlot(true).IncludedInMask(Targets).Actors(), WorldState.FutureTime(delay));

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
        switch ((AID)spell.Action.ID)
        {
            case AID.ChainLightning:
                foreach (var t in spell.Targets)
                    Targets.Set(Raid.FindSlot(t.ID));
                break;
            case AID.ChainLightningAOE:
                Targets.Reset();
                Spreads.Clear();
                break;
        }
    }
}

class P5WrathOfTheHeavensTwister(BossModule module) : Components.GenericAOEs(module, default, "GTFO from twister!")
{
    private readonly List<WPos> _predicted = [.. module.Raid.WithoutSlot().Select(a => a.Position)];
    private readonly IReadOnlyList<Actor> _voidzones = module.Enemies(OID.VoidzoneTwister);

    private static readonly AOEShapeCircle _shape = new(2); // TODO: verify radius

    public bool Active => _voidzones.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var p in _predicted)
            yield return new(_shape, p); // TODO: activation
        foreach (var p in _voidzones)
            yield return new(_shape, p.Position);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.VoidzoneTwister)
            _predicted.Clear();
    }
}

// note: we're not really showing baits here, it's more misleading than helpful...
class P5WrathOfTheHeavensCauterizeBait(BossModule module) : BossComponent(module)
{
    private Actor? _target;

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
        movementHints.Add(actor.Position, SafeSpot(), ArenaColor.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_target == pc)
            Arena.AddCircle(SafeSpot(), 1, ArenaColor.Safe);
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Cauterize)
            _target = actor;
    }

    private WPos SafeSpot()
    {
        var charibert = Module.Enemies(OID.SerCharibert).FirstOrDefault();
        if (charibert == null)
            return default;
        return Module.Center + 20 * (charibert.Position - Module.Center).Normalized();
    }
}

class P5WrathOfTheHeavensAscalonsMercyRevealed(BossModule module) : Components.BaitAwayEveryone(module, module.Enemies(OID.BossP5).FirstOrDefault(), new AOEShapeCone(50, 15.Degrees()), ActionID.MakeSpell(AID.AscalonsMercyRevealedAOE));

// TODO: detect baiter
class P5WrathOfTheHeavensLiquidHeaven(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, ActionID.MakeSpell(AID.LiquidHeaven), m => m.Enemies(OID.VoidzoneLiquidHeaven).Where(z => z.EventState != 7), 1.1f);

// TODO: detect baiter
class P5WrathOfTheHeavensAltarFlare(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.AltarFlareAOE), 8);

class P5WrathOfTheHeavensEmptyDimension(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EmptyDimension), new AOEShapeDonut(6, 70))
{
    private WPos _predicted;

    public bool KnowPosition => _predicted != default;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Casters.Count == 0 && KnowPosition)
            Arena.AddCircle(_predicted, 6, ArenaColor.Safe, 2);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.SerGrinnaux && id == 0x1E43)
            _predicted = actor.Position;
    }
}
