//namespace BossMod;

//// extra utilities for tanks
//abstract class TankActions(AutorotationLegacy autorot, Actor player, uint[] unlockData) : CommonActions(autorot, player, unlockData)
//{
//    protected bool IsOfftank { get; private set; }

//    public override Targeting SelectBetterTarget(AIHints.Enemy initial)
//    {
//        // note: most enemies have 'autoattack range' of 2 (compared to player's 3), so staying at max melee can cause enemy movement
//        // 1. select closest target that should be tanked but is currently not
//        var closestNeedOveraggro = Autorot.Hints.PotentialTargets.Where(e => e.ShouldBeTanked && e.Actor.TargetID != Player.InstanceID).MinBy(e => (e.Actor.Position - Player.Position).LengthSq());
//        if (closestNeedOveraggro != null)
//            return new(closestNeedOveraggro, closestNeedOveraggro.TankDistance, Positional.Front, true);

//        // 2. if initial target is not to be tanked, select any that is to be
//        if (!initial.ShouldBeTanked && Autorot.Hints.PotentialTargets.FirstOrDefault(e => e.ShouldBeTanked) is var enemyToTank && enemyToTank != null)
//            return new(enemyToTank, enemyToTank.TankDistance, Positional.Front, true);

//        return new(initial, initial.TankDistance, Positional.Front, true);
//    }

//    protected override void UpdateInternalState(int autoAction)
//    {
//        var assignments = Service.Config.Get<PartyRolesConfig>();
//        IsOfftank = assignments[Autorot.WorldState.Party.ContentIDs[PartyState.PlayerSlot]] == PartyRolesConfig.Assignment.OT && Autorot.WorldState.Party.WithoutSlot().Any(a => a != Player && a.Role == Role.Tank);
//    }

//    protected bool WantStance() => !IsOfftank || Autorot.Hints.PotentialTargets.Any(e => e.ShouldBeTanked);
//}
