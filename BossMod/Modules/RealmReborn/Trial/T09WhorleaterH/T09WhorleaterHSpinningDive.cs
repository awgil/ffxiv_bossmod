using BossMod.Components;
using System.Linq;
using System.Collections.Generic;
using System;

namespace BossMod.Modules.RealmReborn.Trial.T09WhorleaterH
{
    class SpinningDive : GenericAOEs //TODO: Find out how to detect spinning dives earlier eg. the water column telegraph
    {
        private DateTime _activation;
        private Actor? SpinningDiveHelper;
        private bool dived;
        private static readonly AOEShapeRect rect = new(46, 8);
        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            SpinningDiveHelper = module.Enemies(OID.SpinningDiveHelper).FirstOrDefault();
            if (SpinningDiveHelper != null && !dived)
                yield return new(rect, SpinningDiveHelper.Position, SpinningDiveHelper.Rotation, _activation);
        }
        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.SpinningDiveHelper)
            {
                dived = false;
                _activation = module.WorldState.CurrentTime.AddSeconds(0.6f);
            }
        }
        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.SpinningDiveSnapshot)
                dived = true;
        }
    }

    class SpinningDiveKB : Knockback //TODO: Find out how to detect spinning dives earlier eg. the water column telegraph
    {
        private DateTime _activation;
        private Actor? SpinningDiveHelper;
        private bool dived;
        private static readonly AOEShapeRect rect = new(46, 8);
        public SpinningDiveKB()
        {
            StopAtWall = true;
        }
        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            SpinningDiveHelper = module.Enemies(OID.SpinningDiveHelper).FirstOrDefault();
            if (SpinningDiveHelper != null && !dived)
                yield return new(SpinningDiveHelper.Position, 10, _activation, rect, SpinningDiveHelper.Rotation);
        }
        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.SpinningDiveHelper)
            {
                dived = false;
                _activation = module.WorldState.CurrentTime.AddSeconds(1.4f);
            }
        }
        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.SpinningDiveEffect)
                dived = true;
        }
        public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => (module.FindComponent<Hydroshot>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || (module.FindComponent<Dreadstorm>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false);
    }
}
