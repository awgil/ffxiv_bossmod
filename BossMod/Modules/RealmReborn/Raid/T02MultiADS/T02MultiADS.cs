using System;

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
        public CleaveCommon(AID aid, float hitboxRadius) : base(ActionID.MakeSpell(aid), new AOEShapeCone(6 + hitboxRadius, 60.Degrees())) { }
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
        public GravityField() : base(6, ActionID.MakeSpell(AID.GravityField), m => m.Enemies(OID.GravityField), 2, true) { }
    }

    // note: currently we assume that there is max 1 rot being passed around
    class AllaganRot : BossComponent
    {
        private DateTime[] _rotExpiration = new DateTime[PartyState.MaxPartySize];
        private DateTime[] _immunityExpiration = new DateTime[PartyState.MaxPartySize];
        private int _rotHolderSlot = -1;

        private static float _rotPassRadius = 3;
        private static PartyRolesConfig.Assignment[] _rotPriority = { PartyRolesConfig.Assignment.R1, PartyRolesConfig.Assignment.M1, PartyRolesConfig.Assignment.M2, PartyRolesConfig.Assignment.H1, PartyRolesConfig.Assignment.H2, PartyRolesConfig.Assignment.R2 };

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (_rotHolderSlot == -1 || _rotHolderSlot == slot)
                return; // nothing special if there is no rot yet or if we're rot holder (we let other come to us to get it)

            var rotHolder = module.Raid[_rotHolderSlot];
            if (rotHolder == null)
                return;

            if (WantToPickUpRot(module, assignment))
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(rotHolder.Position, _rotPassRadius - 1), _rotExpiration[_rotHolderSlot]);
            else
                hints.AddForbiddenZone(ShapeDistance.Circle(rotHolder.Position, _rotPassRadius + 2), _immunityExpiration[slot]);
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _rotHolderSlot == playerSlot ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.AllaganRot)
            {
                // predict rot target
                _rotHolderSlot = module.Raid.FindSlot(spell.TargetID);
                if (_rotHolderSlot >= 0)
                    _rotExpiration[_rotHolderSlot] = spell.FinishAt.AddSeconds(15);
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.AllaganRot:
                    _rotHolderSlot = module.Raid.FindSlot(actor.InstanceID);
                    if (_rotHolderSlot >= 0)
                        _rotExpiration[_rotHolderSlot] = status.ExpireAt;
                    break;
                case SID.AllaganImmunity:
                    int slot = module.Raid.FindSlot(actor.InstanceID);
                    if (slot >= 0)
                        _immunityExpiration[slot] = status.ExpireAt;
                    break;
            }
        }

        private bool WantToPickUpRot(BossModule module, PartyRolesConfig.Assignment assignment)
        {
            var deadline = _rotExpiration[_rotHolderSlot];
            if ((deadline - module.WorldState.CurrentTime).TotalSeconds > 5)
                return false; // let rot tick for a while

            // note: rot timer is 15s, immunity is 40s - so if we pass at <= 5s left, we need 5 people in rotation; currently we hardcode priority to R1 (assumed phys) -> M1 -> M2 -> H1 -> H2 -> R2 (spare, assumed caster)
            var assignments = Service.Config.Get<PartyRolesConfig>().SlotsPerAssignment(module.Raid);
            if (assignments.Length == 0)
                return false; // if assignments are unset, we can't define pass priority

            foreach (var next in _rotPriority)
            {
                int nextSlot = assignments[(int)next];
                if (_immunityExpiration[nextSlot] < deadline && !(module.Raid[nextSlot]?.IsDead ?? true))
                    return next == assignment;
            }

            // we're fucked, probably too many people are dead
            return false;
        }
    }

    class T02AI : BossComponent
    {
        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            hints.UpdatePotentialTargets(e =>
            {
                if (e.Actor == module.PrimaryActor)
                {
                    e.Priority = 1;
                    e.ShouldBeInterrupted = true; // interrupt every high voltage; TODO consider interrupt rotation
                    if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT)
                    {
                        if (e.Actor.TargetID == actor.InstanceID)
                        {
                            // player is tanking - continue doing so until 5 vuln stacks
                            //e.ShouldBeTanked = ;
                        }
                    }
                }
            });
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

    [ModuleInfo(PrimaryActorOID = (uint)OID.ADS)]
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

    [ModuleInfo(PrimaryActorOID = (uint)OID.QuarantineNode)]
    public class T02QuarantineNode : BossModule
    {
        public T02QuarantineNode(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(0, 112), 14, 13)) { }
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

    [ModuleInfo(PrimaryActorOID = (uint)OID.AttackNode)]
    public class T02AttackNode : BossModule
    {
        public T02AttackNode(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-44, 94), 17)) { }
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

    [ModuleInfo(PrimaryActorOID = (uint)OID.SanitaryNode)]
    public class T02SanitaryNode : BossModule
    {
        public T02SanitaryNode(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(-43, 52), 18, 15)) { }
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

    [ModuleInfo(PrimaryActorOID = (uint)OID.MonitoringNode)]
    public class T02MonitoringNode : BossModule
    {
        public T02MonitoringNode(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(0, 39), 17, 15)) { }
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

    [ModuleInfo(PrimaryActorOID = (uint)OID.DefenseNode)]
    public class T02DefenseNode : BossModule
    {
        public T02DefenseNode(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(46, 52), 17, 14)) { }
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

    [ModuleInfo(PrimaryActorOID = (uint)OID.DisposalNode)]
    public class T02DisposalNode : BossModule
    {
        public T02DisposalNode(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(41, 94), 14, 20)) { }
    }
}
