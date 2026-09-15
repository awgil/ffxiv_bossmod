namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

class CrushHelm(BossModule module) : BossComponent(module)
{
    public int NumSmallHits { get; private set; }
    public int NumLargeHits { get; private set; }
    private DateTime _lastSmallHit;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CrushHelmAOEFirst:
                if (WorldState.CurrentTime > _lastSmallHit.AddSeconds(0.2f))
                {
                    ++NumSmallHits;
                    _lastSmallHit = WorldState.CurrentTime;
                }
                break;
            case AID.CrushHelmAOERest:
                ++NumLargeHits;
                break;
        }
    }
}
