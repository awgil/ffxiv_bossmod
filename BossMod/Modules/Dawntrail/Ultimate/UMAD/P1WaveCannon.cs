namespace BossMod.Dawntrail.Ultimate.UMAD;

class P1PulseWave(BossModule module) : Components.Knockback(module, AID.PulseWave, true)
{
    WPos _source;

    public DateTime Activation { get; private set; }
    public BitMask Targets;

    bool _blizzardStarted;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.GravenImage)
        {
            _source = source.Position;
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
            yield return new(_source, 13, Activation);
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

class P1BlizzardIIIBlowout(BossModule module) : Components.GroupedAOEs(module, [AID.BlizzardIIIBlowout1, AID.BlizzardIIIBlowout2], new AOEShapeCone(40, 45.Degrees()));
class P1ThrummingThunderIII(BossModule module) : Components.GroupedAOEs(module, [AID.ThrummingThunderIII1, AID.ThrummingThunderIII2], new AOEShapeRect(40, 5));

class P1FlagrantFireIII(BossModule module) : Components.UniformStackSpread(module, 6, 5, 4, 4)
{
    readonly P1PulseWave? _pulseWave = module.FindComponent<P1PulseWave>();
    readonly P1BlizzardIIIBlowout? _blizzard = module.FindComponent<P1BlizzardIIIBlowout>();
    readonly UMADConfig _config = Service.Config.Get<UMADConfig>();

    enum Mechanic { None, Stack, Spread }
    enum Lying { Unsure, No, Yes }

    Mechanic _displayed;
    Lying _lying;

    BitMask _stackTargets; // empty if boss is lying or if the displayed mechanic is spread

    bool HintsInitialized;

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

        foreach (var (_, knownTarget) in Raid.WithSlot().IncludedInMask(_stackTargets))
            AddStack(knownTarget, WorldState.FutureTime(5.8f));

        if (Stacks.Count == 0)
        {
            var partySorted = Raid.WithoutSlot().OrderBy(r => r.Class.IsDD()).ToList();
            AddStack(partySorted[0], WorldState.FutureTime(5.8f));
            AddStack(partySorted[^1], WorldState.FutureTime(5.8f));
        }
    }

    void AddSpreads()
    {
        foreach (var player in Raid.WithoutSlot())
            AddSpread(player, WorldState.FutureTime(5.8f));
    }

    public override void Update()
    {
        if (_pulseWave == null)
        {
            HintsInitialized = true;
            return;
        }

        if (HintsInitialized || !Active || _pulseWave.Targets.NumSetBits() < 4)
            return;

        HintsInitialized = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var myOrder = _config.P1WaveCannonConga[assignment];
        if (!HintsInitialized || myOrder < 0 || _pulseWave?.NumCasts > 0)
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
            mySpot = Arena.Center + new WDir(myOrder > 3 ? 7 : -7, 0);

        var safeDirZ = 1;

        if (_blizzard?.ActiveAOEs(pcSlot, pc).Any(e => e.Check(mySpot + new WDir(0, safeDirZ))) == true)
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
                // ranged move further away from center to give partner space
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

        if (_pulseWave!.Targets[pcSlot])
        {
            var dirToSpot = new WPos(100, 65) - mySpot;
            var dist = 13;
            mySpot += dirToSpot.Normalized() * dist;
        }

        hints.AddForbiddenZone(ShapeContains.InvertedCircle(mySpot, isSpread ? 1 : 6), _pulseWave!.Activation);
    }
}

class P1WaveCannon : Components.UntelegraphedBait
{
    public P1WaveCannon(BossModule module) : base(module, AID.WaveCannon)
    {
        CurrentBaits.Add(new(new(100, 65), Raid.WithSlot().Mask(), new AOEShapeRect(100, 3), WorldState.FutureTime(4.3f), 8));
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

        if (spell.Action == WatchedAction)
            for (var i = 0; i < Towers.Count; i++)
                Towers.Ref(i).ForbiddenSoakers = Raid.WithSlot().WhereSlot(s => _vuln[s] > Towers[i].Activation).Mask();
    }
}
