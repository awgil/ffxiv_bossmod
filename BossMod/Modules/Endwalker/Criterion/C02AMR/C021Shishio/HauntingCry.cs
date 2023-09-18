using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Criterion.C02AMR.C021Shishio
{
    class HauntingCrySwipes : Components.GenericAOEs
    {
        private List<Actor> _casters = new();

        private static AOEShapeCone _shape = new(40, 90.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return _casters.Take(4).Select(c => new AOEInstance(_shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.FinishAt));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.NRightSwipe or AID.NLeftSwipe or AID.SRightSwipe or AID.SLeftSwipe)
                _casters.Add(caster);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.NRightSwipe or AID.NLeftSwipe or AID.SRightSwipe or AID.SLeftSwipe)
            {
                _casters.Remove(caster);
                ++NumCasts;
            }
        }
    }

    class HauntingCryReisho : Components.GenericAOEs
    {
        private List<Actor> _ghosts = new();
        private DateTime _activation;
        private DateTime _ignoreBefore;

        private static AOEShapeCircle _shape = new(6);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _ghosts.Select(g => new AOEInstance(_shape, g.Position, default, _activation));

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var g in _ghosts)
            {
                arena.Actor(g, ArenaColor.Object, true);
                var target = module.WorldState.Actors.Find(g.Tether.Target);
                if (target != null)
                    arena.AddLine(g.Position, target.Position, ArenaColor.Danger);
            }
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if ((OID)source.OID is OID.NHauntingThrall)
            {
                _ghosts.Add(source);
                _activation = module.WorldState.CurrentTime.AddSeconds(5.1f);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.NReisho or AID.SReisho && module.WorldState.CurrentTime > _ignoreBefore)
            {
                ++NumCasts;
                _activation = module.WorldState.CurrentTime.AddSeconds(2.1f);
                _ignoreBefore = module.WorldState.CurrentTime.AddSeconds(1);
            }
        }
    }

    class HauntingCryVermilionAura : Components.CastTowers
    {
        public HauntingCryVermilionAura(AID aid) : base(ActionID.MakeSpell(aid), 4) { }
    }
    class NHauntingCryVermilionAura : HauntingCryVermilionAura { public NHauntingCryVermilionAura() : base(AID.NVermilionAura) { } }
    class SHauntingCryVermilionAura : HauntingCryVermilionAura { public SHauntingCryVermilionAura() : base(AID.SVermilionAura) { } }

    class HauntingCryStygianAura : Components.SpreadFromCastTargets
    {
        public HauntingCryStygianAura(AID aid) : base(ActionID.MakeSpell(aid), 15, true) { }
    }
    class NHauntingCryStygianAura : HauntingCryStygianAura { public NHauntingCryStygianAura() : base(AID.NStygianAura) { } }
    class SHauntingCryStygianAura : HauntingCryStygianAura { public SHauntingCryStygianAura() : base(AID.SStygianAura) { } }
}
