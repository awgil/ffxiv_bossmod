using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Unreal.Un2Sephirot
{
    class P3Earthshaker : Components.GenericAOEs
    {
        private BitMask _targets;

        public bool Active => _targets.Any() && NumCasts < 2;

        private static AOEShape _shape = new AOEShapeCone(60, 15.Degrees());

        public P3Earthshaker() : base(ActionID.MakeSpell(AID.EarthShakerAOE)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            var origin = module.Enemies(OID.BossP3).FirstOrDefault();
            if (origin == null)
                yield break;

            // TODO: timing...
            foreach (var target in module.Raid.WithSlot(true).IncludedInMask(_targets))
                yield return new(_shape, origin.Position, Angle.FromDirection(target.Item2.Position - origin.Position));
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _targets[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if ((IconID)iconID == IconID.Earthshaker)
                _targets.Set(module.Raid.FindSlot(actor.InstanceID));
        }
    }
}
