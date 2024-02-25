using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P8S2
{
    class EndOfDays : Components.GenericAOEs
    {
        public List<(Actor caster, DateTime finish)> Casters = new();

        private static AOEShapeRect _shape = new(60, 5);

        public EndOfDays() : base(ActionID.MakeSpell(AID.EndOfDays)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var c in Casters.Take(3))
            {
                if (c.caster.CastInfo == null)
                    yield return new(_shape, c.caster.Position, c.caster.Rotation, c.finish);
                else
                    yield return new(_shape, c.caster.Position, c.caster.CastInfo.Rotation, c.caster.CastInfo.NPCFinishAt);
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                Casters.RemoveAll(c => c.caster == caster);
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID == OID.IllusoryHephaistosLanes && id == 0x11D3)
                Casters.Add((actor, module.WorldState.CurrentTime.AddSeconds(8))); // ~2s before cast start
        }
    }
}
