using System.Collections.Generic;

namespace BossMod.Endwalker.Criterion.C03AAI.C031Ketuduke
{
    class AngrySeasAOE : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeRect _shape = new(20, 5, 20);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.NAngrySeasAOE or AID.SAngrySeasAOE)
                _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
        }
    }

    // TODO: generalize
    class AngrySeasKnockback : Components.Knockback
    {
        private List<Source> _sources = new();
        private static AOEShapeCone _shape = new(30, 90.Degrees());

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor) => _sources;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.NAngrySeasAOE or AID.SAngrySeasAOE)
            {
                _sources.Clear();
                // charge always happens through center, so create two sources with origin at center looking orthogonally
                _sources.Add(new(module.Bounds.Center, 12, spell.NPCFinishAt, _shape, spell.Rotation + 90.Degrees(), Kind.DirForward));
                _sources.Add(new(module.Bounds.Center, 12, spell.NPCFinishAt, _shape, spell.Rotation - 90.Degrees(), Kind.DirForward));
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.NAngrySeasAOE or AID.SAngrySeasAOE)
            {
                _sources.Clear();
                ++NumCasts;
            }
        }
    }
}
