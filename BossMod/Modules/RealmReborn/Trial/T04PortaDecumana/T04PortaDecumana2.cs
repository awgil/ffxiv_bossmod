using System;
using System.Linq;

namespace BossMod.RealmReborn.Trial.T04PortaDecumana.Phase2
{
    public enum OID : uint
    {
        Boss = 0x3900, // x1
        Helper = 0x233C, // x10
        Aetheroplasm = 0x3902, // spawn during fight
        MagitekBit = 0x3901, // spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 29004, // Boss->player, no cast, single-target
        Teleport = 28628, // Boss->location, no cast, single-target
        TankPurge = 29022, // Boss->self, 5.0s cast, raidwide
        HomingLasers = 29023, // Boss->player, 5.0s cast, single-target, tankbuster

        MagitekRayForward = 29005, // Boss->self, no cast, single-target, visual
        MagitekRayRight = 29006, // Boss->self, no cast, single-target, visual
        MagitekRayLeft = 29007, // Boss->self, no cast, single-target, visual
        MagitekRayAOEForward = 29008, // Helper->self, 2.2s cast, range 40 width 6 rect aoe
        MagitekRayAOERight = 29009, // Helper->self, 2.2s cast, range 40 width 6 rect aoe
        MagitekRayAOELeft = 29010, // Helper->self, 2.2s cast, range 40 width 6 rect aoe

        HomingRay = 29011, // Boss->self, 4.0s cast, single-target, visual
        HomingRayAOE = 29012, // Helper->player, 5.0s cast, range 6 circle spread
        LaserFocus = 29013, // Boss->self, 4.0s cast, single-target, visual
        LaserFocusAOE = 29014, // Helper->player, 5.0s cast, range 6 circle stack

        AethericBoom = 29015, // Boss->self, 4.0s cast, knockback 30
        AetheroplasmSoak = 29016, // Aetheroplasm->self, no cast, range 8 circle aoe
        AetheroplasmCollide = 29017, // Aetheroplasm->self, no cast, raidwide

        BitTeleport = 29018, // MagitekBit->location, no cast, single-target
        AssaultCannon = 29019, // MagitekBit->self, 4.0s cast, range 40 width 4 rect

        CitadelBuster = 29020, // Boss->self, 5.0s cast, range 40 width 12 rect aoe
        Explosion = 29021, // Helper->self, 7.0s cast, raidwide with ? falloff

        LimitBreakRefill = 28542, // Helper->self, no cast, range 40 circle - probably limit break refill
        Ultima = 29024, // Boss->self, 71.0s cast, enrage
    };

    class AethericBoom : Components.KnockbackFromCastTarget
    {
        public AethericBoom() : base(ActionID.MakeSpell(AID.AethericBoom), 20) 
        { 
            StopAtWall = true;
        }
    }

    class OrbsHint : BossComponent
    {
        private bool casting;
        private bool orbsspawned;
        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.AethericBoom)
                casting = true;
        }
        
        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.Aetheroplasm)
            {
                orbsspawned = true;
                casting = false;
            }
        }
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (casting)
            hints.Add("Prepare to soak the orbs!");  
            if (orbsspawned && !module.Enemies(OID.Aetheroplasm).All(x => x.IsDead))
            hints.Add("Soak the orbs!");  
        }
        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var p in module.Enemies(OID.Aetheroplasm).Where(x => !x.IsDead))
                arena.AddCircle(p.Position, 1.4f, ArenaColor.Safe);
        }
    }
    class OrbsAI : BossComponent
    {
        private bool starting;
        private bool finished;
        private int soaks;
        private readonly float maxError = 20 * (MathF.PI / 180);

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.AethericBoom)
                starting = true;
        }
        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.AethericBoom)
                finished = true;
        }
        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.AethericBoom)
                starting = false;
            if ((AID)spell.Action.ID is AID.AetheroplasmCollide)
                finished = false;
            if ((AID)spell.Action.ID is AID.AetheroplasmSoak)
                ++soaks;
            if (soaks == 2)
                finished = false;
        }
        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (starting)
            {
                hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.ArmsLength), actor, 1, false));
                hints.PlannedActions.Add((ActionID.MakeSpell(WHM.AID.Surecast), actor, 1, false));
            }
            if (finished)
                hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.Sprint), actor, 1, false));
            if (module.Enemies(OID.Aetheroplasm).Where(x => !x.IsDead).LastOrDefault() != null)
            {
                var orb = module.Enemies(OID.Aetheroplasm).Where(x => !x.IsDead).LastOrDefault();
                var orbX = orb!.Position.X;
                var orbZ = orb!.Position.Z;
            
                if (orb.Rotation.AlmostEqual(-135.Degrees(), maxError))
                {
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(orbX - 0.5f, orbZ - 0.5f), 1.2f));
                }
                if (orb.Rotation.AlmostEqual(-45.Degrees(), maxError))
                {
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(orbX - 0.5f, orbZ + 0.5f), 1.2f));
                }
                if (orb.Rotation.AlmostEqual(45.Degrees(), maxError))
                {
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(orbX + 0.5f, orbZ + 0.5f), 1.2f));
                }
                if (orb.Rotation.AlmostEqual(135.Degrees(), maxError))
                {
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(orbX + 0.5f, orbZ - 0.5f), 1.2f));
                }
            }
        }
    }

    class TankPurge : Components.RaidwideCast
    {
        public TankPurge() : base(ActionID.MakeSpell(AID.TankPurge)) { }
    }

    class HomingLasers : Components.SingleTargetCast
    {
        public HomingLasers() : base(ActionID.MakeSpell(AID.HomingLasers)) { }
    }

    class MagitekRayF : Components.SelfTargetedAOEs
    {
        public MagitekRayF() : base(ActionID.MakeSpell(AID.MagitekRayAOEForward), new AOEShapeRect(40, 3)) { }
    }

    class MagitekRayR : Components.SelfTargetedAOEs
    {
        public MagitekRayR() : base(ActionID.MakeSpell(AID.MagitekRayAOERight), new AOEShapeRect(40, 3)) { }
    }

    class MagitekRayL : Components.SelfTargetedAOEs
    {
        public MagitekRayL() : base(ActionID.MakeSpell(AID.MagitekRayAOELeft), new AOEShapeRect(40, 3)) { }
    }

    class HomingRay : Components.SpreadFromCastTargets
    {
        public HomingRay() : base(ActionID.MakeSpell(AID.HomingRayAOE), 6) { }
    }

    class LaserFocus : Components.StackWithCastTargets
    {
        public LaserFocus() : base(ActionID.MakeSpell(AID.LaserFocusAOE), 6) { }
    }

    class AssaultCannon : Components.SelfTargetedAOEs
    {
        public AssaultCannon() : base(ActionID.MakeSpell(AID.AssaultCannon), new AOEShapeRect(40, 2)) { }
    }

    class CitadelBuster : Components.SelfTargetedAOEs
    {
        public CitadelBuster() : base(ActionID.MakeSpell(AID.CitadelBuster), new AOEShapeRect(40, 6)) { }
    }

    class Explosion : Components.SelfTargetedAOEs
    {
        public Explosion() : base(ActionID.MakeSpell(AID.Explosion), new AOEShapeCircle(16)) { } // TODO: verify falloff

        // there is an overlap with another mechanic which has to be resolved first
        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (module.FindComponent<AssaultCannon>()!.Casters.Count == 0)
                base.AddAIHints(module, slot, actor, assignment, hints);
        }
    }

    class Ultima : Components.EnrageCastHint
    {
        public Ultima() : base(ActionID.MakeSpell(AID.Ultima), 72) { }
    }

    class T04PortaDecumana2States : StateMachineBuilder
    {
        public T04PortaDecumana2States(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<TankPurge>()
                .ActivateOnEnter<HomingLasers>()
                .ActivateOnEnter<MagitekRayF>()
                .ActivateOnEnter<MagitekRayR>()
                .ActivateOnEnter<MagitekRayL>()
                .ActivateOnEnter<HomingRay>()
                .ActivateOnEnter<LaserFocus>()
                .ActivateOnEnter<AssaultCannon>()
                .ActivateOnEnter<CitadelBuster>()
                .ActivateOnEnter<OrbsAI>()
                .ActivateOnEnter<AethericBoom>()
                .ActivateOnEnter<OrbsHint>()
                .ActivateOnEnter<Explosion>()
                .ActivateOnEnter<Ultima>();
        }
    }

    [ModuleInfo(CFCID = 830, NameID = 2137)]
    public class T04PortaDecumana2 : BossModule
    {
        public T04PortaDecumana2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-704, 480), 20)) { }
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
            foreach (var s in Enemies(OID.Aetheroplasm).Where(x => !x.IsDead))
                Arena.Actor(s, ArenaColor.Object, true);
        }
    }
}
