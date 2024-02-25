// CONTRIB: made by taurenkey, changed by malediktus, not checked
using BossMod.Components;

namespace BossMod.Modules.RealmReborn.Trial.T09WhorleaterH
{
    class GrandFall : LocationTargetedAOEs
    {
        public GrandFall() : base(ActionID.MakeSpell(AID.GrandFall), 8) { }
    }

    class Hydroshot : PersistentVoidzoneAtCastTarget
    {
        public Hydroshot() : base(5, ActionID.MakeSpell(AID.Hydroshot), m => m.Enemies(OID.HydroshotZone), 0) { }
    }
    class Dreadstorm : PersistentVoidzoneAtCastTarget
    {
        public Dreadstorm() : base(5, ActionID.MakeSpell(AID.Dreadstorm), m => m.Enemies(OID.DreadstormZone), 0) { }
    }

    class T09WhorleaterHStates : StateMachineBuilder
    {
        public T09WhorleaterHStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<GrandFall>()
                .ActivateOnEnter<Hydroshot>()
                .ActivateOnEnter<Dreadstorm>()
                .ActivateOnEnter<BodySlamKB>()
                .ActivateOnEnter<BodySlamAOE>()
                .ActivateOnEnter<SpinningDive>()
                .ActivateOnEnter<SpinningDiveKB>()
                .ActivateOnEnter<Hints>();
        }
    }

    [ModuleInfo(CFCID = 72, NameID = 2505)]
    public class T09WhorleaterH(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsRect(new(-0, 0), 14.5f, 20))
    {
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
            foreach (var s in Enemies(OID.Spume))
                Arena.Actor(s, ArenaColor.Vulnerable);
            foreach (var e in Enemies(OID.Tail))
                Arena.Actor(e, ArenaColor.Enemy);
            foreach (var e in Enemies(OID.Sahagin))
                Arena.Actor(e, ArenaColor.Enemy);
            foreach (var e in Enemies(OID.DangerousSahagins))
                Arena.Actor(e, ArenaColor.Enemy);
            foreach (var c in Enemies(OID.Converter))
                Arena.Actor(c, ArenaColor.Object);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            var TankMimikry = actor.FindStatus(2124); //Bluemage Tank Mimikry
            foreach (var e in hints.PotentialTargets)
            {
                if (actor.Class.GetClassCategory() is ClassCategory.Caster or ClassCategory.Healer || (actor.Class is Class.BLU && TankMimikry == null))
                {
                    e.Priority = (OID)e.Actor.OID switch
                    {
                        OID.DangerousSahagins => 4,
                        OID.Spume => 3,
                        OID.Sahagin => 2,
                        OID.Boss => 1,
                        _ => 0
                    };
                }
                if (actor.Class.GetClassCategory() is ClassCategory.PhysRanged)
                {
                    e.Priority = (OID)e.Actor.OID switch
                    {
                        OID.DangerousSahagins => 4,
                        OID.Spume => 3,
                        OID.Sahagin => 2,
                        OID.Tail => 1,
                        _ => 0
                    };
                }
                if (actor.Class.GetClassCategory() is ClassCategory.Tank or ClassCategory.Melee || (actor.Class is Class.BLU && TankMimikry != null))
                {
                    e.Priority = (OID)e.Actor.OID switch
                    {
                        OID.DangerousSahagins => 4,
                        OID.Spume => 3,
                        OID.Sahagin => 2,
                        OID.Boss or OID.Tail => 1,
                        _ => 0
                    };
                }
            }
        }
    }
}
