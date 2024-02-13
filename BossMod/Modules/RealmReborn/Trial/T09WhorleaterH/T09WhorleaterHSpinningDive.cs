using BossMod.Components;
using System.Linq;
using System.Collections.Generic;

namespace BossMod.Modules.RealmReborn.Trial.T09WhorleaterH
{
    class SpinningDive : GenericAOEs //TODO: Find out how to detect spinning dives earlier eg. the water column telegraph
    {
        private Actor? SpinningDiveHelper;
        private bool dived;
        private readonly AOEShapeRect rect = new(46, 8);
        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            SpinningDiveHelper = module.Enemies(OID.SpinningDiveHelper).FirstOrDefault();
            if (SpinningDiveHelper != null && !dived)
                yield return new(rect, SpinningDiveHelper.Position, SpinningDiveHelper.Rotation);
        }
        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.SpinningDiveHelper)
                dived = false;
        }
        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.SpinningDiveSnapshot)
                dived = true;
        }
    }

    class SpinningDiveKB : Knockback //TODO: Find out how to detect spinning dives earlier eg. the water column telegraph
    {
        private Actor? SpinningDiveHelper;
        private bool dived;
        private readonly AOEShapeRect rect = new(46, 8);
        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            SpinningDiveHelper = module.Enemies(OID.SpinningDiveHelper).FirstOrDefault();
            if (SpinningDiveHelper != null && !dived)
                yield return new(SpinningDiveHelper.Position, 10, default, rect, SpinningDiveHelper.Rotation, Kind.AwayFromOrigin);
        }
        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.SpinningDiveHelper)
                dived = false;
        }
        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.SpinningDiveEffect)
                dived = true;
        }
        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints) { }
    }
}
