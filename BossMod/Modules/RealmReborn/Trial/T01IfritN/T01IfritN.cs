using System;
using System.Linq;

namespace BossMod.RealmReborn.Trial.T01IfritN
{
    public enum OID : uint
    {
        Boss = 0xCF, // x1
        InfernalNail = 0xD0, // spawn during fight
        Helper = 0x191, // x19
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast
        Incinerate = 453, // Boss->self, no cast, range 10+R ?-degree cone cleave
        VulcanBurst = 454, // Boss->self, no cast, range 16+R circle unavoidable aoe with knockback ?
        Eruption = 455, // Boss->self, 2.2s cast, visual
        EruptionAOE = 733, // Helper->location, 3.0s cast, range 8 aoe
        Hellfire = 458, // Boss->self, 2.0s cast, infernal nail 'enrage'
        RadiantPlume = 456, // Boss->self, 2.2s cast, visual
        RadiantPlumeAOE = 734, // Helper->location, 3.0s cast, range 8 aoe
    };

    class Hints : BossComponent
    {
        private DateTime _nailSpawn;

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            var nail = module.Enemies(OID.InfernalNail).FirstOrDefault();
            if (_nailSpawn == new DateTime() && nail != null && nail.IsTargetable)
            {
                _nailSpawn = module.WorldState.CurrentTime;
            }
            if (_nailSpawn != new DateTime() && nail != null && nail.IsTargetable && !nail.IsDead)
            {
                hints.Add($"Nail enrage in: {Math.Max(35 - (module.WorldState.CurrentTime - _nailSpawn).TotalSeconds, 0.0f):f1}s");
            }
        }
    }

    class Incinerate : Components.Cleave
    {
        public Incinerate() : base(ActionID.MakeSpell(AID.Incinerate), new AOEShapeCone(16, 60.Degrees())) { } // TODO: verify angle
    }

    class Eruption : Components.LocationTargetedAOEs
    {
        public Eruption() : base(ActionID.MakeSpell(AID.EruptionAOE), 8) { }
    }

    class RadiantPlume : Components.LocationTargetedAOEs
    {
        public RadiantPlume() : base(ActionID.MakeSpell(AID.RadiantPlumeAOE), 8) { }
    }

    class T01IfritNStates : StateMachineBuilder
    {
        public T01IfritNStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Hints>()
                .ActivateOnEnter<Incinerate>()
                .ActivateOnEnter<Eruption>()
                .ActivateOnEnter<RadiantPlume>();
        }
    }

    public class T01IfritN : BossModule
    {
        public T01IfritN(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-0, 0), 20)) { }
    }
}
