namespace BossMod.Dawntrail.Ultimate.UMAD;

class P1RevoltingRuinIIIFirst(BossModule module) : Components.BaitAwayCast(module, AID.RevoltingRuinIIIFirst, new AOEShapeCone(100, 60.Degrees()));

class P1RevoltingRuinIIISecond : Components.GenericBaitAway
{
    Actor? _caster;
    DateTime _resolve;

    public P1RevoltingRuinIIISecond(BossModule module) : base(module, AID.RevoltingRuinIIISecond)
    {
        EnableHints = false;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RevoltingRuinIIIFirst)
        {
            _caster = caster;
            _resolve = Module.CastFinishAt(spell, 3.4f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RevoltingRuinIIIFirst)
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

class P1LightOfJudgment(BossModule module) : Components.RaidwideCast(module, AID.LightOfJudgment);

class P1Hyperdrive(BossModule module) : Components.GenericBaitAway(module, AID.Hyperdrive, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    DateTime _first;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LightOfJudgment)
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
