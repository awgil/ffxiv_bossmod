namespace BossMod.Shadowbringers.Foray.Duel.Duel6Lyon;

class OnFire(BossModule module) : BossComponent(module)
{
    private bool _hasBuff;
    private bool _isCasting;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_isCasting)
            hints.Add("Applies On Fire to Lyon. Use Dispell to remove it");
        if (_hasBuff)
            hints.Add("Lyon has 'On Fire'. Use Dispell to remove it!");
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (actor == Module.PrimaryActor && (SID)status.ID == SID.OnFire)
            _hasBuff = true;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HarnessFire)
            _isCasting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HarnessFire)
            _isCasting = false;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (actor == Module.PrimaryActor && (SID)status.ID == SID.OnFire)
            _hasBuff = false;
    }
}

class WildfiresFury(BossModule module) : Components.RaidwideCast(module, AID.WildfiresFury);

class HeavenAndEarth(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;

    private static readonly AOEShapeCone _shape = new(20, 15.Degrees());

    private int _index;

    private void UpdateIncrement(Angle increment)
    {
        _increment = increment;
        for (int i = 0; i < Sequences.Count; i++)
        {
            var sequence = Sequences[i];
            sequence.Increment = _increment;
            Sequences[i] = sequence;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HeavenAndEarthCW)
            UpdateIncrement(-30.Degrees());
        else if ((AID)spell.Action.ID == AID.HeavenAndEarthCCW)
            UpdateIncrement(30.Degrees());

        if ((AID)spell.Action.ID == AID.HeavenAndEarthStart)
            Sequences.Add(new(_shape, caster.Position, spell.Rotation, _increment, Module.CastFinishAt(spell), 1.2f, 4));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.HeavenAndEarthMove && Sequences.Count > 0)
        {
            AdvanceSequence(_index++ % Sequences.Count, WorldState.CurrentTime);
        }
    }
}

class HeartOfNatureConcentric(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.NaturesPulse1)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Sequences.Count > 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.NaturesPulse1 => 0,
                AID.NaturesPulse2 => 1,
                AID.NaturesPulse3 => 2,
                _ => -1
            };
            AdvanceSequence(order, caster.Position);
        }
    }
}

class CagedHeartOfNature(BossModule module) : Components.StandardAOEs(module, AID.CagedHeartOfNature, new AOEShapeCircle(6));

class WindsPeak(BossModule module) : Components.StandardAOEs(module, AID.WindsPeak1, new AOEShapeCircle(5));

class WindsPeakKB(BossModule module) : Components.Knockback(module)
{
    private DateTime _time;
    private bool _watched;
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_watched && WorldState.CurrentTime < _time.AddSeconds(4.4f))
            yield return new(Module.PrimaryActor.Position, 15, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WindsPeak1)
        {
            _watched = true;
            _time = WorldState.CurrentTime;
            _activation = Module.CastFinishAt(spell);
        }
    }
}

class SplittingRage(BossModule module) : Components.CastHint(module, AID.SplittingRage, "Applies temporary misdirection");

class NaturesBlood(BossModule module) : Components.Exaflare(module, 4)
{
    class LineWithActor : Line
    {
        public Actor Caster;

        public LineWithActor(BossModule module, Actor caster)
        {
            Next = caster.Position;
            Advance = 6 * caster.Rotation.ToDirection();
            NextExplosion = module.CastFinishAt(caster.CastInfo);
            TimeToMove = 1.1f;
            ExplosionsLeft = 7;
            MaxShownExplosions = 3;
            Caster = caster;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NaturesBlood1)
            Lines.Add(new LineWithActor(Module, caster));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Lines.Count > 0 && (AID)spell.Action.ID is AID.NaturesBlood1 or AID.NaturesBlood2)
        {
            int index = Lines.FindIndex(item => ((LineWithActor)item).Caster == caster);
            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}

class MoveMountains(BossModule module) : Components.StandardAOEs(module, AID.MoveMountains3, new AOEShapeRect(40, 3))
{
    // TODO predict rotation
}

class WildfireCrucible(BossModule module) : Components.CastHint(module, AID.WildfireCrucible, "Enrage!", true);
