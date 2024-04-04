namespace BossMod.Shadowbringers.Foray.Duel.Duel2Lyon;

class Enaero : BossComponent
{
    private bool EnaeroBuff;
    private bool casting;

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (casting)
            hints.Add("Applies Enaero to Lyon. Use Dispell to remove it");
        if (EnaeroBuff)
            hints.Add("Enaero on Lyon. Use Dispell to remove it! You only need to do this once per duel, so you can switch to a different action after removing his buff.");
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if (actor == module.PrimaryActor && (SID)status.ID == SID.Enaero)
            EnaeroBuff = true;
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RagingWinds1)
            casting = true;
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RagingWinds1)
            casting = false;
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if (actor == module.PrimaryActor && (SID)status.ID == SID.Enaero)
            EnaeroBuff = false;
    }
}

class HeartOfNatureConcentric : Components.ConcentricAOEs
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)];

    public HeartOfNatureConcentric() : base(_shapes) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.NaturesPulse1)
            AddSequence(caster.Position, spell.NPCFinishAt);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
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

class TasteOfBlood : Components.SelfTargetedAOEs
{
    public TasteOfBlood() : base(ActionID.MakeSpell(AID.TasteOfBlood), new AOEShapeCone(40, 90.Degrees())) { }
}

class TasteOfBloodHint : Components.CastHint
{
    public TasteOfBloodHint() : base(ActionID.MakeSpell(AID.TasteOfBlood), "Go behind Lyon!") { }
}

class RavenousGale : Components.GenericAOEs
{
    private bool activeTwister;
    private bool casting;
    private DateTime _activation;
    private static readonly AOEShapeCircle circle = new(0.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (casting)
            yield return new(circle, actor.Position, default, _activation);
        if (activeTwister)
            foreach (var p in module.Enemies(OID.RavenousGaleVoidzone))
                yield return new(circle, p.Position, default, _activation);
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID == OID.RavenousGaleVoidzone)
        {
            activeTwister = true;
            casting = false;
            _activation = module.WorldState.CurrentTime.AddSeconds(4.6f);
        }
    }

    public override void OnActorDestroyed(BossModule module, Actor actor)
    {
        if ((OID)actor.OID == OID.RavenousGaleVoidzone)
        {
            activeTwister = false;
            casting = false;
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RavenousGale)
            casting = true;
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        base.AddGlobalHints(module, hints);
        if (casting)
            hints.Add("Move a little to avoid voidzone spawning under you");
    }
}

class TwinAgonies : Components.SingleTargetCast
{
    public TwinAgonies() : base(ActionID.MakeSpell(AID.TwinAgonies), "Heavy Tankbuster, use Manawall or tank mitigations") { }
}

class WindsPeak : Components.SelfTargetedAOEs
{
    public WindsPeak() : base(ActionID.MakeSpell(AID.WindsPeak1), new AOEShapeCircle(5)) { }
}

class WindsPeakKB : Components.Knockback
{
    private DateTime Time;
    private bool watched;
    private DateTime _activation;

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        if (watched && module.WorldState.CurrentTime < Time.AddSeconds(4.4f))
            yield return new(module.PrimaryActor.Position, 15, _activation);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WindsPeak1)
        {
            watched = true;
            Time = module.WorldState.CurrentTime;
            _activation = spell.NPCFinishAt;
        }
    }
}

class TheKingsNotice : Components.CastGaze
{
    public TheKingsNotice() : base(ActionID.MakeSpell(AID.TheKingsNotice)) { }
}

class SplittingRage : Components.CastHint
{
    public SplittingRage() : base(ActionID.MakeSpell(AID.SplittingRage), "Applies temporary misdirection") { }
}

class NaturesBlood : Components.Exaflare
{
    public NaturesBlood() : base(4) { }

    class LineWithActor : Line
    {
        public Actor Caster;

        public LineWithActor(Actor caster)
        {
            Next = caster.Position;
            Advance = 6 * caster.Rotation.ToDirection();
            NextExplosion = caster.CastInfo!.NPCFinishAt;
            TimeToMove = 1.1f; //note the actual time between exaflare moves seems to vary by upto 100ms, but all 4 exaflares move at the same time
            ExplosionsLeft = 7;
            MaxShownExplosions = 3;
            Caster = caster;
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NaturesBlood1)
            Lines.Add(new LineWithActor(caster));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (Lines.Count > 0 && (AID)spell.Action.ID is AID.NaturesBlood1 or AID.NaturesBlood2)
        {
            int index = Lines.FindIndex(item => ((LineWithActor)item).Caster == caster);
            AdvanceLine(module, Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}

class SpitefulFlameCircleVoidzone : Components.GenericAOEs
{
    private bool activeOrb;
    private int casts;
    private static readonly AOEShapeCircle circle = new(10);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (activeOrb && casts <= 11 && casts != 0)
            foreach (var p in module.Enemies(OID.VermillionFlame))
                yield return new(circle, p.Position);
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID == OID.VermillionFlame)
            activeOrb = true;
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SpitefulFlame1)
            casts++;
        if (casts == 12)
        {
            casts = 0;
            activeOrb = false;
        }
    }
}

class SpitefulFlameRect : Components.SelfTargetedAOEs
{
    public SpitefulFlameRect() : base(ActionID.MakeSpell(AID.SpitefulFlame2), new AOEShapeRect(80, 2)) { }
}

class DynasticFlame : Components.BaitAwayTethers
{
    private ulong target;
    private int orbcount;
    public DynasticFlame() : base(new AOEShapeCircle(10), (uint)TetherID.fireorbs)
    {
        CenterAtTarget = true;
    }
    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DynasticFlame1)
            target = spell.TargetID;
    }
    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(module, slot, actor, assignment, hints);
        if (target == actor.InstanceID && CurrentBaits.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.Circle(module.Bounds.Center, 18));
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (target == actor.InstanceID && CurrentBaits.Count > 0)
            hints.Add("Go to the edge and run until 4 orbs are spawned");
    }
    public override void OnActorCreated(BossModule module, Actor actor)
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

class SkyrendingStrike : Components.CastHint
{
    public SkyrendingStrike() : base(ActionID.MakeSpell(AID.SkyrendingStrike), "Enrage!", true) { }
}
