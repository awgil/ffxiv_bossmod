namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

class SpiralScourge(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.SpiralScourge), "Use Manawall, Excellence, or Invuln.");
class CallousCrossfire(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.CallousCrossfire), "Use Light Curtain / Reflect.");

class Reflect(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var reflectLeft = actor.FindStatus(2337) is ActorStatus st ? (st.ExpireAt - WorldState.CurrentTime).TotalSeconds : 0;

        var mechanicActive = Module.Enemies(OID.MagitekTurret).Any(e => !e.IsDead);

        var soonestRefresh = WorldState.Client.Cooldowns[ActionDefinitions.GCDGroup].Total + WorldState.Client.Cooldowns[ActionDefinitions.GCDGroup].Remaining + 0.94f;

        if (Module.Enemies(OID.MagitekTurret).Any(e => !e.IsDead) && reflectLeft < soonestRefresh)
            hints.ActionsToExecute.Push(ActionID.MakeBozjaHolster(BozjaHolsterID.LightCurtain, 0), null, ActionQueue.Priority.VeryHigh);
    }
}

class ReactiveMunition(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.AccelerationBomb)
        {
            if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
                PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.AccelerationBomb)
            ClearState(Raid.FindSlot(actor.InstanceID));
    }
}

class SenseWeakness(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SenseWeakness)
        {
            if (Raid.FindSlot(caster.TargetID) is var slot && slot >= 0)
                PlayerStates[slot] = new(Requirement.Move, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SenseWeakness)
            ClearState(Raid.FindSlot(caster.TargetID));
    }
}

class MagitekImpetus(BossModule module) : Components.StatusDrivenForcedMarch(module, 3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, activationLimit: 1);
class ProactiveMunition(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(6), ActionID.MakeSpell(AID.ProactiveMunitionTrackingStart), ActionID.MakeSpell(AID.ProactiveMunitionTrackingMove), 6, 1, 5);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "SourP", GroupType = BossModuleInfo.GroupType.BozjaDuel, GroupID = 778, NameID = 23)] // bnpcname=9695
public class Duel5Menenius(WorldState ws, Actor primary) : BossModule(ws, primary, new(-810, 520 /*y=260.3*/), new ArenaBoundsSquare(20));
