namespace BossMod.Dawntrail.Savage.RM07SBruteAbominator;

class P1BloomingAbomination(BossModule module) : Components.Adds(module, (uint)OID.BloomingAbomination)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var mob in ActiveActors)
        {
            if (hints.FindEnemy(mob) is { } e)
            {
                e.ForbidDOTs = true;
                e.Priority = 1;
                e.ShouldBeInterrupted = mob.CastInfo?.Action.ID == (uint)AID.WindingWildwinds;
            }
        }
    }
}

class P3BloomingAbomination(BossModule module) : Components.AddsPointless(module, (uint)OID.BloomingAbomination);

class CrossingCrosswinds(BossModule module) : Components.StandardAOEs(module, AID.CrossingCrosswinds, new AOEShapeCross(50, 5));
class WindingWildwinds : Components.StandardAOEs
{
    public WindingWildwinds(BossModule module) : base(module, AID.WindingWildwinds, new AOEShapeDonut(5, 60))
    {
        Risky = false;
    }
}

class HurricaneForce(BossModule module) : Components.CastHint(module, AID.HurricaneForce, "Plant enrage!", true);

class QuarrySwamp(BossModule module) : Components.GenericLineOfSightAOE(module, AID.QuarrySwamp, 60, false)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Modify(caster.Position, Module.Enemies(OID.BloomingAbomination).Select(b => (b.Position, b.HitboxRadius)), Module.CastFinishAt(spell));
    }

    public override void Update()
    {
        if (Origin != null)
            Modify(Origin, Module.Enemies(OID.BloomingAbomination).Select(b => (b.Position, b.HitboxRadius)), NextExplosion);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Modify(null, []);
    }
}
