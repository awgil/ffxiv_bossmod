using System.Collections.Generic;

namespace BossMod.Endwalker.Savage.P6SHegemone
{
    class ChorosIxou : Components.GenericAOEs
    {
        public bool FirstDone { get; private set; }
        public bool SecondDone { get; private set; }
        private AOEShapeCone _cone = new(40, 45.Degrees());
        private List<Angle> _directions = new();

        public ChorosIxou() : base(new()) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (SecondDone)
                yield break;

            // TODO: timing
            var offset = (FirstDone ? 90 : 0).Degrees();
            foreach (var dir in _directions)
                yield return new(_cone, module.PrimaryActor.Position, dir + offset);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.ChorosIxouFSFrontAOE or AID.ChorosIxouSFSidesAOE)
                _directions.Add(caster.Rotation);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.ChorosIxouFSFrontAOE or AID.ChorosIxouSFSidesAOE)
                FirstDone = true;
            else if ((AID)spell.Action.ID is AID.ChorosIxouFSSidesAOE or AID.ChorosIxouSFFrontAOE)
                SecondDone = true;
        }
    }
}
