namespace BossMod.Endwalker.Savage.P12S1Athena;

class TrinityOfSouls(BossModule module) : Components.GenericAOEs(module)
{
    private bool _invertMiddle;
    private uint _moves; // bit 0 - move after first, bit1 - move after second
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(60, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void AddGlobalHints(GlobalHints hints)
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

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
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
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
            _invertMiddle = invert;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TrinityOfSoulsDirectTR or AID.TrinityOfSoulsDirectTL or AID.TrinityOfSoulsDirectMR or AID.TrinityOfSoulsDirectML or AID.TrinityOfSoulsDirectBR or AID.TrinityOfSoulsDirectBL
            or AID.TrinityOfSoulsInvertBR or AID.TrinityOfSoulsInvertBL or AID.TrinityOfSoulsInvertMR or AID.TrinityOfSoulsInvertML or AID.TrinityOfSoulsInvertTR or AID.TrinityOfSoulsInvertTL)
        {
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
            ++NumCasts;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        switch ((IconID)iconID)
        {
            case IconID.WingTLFirst:
                VerifyFirstAOE(90.Degrees(), false);
                break;
            case IconID.WingTRFirst:
                VerifyFirstAOE(-90.Degrees(), false);
                break;
            case IconID.WingML:
                AddSubsequentAOE(90.Degrees(), false);
                break;
            case IconID.WingMR:
                AddSubsequentAOE(-90.Degrees(), false);
                break;
            case IconID.WingBLFirst:
                VerifyFirstAOE(90.Degrees(), true);
                break;
            case IconID.WingBRFirst:
                VerifyFirstAOE(-90.Degrees(), true);
                break;
            case IconID.WingTLLast:
                AddSubsequentAOE(90.Degrees(), true);
                break;
            case IconID.WingTRLast:
                AddSubsequentAOE(-90.Degrees(), true);
                break;
            case IconID.WingBLLast:
                AddSubsequentAOE(90.Degrees(), true);
                break;
            case IconID.WingBRLast:
                AddSubsequentAOE(-90.Degrees(), true);
                break;
        }
    }

    private void VerifyFirstAOE(Angle offset, bool inverted)
    {
        if (_aoes.Count == 0)
        {
            ReportError("No AOEs active");
            return;
        }
        if (!Module.PrimaryActor.Position.AlmostEqual(_aoes[0].Origin, 1))
            ReportError($"Unexpected boss position: expected {_aoes[0].Origin}, have {Module.PrimaryActor.Position}");
        if (!_aoes[0].Rotation.AlmostEqual(Module.PrimaryActor.Rotation + offset, 0.05f))
            ReportError($"Unexpected first aoe angle: expected {Module.PrimaryActor.Rotation}+{offset}, have {_aoes[0].Rotation}");
        if (_invertMiddle != inverted)
            ReportError($"Unexpected invert: expected {inverted}, have {_invertMiddle}");
    }

    private void AddSubsequentAOE(Angle offset, bool last)
    {
        var expectedCount = last ? 2 : 1;
        if (_aoes.Count != expectedCount)
            ReportError($"Unexpected AOE count: expected {expectedCount}, got {_aoes.Count}");
        if (_aoes.Count > 0 && !Module.PrimaryActor.Position.AlmostEqual(_aoes[0].Origin, 1))
            ReportError($"Unexpected boss position: expected {_aoes[0].Origin}, have {Module.PrimaryActor.Position}");

        var rotation = (!last && _invertMiddle) ? Module.PrimaryActor.Rotation - offset : Module.PrimaryActor.Rotation + offset;
        _aoes.Add(new(_shape, Module.PrimaryActor.Position, rotation, _aoes.LastOrDefault().Activation.AddSeconds(2.6f)));
        if (_aoes.Count > 1 && !_aoes[^1].Rotation.AlmostEqual(_aoes[^2].Rotation, 0.05f))
            _moves |= last ? 2u : 1u;
    }
}
