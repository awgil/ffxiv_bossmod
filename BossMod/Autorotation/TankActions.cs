using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    // extra utilities for tanks
    abstract class TankActions : CommonActions
    {
        protected bool IsOfftank { get; private set; }
        protected DateTime LastStanceSwap { get; private set; }

        protected TankActions(Autorotation autorot, Actor player, uint[] unlockData, Dictionary<ActionID, ActionDefinition> supportedActions)
            : base(autorot, player, unlockData, supportedActions)
        {
        }

        public override void Dispose()
        {
        }

        public override Targeting SelectBetterTarget(AIHints.Enemy initial)
        {
            var enemiesToTank = Autorot.Hints.PotentialTargets.Where(e => e.ShouldBeTanked);
            if (!enemiesToTank.Any())
                return new(initial); // there is no one to tank...

            // note: most enemies have 'autoattack range' of 2 (compared to player's 3), so staying at max melee can cause enemy movement
            // 1. select closest target that should be tanked but is currently not
            var closestNeedOveraggro = enemiesToTank.Where(e => e.Actor.TargetID != Player.InstanceID).MinBy(e => (e.Actor.Position - Player.Position).LengthSq());
            if (closestNeedOveraggro != null)
                return new(closestNeedOveraggro, closestNeedOveraggro.TankDistance, Positional.Front, true);

            // 2. if initial target is not to be tanked, select any that is to be
            if (!initial.ShouldBeTanked)
                return new(enemiesToTank.First(), enemiesToTank.First().TankDistance, Positional.Front, true);

            return new(initial, initial.TankDistance, Positional.Front, true);
        }

        protected override void UpdateInternalState(int autoAction)
        {
            var assignments = Service.Config.Get<PartyRolesConfig>();
            IsOfftank = assignments[Autorot.WorldState.Party.ContentIDs[PartyState.PlayerSlot]] == PartyRolesConfig.Assignment.OT && Autorot.WorldState.Party.WithoutSlot().Any(a => a != Player && a.Role == Role.Tank);
        }

        protected override void OnActionExecuted(ClientActionRequest request)
        {
            if (request.Action.Type == ActionType.Spell && request.Action.ID is (uint)WAR.AID.Defiance or (uint)PLD.AID.IronWill)
                LastStanceSwap = Autorot.WorldState.CurrentTime;
        }

        protected bool WantStance() => !IsOfftank || Autorot.Hints.PotentialTargets.Any(e => e.ShouldBeTanked);
        protected bool ShouldSwapStance() => (Autorot.WorldState.CurrentTime - LastStanceSwap).TotalSeconds > 0.5 && GetState().HaveTankStance != WantStance();
    }
}
