﻿using System.Linq;

namespace BossMod.RealmReborn.Raid.T02MultiADS
{
    public enum OID : uint
    {
        ADS = 0x7D9, // R2.300, x1
        QuarantineNode = 0x7DA, // R1.150, x1 + extra spawned for consumption
        AttackNode = 0x7DB, // R1.150, x1 + extra spawned for consumption
        SanitaryNode = 0x7DC, // R1.150, x1 + extra spawned for consumption
        MonitoringNode = 0x7DD, // R1.150, x1
        DefenseNode = 0x7DE, // R1.150, x1 + extra spawned for consumption
        DisposalNode = 0x7DF, // R1.150, x1 + extra spawned for consumption

        MonitoringNodeVacuumWaveHelper = 0x5A2, // R0.500, spawns during fight
        QuarantineNodeAllaganRotHelper = 0x8DE, // R0.500, x1
        SanitaryNodeBallastHelper = 0x8DF, // R0.500, x3
        DefenseNodeChainLightingHelper = 0x8E0, // R0.500, x1
        DisposalNodeFirestreamHelper = 0x8E1, // R0.500, x5
        ADSHelper = 0x8E2, // R0.500, x10
        ADSVacuumWaveHelper = 0x8E3, // R0.500, x1
        GravityField = 0x1E8728, // EventObj type, spawns during fight
    };

    public enum AID : uint
    {
        AutoAttack = 1215, // ADS/QuarantineNode/AttackNode/SanitaryNode/MonitoringNode/DefenseNode/DisposalNode->player, no cast, range 6+R 120-degree cone
        CleaveADS = 1412, // ADS->player, no cast, range 6+R ?-degree cone cleave, applies vuln up
        CleaveNode = 1415, // QuarantineNode/AttackNode/SanitaryNode/MonitoringNode/DefenseNode/DisposalNode->player, no cast, range 6+R 120-degree cone cleave, applies vuln up
        HighVoltage = 1216, // ADS/QuarantineNode/AttackNode/SanitaryNode/MonitoringNode/DefenseNode/DisposalNode->self, 3.0s cast (interruptible), raidwide
        RepellingCannons = 1217, // ADS/QuarantineNode/AttackNode/SanitaryNode/MonitoringNode/DefenseNode/DisposalNode->self, 2.5s cast, range 6+R circle aoe
        PiercingLaser = 1227, // ADS->self, 2.2s cast, range 30+R width 6 rect aoe

        VacuumWave = 1224, // MonitoringNodeVacuumWaveHelper/ADSVacuumWaveHelper->self, no cast, raidwide
        ChainLightning = 1225, // DefenseNode/ADS->self, 1.5s cast, single-target, visual
        ChainLightningAOE = 1315, // DefenseNodeChainLightingHelper/ADSHelper->player, no cast, single-target, ???
        Firestream = 1226, // DisposalNode/ADS->self, 0.5s cast, range 2 circle aoe
        FirestreamAOE = 675, // DisposalNodeFirestreamHelper/ADSHelper->self, 2.3s cast, range 35+R width 6 rect aoe (x5)
        Ballast = 1221, // SanitaryNode/ADS->self, 1.0s cast, range 5+R 270-degree cone, visual
        BallastAOE1 = 1343, // SanitaryNodeBallastHelper/ADSHelper->self, 2.0s cast, range 5+R 270-degree cone aoe
        BallastAOE2 = 1222, // SanitaryNodeBallastHelper/ADSHelper->self, 2.0s cast, range 5+R-10+R 270-degree donut cone aoe
        BallastAOE3 = 1223, // SanitaryNodeBallastHelper/ADSHelper->self, 2.0s cast, range 10+R-15+R 270-degree donut cone aoe
        GravityField = 1220, // AttackNode/ADS->location, 1.0s cast, range 6 circle, spawns voidzone
        AllaganRot = 1218, // QuarantineNode/ADS->player, 3.0s cast, single-target, applies debuff
        AllaganRotAOE = 1219, // QuarantineNodeAllaganRotHelper/ADSHelper->player, no cast, raidwide, explosion if debuff ticks down

        NodeRetrieval = 1228, // ADS->QuarantineNode/AttackNode/SanitaryNode/DefenseNode/DisposalNode, no cast, single-target, visual
        Object199 = 1229, // ADS->self, no cast, enrage
    };

    public enum SID : uint
    {
        VulnerabilityUp = 202, // ADS/QuarantineNode/AttackNode/SanitaryNode/MonitoringNode/DefenseNode/DisposalNode->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8
        AllaganRot = 333, // QuarantineNode/ADS->player, extra=0x0
        AllaganImmunity = 334, // none->player, extra=0x0
    };

    class CleaveCommon : Components.Cleave
    {
        public CleaveCommon(AID aid, float hitboxRadius) : base(ActionID.MakeSpell(aid), new AOEShapeCone(6 + hitboxRadius, 60.Degrees()), activeWhileCasting: false) { }
    }
    class CleaveADS : CleaveCommon { public CleaveADS() : base(AID.CleaveADS, 2.3f) { } }
    class CleaveNode : CleaveCommon { public CleaveNode() : base(AID.CleaveNode, 1.15f) { } }

    class HighVoltage : Components.CastHint
    {
        public HighVoltage() : base(ActionID.MakeSpell(AID.HighVoltage), "Interruptible") { }
    }

    class RepellingCannons : Components.SelfTargetedAOEs
    {
        public RepellingCannons() : base(ActionID.MakeSpell(AID.RepellingCannons), new AOEShapeCircle(8.3f)) { }
    }

    class PiercingLaser : Components.SelfTargetedAOEs
    {
        public PiercingLaser() : base(ActionID.MakeSpell(AID.PiercingLaser), new AOEShapeRect(32.3f, 3)) { }
    }

    // TODO: chain lightning?..

    class Firestream : Components.SelfTargetedAOEs
    {
        public Firestream() : base(ActionID.MakeSpell(AID.FirestreamAOE), new AOEShapeRect(35.5f, 3)) { }
    }

    class Ballast1 : Components.SelfTargetedAOEs
    {
        public Ballast1() : base(ActionID.MakeSpell(AID.BallastAOE1), new AOEShapeCone(5.5f, 135.Degrees())) { }
    }

    class Ballast2 : Components.SelfTargetedAOEs
    {
        public Ballast2() : base(ActionID.MakeSpell(AID.BallastAOE2), new AOEShapeDonutSector(5.5f, 10.5f, 135.Degrees())) { }
    }

    class Ballast3 : Components.SelfTargetedAOEs
    {
        public Ballast3() : base(ActionID.MakeSpell(AID.BallastAOE3), new AOEShapeDonutSector(10.5f, 15.5f, 135.Degrees())) { }
    }

    class GravityField : Components.PersistentVoidzoneAtCastTarget
    {
        public GravityField() : base(6, ActionID.MakeSpell(AID.GravityField), m => m.Enemies(OID.GravityField), 1) { }
    }

    class T02AI : BossComponent
    {
        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            foreach (var e in hints.PotentialTargets)
            {
                if (e.Actor == module.PrimaryActor)
                {
                    int targetVulnStacks = module.WorldState.Actors.Find(e.Actor.TargetID)?.FindStatus(SID.VulnerabilityUp)?.Extra ?? 0;
                    e.AttackStrength = 0.2f + 0.05f * targetVulnStacks;
                    e.Priority = 1;
                    e.ShouldBeInterrupted = true; // interrupt every high voltage; TODO consider interrupt rotation
                    if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT)
                    {
                        if (e.Actor.TargetID == actor.InstanceID)
                        {
                            // player is tanking - continue doing so until OT taunts
                            e.ShouldBeTanked = true;
                            e.PreferProvoking = false;
                        }
                        else
                        {
                            // player is not tanking - taunt off when current tank has >=4 vuln stacks and self has no stacks
                            e.ShouldBeTanked = e.PreferProvoking = targetVulnStacks >= 4 && actor.FindStatus(SID.VulnerabilityUp) == null;
                        }
                    }
                }
            }
        }
    }

    class T02ADSStates : StateMachineBuilder
    {
        public T02ADSStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<CleaveADS>()
                .ActivateOnEnter<HighVoltage>()
                .ActivateOnEnter<RepellingCannons>()
                .ActivateOnEnter<PiercingLaser>()
                .ActivateOnEnter<Firestream>()
                .ActivateOnEnter<Ballast1>()
                .ActivateOnEnter<Ballast2>()
                .ActivateOnEnter<Ballast3>()
                .ActivateOnEnter<GravityField>()
                .ActivateOnEnter<AllaganRot>()
                .ActivateOnEnter<T02AI>();
        }
    }

    [ConfigDisplay(Order = 0x120, Parent = typeof(RealmRebornConfig))]
    public class T02ADSConfig : CooldownPlanningConfigNode
    {
        public T02ADSConfig() : base(50) { }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.ADS, CFCID = 94, NameID = 1459)]
    public class T02ADS : BossModule
    {
        public T02ADS(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(0, 77), 18, 13)) { }
    }

    class T02QuarantineNodeStates : StateMachineBuilder
    {
        public T02QuarantineNodeStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<CleaveNode>()
                .ActivateOnEnter<HighVoltage>()
                .ActivateOnEnter<RepellingCannons>()
                .ActivateOnEnter<AllaganRot>()
                .ActivateOnEnter<T02AI>();
        }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.QuarantineNode, CFCID = 94, NameID = 1468)]
    public class T02QuarantineNode : BossModule
    {
        public T02QuarantineNode(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(0, 112), 14, 13)) { }
        protected override bool CheckPull() => base.CheckPull() && !Enemies(OID.ADS).Any(e => e.InCombat); // don't start modules for temporary node actors spawned during main boss fight
    }

    class T02AttackNodeStates : StateMachineBuilder
    {
        public T02AttackNodeStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<CleaveNode>()
                .ActivateOnEnter<HighVoltage>()
                .ActivateOnEnter<RepellingCannons>()
                .ActivateOnEnter<GravityField>()
                .ActivateOnEnter<T02AI>();
        }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.AttackNode, CFCID = 94, NameID = 1469)]
    public class T02AttackNode : BossModule
    {
        public T02AttackNode(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-44, 94), 17)) { }
        protected override bool CheckPull() => base.CheckPull() && !Enemies(OID.ADS).Any(e => e.InCombat); // don't start modules for temporary node actors spawned during main boss fight
    }

    class T02SanitaryNodeStates : StateMachineBuilder
    {
        public T02SanitaryNodeStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<CleaveNode>()
                .ActivateOnEnter<HighVoltage>()
                .ActivateOnEnter<RepellingCannons>()
                .ActivateOnEnter<Ballast1>()
                .ActivateOnEnter<Ballast2>()
                .ActivateOnEnter<Ballast3>()
                .ActivateOnEnter<T02AI>();
        }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.SanitaryNode, CFCID = 94, NameID = 1470)]
    public class T02SanitaryNode : BossModule
    {
        public T02SanitaryNode(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(-43, 52), 18, 15)) { }
        protected override bool CheckPull() => base.CheckPull() && !Enemies(OID.ADS).Any(e => e.InCombat); // don't start modules for temporary node actors spawned during main boss fight
    }

    class T02MonitoringNodeStates : StateMachineBuilder
    {
        public T02MonitoringNodeStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<CleaveNode>()
                .ActivateOnEnter<HighVoltage>()
                .ActivateOnEnter<RepellingCannons>()
                .ActivateOnEnter<T02AI>();
        }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.MonitoringNode, CFCID = 94, NameID = 1471)]
    public class T02MonitoringNode : BossModule
    {
        public T02MonitoringNode(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(0, 39), 17, 15)) { }
        protected override bool CheckPull() => base.CheckPull() && !Enemies(OID.ADS).Any(e => e.InCombat); // don't start modules for temporary node actors spawned during main boss fight
    }

    class T02DefenseNodeStates : StateMachineBuilder
    {
        public T02DefenseNodeStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<CleaveNode>()
                .ActivateOnEnter<HighVoltage>()
                .ActivateOnEnter<RepellingCannons>()
                .ActivateOnEnter<T02AI>();
        }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.DefenseNode, CFCID = 94, NameID = 1472)]
    public class T02DefenseNode : BossModule
    {
        public T02DefenseNode(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(46, 52), 17, 14)) { }
        protected override bool CheckPull() => base.CheckPull() && !Enemies(OID.ADS).Any(e => e.InCombat); // don't start modules for temporary node actors spawned during main boss fight
    }

    class T02DisposalNodeStates : StateMachineBuilder
    {
        public T02DisposalNodeStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<CleaveNode>()
                .ActivateOnEnter<HighVoltage>()
                .ActivateOnEnter<RepellingCannons>()
                .ActivateOnEnter<Firestream>()
                .ActivateOnEnter<T02AI>();
        }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.DisposalNode, CFCID = 94, NameID = 1473)]
    public class T02DisposalNode : BossModule
    {
        public T02DisposalNode(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(41, 94), 14, 20)) { }
        protected override bool CheckPull() => base.CheckPull() && !Enemies(OID.ADS).Any(e => e.InCombat); // don't start modules for temporary node actors spawned during main boss fight
    }
}
