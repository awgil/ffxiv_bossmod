namespace BossMod.Endwalker.Alliance.A34Eulogia;

class Quintessence(BossModule module) : Components.GenericAOEs(module)
{
    private readonly (AOEShape? shape, WPos origin, Angle rotation, DateTime activation)[] _forms = [default, default, default];
    private int _numAssignedForms;

    private static readonly AOEShapeCone _shapeRight = new(50, 90.Degrees(), -90.Degrees());
    private static readonly AOEShapeCone _shapeLeft = new(50, 90.Degrees(), 90.Degrees());
    private static readonly AOEShapeDonut _shapeDonut = new(8, 50);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NumCasts + 1 < _forms.Length && _forms[NumCasts + 1] is var future && future.shape != null && future.origin != default)
            yield return new(future.shape, future.origin, future.rotation, future.activation, ArenaColor.AOE, false);
        if (NumCasts < _forms.Length && _forms[NumCasts] is var imminent && imminent.shape != null && imminent.origin != default)
            yield return new(imminent.shape, imminent.origin, imminent.rotation, imminent.activation, ArenaColor.Danger);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state != 0x00020001 || index is < 0x4C or > 0x57)
            return;

        // there are 12 possible arrows: from center to each corner (4) + pairs between neighbouring corners (8):
        //     <-- 57 ---
        //   * --- 54 --> *
        //  |^ \        / |^
        //  ||  4E    4F  ||
        // 55|    \  /   56|
        //  |52   /  \    |51
        //  ||  4D    4C  ||
        //  v| /        \ v|
        //   * <-- 50 --- *
        //     --- 53 -->
        if ((index <= 0x4F) != (_numAssignedForms == 0))
            ReportError($"Unexpected ENVC {index:X}: order {_numAssignedForms}");

        WDir offset = index switch
        {
            0x4C or 0x53 or 0x56 => new(+9, +9),
            0x4D or 0x50 or 0x55 => new(-9, +9),
            0x4E or 0x52 or 0x57 => new(-9, -9),
            0x4F or 0x51 or 0x54 => new(+9, -9),
            _ => default
        };
        var from = _numAssignedForms == 0 ? Module.Center : _forms[_numAssignedForms - 1].origin;
        var to = Module.Center + offset;
        _forms[_numAssignedForms].origin = to;
        _forms[_numAssignedForms].rotation = Angle.FromDirection(to - from);
        ++_numAssignedForms;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (order, shape) = (AID)spell.Action.ID switch
        {
            AID.FirstFormRight => (0, _shapeRight),
            AID.SecondFormRight => (1, _shapeRight),
            AID.ThirdFormRight => (2, _shapeRight),
            AID.FirstFormLeft => (0, _shapeLeft),
            AID.SecondFormLeft => (1, _shapeLeft),
            AID.ThirdFormLeft => (2, _shapeLeft),
            AID.FirstFormDonut => (0, _shapeDonut),
            AID.SecondFormDonut => (1, _shapeDonut),
            AID.ThirdFormDonut => (2, _shapeDonut),
            _ => (-1, (AOEShape?)null)
        };
        if (shape == null)
            return;

        if (_forms[order].shape != null)
            ReportError($"Duplicate shape for order {order}: {spell.Action}");

        _forms[order].shape = shape;
        _forms[order].activation = spell.NPCFinishAt.AddSeconds(19.5f - order * 3.7f);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.QuintessenceAOE1Right or AID.QuintessenceAOE1Left or AID.QuintessenceAOE1Donut or AID.QuintessenceAOE2Right or AID.QuintessenceAOE2Left or AID.QuintessenceAOE2Donut
            or AID.QuintessenceAOE3Right or AID.QuintessenceAOE3Left or AID.QuintessenceAOE3Donut)
        {
            ++NumCasts;
        }
    }
}
