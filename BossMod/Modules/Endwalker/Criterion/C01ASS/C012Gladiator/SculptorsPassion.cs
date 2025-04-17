namespace BossMod.Endwalker.Criterion.C01ASS.C012Gladiator;

class SculptorsPassion(BossModule module, AID aid) : Components.GenericWildCharge(module, 4, aid)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.SculptorsPassionTargetSelection)
        {
            Source = Module.PrimaryActor;
            foreach (var (slot, player) in Raid.WithSlot(true))
                PlayerRoles[slot] = player.InstanceID == spell.MainTargetID ? PlayerRole.Target : player.Role == Role.Tank ? PlayerRole.Share : PlayerRole.ShareNotFirst;
        }
    }
}
class NSculptorsPassion(BossModule module) : SculptorsPassion(module, AID.NSculptorsPassion);
class SSculptorsPassion(BossModule module) : SculptorsPassion(module, AID.SSculptorsPassion);
