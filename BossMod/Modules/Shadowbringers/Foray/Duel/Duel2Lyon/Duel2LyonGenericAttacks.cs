using BossMod.Components;
using System;
using System.Collections.Generic;

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

class HeartOfNatureConcentric : ConcentricAOEs
{
    private static AOEShape[] _shapes = {new AOEShapeCircle(10), new AOEShapeDonut(10,20), new AOEShapeDonut(20,30)};

    public HeartOfNatureConcentric() : base(_shapes) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.NaturesPulse1)
            AddSequence(caster.Position, spell.FinishAt);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
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

class TasteOfBlood : SelfTargetedAOEs
{
    public TasteOfBlood() : base(ActionID.MakeSpell(AID.TasteOfBlood), new AOEShapeCone(40,90.Degrees())) { } 
}

class TasteOfBloodHint : CastHint
{
    public TasteOfBloodHint() : base(ActionID.MakeSpell(AID.TasteOfBlood), "Go behind Lyon!") { }
}

class RavenousGale : GenericAOEs
{
    private bool activeTwister;
    private bool casting;
    private static readonly AOEShapeCircle circle = new(0.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        var player = module.Raid.Player();
        if (casting && player != null)
            yield return new(circle, player.Position, player.Rotation, new());
        if (activeTwister)
            foreach (var p in module.Enemies(OID.RavenousGaleVoidzone))
                yield return new(circle, p.Position, p.Rotation, new());
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID == OID.RavenousGaleVoidzone)
            activeTwister = true;
            casting = false;
    }

    public override void OnActorDestroyed(BossModule module, Actor actor)
    {
        if ((OID)actor.OID == OID.RavenousGaleVoidzone)
            activeTwister = false;
            casting = false;
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RavenousGale)
            casting = true;
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (casting)
            hints.Add("Move a little to avoid voidzone spawning under you");
    }
}

class TwinAgonies : SingleTargetCast
{
    public TwinAgonies() : base(ActionID.MakeSpell(AID.TwinAgonies), "Heavy Tankbuster, use Manawall or tank mitigations") { }
}

class WindsPeak : SelfTargetedAOEs
{
    public WindsPeak() : base(ActionID.MakeSpell(AID.WindsPeak1), new AOEShapeCircle(5)) { } 
}

class WindsPeakKB : Knockback
{
    private DateTime Time;
    private bool watched;

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        if (watched && module.WorldState.CurrentTime < Time.AddSeconds(4.4f))
            yield return new(module.PrimaryActor.Position, 15, default, default, module.PrimaryActor.Rotation);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WindsPeak1)
        {
            watched = true;
            Time = module.WorldState.CurrentTime;
        }
    }
}

class TheKingsNotice : CastGaze
{
    public TheKingsNotice() : base(ActionID.MakeSpell(AID.TheKingsNotice)) { }
}

class SplittingRage : CastHint
{
    public SplittingRage() : base(ActionID.MakeSpell(AID.SplittingRage), "Applies temporary misdirection") { }
}

class NaturesBlood : Exaflare
{
    public NaturesBlood() : base(4) { }

    class LineWithActor : Line
    {
        public Actor Caster;

        public LineWithActor(Actor caster)
        {
            Next = caster.Position;
            Advance = 6 * caster.Rotation.ToDirection();
            NextExplosion = caster.CastInfo!.FinishAt;
            TimeToMove = 1.1f; //note the actual time between exaflare moves seems to vary by upto 100ms, but all 4 exaflares move at the same time
            ExplosionsLeft = 7;
            MaxShownExplosions = 3;
            Caster = caster;
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NaturesBlood1)
        {
            Lines.Add(new LineWithActor(caster));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NaturesBlood1 or AID.NaturesBlood2)
        {
            int index = Lines.FindIndex(item => ((LineWithActor)item).Caster == caster);
            AdvanceLine(module, Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}

class SpitefulFlameCircleVoidzone : GenericAOEs
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

class SpitefulFlameRect : SelfTargetedAOEs
{
    public SpitefulFlameRect() : base(ActionID.MakeSpell(AID.SpitefulFlame2), new AOEShapeRect(80,2)) { }
}

class DynasticFlame : UniformStackSpread
{
    private bool tethered;
    private int casts;

    public DynasticFlame() : base(0, 10, alwaysShowSpreads: true) { }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        var player = module.Raid.Player();
        if ((TetherID)tether.ID == TetherID.fireorbs && player != null)
        {
            AddSpread(player);
            tethered = true;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DynasticFlame1 or AID.DynasticFlame2)
            casts++;
        if (casts == 4)
        {
            casts = 0;
            Spreads.Clear();
            tethered = false;
        }
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var player = module.Raid.Player();
        if(player == actor && tethered)
            hints.AddForbiddenZone(ShapeDistance.Circle(module.Bounds.Center, 18));
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (tethered)
            hints.Add("Go to the edge and run until 4 orbs are spawned");
    }
}

class SkyrendingStrike : CastHint
{
    private bool casting;
    private DateTime enragestart;

    public SkyrendingStrike() : base(ActionID.MakeSpell(AID.SkyrendingStrike), "") { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SkyrendingStrike)
        {
            casting = true;
            enragestart = module.WorldState.CurrentTime;
        }
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (casting && module.PrimaryActor.IsTargetable)
            hints.Add($"Enrage! {Math.Max(35 - (module.WorldState.CurrentTime - enragestart).TotalSeconds, 0.0f):f1}s left.");
    }
}
