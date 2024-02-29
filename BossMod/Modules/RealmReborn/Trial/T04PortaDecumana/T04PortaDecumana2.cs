﻿using System;
using System.Collections.Generic;
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

    class AethericBoom : Components.KnockbackFromCastTarget
    {
        public AethericBoom() : base(ActionID.MakeSpell(AID.AethericBoom), 30)
        {
            StopAtWall = true;
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (Casters.Count > 0)
                hints.Add("Prepare to soak the orbs!");
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (Casters.Count > 0)
            {
                hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.ArmsLength), actor, 1, false));
                hints.PlannedActions.Add((ActionID.MakeSpell(WHM.AID.Surecast), actor, 1, false));
            }
        }
    }

    class Aetheroplasm : BossComponent
    {
        private IReadOnlyList<Actor> _orbs = ActorEnumeration.EmptyList;

        public IEnumerable<Actor> ActiveOrbs => _orbs.Where(orb => !orb.IsDead);

        public override void Init(BossModule module)
        {
            _orbs = module.Enemies(OID.Aetheroplasm);
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (ActiveOrbs.Any())
                hints.Add("Soak the orbs!");
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            var orb = ActiveOrbs.FirstOrDefault();
            if (orb != null)
            {
                hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.Sprint), actor, 1, false));
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(orb.Position + 0.7f * orb.Rotation.ToDirection(), 1.2f));
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var orb in ActiveOrbs)
                arena.AddCircle(orb.Position, 1.4f, ArenaColor.Safe);
        }
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

    class Ultima : Components.CastHint
    {
        public Ultima() : base(ActionID.MakeSpell(AID.Ultima), "Enrage!", true) { }
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
                .ActivateOnEnter<AethericBoom>()
                .ActivateOnEnter<Aetheroplasm>()
                .ActivateOnEnter<AssaultCannon>()
                .ActivateOnEnter<CitadelBuster>()
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
