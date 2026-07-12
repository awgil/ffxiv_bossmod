namespace BossMod.Dawntrail.Ultimate.UMAD;

class P4GrandCross(BossModule module) : Components.RaidwideCast(module, AID.GrandCross);
class P4Inferno(BossModule module) : Components.RaidwideCast(module, AID.InfernoHitP4);
class P4Tsunami(BossModule module) : Components.RaidwideCast(module, AID.TsunamiCastP4);
class P4UltimaUpsurge(BossModule module) : Components.RaidwideCast(module, AID.UltimaUpsurge);

class P4StrayDonut(BossModule module) : Components.GroupedAOEs(module, [AID.StrayFlamesInvertedP4, AID.StraySprayNormalP4], new AOEShapeDonut(6, 40));
class P4StrayCircle(BossModule module) : Components.GroupedAOEs(module, [AID.StraySprayInvertedP4, AID.StrayFlamesNormalP4], new AOEShapeCircle(6));
class P4StrayCounter(BossModule module) : Components.CastCounterMulti(module, [AID.StrayFlamesNormalP4, AID.StrayFlamesInvertedP4, AID.StraySprayNormalP4, AID.StraySprayInvertedP4])
{
    public int NumCasters { get; private set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (WatchedActions.Contains(spell.Action))
            NumCasters++;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (WatchedActions.Contains(spell.Action))
            NumCasters--;
    }
}
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

    public class GlobalP4Hints
    {
        public bool? Gaze1;
        public bool? Fire;
        public bool? Gaze2;
        public bool? Water;
    }

    public readonly GlobalP4Hints HelpHints = new();

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.P4TruthLie)
        {
            switch (status.Extra)
            {
                case 0x45F:
                    _chaosTrue = false;
                    break;
                case 0x460:
                    _chaosTrue = true;
                    break;
                case 0x461:
                    _exdeathTrue = false;
                    if (HelpHints.Gaze1 == null)
                        HelpHints.Gaze1 = false;
                    else
                        HelpHints.Gaze2 ??= false;
                    break;
                case 0x462:
                    _exdeathTrue = true;
                    if (HelpHints.Gaze1 == null)
                        HelpHints.Gaze1 = true;
                    else
                        HelpHints.Gaze2 ??= true;
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

        if (dbf == Debuff.Entropy)
            HelpHints.Fire = real;
        if (dbf == Debuff.Fluid)
            HelpHints.Water = real;

        if (dbf != default && Raid.TryFindSlot(actor, out var slot))
        {
            Debuffs[slot].Add((dbf, real, status.ExpireAt));
            Debuffs[slot].SortBy(s => s.Expiration);
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (HelpHints.Gaze1 is { } g1)
            hints.Add($"Gazes 1: " + (g1 ? "Normal" : "Inverted"));
        if (HelpHints.Fire is { } f)
            hints.Add($"Fire: " + (f ? "Move out" : "Stay in"));
        if (HelpHints.Gaze2 is { } g2)
            hints.Add("Gazes 2: " + (g2 ? "Normal" : "Inverted"));
        if (HelpHints.Water is { } w)
            hints.Add("Water: " + (w ? "Stay in" : "Move out"));
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
        EnableHints = false;

        foreach (var (_, player, debuff, real, exp) in module.FindComponent<P4Debuffs>()!.EnumerateDebuffs(WorldState.FutureTime(15)))
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

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (EnableHints)
            base.AddHints(slot, actor, hints);
        else if (IsSpreadTarget(actor))
            hints.Add("Prepare to spread!", false);
        else
            hints.Add("Prepare to stack!", false);
    }
}

class P4DeathBomb : Components.StayMove
{
    public P4DeathBomb(BossModule module) : base(module, 5)
    {
        foreach (var (i, _, debuff, real, exp) in Module.FindComponent<P4Debuffs>()!.EnumerateDebuffs(WorldState.FutureTime(15)))
            if (debuff == P4Debuffs.Debuff.Bomb)
                SetState(i, new(real ? Requirement.Stay : Requirement.Move, exp));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.AccelerationBomb && Raid.TryFindSlot(actor, out var slot))
            ClearState(slot);
    }
}

class P4DeathShriek : Components.GenericGaze
{
    readonly List<(Actor, DateTime)> _sources = [];

    public P4DeathShriek(BossModule module) : base(module)
    {
        EnableHints = false;

        foreach (var (_, player, debuff, real, exp) in Module.FindComponent<P4Debuffs>()!.EnumerateDebuffs(WorldState.FutureTime(10)))
            if (debuff == P4Debuffs.Debuff.Shriek)
            {
                Inverted = !real;
                _sources.Add((player, exp));
            }
    }

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        foreach (var (player, time) in _sources)
            if (player != actor)
                yield return new(player.Position, time);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DeathShriek1 or AID.DeathShriek2)
        {
            NumCasts++;
            if (_sources.Count > 0)
                _sources.RemoveAt(0);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (EnableHints && _sources.Any(s => s.Item1 == actor))
            hints.Add("You have gaze, be careful!", false);
    }
}
