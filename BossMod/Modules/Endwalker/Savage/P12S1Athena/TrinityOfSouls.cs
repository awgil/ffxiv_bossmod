using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P12S1Athena
{
    class TrinityOfSouls : Components.GenericAOEs
    {
        private bool _invertMiddle;
        private uint _moves; // bit 0 - move after first, bit1 - move after second
        private List<AOEInstance> _aoes = new();

        private static AOEShapeCone _shape = new(60, 90.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(1);

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (_aoes.Count == 3 || NumCasts > 0)
            {
                var hint = _moves switch
                {
                    0 => "stay",
                    1 => "move after first",
                    2 => "move before last",
                    _ => "move twice"
                };
                hints.Add($"Trinity: {hint}");
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var (angle, invert) = (AID)spell.Action.ID switch
            {
                AID.TrinityOfSoulsDirectTR => (-90.Degrees(), false),
                AID.TrinityOfSoulsDirectTL => (90.Degrees(), false),
                AID.TrinityOfSoulsInvertBR => (-90.Degrees(), true),
                AID.TrinityOfSoulsInvertBL => (90.Degrees(), true),
                _ => (new Angle(), false)
            };
            if (angle != default)
            {
                _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.FinishAt));
                _invertMiddle = invert;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.TrinityOfSoulsDirectTR or AID.TrinityOfSoulsDirectTL or AID.TrinityOfSoulsDirectMR or AID.TrinityOfSoulsDirectML or AID.TrinityOfSoulsDirectBR or AID.TrinityOfSoulsDirectBL
                or AID.TrinityOfSoulsInvertBR or AID.TrinityOfSoulsInvertBL or AID.TrinityOfSoulsInvertMR or AID.TrinityOfSoulsInvertML or AID.TrinityOfSoulsInvertTR or AID.TrinityOfSoulsInvertTL)
            {
                if (_aoes.Count > 0)
                    _aoes.RemoveAt(0);
                ++NumCasts;
            }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            switch ((IconID)iconID)
            {
                case IconID.WingTLFirst:
                    VerifyFirstAOE(module, 90.Degrees(), false);
                    break;
                case IconID.WingTRFirst:
                    VerifyFirstAOE(module, -90.Degrees(), false);
                    break;
                case IconID.WingML:
                    AddSubsequentAOE(module, 90.Degrees(), false);
                    break;
                case IconID.WingMR:
                    AddSubsequentAOE(module, -90.Degrees(), false);
                    break;
                case IconID.WingBLFirst:
                    VerifyFirstAOE(module, 90.Degrees(), true);
                    break;
                case IconID.WingBRFirst:
                    VerifyFirstAOE(module, -90.Degrees(), true);
                    break;
                case IconID.WingTLLast:
                    AddSubsequentAOE(module, 90.Degrees(), true);
                    break;
                case IconID.WingTRLast:
                    AddSubsequentAOE(module, -90.Degrees(), true);
                    break;
                case IconID.WingBLLast:
                    AddSubsequentAOE(module, 90.Degrees(), true);
                    break;
                case IconID.WingBRLast:
                    AddSubsequentAOE(module, -90.Degrees(), true);
                    break;
            }
        }

        private void VerifyFirstAOE(BossModule module, Angle offset, bool inverted)
        {
            if (_aoes.Count == 0)
            {
                module.ReportError(this, "No AOEs active");
                return;
            }
            if (!module.PrimaryActor.Position.AlmostEqual(_aoes[0].Origin, 1))
                module.ReportError(this, $"Unexpected boss position: expected {_aoes[0].Origin}, have {module.PrimaryActor.Position}");
            if (!_aoes[0].Rotation.AlmostEqual(module.PrimaryActor.Rotation + offset, 0.05f))
                module.ReportError(this, $"Unexpected first aoe angle: expected {module.PrimaryActor.Rotation}+{offset}, have {_aoes[0].Rotation}");
            if (_invertMiddle != inverted)
                module.ReportError(this, $"Unexpected invert: expected {inverted}, have {_invertMiddle}");
        }

        private void AddSubsequentAOE(BossModule module, Angle offset, bool last)
        {
            var expectedCount = last ? 2 : 1;
            if (_aoes.Count != expectedCount)
                module.ReportError(this, $"Unexpected AOE count: expected {expectedCount}, got {_aoes.Count}");
            if (_aoes.Count > 0 && !module.PrimaryActor.Position.AlmostEqual(_aoes[0].Origin, 1))
                module.ReportError(this, $"Unexpected boss position: expected {_aoes[0].Origin}, have {module.PrimaryActor.Position}");

            var rotation = (!last && _invertMiddle) ? module.PrimaryActor.Rotation - offset : module.PrimaryActor.Rotation + offset;
            _aoes.Add(new(_shape, module.PrimaryActor.Position, rotation, _aoes.LastOrDefault().Activation.AddSeconds(2.6f)));
            if (_aoes.Count > 1 && !_aoes[_aoes.Count - 1].Rotation.AlmostEqual(_aoes[_aoes.Count - 2].Rotation, 0.05f))
                _moves |= last ? 2u : 1u;
        }
    }
}
