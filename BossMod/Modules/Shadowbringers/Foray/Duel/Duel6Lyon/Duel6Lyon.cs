namespace BossMod.Shadowbringers.Foray.Duel.Duel6Lyon;

public enum OID : uint
{
    Boss = 0x31C1,
    Helper = 0x233C,
    VermillionFlame = 0x2E8F,
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    WildfiresFury = 0x5D39, // Damage
    HarnessFire = 0x5D38, // Boss->self, 3.0s cast, single-target
    HeartOfNature = 0x5D24, // Boss->self, 3.0s cast, range 80 circle
    CagedHeartOfNature = 0x5D1D, // Boss->self, 3.0s cast, range 10 circle
    NaturesPulse1 = 0x5D25, // Helper->self, 4.0s cast, range 10 circle
    NaturesPulse2 = 0x5D26, // Helper->self, 5.5s cast, range 10-20 donut
    NaturesPulse3 = 0x5D27, // Helper->self, 7.0s cast, range 20-30 donut
    TasteOfBlood = 0x5D23, // Boss->self, 4.0s cast, range 40 180-degree cone
    SoulAflame = 0x5D2C,
    FlamesMeet1 = 0x5D2D, // VermillionFlame->self, 6.5s cast; makes the orb light up
    FlamesMeet2 = 0x5D2E, // VermillionFlame->self, 11s cast; actual AOE
    HeavenAndEarthCW = 0x5D2F,
    HeavenAndEarthCCW = 0x5FEA,
    HeavenAndEarthRotate = 0x5D30, // Unused by module
    HeavenAndEarthStart = 0x5D31,
    HeavenAndEarthMove = 0x5D32,
    MoveMountains1 = 0x5D33, // Boss->self, 5s cast, first attack
    MoveMountains2 = 0x5D34, // Boss->self, no cast, second attack
    MoveMountains3 = 0x5D35, // Helper->self, 5s cast, first line
    MoveMountains4 = 0x5D36, // Helper->self, no cast, second line
    WindsPeak1 = 0x5D2A, // Boss->self, 3.0s cast, range 5 circle
    WindsPeak2 = 0x5D2B, // Helper->self, 4.0s cast, range 50 circle
    NaturesBlood1 = 0x5D28, // Helper->self, 7.5s cast, range 4 circle
    NaturesBlood2 = 0x5D29, // Helper->self, no cast, range 4 circle
    SplittingRage = 0x5D37, // Boss->self, 3.0s cast, range 50 circle
    DuelOrDie = 0x5D1C,
    WildfireCrucible = 0x5D3B, //enrage, 25s cast time
    NaturesBlood3 = 23841, // Helper->location, 4.5s cast, range 4 circle
    NaturesBlood4 = 23842, // Helper->location, no cast, range 4 circle
    NaturesPulse4 = 23838, // Helper->self, 7.0s cast, range 10 circle
    NaturesPulse5 = 23839, // Helper->self, 8.5s cast, range ?-20 donut
    NaturesPulse6 = 23840, // Helper->self, 10.0s cast, range ?-30 donut
}

public enum SID : uint
{
    OnFire = 0x9F3, // Boss->Boss
    TemporaryMisdirection = 1422, // Boss->player, extra=0x2D0
    DuelOrDie = 0x9F1, // Boss/Helper->player
}

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

class WildfiresFury(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WildfiresFury));

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
            Sequences.Add(new(_shape, caster.Position, spell.Rotation, _increment, Module.CastFinishAt(spell), 1.2f, 3));
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
        if ((AID)spell.Action.ID is AID.NaturesPulse1 or AID.NaturesPulse4)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Sequences.Count > 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.NaturesPulse1 or AID.NaturesPulse4 => 0,
                AID.NaturesPulse2 or AID.NaturesPulse5 => 1,
                AID.NaturesPulse3 or AID.NaturesPulse6 => 2,
                _ => -1
            };
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(1.5f));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // prevent AI from prepositioning in the second ring of either AOE
        if (Sequences.Count == 2 && Sequences.All(s => s.NumCastsDone == 0))
        {
            hints.AddForbiddenZone(p =>
            {
                if (p.InDonut(Sequences[0].Origin, 10, 20) && p.InDonut(Sequences[1].Origin, 10, 20))
                    return -1;
                return 0;
            }, Sequences[0].NextActivation);
        }
    }
}

class CagedHeartOfNature(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CagedHeartOfNature), new AOEShapeCircle(6));

class WindsPeak(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WindsPeak1), new AOEShapeCircle(5));

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

class SplittingRage(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.SplittingRage), "Applies temporary misdirection");

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
        if ((AID)spell.Action.ID is AID.NaturesBlood1 or AID.NaturesBlood3)
            Lines.Add(new LineWithActor(Module, caster));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Lines.Count > 0 && (AID)spell.Action.ID is AID.NaturesBlood1 or AID.NaturesBlood2 or AID.NaturesBlood3 or AID.NaturesBlood4)
        {
            int index = Lines.FindIndex(item => ((LineWithActor)item).Caster == caster);
            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}

class MoveMountains(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    private static readonly AOEShape Shape = new AOEShapeRect(40, 3);

    private static readonly Angle[] Pattern2 = new float[] { 22.5f, 67.5f, 112.5f, 180f, -112.5f, -67.5f, -22.5f }.Select(f => f.Degrees()).ToArray();

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MoveMountains3)
        {
            aoes.Add(new AOEInstance(Shape, caster.Position, caster.Rotation, Module.CastFinishAt(spell), ArenaColor.Danger));
            if (aoes.Count == 7)
                foreach (var offset in Pattern2)
                    aoes.Add(new AOEInstance(Shape, caster.Position, Module.PrimaryActor.Rotation + offset, Module.CastFinishAt(spell, 2.3f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.MoveMountains3 or AID.MoveMountains4)
        {
            aoes.RemoveAt(0);
            if (aoes.Count == 7)
                for (var i = 0; i < 7; i++)
                    aoes.Ref(i).Color = ArenaColor.Danger;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        // draw lighter colored future aoes beneath imminent aoes so opacity doesnt look bad
        foreach (var c in Enumerable.Reverse(aoes))
            c.Shape.Draw(Arena, c.Origin, c.Rotation, c.Color);
    }
}

class FlamesMeet(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCross _shape = new(40, 7);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (int i = 0; i < _aoes.Count; i++)
        {
            AOEInstance aoe = _aoes[i];
            if (i == 0)
                aoe.Color = ArenaColor.Danger;
            yield return aoe;
            // Only show the first 2 so it's obvious which one to go to.
            if (i == 1)
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FlamesMeet2)
            _aoes.Add(new(_shape, caster.Position));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FlamesMeet2 && _aoes.Count > 0)
            _aoes.RemoveAt(0);
    }
}

class WildfireCrucible(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.WildfireCrucible), "Enrage!", true);

class Duel6LyonStates : StateMachineBuilder
{
    public Duel6LyonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OnFire>()
            .ActivateOnEnter<WildfiresFury>()
            .ActivateOnEnter<HeavenAndEarth>()
            .ActivateOnEnter<HeartOfNatureConcentric>()
            .ActivateOnEnter<TasteOfBloodAndDuelOrDie>()
            .ActivateOnEnter<FlamesMeet>()
            .ActivateOnEnter<WindsPeak>()
            .ActivateOnEnter<WindsPeakKB>()
            .ActivateOnEnter<SplittingRage>()
            .ActivateOnEnter<NaturesBlood>()
            .ActivateOnEnter<MoveMountains>()
            .ActivateOnEnter<WildfireCrucible>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "SourP", GroupType = BossModuleInfo.GroupType.BozjaDuel, GroupID = 778, NameID = 31)]
public class Duel6Lyon(WorldState ws, Actor primary) : BossModule(ws, primary, new(50f, -410f), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        var tasteOfBlood = FindComponent<TasteOfBloodAndDuelOrDie>();
        if (tasteOfBlood?.Casters.Count > 0)
        {
            foreach (var caster in tasteOfBlood.Casters)
            {
                bool isDueler = tasteOfBlood.Duelers.Contains(caster);
                Arena.Actor(caster, isDueler ? ArenaColor.Danger : ArenaColor.Enemy, true);
            }
        }
        else
        {
            base.DrawEnemies(pcSlot, pc);
        }
    }
}
