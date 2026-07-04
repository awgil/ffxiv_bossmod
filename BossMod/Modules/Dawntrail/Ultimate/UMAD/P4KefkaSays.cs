namespace BossMod.Dawntrail.Ultimate.UMAD;

class P4GrandCross(BossModule module) : Components.RaidwideCast(module, AID._Ability_GrandCross);
class P4Inferno(BossModule module) : Components.RaidwideCast(module, AID._Ability_Inferno1);
class P4Tsunami(BossModule module) : Components.RaidwideCast(module, AID._Ability_Tsunami1);

class P4StrayFlames(BossModule module) : Components.StandardAOEs(module, AID.StrayFlames, new AOEShapeDonut(6, 40));
class P4StrayCircle(BossModule module) : Components.GroupedAOEs(module, [AID._Ability_StraySpray, AID.StrayFlames1], new AOEShapeCircle(6));
class P4EdgeOfDeath(BossModule module) : Components.StandardAOEs(module, AID._Ability_EdgeOfDeath, new AOEShapeRect(48, 1));

class P4Antilight(BossModule module) : Components.GroupedAOEs(module, [AID._Ability_WhiteAntilight, AID._Ability_BlackAntilight], new AOEShapeRect(47, 10.5f))
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
            var casterColor = cast.IsSpell(AID._Ability_WhiteAntilight) ? Wound.White : Wound.Black;
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

    enum Debuff
    {
        None,
        Shriek, // gaze
        Lightning, // spread
        Water, // stack
        Bomb, // pyretic
        Entropy, // baited circle AOE 
        Fluid, // baited donut AOE
    }

    readonly List<(Debuff D, bool Real, DateTime Expiration)>[] _debuffs = Utils.GenArray<List<(Debuff D, bool Real, DateTime Expiration)>>(8, () => []);

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
        foreach (var (db, t, e) in _debuffs[slot])
            hints.Add($"{DebuffReadable(db, t)} in {(e - WorldState.CurrentTime).TotalSeconds:f1}s", false);
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
            _debuffs[slot].Add((dbf, real, status.ExpireAt));
            _debuffs[slot].SortBy(s => s.Expiration);
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
            var ix = _debuffs[slot].FindIndex(d => d.D == dbf);
            if (ix >= 0)
                _debuffs[slot].RemoveAt(ix);
        }
    }
}
