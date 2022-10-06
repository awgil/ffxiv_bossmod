using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    // extra utilities for tanks
    abstract class TankActions : CommonActions
    {
        protected bool IsOfftank { get; private set; }
        protected bool HaveStance { get; private set; }
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
            // 1. select closest target that should be tanked but is currently not
            var enemiesToTank = Autorot.Hints.PotentialTargets.Where(ShouldBeTanked);
            var closestNeedOveraggro = enemiesToTank.Where(e => e.Actor.TargetID != Player.InstanceID).MinBy(e => (e.Actor.Position - Player.Position).LengthSq());
            if (closestNeedOveraggro != null)
                return new(closestNeedOveraggro);

            // 2. if initial target is not to be tanked, select any that is to be
            if (!ShouldBeTanked(initial))
                return new(enemiesToTank.FirstOrDefault() ?? initial);

            return new(initial);
        }

        protected override void UpdateInternalState(int autoAction)
        {
            var assignments = Service.Config.Get<PartyRolesConfig>();
            IsOfftank = assignments[Autorot.WorldState.Party.ContentIDs[PartyState.PlayerSlot]] == PartyRolesConfig.Assignment.OT && Autorot.WorldState.Party.WithoutSlot(true).Any(a => a != Player && a.Role == Role.Tank);
            HaveStance = CommonDefinitions.HasTankStance(Player);
        }

        protected override void OnActionExecuted(ActionID action, Actor? target)
        {
            if (action.Type == ActionType.Spell && action.ID is (uint)WAR.AID.Defiance or (uint)PLD.AID.IronWill)
                LastStanceSwap = Autorot.WorldState.CurrentTime;
        }

        protected bool ShouldBeTanked(AIHints.Enemy enemy) => enemy.TankAffinity == (IsOfftank ? AIHints.TankAffinity.OT : AIHints.TankAffinity.MT);
        protected bool WantStance() => !IsOfftank || Autorot.Hints.PotentialTargets.Any(ShouldBeTanked);
        protected bool ShouldSwapStance() => (Autorot.WorldState.CurrentTime - LastStanceSwap).TotalSeconds > 0.5 && HaveStance != WantStance();
    }
}
