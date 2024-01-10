using BossMod.Components;
using System.Linq;
using System.Collections.Generic;

namespace BossMod.Modules.RealmReborn.Trial.T09WhorleaterH;
   class SpinningDive : GenericAOEs //TODO: Find out how to detect spinning dives earlier eg. the water column telegraph
    {     
        private float activeHelper;       
        private static AOEShapeRect rect = new(46, 8);
        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
                var helper = module.Enemies(OID.SpinningDiveHelper).FirstOrDefault();      
                 if (helper != null && activeHelper >=1)
            yield return new(rect, helper.Position, helper.Rotation, new());
            }
        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.SpinningDiveHelper)
             activeHelper =1;
        }
        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if ((AID)spell.Action.ID == AID.SpinningDiveSnapshot)
            {
                activeHelper = 0;
            }
        }
    }

        class SpinningDiveKB : Knockback //TODO: Find out how to detect spinning dives earlier eg. the water column telegraph
    {        
        private float activeHelper;       
         public SpinningDiveKB() : base(ActionID.MakeSpell(AID.SpinningDiveEffect)) { }
        private static AOEShapeRect rect = new(46, 8);
        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
                var helper = module.Enemies(OID.SpinningDiveHelper).FirstOrDefault();  
                if (helper != null && activeHelper >=1)
                yield return new(helper.Position, 10, default, rect, helper.Rotation, Kind.AwayFromOrigin);
        }

        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.SpinningDiveHelper)
             activeHelper = 1;
        }
        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if ((AID)spell.Action.ID == AID.SpinningDiveEffect)
            {
                activeHelper = 0;
            }
        }
        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints){}
    }