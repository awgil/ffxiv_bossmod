namespace BossMod.Shadowbringers.Foray.Duel.Duel2Lyon;

class Enaero(BossModule module) : Components.DispelHint(module, (uint)SID.Enaero, AID.RagingWinds1);

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
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(1.5f));
            Sequences.RemoveAll(s => s.NumCastsDone >= _shapes.Length);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        var origins = Sequences.Select(s => s.Origin);

        if (Sequences.All(s => s.NumCastsDone == 0))
        {
            // preposition on edge of aoe to make next dodge easier
            hints.GoalZones.Add(p => origins.Any(o => p.InCircle(o, 12)) ? 5 : 0);

            // for multi-quake, standing inside both donuts will get us killed
            if (Sequences.Count == 2)
                hints.AddForbiddenZone(p => origins.All(o => p.InCircle(o, 20)), Sequences[0].NextActivation);
        }
    }
}

class TasteOfBlood(BossModule module) : Components.StandardAOEs(module, AID.TasteOfBlood, new AOEShapeCone(40, 90.Degrees()));

class RavenousGaleTwister(BossModule module) : Components.CastTwister(module, 1, (uint)OID.RavenousGaleVoidzone, AID.RavenousGale, 0.4f, 0.25f);
class RavenousGaleHint(BossModule module) : Components.CastHint(module, AID.RavenousGale, "Twisters spawning");

class TwinAgonies(BossModule module) : Components.SingleTargetCast(module, AID.TwinAgonies, "Use Manawall or tank mitigations!");
class WindsPeak(BossModule module) : Components.StandardAOEs(module, AID.WindsPeak1, new AOEShapeCircle(5));

class WindsPeakKB(BossModule module) : Components.KnockbackFromCastTarget(module, AID.WindsPeak2, 15)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.PrimaryActor.CastInfo == null && base.DestinationUnsafe(slot, actor, pos);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
            hints.AddForbiddenZone(ShapeContains.Donut(src.Origin, 5, 50), src.Activation);
    }
}

class TheKingsNotice(BossModule module) : Components.CastGaze(module, AID.TheKingsNotice);
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
            NextExplosion = module.CastFinishAt(caster.CastInfo!);
            TimeToMove = 1.1f; //note the actual time between exaflare moves seems to vary by upto 100ms, but all 4 exaflares move at the same time
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

class VermillionFlame(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Actor, DateTime Activation, int Casts)> Balls = [];

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.VermillionFlame)
            Balls.Add((actor, WorldState.FutureTime(2.6f), 0));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SpitefulFlame1 && Balls.FindIndex(b => b.Actor == caster) is var slot && slot >= 0)
            Balls.Ref(slot).Activation = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SpitefulFlame1)
        {
            var ix = Balls.FindIndex(b => b.Actor == caster);
            if (ix >= 0)
            {
                Balls.Ref(ix).Casts++;
                if (Balls[ix].Casts >= 3)
                    Balls.RemoveAt(ix);
            }
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Balls.Select(b => new AOEInstance(new AOEShapeCircle(10), b.Actor.Position, Activation: b.Activation));
}

class SpitefulFlameRect(BossModule module) : Components.StandardAOEs(module, AID.SpitefulFlame2, new AOEShapeRect(80, 2));

class DynasticFlame : Components.BaitAwayTethers
{
    private int orbcount;

    public DynasticFlame(BossModule module) : base(module, new AOEShapeCircle(10), (uint)TetherID.fireorbs)
    {
        CenterAtTarget = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.AddForbiddenZone(p => !p.InDonutCone(Arena.Center, 17, 20, Angle.FromDirection(actor.Position - Arena.Center) + 18.Degrees(), 20.Degrees()), orbcount > 0 ? default : DateTime.MaxValue);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.VermillionFlame)
            ++orbcount;
        if (orbcount == 4)
        {
            CurrentBaits.Clear();
            orbcount = 0;
        }
    }
}

class SkyrendingStrike(BossModule module) : Components.CastHint(module, AID.SkyrendingStrike, "Enrage!", true);
