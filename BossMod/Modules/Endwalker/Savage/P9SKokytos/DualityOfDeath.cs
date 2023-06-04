using System.Linq;

namespace BossMod.Endwalker.Savage.P9SKokytos
{
    class DualityOfDeath : Components.GenericBaitAway
    {
        private ulong _firstFireTarget;

        private static AOEShapeCircle _shape = new(6);

        public DualityOfDeath() : base(ActionID.MakeSpell(AID.DualityOfDeathFire), centerAtTarget: true) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (ActiveBaitsOn(actor).Any())
            {
                if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _shape.Radius).Any())
                    hints.Add("GTFO from raid!");
                if (module.PrimaryActor.TargetID == _firstFireTarget)
                    hints.Add(actor.InstanceID != _firstFireTarget ? "Taunt!" : "Pass aggro!");
            }
            else if (ActiveBaits.Any(b => IsClippedBy(actor, b)))
            {
                hints.Add("GTFO from tanks!");
            }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.DualityOfDeath)
            {
                CurrentBaits.Add(new(module.PrimaryActor, actor, _shape));
                _firstFireTarget = module.PrimaryActor.TargetID;
            }
        }
    }
}
