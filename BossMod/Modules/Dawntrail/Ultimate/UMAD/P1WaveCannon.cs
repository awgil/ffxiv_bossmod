namespace BossMod.Dawntrail.Ultimate.UMAD;

class P1PulseWave(BossModule module) : Components.Knockback(module, AID.PulseWave, true)
{
    public static readonly WPos Origin = new(100, 65);
    public const float Distance = 13f;

    public DateTime Activation { get; private set; }
    public BitMask Targets;

    bool _blizzardStarted;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.GravenImage)
        {
            Targets.Set(Raid.FindSlot(tether.Target));
            Activation = WorldState.FutureTime(5.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Targets.Clear(Raid.FindSlot(spell.MainTargetID));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        _blizzardStarted |= (AID)spell.Action.ID is AID.BlizzardIIIBlowout1 or AID.BlizzardIIIBlowout2;
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (Targets[slot])
            yield return new(Origin, Distance, Activation);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Targets[slot] || _blizzardStarted)
            return;

        // TODO tweak zone size. we want the player to move north if they get tethered, since blizzard zones are only visible for ~2.5s before kb activates
        // max melee is 9.5y circle around arena center
        hints.AddForbiddenZone(ShapeContains.InvertedRect(new(100, 80), new(100, 93), 40), Activation);
    }
}

class P1BlizzardIIIBlowout(BossModule module) : Components.GroupedAOEs(module, [AID.BlizzardIIIBlowout1, AID.BlizzardIIIBlowout2], new AOEShapeCone(40, 45.Degrees()))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Module.FindComponent<P1PulseWave>() is { } pw)
            return base.ActiveAOEs(slot, actor).Select(a => a with { Risky = !pw.Targets[slot] });

        return base.ActiveAOEs(slot, actor);
    }
}
class P1ThrummingThunderIII(BossModule module) : Components.GroupedAOEs(module, [AID.ThrummingThunderIII1, AID.ThrummingThunderIII2], new AOEShapeRect(40, 5));

class P1FlagrantFireIII(BossModule module) : Components.UniformStackSpread(module, 6, 5, 4, 4)
{
    readonly UMADConfig _config = Service.Config.Get<UMADConfig>();

    enum Mechanic { None, Stack, Spread }
    enum Lying { Unsure, No, Yes }

    Mechanic _displayed;
    Lying _lying;

    BitMask _stackTargets; // empty if boss is lying or if the displayed mechanic is spread

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.FireSpread:
                _displayed = Mechanic.Spread;
                Init();
                break;
            case IconID.FireStack:
                _stackTargets.Set(Raid.FindSlot(targetID));
                _displayed = Mechanic.Stack;
                Init();
                break;
            case IconID.MysteryMagicFireLie:
                _lying = Lying.Yes;
                Init();
                break;
            case IconID.MysteryMagicFireTruth:
                _lying = Lying.No;
                Init();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FlagrantFireIIIStack:
                Stacks.Clear();
                break;
            case AID.FlagrantFireIIISpread:
                Spreads.Clear();
                break;
        }
    }

    void Init()
    {
        if (Stacks.Count > 0 || Spreads.Count > 0)
            return;

        switch ((_displayed, _lying))
        {
            case (Mechanic.Stack, Lying.Yes):
            case (Mechanic.Spread, Lying.No):
                AddSpreads();
                break;
            case (Mechanic.Stack, Lying.No):
            case (Mechanic.Spread, Lying.Yes):
                AddStacks();
                break;
        }
    }

    void AddStacks()
    {
        // wait for other stack target to be telegraphed
        if (_stackTargets.NumSetBits() == 1)
            return;

        BitMask maskLeft = new(), maskRight = new();
        foreach (var (slot, group) in _config.P1WaveCannonConga.Resolve(Raid))
        {
            if (group < 4)
                maskLeft.Set(slot);
            else
                maskRight.Set(slot);
        }

        void addMasked(int slot, Actor target)
        {
            var allowedMask = maskLeft[slot] ? maskLeft : maskRight[slot] ? maskRight : default;
            AddStack(target, WorldState.FutureTime(5.8f), allowedMask.Any() ? ~allowedMask : default);
        }

        foreach (var (slot, knownTarget) in Raid.WithSlot().IncludedInMask(_stackTargets))
            addMasked(slot, knownTarget);

        if (Stacks.Count == 0)
        {
            var partySorted = Raid.WithSlot().OrderBy(r => r.Item2.Class.IsDD()).ToList();
            addMasked(partySorted[0].Item1, partySorted[0].Item2);
            addMasked(partySorted[^1].Item1, partySorted[^1].Item2);
        }
    }

    void AddSpreads()
    {
        foreach (var player in Raid.WithoutSlot())
            AddSpread(player, WorldState.FutureTime(5.8f));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var myOrder = _config.P1WaveCannonConga[assignment];
        var (isKB, kbAt) = Module.FindComponent<P1PulseWave>() is { } pw ? (pw.Targets[slot], pw.Activation) : (actor.PendingKnockbacks.Count > 0, default);
        if (myOrder < 0 || EnableHints && !isKB)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        var pcSlot = slot;
        var pc = actor;

        WPos mySpot;

        var isSpread = Spreads.Count > 0;

        if (isSpread)
            // space players evenly-ish across the arena
            mySpot = Arena.Center + new WDir((myOrder - 3.5f) * 5, 0);
        else
            mySpot = Arena.Center + new WDir(myOrder > 3 ? 6 : -6, 0);

        var safeDirZ = 1;

        if (Module.FindComponent<P1BlizzardIIIBlowout>()?.ActiveAOEs(pcSlot, pc).Any(e => e.Check(mySpot + new WDir(0, safeDirZ))) == true)
            safeDirZ = -safeDirZ;

        if (isSpread)
        {
            var bossDirX = myOrder > 3 ? -1 : 1;

            switch (myOrder)
            {
                // MT/M1 can move in horizontally to relative n/s to give other melees space
                case 3:
                case 4:
                    mySpot += new WDir(1.5f * bossDirX, 6 * safeDirZ);
                    break;
                // OT/M2 can also move in horizontally, staying relative e/w
                case 2:
                case 5:
                    mySpot += new WDir(1.5f * bossDirX, safeDirZ);
                    break;
                // ranged 1s move further away from center line to give partner space
                case 1:
                case 6:
                    mySpot.Z += safeDirZ * 3;
                    break;
                default:
                    mySpot.Z += safeDirZ;
                    break;
            }
        }
        else
        {
            mySpot.Z += safeDirZ * 2;
        }

        if (isKB)
        {
            var dirToSpot = P1PulseWave.Origin - mySpot;
            mySpot += dirToSpot.Normalized() * P1PulseWave.Distance;
        }

        hints.AddForbiddenZone(ShapeContains.InvertedCircle(mySpot, isSpread ? 1 : 4), kbAt);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (IsSpreadTarget(player))
            return PlayerPriority.Danger;
        if (IsStackTarget(player))
            return PlayerPriority.Interesting;
        return Active ? PlayerPriority.Normal : PlayerPriority.Irrelevant;
    }
}

class P1WaveCannon : Components.UntelegraphedBait
{
    public P1WaveCannon(BossModule module) : base(module, AID.WaveCannon)
    {
        CurrentBaits.Add(new(P1PulseWave.Origin, Raid.WithSlot().Mask(), new AOEShapeRect(100, 3), WorldState.FutureTime(4.3f), 8));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

class P1Explosion(BossModule module) : Components.CastTowers(module, AID.ExplosionP1, 4)
{
    readonly DateTime[] _vuln = new DateTime[8];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MagicVulnerabilityUp && Raid.TryFindSlot(actor.InstanceID, out var slot))
            _vuln[slot] = status.ExpireAt;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction && Towers.Count == 4)
        {
            Towers.SortBy(t => t.Position.X);

            var itower = 0;
            foreach (var (slot, group) in Service.Config.Get<UMADConfig>().P1WaveCannonConga.Resolve(Raid).OrderBy(s => s.group))
            {
                if (_vuln[slot] < Towers[itower].Activation)
                {
                    Towers.Ref(itower++).ForbiddenSoakers = ~BitMask.Build(slot);
                    if (itower >= 4)
                        break;
                }
            }

            if (itower == 0)
            {
                var vulnAll = BitMask.Build([.. Enumerable.Range(0, _vuln.Length).Where(i => _vuln[i] != default)]);
                for (var i = 0; i < Towers.Count; i++)
                    Towers.Ref(i).ForbiddenSoakers = vulnAll;
            }
        }
    }
}
