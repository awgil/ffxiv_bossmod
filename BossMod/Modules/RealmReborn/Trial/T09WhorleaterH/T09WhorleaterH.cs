using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

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
        DreadstormZone = 0x1E9231, // R0.500, EventObj type, spawn during fight
        Actor1e922f = 0x1E922F, // R2.000, x1, EventObj type
        AnotherSpinningDiveHelper = 0xA88, // R0.500, x14, 523 type
        Actor1e9229 = 0x1E9229, // R2.000, x1, EventObj type
        Actor1e8536 = 0x1E8536, // R2.000, x1, EventObj type
        Leviathan_Unk2 = 0xACD, // R0.500, x4
        Actor1e922b = 0x1E922B, // R2.000, x1, EventObj type
        ElementalConverter = 0xB84, // R0.500, x1
        Exit = 0x1E850B, // R0.500, x1, EventObj type
        Actor1e9228 = 0x1E9228, // R2.000, x1, EventObj type
        SpinningDiveHelper = 0xA87, // R4.500, 523 type, spawn during fight

    };

    public enum AID : uint
    {
        BodySlamRectAOE = 1860, // Boss->self, no cast, range 30+R width 10 rect
        BodySlamNorth = 1938, // ACD->self, no cast, ???
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
        SpinningDiveSnapshot = 1869, // A87->self, no cast, range 46+R width 16 rect
        SpinningDiveEffect = 1861, // A88->self, no cast, range 46+R width 16 rect
        Splash = 1871, // Spume->self, no cast, range 50+R circle
        GrandFall = 1873, // A88->location, 3.0s cast, range 8 circle
        TidalWave = 1872, // Boss->self, no cast, range 60+R circle
        BodySlamSouth = 1937, // ACD->self, no cast, ???
        Dreadstorm = 1865, // Wavetooth Sahagin --> location, 3.0s cast, range 50, 6 circle
        Ruin = 2214, // Sahagin-->player --> 1.0s cast time
    };

    public enum SID : uint
    {
        VeilOfTheWhorl = 478, // Boss->A88/Boss/A87, extra=0x64
        MantleOfTheWhorl = 477, // Tail->Tail, extra=0x64
        Paralysis = 17, // Sahagin->player, extra=0x0
        Invincibility = 775, // none->A88/Boss/Tail/A87, extra=0x0
        Dropsy = 272, // none->player, extra=0x0
        Heavy = 14, // A88->player, extra=0x19
        Hysteria = 296 // from dreadstorm AOE
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
                hints.Add("Kill Sahagins or lose control!");
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

    }
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
    class BodySlamKB : Knockback
    {
        public float Distance;
        public Angle Direction;

        public float LeviathanZ;

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            if (Distance > 0)
                yield return new(new(), Distance, default, null, Direction, Kind.DirForward);
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
            if ((AID)spell.Action.ID is AID.BodySlamNorth or AID.BodySlamSouth)
            {
                Distance = 0;
            }
        }
    }
        class BodySlamAOE : GenericAOEs
        {
        public float active;
        public Angle Direction;
        public float LeviathanZ;
        private static AOEShapeRect rect = new(30, 5);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (active > 0)
                 yield return new(rect, module.PrimaryActor.Position, Direction, new());
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
                    if ((boss.Position.Z+boss.Position.X)<=-1f && (boss.Position.Z+boss.Position.X)>=-2f)// Leviathan head slams SW
                    Direction = boss.Position.Z <= 0 ? 0.Degrees() : 90.Degrees(); 
                    active = 1;
                    if ((boss.Position.Z+boss.Position.X)<=28f && (boss.Position.Z+boss.Position.X)>=27f)// Leviathan head slams SE
                    Direction = boss.Position.Z <= 0 ? 0.Degrees() : 270.Degrees(); 
                    active = 1;
                    if ((boss.Position.Z+boss.Position.X)<=-27f && (boss.Position.Z+boss.Position.X)>=-28f)// Leviathan head slams NW
                    Direction = boss.Position.Z <= 0 ? 90.Degrees() : 0.Degrees(); 
                    active = 1;
                    if ((boss.Position.Z+boss.Position.X)<=2f && (boss.Position.Z+boss.Position.X)>=1f)// Leviathan head slams NE
                    Direction = boss.Position.Z <= 0 ? 270.Degrees() : 0.Degrees(); 
                    active = 1;
                }
            }
        }
        
        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if ((AID)spell.Action.ID == AID.BodySlamRectAOE)
            {
                active = 0;
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
                .ActivateOnEnter<Dreadstorm>()
                .ActivateOnEnter<BodySlamKB>()
                .ActivateOnEnter<BodySlamAOE>()
                .ActivateOnEnter<SpinningDive>()
                .ActivateOnEnter<SpinningDiveKB>()
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