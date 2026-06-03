namespace BossMod.Dawntrail.Ultimate.UMAD;

class P1RevoltingRuinIIIFirst(BossModule module) : Components.BaitAwayCast(module, AID._Ability_RevoltingRuinIII, new AOEShapeCone(100, 60.Degrees()));

class P1RevoltingRuinIIISecond : Components.GenericBaitAway
{
    Actor? _caster;
    DateTime _resolve;

    public P1RevoltingRuinIIISecond(BossModule module) : base(module, AID._Ability_RevoltingRuinIII1)
    {
        EnableHints = false;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_RevoltingRuinIII)
        {
            _caster = caster;
            _resolve = Module.CastFinishAt(spell, 3.4f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_RevoltingRuinIII)
            EnableHints = true;

        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _caster = null;
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_caster == null)
            return;

        var second = RaidByEnmity(_caster, true).Skip(1).FirstOrDefault();
        if (second != null)
            CurrentBaits.Add(new(_caster, second, new AOEShapeCone(100, 60.Degrees()), _resolve));
    }
}

class P1PulseWave(BossModule module) : Components.Knockback(module, AID._Ability_PulseWave, true)
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

class P1BlizzardIIIBlowout(BossModule module) : Components.GroupedAOEs(module, [AID._Ability_BlizzardIIIBlowout1, AID._Ability_BlizzardIIIBlowout2], new AOEShapeCone(40, 45.Degrees()));

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
            case IconID._Gen_Icon_m0462trg_a0c:
                _displayed = Mechanic.Spread;
                Init();
                break;
            case IconID._Gen_Icon_m0462trg_b0c:
                _stackTargets.Set(Raid.FindSlot(targetID));
                _displayed = Mechanic.Stack;
                Init();
                break;
            case IconID._Gen_Icon_m0462trg_c01c:
                _lying = Lying.Yes;
                Init();
                break;
            case IconID._Gen_Icon_m0462trg_c02c:
                _lying = Lying.No;
                Init();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Ability_FlagrantFireIII:
                Stacks.Clear();
                break;
            case AID._Ability_FlagrantFireIII1:
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
    public P1WaveCannon(BossModule module) : base(module, AID._Ability_WaveCannon)
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

class P1Explosion(BossModule module) : Components.CastTowers(module, AID._Ability_Explosion, 4)
{
    readonly DateTime[] _vuln = new DateTime[8];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_MagicVulnerabilityUp && Raid.TryFindSlot(actor.InstanceID, out var slot))
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

class P1ThrummingThunderIII(BossModule module) : Components.GroupedAOEs(module, [AID._Ability_ThrummingThunderIII2, AID._Ability_ThrummingThunderIII], new AOEShapeRect(40, 5));

class P1DoubleTroubleTrap : Components.UniformStackSpread
{
    public int NumCasts { get; private set; }

    public P1DoubleTroubleTrap(BossModule module) : base(module, 6, 0, 4)
    {
        EnableHints = false;
        PermitOverlap = true;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_DoubleTroubleTrap)
            AddStack(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_DoubleTroubleTrap1)
        {
            Stacks.Clear();
            NumCasts++;
        }
    }
}

class P1DoubleTroubleTrapKB(BossModule module) : Components.Knockback(module, AID._Ability_DoubleTroubleTrap1, true)
{
    readonly List<(Actor Source, DateTime Activation)> _sources = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var src in _sources)
            if (actor.Position.InCircle(src.Source.Position, 6))
                yield return new(src.Source.Position, 14, src.Activation);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_DoubleTroubleTrap && NumCasts == 0)
            _sources.Add((actor, status.ExpireAt));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (_sources.Count > 0)
                _sources.RemoveAt(0);
        }
    }
}

class P1LightOfJudgment(BossModule module) : Components.RaidwideCast(module, AID._Ability_LightOfJudgment);

class P1Hyperdrive(BossModule module) : Components.GenericBaitAway(module, AID._Ability_Hyperdrive, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    DateTime _first;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_LightOfJudgment)
            _first = Module.CastFinishAt(spell, 3.1f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (NumCasts >= 3)
                _first = default;
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        if (_first != default && WorldState.Actors.Find(Module.PrimaryActor.TargetID) is { } target)
            CurrentBaits.Add(new(Module.PrimaryActor, target, new AOEShapeCircle(5), _first));
    }
}

class P1GravitasVitrophyre : Components.UniformStackSpread
{
    readonly List<Spread> _predicted = [];

    public void SetNegativeOffset(float value)
    {
        for (var i = 0; i < Stacks.Count; i++)
            Stacks.Ref(i).Activation -= TimeSpan.FromSeconds(value);
        for (var i = 0; i < Spreads.Count; i++)
            Spreads.Ref(i).Activation -= TimeSpan.FromSeconds(value);
    }

    public P1GravitasVitrophyre(BossModule module) : base(module, 5, 0)
    {
        PermitOverlap = true;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_chn_elem0f && WorldState.Actors.Find(tether.Target) is { } target)
        {
            if (source.Position.AlmostEqual(new(102.5f, 27), 5))
                AddStack(target, WorldState.FutureTime(6.5f));
            else
                _predicted.Add(new(target, 5, WorldState.FutureTime(10.6f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_Gravitas && Stacks.Count > 0)
        {
            Stacks.RemoveAt(0);
            Spreads.AddRange(_predicted);
            _predicted.Clear();
        }

        if ((AID)spell.Action.ID == AID._Ability_Vitrophyre && Spreads.Count > 0)
            Spreads.RemoveAt(0);
    }
}

class P1GravitasPuddle(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID._Ability_Gravitas, m => m.Enemies(OID.GravitasP1).Where(e => e.EventState != 7), 0.7f);

class P1IntemperateWill(BossModule module) : Components.GenericAOEs(module, AID._Ability_IntemperateWill)
{
    AOEInstance? _predicted;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_predicted);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EBFBD && state == 0x00400080)
            _predicted = new(new AOEShapeCone(100, 90.Degrees()), Arena.Center, 90.Degrees(), WorldState.FutureTime(5.2f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _predicted = null;
        }
    }
}

[ModuleInfo(Incomplete = true, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1094, NameID = 7131, PlanLevel = 100)]
public class UMAD(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    public override bool ShouldPrioritizeAllEnemies => true;
}
