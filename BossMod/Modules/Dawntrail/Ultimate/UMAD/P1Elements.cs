namespace BossMod.Dawntrail.Ultimate.UMAD;

class P1PulseWave(BossModule module) : Components.Knockback(module, AID.PulseWave, true)
{
    DateTime _activation;
    BitMask _targets;
    WPos _source;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_chn_elem0f)
        {
            _source = source.Position;
            _targets.Set(Raid.FindSlot(tether.Target));
            _activation = WorldState.FutureTime(5.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _targets.Clear(Raid.FindSlot(spell.MainTargetID));
        }
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_targets[slot])
            yield return new(_source, 13, _activation);
    }
}

class P1BlizzardIIIBlowout(BossModule module) : Components.GroupedAOEs(module, [AID.BlizzardIIIBlowout2, AID.BlizzardIIIBlowout1], new AOEShapeCone(40, 45.Degrees()));
class P1ThrummingThunderIII(BossModule module) : Components.GroupedAOEs(module, [AID.ThrummingThunderIII1, AID.ThrummingThunderIII2], new AOEShapeRect(40, 5));

class P1FlagrantFireIII(BossModule module) : Components.UniformStackSpread(module, 6, 5, 4, 4)
{
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
