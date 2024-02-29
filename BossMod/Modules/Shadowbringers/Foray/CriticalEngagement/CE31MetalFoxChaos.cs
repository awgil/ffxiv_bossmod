// CONTRIB: made by malediktus, not checked
using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE31MetalFoxChaos
{
    public enum OID : uint
    {
        Boss = 0x2DB5, // R=8.0
        Helper = 0x233C, // R=0.500
        MagitekBit = 0x2DB6, // R=1.2
    };

    public enum AID : uint
    {
        Attack = 6497, // Boss->player, no cast, single-target
        MagitektBitTeleporting = 20192, // 2DB6->location, no cast, ???
        DiffractiveLaser = 20138, // Boss->self, 7,0s cast, range 60 150-degree cone
        RefractedLaser = 20141, // 2DB6->self, no cast, range 100 width 6 rect
        LaserShower = 20136, // Boss->self, 3,0s cast, single-target
        LaserShower2 = 20140, // Helper->location, 5,0s cast, range 10 circle
        Rush = 20139, // Boss->player, 3,0s cast, width 14 rect charge
        SatelliteLaser = 20137, // Boss->self, 10,0s cast, range 100 circle
    };

    class MagitekBitLasers : Components.GenericAOEs
    {
        private int numcasts;
        private bool SatelliteLaser;
        private bool DiffractiveLaserAngle0;
        private bool DiffractiveLaserAngle90;
        private bool DiffractiveLaserAngle180;
        private bool DiffractiveLaserAngleM90;
        private bool LaserShowerAngle90;
        private bool LaserShowerAngleM90;
        private bool LaserShowerAngle0;
        private bool LaserShowerAngle180;
        private DateTime time;
        private DateTime _activation1;
        private DateTime _activation2;
        private DateTime _activation3;
        private static readonly float maxError = MathF.PI / 180;
        private static readonly AOEShapeRect rect = new(100, 3);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var p in module.Enemies(OID.MagitekBit))
            {
                if (module.Bounds.Contains(p.Position))
                {
                    if (SatelliteLaser && module.WorldState.CurrentTime > time.AddSeconds(2.5f) && (p.Rotation.AlmostEqual(180.Degrees(), maxError) || p.Rotation.AlmostEqual(90.Degrees(), maxError) || p.Rotation.AlmostEqual(0.Degrees(), maxError) || p.Rotation.AlmostEqual(-90.Degrees(), maxError)))
                        yield return new(rect, p.Position, p.Rotation, _activation1);
                    if (DiffractiveLaserAngle0 && module.WorldState.CurrentTime > time.AddSeconds(2))
                    {
                        if (numcasts < 5 && p.Rotation.AlmostEqual(180.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation1, ArenaColor.Danger);
                        if (numcasts < 5 && (p.Rotation.AlmostEqual(90.Degrees(), maxError) || p.Rotation.AlmostEqual(-90.Degrees(), maxError)))
                            yield return new(rect, p.Position, p.Rotation, _activation2);
                        if (numcasts >= 5 && numcasts < 9 && (p.Rotation.AlmostEqual(90.Degrees(), maxError) || p.Rotation.AlmostEqual(-90.Degrees(), maxError)))
                            yield return new(rect, p.Position, p.Rotation, _activation2, ArenaColor.Danger);
                        if (numcasts >= 5 && numcasts < 9 && p.Rotation.AlmostEqual(0.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation3);
                        if (numcasts >= 9 && p.Rotation.AlmostEqual(0.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation3, ArenaColor.Danger);
                    }
                    if (DiffractiveLaserAngleM90 && module.WorldState.CurrentTime > time.AddSeconds(2))
                    {
                        if (numcasts < 5 && p.Rotation.AlmostEqual(90.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation1, ArenaColor.Danger);
                        if (numcasts < 5 && (p.Rotation.AlmostEqual(0.Degrees(), maxError) || p.Rotation.AlmostEqual(180.Degrees(), maxError)))
                            yield return new(rect, p.Position, p.Rotation, _activation2);
                        if (numcasts >= 5 && numcasts < 9 && (p.Rotation.AlmostEqual(0.Degrees(), maxError) || p.Rotation.AlmostEqual(180.Degrees(), maxError)))
                            yield return new(rect, p.Position, p.Rotation, _activation2, ArenaColor.Danger);
                        if (numcasts >= 5 && numcasts < 9 && p.Rotation.AlmostEqual(-90.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation3);
                        if (numcasts >= 9 && p.Rotation.AlmostEqual(-90.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation3, ArenaColor.Danger);
                    }
                    if (DiffractiveLaserAngle90 && module.WorldState.CurrentTime > time.AddSeconds(2))
                    {
                        if (numcasts < 5 && p.Rotation.AlmostEqual(-90.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation1, ArenaColor.Danger);
                        if (numcasts < 5 && (p.Rotation.AlmostEqual(0.Degrees(), maxError) || p.Rotation.AlmostEqual(180.Degrees(), maxError)))
                            yield return new(rect, p.Position, p.Rotation, _activation2);
                        if (numcasts >= 5 && numcasts < 9 && (p.Rotation.AlmostEqual(0.Degrees(), maxError) || p.Rotation.AlmostEqual(180.Degrees(), maxError)))
                            yield return new(rect, p.Position, p.Rotation, _activation2, ArenaColor.Danger);
                        if (numcasts >= 5 && numcasts < 9 && p.Rotation.AlmostEqual(90.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation3);
                        if (numcasts >= 9 && p.Rotation.AlmostEqual(90.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation3, ArenaColor.Danger);
                    }
                    if (DiffractiveLaserAngle180 && module.WorldState.CurrentTime > time.AddSeconds(2))
                    {
                        if (numcasts < 5 && p.Rotation.AlmostEqual(0.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation1, ArenaColor.Danger);
                        if (numcasts < 5 && (p.Rotation.AlmostEqual(90.Degrees(), maxError) || p.Rotation.AlmostEqual(-90.Degrees(), maxError)))
                            yield return new(rect, p.Position, p.Rotation, _activation2);
                        if (numcasts >= 5 && numcasts < 9 && (p.Rotation.AlmostEqual(90.Degrees(), maxError) || p.Rotation.AlmostEqual(-90.Degrees(), maxError)))
                            yield return new(rect, p.Position, p.Rotation, _activation2, ArenaColor.Danger);
                        if (numcasts >= 5 && numcasts < 9 && p.Rotation.AlmostEqual(180.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation3);
                        if (numcasts >= 9 && p.Rotation.AlmostEqual(180.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation3, ArenaColor.Danger);
                    }
                    if (LaserShowerAngle90)
                    {
                        if (numcasts < 5 && p.Rotation.AlmostEqual(90.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation1, ArenaColor.Danger);
                        if (numcasts < 5 && (p.Rotation.AlmostEqual(0.Degrees(), maxError) || p.Rotation.AlmostEqual(180.Degrees(), maxError)))
                            yield return new(rect, p.Position, p.Rotation, _activation2);
                        if (numcasts >= 5 && numcasts < 9 && (p.Rotation.AlmostEqual(0.Degrees(), maxError) || p.Rotation.AlmostEqual(180.Degrees(), maxError)))
                            yield return new(rect, p.Position, p.Rotation, _activation2, ArenaColor.Danger);
                        if (numcasts >= 5 && numcasts < 9 && p.Rotation.AlmostEqual(-90.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation3);
                        if (numcasts >= 9 && p.Rotation.AlmostEqual(-90.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation3, ArenaColor.Danger);
                    }
                    if (LaserShowerAngleM90)
                    {
                        if (numcasts < 5 && p.Rotation.AlmostEqual(-90.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation1, ArenaColor.Danger);
                        if (numcasts < 5 && (p.Rotation.AlmostEqual(0.Degrees(), maxError) || p.Rotation.AlmostEqual(180.Degrees(), maxError)))
                            yield return new(rect, p.Position, p.Rotation, _activation2);
                        if (numcasts >= 5 && numcasts < 9 && (p.Rotation.AlmostEqual(0.Degrees(), maxError) || p.Rotation.AlmostEqual(180.Degrees(), maxError)))
                            yield return new(rect, p.Position, p.Rotation, _activation2, ArenaColor.Danger);
                        if (numcasts >= 5 && numcasts < 9 && p.Rotation.AlmostEqual(90.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation3);
                        if (numcasts >= 9 && p.Rotation.AlmostEqual(90.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation3, ArenaColor.Danger);
                    }
                    if (LaserShowerAngle0)
                    {
                        if (numcasts < 5 && p.Rotation.AlmostEqual(0.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation1, ArenaColor.Danger);
                        if (numcasts < 5 && (p.Rotation.AlmostEqual(90.Degrees(), maxError) || p.Rotation.AlmostEqual(-90.Degrees(), maxError)))
                            yield return new(rect, p.Position, p.Rotation, _activation2);
                        if (numcasts >= 5 && numcasts < 9 && (p.Rotation.AlmostEqual(90.Degrees(), maxError) || p.Rotation.AlmostEqual(-90.Degrees(), maxError)))
                            yield return new(rect, p.Position, p.Rotation, _activation2, ArenaColor.Danger);
                        if (numcasts >= 5 && numcasts < 9 && p.Rotation.AlmostEqual(180.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation3);
                        if (numcasts >= 9 && p.Rotation.AlmostEqual(180.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation3, ArenaColor.Danger);
                    }
                    if (LaserShowerAngle180)
                    {
                        if (numcasts < 5 && p.Rotation.AlmostEqual(180.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation1, ArenaColor.Danger);
                        if (numcasts < 5 && (p.Rotation.AlmostEqual(90.Degrees(), maxError) || p.Rotation.AlmostEqual(-90.Degrees(), maxError)))
                            yield return new(rect, p.Position, p.Rotation, _activation2);
                        if (numcasts >= 5 && numcasts < 9 && (p.Rotation.AlmostEqual(90.Degrees(), maxError) || p.Rotation.AlmostEqual(-90.Degrees(), maxError)))
                            yield return new(rect, p.Position, p.Rotation, _activation2, ArenaColor.Danger);
                        if (numcasts >= 5 && numcasts < 9 && p.Rotation.AlmostEqual(0.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation3);
                        if (numcasts >= 9 && p.Rotation.AlmostEqual(0.Degrees(), maxError))
                            yield return new(rect, p.Position, p.Rotation, _activation3, ArenaColor.Danger);
                    }
                }
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.SatelliteLaser)
            {
                SatelliteLaser = true;
                time = module.WorldState.CurrentTime;
                _activation1 = module.WorldState.CurrentTime.AddSeconds(12.3f);
            }
            if ((AID)spell.Action.ID == AID.DiffractiveLaser)
            {
                {
                    if (spell.Rotation.AlmostEqual(0.Degrees(), maxError))
                        DiffractiveLaserAngle0 = true;
                    if (spell.Rotation.AlmostEqual(-90.Degrees(), maxError))
                        DiffractiveLaserAngleM90 = true;
                    if (spell.Rotation.AlmostEqual(90.Degrees(), maxError))
                        DiffractiveLaserAngle90 = true;
                    if (spell.Rotation.AlmostEqual(180.Degrees(), maxError))
                        DiffractiveLaserAngle180 = true;
                }
                time = module.WorldState.CurrentTime;
                _activation1 = module.WorldState.CurrentTime.AddSeconds(8.8f);
                _activation2 = module.WorldState.CurrentTime.AddSeconds(10.6f);
                _activation3 = module.WorldState.CurrentTime.AddSeconds(12.4f);
            }
            if ((AID)spell.Action.ID == AID.LaserShower2)
            {
                {
                    if (caster.Rotation.AlmostEqual(90.Degrees(), maxError))
                        LaserShowerAngle90 = true;
                    if (caster.Rotation.AlmostEqual(0.Degrees(), maxError))
                        LaserShowerAngle0 = true;
                    if (caster.Rotation.AlmostEqual(180.Degrees(), maxError))
                        LaserShowerAngle180 = true;
                    if (caster.Rotation.AlmostEqual(-90.Degrees(), maxError))
                        LaserShowerAngleM90 = true;
                }
                _activation1 = module.WorldState.CurrentTime.AddSeconds(6.5f);
                _activation2 = module.WorldState.CurrentTime.AddSeconds(8.3f);
                _activation3 = module.WorldState.CurrentTime.AddSeconds(10.1f);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if ((AID)spell.Action.ID == AID.RefractedLaser)
                ++numcasts;
            if (numcasts == 14)
            {
                numcasts = 0;
                DiffractiveLaserAngle0 = false;
                DiffractiveLaserAngle90 = false;
                DiffractiveLaserAngleM90 = false;
                DiffractiveLaserAngle180 = false;
                SatelliteLaser = false;
                LaserShowerAngle90 = false;
                LaserShowerAngleM90 = false;
                LaserShowerAngle0 = false;
                LaserShowerAngle180 = false;
            }
        }
    }

    class Rush : Components.BaitAwayChargeCast
    {
        public Rush() : base(ActionID.MakeSpell(AID.Rush), 7) { }
    }

    class LaserShower : Components.LocationTargetedAOEs
    {
        public LaserShower() : base(ActionID.MakeSpell(AID.LaserShower2), 10) { }
    }

    class DiffractiveLaser : Components.SelfTargetedAOEs
    {
        public DiffractiveLaser() : base(ActionID.MakeSpell(AID.DiffractiveLaser), new AOEShapeCone(60, 75.Degrees())) { }
    }

    class SatelliteLaser : Components.RaidwideCast
    {
        public SatelliteLaser() : base(ActionID.MakeSpell(AID.SatelliteLaser), "Raidwide + all lasers fire at the same time") { }
    }

    class CE31MetalFoxChaosStates : StateMachineBuilder
    {
        public CE31MetalFoxChaosStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<SatelliteLaser>()
                .ActivateOnEnter<DiffractiveLaser>()
                .ActivateOnEnter<LaserShower>()
                .ActivateOnEnter<MagitekBitLasers>()
                .ActivateOnEnter<Rush>();
        }
    }

    [ModuleInfo(CFCID = 735, DynamicEventID = 13)]
    public class CE31MetalFoxChaos : BossModule
    {
        public CE31MetalFoxChaos(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-234, 262), 30.2f)) { }
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.MagitekBit))
                Arena.Actor(s, ArenaColor.Vulnerable, true);
        }
    }
}
