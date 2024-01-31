using System;
using System.Collections.Generic;
using System.Linq;

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
        private static readonly AOEShapeRect rect = new(100,3);
        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
                foreach (var p in module.Enemies(OID.MagitekBit))
                {
                if (module.Bounds.Contains(p.Position) && module.WorldState.CurrentTime > time.AddSeconds(2)) //sometimes the bits teleport into weird locations, the angles got a range because the devs are sloppy and the bits can have variations in angle of upto 0.95°, delay to prevent flickering while all bits rotate into their final angle
                    {
                    if (SatelliteLaser)
                        {
                            if (module.WorldState.CurrentTime < time.AddSeconds(12.5f) && p.Rotation.AlmostEqual(90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if (module.WorldState.CurrentTime < time.AddSeconds(12.5f) && p.Rotation.AlmostEqual(180.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if (module.WorldState.CurrentTime < time.AddSeconds(12.5f) && p.Rotation.AlmostEqual(0.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if (module.WorldState.CurrentTime < time.AddSeconds(12.5f) && p.Rotation.AlmostEqual(-90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                        }
                    if (DiffractiveLaserAngle0)
                        {
                            if(module.WorldState.CurrentTime < time.AddSeconds(8.9f) && p.Rotation.AlmostEqual(180.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if(module.WorldState.CurrentTime < time.AddSeconds(8.9f) && p.Rotation.AlmostEqual(-90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, default, ArenaColor.Danger);
                            if(module.WorldState.CurrentTime < time.AddSeconds(10.7f) && module.WorldState.CurrentTime > time.AddSeconds(8.9f) && p.Rotation.AlmostEqual(-90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if(module.WorldState.CurrentTime < time.AddSeconds(10.7f) && module.WorldState.CurrentTime > time.AddSeconds(8.9f) && p.Rotation.AlmostEqual(0.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, default, ArenaColor.Danger);
                            if(module.WorldState.CurrentTime > time.AddSeconds(10.7f) && p.Rotation.AlmostEqual(0.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                        }
                    if (DiffractiveLaserAngleM90)
                        {
                            if(module.WorldState.CurrentTime < time.AddSeconds(8.9f) && p.Rotation.AlmostEqual(90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if(module.WorldState.CurrentTime < time.AddSeconds(8.9f) && p.Rotation.AlmostEqual(0.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, default, ArenaColor.Danger);
                            if(module.WorldState.CurrentTime < time.AddSeconds(10.7f) && module.WorldState.CurrentTime > time.AddSeconds(8.9f) && p.Rotation.AlmostEqual(0.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if(module.WorldState.CurrentTime < time.AddSeconds(10.7f) && module.WorldState.CurrentTime > time.AddSeconds(8.9f) && p.Rotation.AlmostEqual(-90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, default, ArenaColor.Danger);
                            if(module.WorldState.CurrentTime > time.AddSeconds(10.7f) && p.Rotation.AlmostEqual(-90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                        }
                    if (DiffractiveLaserAngle90)
                        {
                            if(module.WorldState.CurrentTime < time.AddSeconds(8.9f) && p.Rotation.AlmostEqual(-90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if(module.WorldState.CurrentTime < time.AddSeconds(8.9f) && p.Rotation.AlmostEqual(0.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, default, ArenaColor.Danger);
                            if(module.WorldState.CurrentTime < time.AddSeconds(10.7f) && module.WorldState.CurrentTime > time.AddSeconds(8.9f) && p.Rotation.AlmostEqual(0.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if(module.WorldState.CurrentTime < time.AddSeconds(10.7f) && module.WorldState.CurrentTime > time.AddSeconds(8.9f) && p.Rotation.AlmostEqual(90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, default, ArenaColor.Danger);
                            if(module.WorldState.CurrentTime > time.AddSeconds(10.7f) && p.Rotation.AlmostEqual(90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                        }
                    if (DiffractiveLaserAngle180)
                        {
                            if(module.WorldState.CurrentTime < time.AddSeconds(8.9f) && p.Rotation.AlmostEqual(0.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if(module.WorldState.CurrentTime < time.AddSeconds(8.9f) && p.Rotation.AlmostEqual(90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, default, ArenaColor.Danger);
                            if(module.WorldState.CurrentTime < time.AddSeconds(10.7f) && module.WorldState.CurrentTime > time.AddSeconds(8.9f) && p.Rotation.AlmostEqual(90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if(module.WorldState.CurrentTime < time.AddSeconds(10.7f) && module.WorldState.CurrentTime > time.AddSeconds(8.9f) && p.Rotation.AlmostEqual(180.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, default, ArenaColor.Danger);
                            if(module.WorldState.CurrentTime > time.AddSeconds(10.7f) && p.Rotation.AlmostEqual(180.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                        }
                    if (LaserShowerAngle90)
                        {
                            if(module.WorldState.CurrentTime < time.AddSeconds(6.6f) && p.Rotation.AlmostEqual(90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if(module.WorldState.CurrentTime < time.AddSeconds(6.6f) && p.Rotation.AlmostEqual(0.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, default, ArenaColor.Danger);
                            if(module.WorldState.CurrentTime < time.AddSeconds(8.4f) && module.WorldState.CurrentTime > time.AddSeconds(6.6f) && p.Rotation.AlmostEqual(0.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if(module.WorldState.CurrentTime < time.AddSeconds(8.4f) && module.WorldState.CurrentTime > time.AddSeconds(6.6f) && p.Rotation.AlmostEqual(-90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, default, ArenaColor.Danger);
                            if(module.WorldState.CurrentTime > time.AddSeconds(8.4f) && p.Rotation.AlmostEqual(-90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                        }
                    if (LaserShowerAngleM90)
                        {
                            if(module.WorldState.CurrentTime < time.AddSeconds(6.6f) && p.Rotation.AlmostEqual(-90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if(module.WorldState.CurrentTime < time.AddSeconds(6.6f) && p.Rotation.AlmostEqual(0.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, default, ArenaColor.Danger);
                            if(module.WorldState.CurrentTime < time.AddSeconds(8.4f) && module.WorldState.CurrentTime > time.AddSeconds(6.6f) && p.Rotation.AlmostEqual(0.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if(module.WorldState.CurrentTime < time.AddSeconds(8.4f) && module.WorldState.CurrentTime > time.AddSeconds(6.6f) && p.Rotation.AlmostEqual(90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, default, ArenaColor.Danger);
                            if(module.WorldState.CurrentTime > time.AddSeconds(8.4f) && p.Rotation.AlmostEqual(90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                        }
                    if (LaserShowerAngle0)
                        {
                            if(module.WorldState.CurrentTime < time.AddSeconds(6.6f) && p.Rotation.AlmostEqual(0.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if(module.WorldState.CurrentTime < time.AddSeconds(6.6f) && p.Rotation.AlmostEqual(90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, default, ArenaColor.Danger);
                            if(module.WorldState.CurrentTime < time.AddSeconds(8.4f) && module.WorldState.CurrentTime > time.AddSeconds(6.6f) && p.Rotation.AlmostEqual(90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if(module.WorldState.CurrentTime < time.AddSeconds(8.4f) && module.WorldState.CurrentTime > time.AddSeconds(6.6f) && p.Rotation.AlmostEqual(180.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, default, ArenaColor.Danger);
                            if(module.WorldState.CurrentTime > time.AddSeconds(8.4f) && p.Rotation.AlmostEqual(180.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                        }
                    if (LaserShowerAngle180)
                        {
                            if(module.WorldState.CurrentTime < time.AddSeconds(6.6f) && p.Rotation.AlmostEqual(180.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if(module.WorldState.CurrentTime < time.AddSeconds(6.6f) && p.Rotation.AlmostEqual(90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, default, ArenaColor.Danger);
                            if(module.WorldState.CurrentTime < time.AddSeconds(8.4f) && module.WorldState.CurrentTime > time.AddSeconds(6.6f) && p.Rotation.AlmostEqual(90.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                            if(module.WorldState.CurrentTime < time.AddSeconds(8.4f) && module.WorldState.CurrentTime > time.AddSeconds(6.6f) && p.Rotation.AlmostEqual(0.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, default, ArenaColor.Danger);
                            if(module.WorldState.CurrentTime > time.AddSeconds(8.4f) && p.Rotation.AlmostEqual(0.Degrees(),MathF.PI/180))
                                yield return new(rect, p.Position, p.Rotation, new());
                        }
                    }
                }
                if ((SatelliteLaser || DiffractiveLaserAngle0 || DiffractiveLaserAngle90 || DiffractiveLaserAngleM90 || DiffractiveLaserAngle180) && module.WorldState.CurrentTime >= time.AddSeconds(12.5f))
                    {
                        DiffractiveLaserAngle0 = false;
                        DiffractiveLaserAngle90 = false;
                        DiffractiveLaserAngleM90 = false;
                        DiffractiveLaserAngle180 = false;
                        SatelliteLaser = false;
                    }
                if ((LaserShowerAngle90 || LaserShowerAngleM90 || LaserShowerAngle0 || LaserShowerAngle180) && module.WorldState.CurrentTime >= time.AddSeconds(10.25f))
                    {
                        LaserShowerAngle90 = false;
                        LaserShowerAngleM90 = false;
                        LaserShowerAngle0 = false;
                        LaserShowerAngle180 = false;
                    }
                
        }
        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.SatelliteLaser)
                SatelliteLaser = true;
                time = module.WorldState.CurrentTime;
            if ((AID)spell.Action.ID == AID.DiffractiveLaser)
                {
                    {
                        if(spell.Rotation.AlmostEqual(0.Degrees(),MathF.PI/180))
                            DiffractiveLaserAngle0 = true;
                        if(spell.Rotation.AlmostEqual(-90.Degrees(),MathF.PI/180))
                            DiffractiveLaserAngleM90 = true;
                        if(spell.Rotation.AlmostEqual(90.Degrees(),MathF.PI/180))
                            DiffractiveLaserAngle90 = true;
                        if(spell.Rotation.AlmostEqual(180.Degrees(),MathF.PI/180))
                            DiffractiveLaserAngle180 = true;
                    }
                    time = module.WorldState.CurrentTime;
                }
            if ((AID)spell.Action.ID == AID.LaserShower2)
                {
                    {
                        if(caster.Rotation.AlmostEqual(90.Degrees(),MathF.PI/180))
                            LaserShowerAngle90 = true;
                        if(caster.Rotation.AlmostEqual(0.Degrees(),MathF.PI/180))
                            LaserShowerAngle0 = true;
                        if(caster.Rotation.AlmostEqual(-180.Degrees(),MathF.PI/180))
                            LaserShowerAngle180 = true;
                        if(caster.Rotation.AlmostEqual(-90.Degrees(),MathF.PI/180))
                            LaserShowerAngleM90 = true;
                    }
                    time = module.WorldState.CurrentTime;
                }
        }
        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
                hints.Add($"SatelliteLaser is {SatelliteLaser}\nDL180° is {DiffractiveLaserAngle180}, DL0° is {DiffractiveLaserAngle0}, DLM90° is {DiffractiveLaserAngleM90},DL90° is {DiffractiveLaserAngle90}\nLS180° is {LaserShowerAngle180}, LS0° is {LaserShowerAngle0}, LSM90° is {LaserShowerAngleM90},LS90° is {LaserShowerAngle90}");

        }
    }
    class Rush : Components.GenericWildCharge
    {
        public Rush() : base(7, ActionID.MakeSpell(AID.Rush)) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                Source = caster;
                foreach (var (slot, player) in module.Raid.WithSlot())
                {
                    PlayerRoles[slot] = player.InstanceID == spell.TargetID ? PlayerRole.Target : PlayerRole.Avoid;
                }
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                Source = null;
            }
        }
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
    class CE31DainsleifStates : StateMachineBuilder
    {
        public CE31DainsleifStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<SatelliteLaser>()
                .ActivateOnEnter<DiffractiveLaser>()
                .ActivateOnEnter<LaserShower>()
                .ActivateOnEnter<MagitekBitLasers>()                     
                .ActivateOnEnter<Rush>();
        }
    }

    [ModuleInfo(CFCID = 735, DynamicEventID = 6)]
    public class CE31Dainsleif : BossModule
    {
        public CE31Dainsleif(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-234, 262), 30.2f)) { }
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.MagitekBit))
                Arena.Actor(s, ArenaColor.Object, true);
        }    
    }
}
