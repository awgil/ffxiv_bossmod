namespace BossMod.Endwalker.Trial.T08Asura;

class ManyFaces : Components.GenericAOEs
{
    private static readonly AOEShapeCone cone = new(20, 90.Degrees());
    private DateTime _activation;
    private bool delight;
    private bool wrath;
    private Angle _rotationWrath;
    private Angle _rotationDelight;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (delight)
            yield return new(cone, module.Bounds.Center, _rotationDelight, activation: _activation);
        if (wrath)
            yield return new(cone, module.Bounds.Center, _rotationWrath, activation: _activation);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TheFaceOfWrathA or AID.TheFaceOfWrathB)
            delight = true;
        if ((AID)spell.Action.ID is AID.TheFaceOfDelightA or AID.TheFaceOfDelightB)
            wrath = true;
        if ((AID)spell.Action.ID == AID.FaceMechanicWrath)
        {
            _activation = spell.NPCFinishAt;
            _rotationDelight = spell.Rotation;
        }
        if ((AID)spell.Action.ID == AID.FaceMechanicDelight)
        {
            _activation = spell.NPCFinishAt;
            _rotationWrath = spell.Rotation;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TheFaceOfDelightSnapshot or AID.TheFaceOfWrathSnapshot)
        {
            delight = false;
            wrath = false;
        }
    }
}
