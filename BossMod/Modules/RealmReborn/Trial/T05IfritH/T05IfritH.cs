using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.RealmReborn.Trial.T05IfritH
{
    public enum OID : uint
    {
        Boss = 0xD1, // x4
        InfernalNail = 0xD2, // spawn during fight
        Helper = 0x1B2, // x20
    };

    public enum AID : uint
    {
        AutoAttack = 451, // Boss->player, no cast, range 8+R ?-degree cone cleave
        Incinerate = 1353, // Boss->self, no cast, range 10+R 120-degree cone cleave
        VulcanBurst = 1354, // Boss->self, no cast, range 16+R circle knockback 10
        Eruption = 1355, // Boss->self, 2.2s cast, single-target, visual
        EruptionAOE = 1358, // Helper->location, 3.0s cast, range 8 circle aoe
        CrimsonCyclone = 457, // Boss->self, 3.0s cast, range 38+R width 12 rect aoe
        Sear = 452, // Helper->location, no cast, range 8 circle aoe around boss
        RadiantPlume = 1356, // Boss->self, 2.2s cast, single-target, visual
        RadiantPlumeAOE = 1359, // Helper->location, 3.0s cast, range 8 circle aoe
        Hellfire = 1357, // Boss->self, 2.0s cast, infernal nail 'enrage' (raidwide if killed)
    };

    class Hints : BossComponent
    {
        private DateTime _nailSpawn;

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            bool nailsActive = ((T05IfritH)module).ActiveNails.Any();
            if (_nailSpawn == new DateTime() && nailsActive)
            {
                _nailSpawn = module.WorldState.CurrentTime;
            }
            if (_nailSpawn != new DateTime() && nailsActive)
            {
                hints.Add($"Nail enrage in: {Math.Max(55 - (module.WorldState.CurrentTime - _nailSpawn).TotalSeconds, 0.0f):f1}s");
            }
        }
    }

    class Incinerate : Components.Cleave
    {
        public Incinerate() : base(ActionID.MakeSpell(AID.Incinerate), new AOEShapeCone(15, 60.Degrees())) { }
    }

    class Eruption : Components.LocationTargetedAOEs
    {
        public Eruption() : base(ActionID.MakeSpell(AID.EruptionAOE), 8) { }
    }

    class CrimsonCyclone : Components.GenericAOEs
    {
        private List<Actor> _casters = new();

        private static AOEShape _shape = new AOEShapeRect(43, 6);

        public CrimsonCyclone() : base(ActionID.MakeSpell(AID.CrimsonCyclone)) { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return _casters.Select(c => (_shape, c.Position, c.CastInfo?.Rotation ?? c.Rotation, c.CastInfo?.FinishAt ?? module.WorldState.CurrentTime.AddSeconds(4)));
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID == OID.Boss && actor != module.PrimaryActor && id == 0x008D)
                _casters.Add(actor);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _casters.Remove(caster);
        }
    }

    class RadiantPlume : Components.LocationTargetedAOEs
    {
        public RadiantPlume() : base(ActionID.MakeSpell(AID.RadiantPlumeAOE), 8) { }
    }

    class T05IfritHStates : StateMachineBuilder
    {
        public T05IfritHStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Hints>()
                .ActivateOnEnter<Incinerate>()
                .ActivateOnEnter<Eruption>()
                .ActivateOnEnter<CrimsonCyclone>()
                .ActivateOnEnter<RadiantPlume>();
        }
    }

    public class T05IfritH : BossModule
    {
        private List<Actor> _nails;
        public IEnumerable<Actor> ActiveNails => _nails.Where(n => n.IsTargetable && !n.IsDead);

        public T05IfritH(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 0), 20))
        {
            _nails = Enemies(OID.InfernalNail);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            hints.AssignPotentialTargetPriorities(a => (OID)a.OID switch
            {
                OID.InfernalNail => 2,
                OID.Boss => a == PrimaryActor ? 1 : 0,
                _ => 0,
            });
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var n in ActiveNails)
                Arena.Actor(n, ArenaColor.Enemy);
        }
    }
}
