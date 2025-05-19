using Lumina.Extensions;

namespace BossMod.Shadowbringers.Alliance.A14Engels;

class GuidedMissileBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.Chase, centerAtTarget: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveBaitsOn(actor).FirstOrNull() is { } bait)
            hints.AddForbiddenZone(new AOEShapeRect(25, 25, 25), Arena.Center, activation: bait.Activation);
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GuidedMissileFirst)
            CurrentBaits.Clear();
    }
}
class GuidedMissile(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(6), AID.GuidedMissileFirst, AID.GuidedMissileRest, 6, 1, 4)
{
    private readonly List<(int, Actor)> _targets = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == ActionFirst)
        {
            var pos = spell.TargetID == caster.InstanceID ? caster.Position : WorldState.Actors.Find(spell.TargetID)?.Position ?? spell.LocXZ;
            var (slot, target) = _targets.ExcludedFromMask(ExcludedTargets).MinBy(ip => (ip.Item2.Position - pos).LengthSq());
            if (target != null)
            {
                Chasers.Add(new(Shape, target, pos, 0, MaxCasts, Module.CastFinishAt(spell), SecondsBetweenActivations)); // initial cast does not move anywhere
                ExcludedTargets.Set(slot);
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Chase)
            _targets.Add((Raid.FindSlot(actor.InstanceID), actor));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (spell.Action == ActionRest && Chasers.Count == 0)
        {
            ExcludedTargets.Reset();
            _targets.Clear();
        }
    }
}
