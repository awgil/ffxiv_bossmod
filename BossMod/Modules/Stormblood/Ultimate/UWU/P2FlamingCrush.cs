using System.Linq;

namespace BossMod.Stormblood.Ultimate.UWU
{
    class FlamingCrush : Components.StackSpread
    {
        public FlamingCrush() : base(4, 0, 6) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.FlamingCrush)
            {
                StackTargets.Add(actor);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.FlamingCrush)
            {
                StackTargets.Clear();
            }
        }
    }

    // during P2, everyone except searing wind targets (typically two healers) should stack
    class P2FlamingCrush : FlamingCrush
    {
        public override void Init(BossModule module)
        {
            if (module.FindComponent<P2SearingWind>() is var searingWind && searingWind != null)
                AvoidTargets.AddRange(searingWind.SpreadTargets);
        }
    }

    // during P4 (annihilation), everyone should stack (except maybe ranged/caster that will handle mesohigh)
    class P4FlamingCrush : FlamingCrush { }

    // during P5 (suppression), everyone except mesohigh handler (typically tank) should stack
    class P5FlamingCrush : FlamingCrush
    {
        public override void Init(BossModule module)
        {
            AvoidTargets.AddRange(module.Raid.WithoutSlot(true).Where(p => p.FindStatus(SID.ThermalLow) != null && p.Role != Role.Healer));
        }
    }
}
