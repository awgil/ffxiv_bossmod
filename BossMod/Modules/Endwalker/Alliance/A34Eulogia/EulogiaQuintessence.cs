namespace BossMod.Endwalker.Alliance.A34Eulogia;

class Quintessence(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(50, 90.Degrees());
    private static readonly AOEShapeDonut donut = new(8, 60);
    private static readonly Angle _rot1 = 135.Degrees();
    private static readonly Angle _rot2 = 180.Degrees();
    private static readonly Angle _rot3 = 90.Degrees();
    private static readonly Angle _rot4 = 45.Degrees();
    private static readonly Angle _rot5 = 0.Degrees();

    private byte _index;
    private WPos position;
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return new(_aoes[0].Shape, _aoes[0].Origin, _aoes[0].Rotation, _aoes[0].Activation, ArenaColor.Danger);
        if (_aoes.Count > 1)
            yield return new(_aoes[1].Shape, _aoes[1].Origin, _aoes[1].Rotation, _aoes[1].Activation);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001)
        {
            _index = index;
            if (index is 0x4F or 0x54 or 0x51)
                position = new WPos(954, -954);
            if (index is 0x52 or 0x57 or 0x4E)
                position = new WPos(936, -954);
            if (index is 0x55 or 0x4D or 0x50)
                position = new WPos(936, -936);
            if (index is 0x56 or 0x53 or 0x4C)
                position = new WPos(954, -936);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var _activation = _aoes.Count == 0 ? spell.NPCFinishAt.AddSeconds(19.4f) : _aoes.Count == 1 ? spell.NPCFinishAt.AddSeconds(15.7f) : spell.NPCFinishAt.AddSeconds(12.1f);
        if ((AID)spell.Action.ID == AID.FirstFormRight)
        {
            if (_index == 0x4F)
                _aoes.Add(new(cone, position, _rot4, _activation));
            if (_index == 0x4D)
                _aoes.Add(new(cone, position, -_rot1, _activation));
            if (_index == 0x4C)
                _aoes.Add(new(cone, position, -_rot4, _activation));
            if (_index == 0x4E)
                _aoes.Add(new(cone, position, _rot1, _activation));
        }
        if ((AID)spell.Action.ID == AID.FirstFormLeft)
        {
            if (_index == 0x4E)
                _aoes.Add(new(cone, position, -_rot4, _activation));
            if (_index == 0x4F)
                _aoes.Add(new(cone, position, -_rot1, _activation));
            if (_index == 0x4C)
                _aoes.Add(new(cone, position, _rot1, _activation));
            if (_index == 0x4D)
                _aoes.Add(new(cone, position, _rot4, _activation));
        }
        // known donut indices:
        // 1st form: 0x4F, 0x4C, 0x4E
        // 2nd form: 0x56, 0x51
        // 3rd form: 0x53, 0x54, 0x55, 0x57, 0x56, 0x51, 0x50
        // but since we don't need a direction for donuts, we dont need to check it
        if ((AID)spell.Action.ID is AID.FirstFormDonut or AID.SecondFormDonut or AID.ThirdFormDonut)
            _aoes.Add(new(donut, position, default, _activation));
        if ((AID)spell.Action.ID is AID.SecondFormRight or AID.ThirdFormRight)
        {
            if (_index is 0x52 or 0x51) //replay for proof of index 51 still missing
                _aoes.Add(new(cone, position, _rot3, _activation));
            if (_index is 0x57 or 0x50)
                _aoes.Add(new(cone, position, _rot2, _activation));
            if (_index is 0x54 or 0x53)
                _aoes.Add(new(cone, position, _rot5, _activation));
            if (_index is 0x56 or 0x55)
                _aoes.Add(new(cone, position, -_rot3, _activation));
        }
        if ((AID)spell.Action.ID is AID.SecondFormLeft or AID.ThirdFormLeft)
        {
            if (_index is 0x50 or 0x57)
                _aoes.Add(new(cone, position, _rot5, _activation));
            if (_index is 0x55 or 0x56)
                _aoes.Add(new(cone, position, _rot3, _activation));
            if (_index is 0x52 or 0x51)
                _aoes.Add(new(cone, position, -_rot3, _activation));
            if (_index is 0x53 or 0x54)
                _aoes.Add(new(cone, position, _rot2, _activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.QuintessenceFirstRight or AID.QuintessenceFirstLeft or AID.QuintessenceFirstDonut or AID.QuintessenceSecondRight or AID.QuintessenceSecondLeft or AID.QuintessenceSecondDonut or AID.QuintessenceThirdRight or AID.QuintessenceThirdLeft or AID.QuintessenceThirdDonut)
        {
            ++NumCasts;
            _aoes.RemoveAt(0);
        }
    }
}
