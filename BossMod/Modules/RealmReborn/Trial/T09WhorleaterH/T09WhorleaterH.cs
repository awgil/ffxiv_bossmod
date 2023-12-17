using BossMod.Endwalker.Extreme.Ex7Zeromus;
using BossMod.ReplayVisualization;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Modules.RealmReborn.Trial.T09WhorleaterH
{
    public enum OID : uint
    {
        Boss = 0xA67, // Leviathan's Head
        Tail = 0xA86, // Tail
        Spume = 0xA85, // Adds for the converter
        Sahagin = 0xA84, // Hostile adds
        DangerousSahagins = 0xA83,
        Converter = 0x1E922A, // Elemental converter
        HydroshotZone = 0x1E9230 // Hydroshot AoE puddle
    };

    public enum AID : uint
    {
        AutoAttack = 870,
        GrandFall = 1873, // Big AoE, normally during elemental converter activation
        Splash = 1871,
        BodySlamStart = 1938,
        BodySlam = 1860,
        Hydroshot = 1864
    }

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            var converter = module.Enemies(OID.Converter).Where(x => x.IsTargetable).FirstOrDefault();
            if (converter != null)
            {
                hints.Add($"Activate the {converter.Name} or wipe!");
            }

            if (module.Enemies(OID.DangerousSahagins).Any(x => x.IsTargetable && !x.IsDead))
            {
                hints.Add($"Kill Sahagins or lose control!");
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var tail = module.Enemies(OID.Tail).Where(x => x.IsTargetable && x.FindStatus(775) == null && x.FindStatus(477) != null).FirstOrDefault();
            if (tail != null)
            {
                if (actor.Class.GetClassCategory() is ClassCategory.Caster or ClassCategory.Healer)
                {
                    hints.Add("Attack the head! (Attacking the tail will reflect damage onto you)");
                }
                if (actor.Class.GetClassCategory() is ClassCategory.PhysRanged)
                {
                    hints.Add("Attack the tail! (Attacking the head will reflect damage onto you)");
                }
            }
        }
    }
    class GrandFall : Components.LocationTargetedAOEs
    {
        public GrandFall() : base(ActionID.MakeSpell(AID.GrandFall), 8) { }
    }
    class Hydroshot : Components.PersistentVoidzoneAtCastTarget
    {
        public Hydroshot() : base(5, ActionID.MakeSpell(AID.Hydroshot), m => m.Enemies(OID.HydroshotZone), 0) { }
    }

    class BodySlam : Components.Knockback
    {
        public float Distance;
        public Angle Direction;
        public DateTime Activation;

        public float LeviathanZ;

        public BodySlam() : base(ActionID.MakeSpell(AID.BodySlamStart)) { }

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            if (Distance > 0)
                yield return new(new(), Distance, Activation, null, Direction, Kind.DirForward);
        }

        public override void Update(BossModule module)
        {
            base.Update(module);
            var boss = module.Enemies(OID.Boss).FirstOrDefault();

            if (boss != null)
            {
                if (LeviathanZ == default)
                    LeviathanZ = module.Enemies(OID.Boss).First().Position.Z;

                if (boss.Position.Z != LeviathanZ && boss.Position.Z != 0)
                {
                    LeviathanZ = boss.Position.Z;
                    Distance = 25;
                    Direction = boss.Position.Z <= 0 ? 180.Degrees() : 0.Degrees();
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if ((AID)spell.Action.ID == AID.BodySlamStart)
            {
                Distance = 0;
            }
        }
    }
    class T09WhorleaterHStates : StateMachineBuilder
    {
        public T09WhorleaterHStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<GrandFall>()
                .ActivateOnEnter<Hydroshot>()
                .ActivateOnEnter<BodySlam>()
                .ActivateOnEnter<Hints>();
        }
    }

    public class T09WhorleaterH : BossModule
    {
        private List<Actor> _spumes;
        public T09WhorleaterH(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(-0, 0), 14.5f, 20)) 
        {
            _spumes = Enemies(OID.Spume);
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
            foreach (var s in _spumes)
                Arena.Actor(s, ArenaColor.PlayerInteresting, false);
            foreach (var e in Enemies(OID.Tail))
                Arena.Actor(e, ArenaColor.Enemy, false);
            foreach (var e in Enemies(OID.Sahagin))
                Arena.Actor(e, ArenaColor.Enemy, false);
            foreach (var e in Enemies(OID.DangerousSahagins))
                Arena.Actor(e, ArenaColor.Enemy, false);
            foreach (var c in Enemies(OID.Converter))
                Arena.Actor(c, ArenaColor.Object, false);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.Spume => 3,
                    OID.Sahagin => 2,
                    OID.Boss or OID.Tail => 1,
                    _ => 0
                };
            }
        }
    }
}