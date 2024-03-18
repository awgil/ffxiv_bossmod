using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Modules.Endwalker.SoloDuty.Endwalker;
class TidalWave : Components.KnockbackFromCastTarget
{
    public TidalWave() : base(ActionID.MakeSpell(AID.TidalWaveVisual), 25, kind: Kind.DirForward) { }
}

class Megaflare : Components.LocationTargetedAOEs
{
    public Megaflare() : base(ActionID.MakeSpell(AID.Megaflare), 6) { }
}

class AkhMorn : Components.CastHint
{
    public AkhMorn() : base(ActionID.MakeSpell(AID.AkhMorn), "6x hitting tankbuster") { } // 8x in phase 2

    public bool casting;

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkhMorn)
            casting = true;
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkhMorn)
            casting = false;
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (casting)
            arena.AddCircle(new WPos(pc.Position.X, pc.Position.Z), 4, ArenaColor.Border);
    }
}

class JudgementBolt : Components.CastHint
{
    public JudgementBolt() : base(ActionID.MakeSpell(AID.JudgementBolt), "Raidwide. Avoid puddles.") { }

    public bool casting;

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(module, slot, actor, assignment, hints);
        foreach (var e in module.Enemies(OID.Puddles))
            hints.AddForbiddenZone(ShapeDistance.Circle(e.Position, 5));
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (casting && actor.Statuses.Any(s => s.ID == (uint)SID._Gen_LightningResistanceDownII))
            hints.Add("GTFO from puddles!");
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.JudgementBolt)
            casting = true;
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.JudgementBolt)
            casting = false;
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var e in module.Enemies(OID.Puddles))
            arena.AddCircle(new WPos(e.Position.X, e.Position.Z), 5, ArenaColor.Border);
        if (casting)
            foreach (var e in module.Enemies(OID.Puddles))
                arena.AddCircleFilled(new WPos(e.Position.X, e.Position.Z), 5, ArenaColor.Vulnerable);
    }
}

class Hellfire : Components.CastHint
{
    public Hellfire() : base(ActionID.MakeSpell(AID.Hellfire), "Raidwide. Stand in puddles for damage reduction.") { }

    public bool casting;

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(module, slot, actor, assignment, hints);
        var shapes = new List<Func<WPos, float>>();
        foreach (var e in module.Enemies(OID.Puddles))
            shapes.Add(ShapeDistance.InvertedCircle(e.Position, 5));

        hints.AddForbiddenZone(p => shapes.Select(f => f(p)).Max());
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (casting && actor.Statuses.Any(s => s.ID != (uint)SID._Gen_LightningResistanceDownII))
            hints.Add("Get in puddles for damage reduction!");
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Hellfire)
            casting = true;
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Hellfire)
            casting = false;
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var e in module.Enemies(OID.Puddles))
            arena.AddCircle(new WPos(e.Position.X, e.Position.Z), 5, ArenaColor.Border);
        if (casting)
            foreach (var e in module.Enemies(OID.Puddles))
                arena.AddCircleFilled(new WPos(e.Position.X, e.Position.Z), 5, ArenaColor.SafeFromAOE);
    }
}

class StarBeyondStars : Components.SelfTargetedAOEs
{
    public StarBeyondStars() : base(ActionID.MakeSpell(AID.StarBeyondStarsHelper), new AOEShapeCone(50, 15.Degrees()), 6) { }
}

class TheEdgeUnbound : Components.SelfTargetedAOEs
{
    public TheEdgeUnbound() : base(ActionID.MakeSpell(AID.TheEdgeUnbound), new AOEShapeCircle(10)) { }
}

class WyrmsTongue : Components.SelfTargetedAOEs
{
    public WyrmsTongue() : base(ActionID.MakeSpell(AID.WyrmsTongueHelper), new AOEShapeCone(40, 30.Degrees())) { }
}

class NineNightsAvatar : Components.SelfTargetedAOEs
{
    public NineNightsAvatar() : base(ActionID.MakeSpell(AID.NineNightsAvatar), new AOEShapeCircle(10)) { }
}

class NineNightsHelpers : Components.SelfTargetedAOEs
{
    public NineNightsHelpers() : base(ActionID.MakeSpell(AID.NineNightsHelpers), new AOEShapeCircle(10), 6) { }
}

class VeilAsunder : Components.LocationTargetedAOEs
{
    public VeilAsunder() : base(ActionID.MakeSpell(AID.VeilAsunderHelper), 6) { }
}

class Exaflare : Components.Exaflare
{
    public Exaflare() : base(6) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ExaflareFirstHit)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 8 * spell.Rotation.ToDirection(), NextExplosion = spell.NPCFinishAt, TimeToMove = 2.1f, ExplosionsLeft = 5, MaxShownExplosions = 2 });
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ExaflareFirstHit or AID.ExaflareRest)
        {
            ++NumCasts;
            int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            if (index == -1)
            {
                module.ReportError(this, $"Failed to find entry for {caster.InstanceID:X}");
                return;
            }

            AdvanceLine(module, Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}

class MortalCoil : Components.SelfTargetedAOEs
{
    public MortalCoil() : base(ActionID.MakeSpell(AID.MortalCoilVisual), new AOEShapeDonut(8, 20)) { }
}

class DiamondDust : Components.RaidwideCast
{
    public DiamondDust() : base(ActionID.MakeSpell(AID.DiamondDustVisual), "Raidwide. Turns floor to ice.") { }
}

class DeadGaze : Components.CastGaze
{
    public DeadGaze() : base(ActionID.MakeSpell(AID.DeadGaze)) { }
}

class TidalWave2 : Components.KnockbackFromCastTarget
{
    public TidalWave2() : base(ActionID.MakeSpell(AID.TidalWaveVisual2), 25, kind: Kind.DirForward) { }
}

class AetherialRayHint : Components.CastHint
{
    public AetherialRayHint() : base(ActionID.MakeSpell(AID.AetherialRay), "5x hitting tankbuster") { }
}

class AetherialRay : Components.SelfTargetedAOEs
{
    public AetherialRay() : base(ActionID.MakeSpell(AID.AetherialRayVisual), new AOEShapeRect(100, 5)) { }
}

class SilveredEdge : Components.SelfTargetedAOEs
{
    public SilveredEdge() : base(ActionID.MakeSpell(AID.SilveredEdge), new AOEShapeRect(40, 3)) { }
}

class SwiftAsShadow : Components.ChargeAOEs
{
    public SwiftAsShadow() : base(ActionID.MakeSpell(AID.SwiftAsShadow), 1) { }
}

class CandlewickPointBlank : Components.SelfTargetedAOEs
{
    public CandlewickPointBlank() : base(ActionID.MakeSpell(AID.CandlewickPointBlank), new AOEShapeCircle(10)) { }
}

class CandlewickDonut : Components.SelfTargetedAOEs
{
    public CandlewickDonut() : base(ActionID.MakeSpell(AID.CandlewickDonut), new AOEShapeDonut(10, 30)) { }
}

class Extinguishment : Components.SelfTargetedAOEs
{
    public Extinguishment() : base(ActionID.MakeSpell(AID.ExtinguishmentVisual), new AOEShapeDonut(10, 30)) { }
}

class TheEdgeUnbound2 : Components.SelfTargetedAOEs
{
    public TheEdgeUnbound2() : base(ActionID.MakeSpell(AID.TheEdgeUnbound2), new AOEShapeCircle(10)) { }
}

class UnmovingDvenadkatik : Components.SelfTargetedAOEs
{
    public UnmovingDvenadkatik() : base(ActionID.MakeSpell(AID.UnmovingDvenadkatikVisual), new AOEShapeCone(50, 15.Degrees()), 10) { }
}