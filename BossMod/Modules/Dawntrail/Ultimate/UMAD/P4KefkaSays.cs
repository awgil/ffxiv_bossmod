namespace BossMod.Dawntrail.Ultimate.UMAD;

class P4GrandCross(BossModule module) : Components.RaidwideCast(module, AID.GrandCross);
class P4Inferno(BossModule module) : Components.RaidwideCast(module, AID.InfernoHitP4);
class P4Tsunami(BossModule module) : Components.RaidwideCast(module, AID.TsunamiCastP4);

class P4StrayDonut(BossModule module) : Components.GroupedAOEs(module, [AID.StrayFlamesInvertedP4, AID.StraySprayNormalP4], new AOEShapeDonut(6, 40));
class P4StrayCircle(BossModule module) : Components.GroupedAOEs(module, [AID.StraySprayInvertedP4, AID.StrayFlamesNormalP4], new AOEShapeCircle(6));
class P4EdgeOfDeath(BossModule module) : Components.StandardAOEs(module, AID.EdgeOfDeath, new AOEShapeRect(48, 1));

class P4Antilight(BossModule module) : Components.GroupedAOEs(module, [AID.WhiteAntilight, AID.BlackAntilight], new AOEShapeRect(47, 10.5f))
{
    enum Wound
    {
        None,
        White,
        Black
    }
    enum Requirement
    {
        None,
        Live,
        Die
    }

    record struct Debuff(Requirement Req, Wound Color);

    readonly Debuff[] _debuffs = new Debuff[8];

    bool _castWasFake;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.P4TruthLie)
        {
            if (status.Extra == 0x461)
                _castWasFake = true;
            if (status.Extra == 0x462)
                _castWasFake = false;
        }

        if (!Raid.TryFindSlot(actor, out var slot))
            return;

        switch ((SID)status.ID)
        {
            case SID.BeyondDeath1:
                _debuffs[slot] = _debuffs[slot] with { Req = Requirement.Live };
                break;
            case SID.BeyondDeath2:
                _debuffs[slot] = _debuffs[slot] with { Req = Requirement.Die };
                break;
            case SID.AllaganField:
                _debuffs[slot] = _debuffs[slot] with { Req = _castWasFake ? Requirement.Die : Requirement.Live };
                break;

            case SID.WhiteWound1:
            case SID.BlackWound2:
                _debuffs[slot] = _debuffs[slot] with { Color = Wound.Black };
                break;
            case SID.WhiteWound2:
            case SID.BlackWound1:
                _debuffs[slot] = _debuffs[slot] with { Color = Wound.White };
                break;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var req = _debuffs[slot];

        foreach (var c in Casters)
        {
            var cast = c.CastInfo!;
            var casterColor = cast.IsSpell(AID.WhiteAntilight) ? Wound.White : Wound.Black;
            var isFatal = casterColor == req.Color;
            var mustDie = req.Req == Requirement.Die;
            if (isFatal != mustDie)
                yield return new(Shape, cast.LocXZ, cast.Rotation, Module.CastFinishAt(cast));
        }
    }
}

class P4Debuffs(BossModule module) : BossComponent(module)
{
    bool _exdeathTrue;
    bool _chaosTrue;

    public enum Debuff
    {
        None,
        Shriek, // gaze
        Lightning, // spread
        Water, // stack
        Bomb, // pyretic
        Entropy, // baited circle AOE 
        Fluid, // baited donut AOE
    }

    public readonly List<(Debuff D, bool Real, DateTime Expiration)>[] Debuffs = Utils.GenArray<List<(Debuff D, bool Real, DateTime Expiration)>>(8, () => []);

    public IEnumerable<(int Slot, Actor Actor, Debuff Debuff, bool Real, DateTime Expiration)> EnumerateDebuffs(DateTime deadline)
    {
        foreach (var (i, player) in Raid.WithSlot())
            foreach (var (d, r, e) in Debuffs[i])
                if (deadline > e)
                    yield return (i, player, d, r, e);
    }

    static string DebuffReadable(Debuff d, bool real) => d switch
    {
        Debuff.Shriek => real ? "Gaze" : "Inverted gaze",
        Debuff.Lightning => real ? "Spread (lightning)" : "Stack (fake lightning)",
        Debuff.Water => real ? "Stack (water)" : "Spread (fake water)",
        Debuff.Bomb => real ? "Accel bomb" : "Deep freeze (fake bomb)",
        Debuff.Entropy => real ? "Delayed puddle" : "Delayed donut",
        Debuff.Fluid => real ? "Delayed donut" : "Delayed puddle",
        _ => ""
    };

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var (db, t, e) in Debuffs[slot])
            hints.Add($"{DebuffReadable(db, t)} in {Math.Max(0, (e - WorldState.CurrentTime).TotalSeconds):f1}s", false);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.P4TruthLie)
        {
            switch (status.Extra)
            {
                case 0x45F:
                    _chaosTrue = false;
                    break;
                case 0x460F:
                    _chaosTrue = true;
                    break;
                case 0x461:
                    _exdeathTrue = false;
                    break;
                case 0x462:
                    _exdeathTrue = true;
                    break;
            }
        }

        var (dbf, real) = (SID)status.ID switch
        {
            SID.CursedShriek => (Debuff.Shriek, _exdeathTrue),
            SID.ForkedLightning => (Debuff.Lightning, _exdeathTrue),
            SID.CompressedWater => (Debuff.Water, _exdeathTrue),
            SID.AccelerationBomb => (Debuff.Bomb, _exdeathTrue),
            SID.EntropyP4 => (Debuff.Entropy, _chaosTrue),
            SID.DynamicFluidP4 => (Debuff.Fluid, _chaosTrue),
            _ => (Debuff.None, false)
        };

        if (dbf != default && Raid.TryFindSlot(actor, out var slot))
        {
            Debuffs[slot].Add((dbf, real, status.ExpireAt));
            Debuffs[slot].SortBy(s => s.Expiration);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        var dbf = (SID)status.ID switch
        {
            SID.CursedShriek => Debuff.Shriek,
            SID.ForkedLightning => Debuff.Lightning,
            SID.CompressedWater => Debuff.Water,
            SID.AccelerationBomb => Debuff.Bomb,
            SID.EntropyP4 => Debuff.Entropy,
            SID.DynamicFluidP4 => Debuff.Fluid,
            _ => Debuff.None
        };

        if (dbf != default && Raid.TryFindSlot(actor, out var slot))
        {
            var ix = Debuffs[slot].FindIndex(d => d.D == dbf);
            if (ix >= 0)
                Debuffs[slot].RemoveAt(ix);
        }
    }
}

class P4DeathWaveBolt : Components.UniformStackSpread
{
    public int NumCasts { get; private set; }

    public P4DeathWaveBolt(BossModule module) : base(module, 8, 8, 3, 3, true)
    {
        foreach (var (_, player, debuff, real, exp) in module.FindComponent<P4Debuffs>()!.EnumerateDebuffs(WorldState.FutureTime(10)))
        {
            switch (debuff)
            {
                case P4Debuffs.Debuff.Water:
                    if (real)
                        AddStack(player, exp);
                    else
                        AddSpread(player, exp);
                    break;
                case P4Debuffs.Debuff.Lightning:
                    if (real)
                        AddSpread(player, exp);
                    else
                        AddStack(player, exp);
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DeathWaveNormal:
            case AID.DeathBoltInverted:
                NumCasts++;
                Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                break;

            case AID.DeathWaveInverted:
            case AID.DeathBoltNormal:
                NumCasts++;
                Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                break;
        }
    }
}

class P4DeathBomb : Components.StayMove
{
    public P4DeathBomb(BossModule module) : base(module)
    {
        foreach (var (i, _, debuff, real, exp) in Module.FindComponent<P4Debuffs>()!.EnumerateDebuffs(WorldState.FutureTime(10)))
        {
            if (debuff == P4Debuffs.Debuff.Bomb)
                SetState(i, new(real ? Requirement.Stay : Requirement.Move, exp));
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.AccelerationBomb && Raid.TryFindSlot(actor, out var slot))
            ClearState(slot);
    }
}
