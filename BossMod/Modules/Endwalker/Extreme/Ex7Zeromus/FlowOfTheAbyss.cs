using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Extreme.Ex7Zeromus
{
    class FlowOfTheAbyssDimensionalSurge : Components.SelfTargetedAOEs
    {
        public FlowOfTheAbyssDimensionalSurge() : base(ActionID.MakeSpell(AID.FlowOfTheAbyssDimensionalSurge), new AOEShapeRect(60, 7)) { }
    }

    class FlowOfTheAbyssSpreadStack : Components.GenericStackSpread
    {
        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            switch ((IconID)iconID)
            {
                case IconID.AkhRhai:
                    Spreads.Add(new(actor, 5, module.WorldState.CurrentTime.AddSeconds(5)));
                    break;
                case IconID.DarkBeckonsUmbralRays:
                    Stacks.Add(new(actor, 6, 8, 8, module.WorldState.CurrentTime.AddSeconds(5)));
                    break;
                case IconID.UmbralPrism:
                    Stacks.Add(new(actor, 5, 2, 2, module.WorldState.CurrentTime.AddSeconds(5)));
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.AkhRhaiStart or AID.UmbralRays or AID.UmbralPrism)
            {
                Spreads.Clear();
                Stacks.Clear();
            }
        }
    }

    class FlowOfTheAbyssAkhRhai : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeCircle _shape = new(5);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.AkhRhaiStart:
                    _aoes.Add(new(_shape, caster.Position));
                    break;
                case AID.AkhRhaiAOE:
                    if (++NumCasts >= _aoes.Count * 10)
                        _aoes.Clear();
                    break;
            }
        }
    }

    class ChasmicNails : Components.GenericAOEs
    {
        private List<(Angle rot, DateTime activation)> _angles = new();

        private static AOEShapeCone _shape = new(60, 20.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var a in _angles.Skip(1).Take(2))
                yield return new(_shape, module.PrimaryActor.Position, a.rot, a.activation);
            if (_angles.Count > 0)
                yield return new(_shape, module.PrimaryActor.Position, _angles[0].rot, _angles[0].activation, ArenaColor.Danger);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.ChasmicNailsAOE1 or AID.ChasmicNailsAOE2 or AID.ChasmicNailsAOE3 or AID.ChasmicNailsAOE4 or AID.ChasmicNailsAOE5)
            {
                _angles.Add((spell.Rotation, spell.NPCFinishAt));
                _angles.SortBy(a => a.activation);
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.ChasmicNailsAOE1 or AID.ChasmicNailsAOE2 or AID.ChasmicNailsAOE3 or AID.ChasmicNailsAOE4 or AID.ChasmicNailsAOE5)
            {
                ++NumCasts;
                if (_angles.Count > 0)
                    _angles.RemoveAt(0);
            }
        }
    }
}
