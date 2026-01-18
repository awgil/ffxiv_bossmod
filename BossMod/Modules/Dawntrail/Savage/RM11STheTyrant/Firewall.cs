namespace BossMod.Dawntrail.Savage.RM11STheTyrant;

class Firewall(BossModule module) : Components.GenericWildCharge(module, 3, AID.GreatWallOfFireRect, 60)
{
    private readonly DateTime[] _fireVuln = new DateTime[8];
    private readonly DateTime[] _invuln = new DateTime[8];

    public override void Update()
    {
        Array.Fill(PlayerRoles, PlayerRole.Ignore);

        if (Activation == default)
            return;

        var targetSolo = Raid.TryFindSlot(Module.PrimaryActor.TargetID, out var bossTarget) && _invuln[bossTarget] > Activation;

        foreach (var (i, player) in Raid.WithSlot(true))
        {
            if (targetSolo)
            {
                PlayerRoles[i] = player.InstanceID == Module.PrimaryActor.TargetID ? PlayerRole.Target : PlayerRole.Avoid;
            }
            else
            {
                PlayerRoles[i] = player.InstanceID == Module.PrimaryActor.TargetID
                    ? (_fireVuln[i] > Activation ? PlayerRole.TargetNotFirst : PlayerRole.Target)
                    : player.Role == Role.Tank
                        ? (_fireVuln[i] > Activation ? PlayerRole.ShareNotFirst : PlayerRole.Share)
                        : PlayerRole.Avoid;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GreatWallOfFireCast)
        {
            Source = Module.PrimaryActor;
            Activation = Module.CastFinishAt(spell, 0.3f);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.FireResistanceDownII && Raid.TryFindSlot(actor, out var slot))
            _fireVuln[slot] = status.ExpireAt;
        if (Components.GenericSharedTankbuster.InvulnStatuses.Contains(status.ID) && Raid.TryFindSlot(actor, out var slot2))
            _invuln[slot2] = status.ExpireAt;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (NumCasts == 1)
                Activation = WorldState.FutureTime(3.2f);
            else
            {
                Activation = default;
                Source = null;
            }
        }
    }
}

class FirewallExplosion(BossModule module) : Components.StandardAOEs(module, AID.WallExplosion, new AOEShapeRect(60, 3));
