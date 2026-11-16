namespace BossMod.Dawntrail.Unreal.Un2Shinryu;

class CocoonMarker(BossModule module) : Components.UniformStackSpread(module, 0, 10, alwaysShowSpreads: true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.CocoonMarker)
            AddSpread(actor, WorldState.FutureTime(8));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MeteorImpactMarker)
            Spreads.Clear();
    }

    public override void Update() => Spreads.RemoveAll(s => s.Activation.AddSeconds(1) < WorldState.CurrentTime);
}
