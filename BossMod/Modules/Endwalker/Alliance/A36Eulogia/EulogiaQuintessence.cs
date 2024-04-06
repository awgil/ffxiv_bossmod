namespace BossMod.Endwalker.Alliance.A36Eulogia;

class Quintessence : Components.GenericAOEs
{
    private static readonly AOEShapeCone cone = new(50, 90.Degrees());
    private static readonly AOEShapeDonut donut = new(8, 60);
    private static readonly Angle _rot1 = -135.Degrees();
    private static readonly Angle _rot2 = 180.Degrees();
    private static readonly Angle _rot3 = 90.Degrees();
    private static readonly Angle _rot4 = -45.Degrees();
    private static readonly Angle _rot5 = 135.Degrees();
    private static readonly Angle _rot6 = 0.Degrees();
    private static readonly Angle _rot7 = -90.Degrees();
    private static readonly Angle _rot8 = 45.Degrees();

    private byte _index;
    private WPos position;
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return new(_aoes[0].Shape, _aoes[0].Origin, _aoes[0].Rotation, _aoes[0].Activation, ArenaColor.Danger);
        if (_aoes.Count > 1)
            yield return new(_aoes[1].Shape, _aoes[1].Origin, _aoes[1].Rotation, _aoes[1].Activation);
    }

    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        if (state == 0x00020001)
        {
            if (index is 0x4F or 0x54 or 0x51)
            {
                _index = index;
                position = new WPos(954, -954);
            }
            if (index is 0x52 or 0x57 or 0x4E)
            {
                _index = index;
                position = new WPos(936, -954);
            }
            if (index is 0x55 or 0x4D or 0x50)
            {
                _index = index;
                position = new WPos(936, -936);
            }
            if (index is 0x56 or 0x53 or 0x4C)
            {
                _index = index;
                position = new WPos(954, -936);
            }
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var _activation1 = spell.NPCFinishAt.AddSeconds(19.4f);
        var _activation2 = spell.NPCFinishAt.AddSeconds(15.7f);
        var _activation3 = spell.NPCFinishAt.AddSeconds(12.1f);
        if ((AID)spell.Action.ID == AID.FirstFormRight)
        {
            if (_index == 0x4F)
                _aoes.Add(new(cone, position, _rot8, _activation1));
            if (_index == 0x4D)
                _aoes.Add(new(cone, position, _rot1, _activation1));
            if (_index == 0x4C)
                _aoes.Add(new(cone, position, _rot4, _activation1));
            if (_index == 0x4E)
                _aoes.Add(new(cone, position, _rot5, _activation1));
        }
        if ((AID)spell.Action.ID == AID.FirstFormLeft)
        {
            if (_index == 0x4E)
                _aoes.Add(new(cone, position, _rot4, _activation1));
            if (_index == 0x4F)
                _aoes.Add(new(cone, position, _rot1, _activation1));
            if (_index == 0x4C)
                _aoes.Add(new(cone, position, _rot5, _activation1));
            if (_index == 0x4D)
                _aoes.Add(new(cone, position, _rot8, _activation1));
        }
        if ((AID)spell.Action.ID == AID.FirstFormAOE)
        {
            if (_index is 0x4F or 0x4C or 0x4E)
                _aoes.Add(new(donut, position, activation: _activation1));
        }
        if ((AID)spell.Action.ID == AID.SecondFormRight)
        {
            if (_index is 0x57 or 0x50)
                _aoes.Add(new(cone, position, _rot2, _activation2));
            if (_index is 0x54 or 0x53)
                _aoes.Add(new(cone, position, _rot6, _activation2));
            if (_index is 0x56 or 0x55)
                _aoes.Add(new(cone, position, _rot7, _activation2));
        }
        if ((AID)spell.Action.ID == AID.SecondFormLeft)
        {
            if (_index is 0x55 or 0x56)
                _aoes.Add(new(cone, position, _rot3, _activation2));
            if (_index is 0x52 or 0x51)
                _aoes.Add(new(cone, position, _rot7, _activation2));
        }
        if ((AID)spell.Action.ID == AID.SecondFormAOE)
        {
            if (_index is 0x56 or 0x51)
                _aoes.Add(new(donut, position, activation: _activation2));
        }
        if ((AID)spell.Action.ID == AID.ThirdFormRight)
        {
            if (_index is 0x50 or 0x57)
                _aoes.Add(new(cone, position, _rot2, _activation3));
            if (_index is 0x53 or 0x54)
                _aoes.Add(new(cone, position, _rot6, _activation3));
        }
        if ((AID)spell.Action.ID == AID.ThirdFormLeft)
        {
            if (_index is 0x50 or 0x57)
                _aoes.Add(new(cone, position, _rot6, _activation3));
            if (_index is 0x55 or 0x56)
                _aoes.Add(new(cone, position, _rot3, _activation3));
            if (_index is 0x52 or 0x51)
                _aoes.Add(new(cone, position, _rot7, _activation3));
            if (_index == 0x53)
                _aoes.Add(new(cone, position, _rot2, _activation3));
        }
        if ((AID)spell.Action.ID == AID.ThirdFormAOE)
        {
            if (_index is 0x53 or 0x54 or 0x57 or 0x56 or 0x51 or 0x50)
                _aoes.Add(new(donut, position, activation: _activation3));
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.QuintessenceFirstRight or AID.QuintessenceFirstLeft or AID.QuintessenceFirstAOE or AID.QuintessenceSecondRight or AID.QuintessenceSecondLeft or AID.QuintessenceSecondAOE or AID.QuintessenceThirdRight or AID.QuintessenceThirdLeft or AID.QuintessenceThirdAOE)
        {
            ++NumCasts;
            _aoes.RemoveAt(0);
        }
    }
}
