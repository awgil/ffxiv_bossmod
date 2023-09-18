using System.Collections.Generic;

namespace BossMod.Endwalker.Criterion.C02AMR.C020Trash2
{
    class BladeOfTheTengu : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeCone _shape = new(50, 45.Degrees()); // TODO: verify angle

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var secondAngle = (AID)spell.Action.ID switch
            {
                AID.NBackwardBlows or AID.SBackwardBlows => 180.Degrees(),
                AID.NLeftwardBlows or AID.SLeftwardBlows => 90.Degrees(),
                AID.NRightwardBlows or AID.SRightwardBlows => -90.Degrees(),
                _ => default
            };
            if (secondAngle != default)
            {
                NumCasts = 0;
                _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.FinishAt.AddSeconds(0.1f)));
                _aoes.Add(new(_shape, caster.Position, spell.Rotation + secondAngle, spell.FinishAt.AddSeconds(1.9f)));
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.NBladeOfTheTengu or AID.SBladeOfTheTengu)
            {
                if (_aoes.Count > 0)
                    _aoes.RemoveAt(0);
                ++NumCasts;
            }
        }
    }

    class WrathOfTheTengu : Components.RaidwideCast
    {
        public WrathOfTheTengu(AID aid) : base(ActionID.MakeSpell(aid), "Raidwide with bleed") { }
    }
    class NWrathOfTheTengu : WrathOfTheTengu { public NWrathOfTheTengu() : base(AID.NWrathOfTheTengu) { } }
    class SWrathOfTheTengu : WrathOfTheTengu { public SWrathOfTheTengu() : base(AID.SWrathOfTheTengu) { } }

    class GazeOfTheTengu : Components.CastGaze
    {
        public GazeOfTheTengu(AID aid) : base(ActionID.MakeSpell(aid)) { }
    }
    class NGazeOfTheTengu : GazeOfTheTengu { public NGazeOfTheTengu() : base(AID.NGazeOfTheTengu) { } }
    class SGazeOfTheTengu : GazeOfTheTengu { public SGazeOfTheTengu() : base(AID.SGazeOfTheTengu) { } }

    class C020KotenguStates : StateMachineBuilder
    {
        private bool _savage;

        public C020KotenguStates(BossModule module, bool savage) : base(module)
        {
            _savage = savage;
            TrivialPhase()
                .ActivateOnEnter<BladeOfTheTengu>()
                .ActivateOnEnter<NWrathOfTheTengu>(!savage)
                .ActivateOnEnter<NGazeOfTheTengu>(!savage)
                .ActivateOnEnter<SWrathOfTheTengu>(savage)
                .ActivateOnEnter<SGazeOfTheTengu>(savage)
                // for yamabiko
                .ActivateOnEnter<NMountainBreeze>(!savage)
                .ActivateOnEnter<SMountainBreeze>(savage);
        }
    }
    class C020NKotenguStates : C020KotenguStates { public C020NKotenguStates(BossModule module) : base(module, false) { } }
    class C020SKotenguStates : C020KotenguStates { public C020SKotenguStates(BossModule module) : base(module, true) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.NKotengu)]
    public class C020NKotengu : C020Trash2 { public C020NKotengu(WorldState ws, Actor primary) : base(ws, primary) { } }

    [ModuleInfo(PrimaryActorOID = (uint)OID.SKotengu)]
    public class C020SKotengu : C020Trash2 { public C020SKotengu(WorldState ws, Actor primary) : base(ws, primary) { } }
}
