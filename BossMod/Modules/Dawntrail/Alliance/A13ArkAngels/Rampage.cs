namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class Rampage(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCircle _shapeLast = new(20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.RampagePreviewCharge:
                var toDest = spell.LocXZ - caster.Position;
                AOEs.Add(new(new AOEShapeRect(toDest.Length(), 5), caster.Position, Angle.FromDirection(toDest), Module.CastFinishAt(spell, 5.1f + 0.2f * AOEs.Count)));
                break;
            case AID.RampagePreviewLast:
                AOEs.Add(new(_shapeLast, spell.LocXZ, default, Module.CastFinishAt(spell, 6.3f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RampageAOECharge or AID.RampageAOELast)
        {
            ++NumCasts;
            if (AOEs.Count > 0)
                AOEs.RemoveAt(0);
        }
    }
}
