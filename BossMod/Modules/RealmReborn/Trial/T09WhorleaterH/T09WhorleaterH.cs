// CONTRIB: made by taurenkey, not checked
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Modules.RealmReborn.Trial.T09WhorleaterH
{
    public enum OID : uint
    {
        Boss = 0xA67, // R4.500, x1
        Tail = 0xA86, // R4.500, spawn during fight
        Spume = 0xA85, // R1.000, spawn during fight
        Sahagin = 0xA84, // R1.500, spawn during fight
        DangerousSahagins = 0xA83, // R1.500, spawn during fight
        Converter = 0x1E922A, // R2.000, x1, EventObj type
        HydroshotZone = 0x1E9230, // R0.500, EventObj type, spawn during fight
        Actor1e922f = 0x1E922F, // R2.000, x1, EventObj type
        Leviathan_Unk = 0xA88, // R0.500, x14, 523 type
        Actor1e9229 = 0x1E9229, // R2.000, x1, EventObj type
        Actor1e8536 = 0x1E8536, // R2.000, x1, EventObj type
        Leviathan_Unk2 = 0xACD, // R0.500, x4
        Actor1e922b = 0x1E922B, // R2.000, x1, EventObj type
        ElementalConverter = 0xB84, // R0.500, x1
        Exit = 0x1E850B, // R0.500, x1, EventObj type
        Actor1e9228 = 0x1E9228, // R2.000, x1, EventObj type
        Leviathan_Unk3 = 0xA87, // R4.500, 523 type, spawn during fight

    };

    public enum AID : uint
    {
        BodySlam = 1860, // Boss->self, no cast, range 30+R width 10 rect
        BodySlamStart = 1938, // ACD->self, no cast, ???
        VeilOfTheWhorl = 2165, // Boss->self, no cast, single-target
        Attack = 1853, // Boss->player, no cast, single-target
        MantleOfTheWhorl = 2164, // Tail->self, no cast, single-target
        ScaleDarts = 1857, // Tail->player, no cast, single-target
        AutoAttack = 870, // Sahagin->player, no cast, single-target
        AetherDraw = 1870, // Spume->B84, no cast, single-target
        StunShot = 1862, // Sahagin->player, no cast, single-target
        DreadTide = 1877, // A88->location, no cast, range 2 circle
        DreadTide_Boss = 1876, // Boss->self, no cast, single-target
        AquaBreath = 1855, // Boss->self, no cast, range 10+R circle
        TailWhip = 1856, // Tail->self, no cast, range 10+R circle
        Waterspout = 1859, // A88->location, no cast, range 4 circle
        Waterspout_Boss = 1858, // Boss->self, no cast, single-target
        Hydroshot = 1864, // Sahagin->location, 2.5s cast, range 5 circle
        TidalRoar = 1868, // A88->self, no cast, range 60+R circle
        TidalRoar_Boss = 1867, // Boss->self, no cast, single-target
        SpinningDive1 = 1869, // A87->self, no cast, range 46+R width 16 rect
        SpinningDive2 = 1861, // A88->self, no cast, range 46+R width 16 rect
        Splash = 1871, // Spume->self, no cast, range 50+R circle
        GrandFall = 1873, // A88->location, 3.0s cast, range 8 circle
        TidalWave = 1872, // Boss->self, no cast, range 60+R circle
        Dash = 1937, // ACD->self, no cast, ???
    };

    public enum SID : uint
    {
        VeilOfTheWhorl = 478, // Boss->A88/Boss/A87, extra=0x64
        MantleOfTheWhorl = 477, // Tail->Tail, extra=0x64
        Paralysis = 17, // Sahagin->player, extra=0x0
        Invincibility = 775, // none->A88/Boss/Tail/A87, extra=0x0
        Dropsy = 272, // none->player, extra=0x0
        Heavy = 14, // A88->player, extra=0x19
    };




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

    class Dash : Components.SelfTargetedAOEs
    {
        public Dash() : base(ActionID.MakeSpell(AID.Dash), new AOEShapeRect(20, 14)) { }
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
                .ActivateOnEnter<Dash>()
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