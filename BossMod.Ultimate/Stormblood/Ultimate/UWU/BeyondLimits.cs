namespace BossMod.Stormblood.Ultimate.UWU;

class BeyondLimits : Components.GenericAOEs
{
    int _soaker;
    WPos? _position;

    private readonly int[] SoakOrder = Utils.MakeArray(3, -1);

    public BeyondLimits(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;

        foreach (var (slot, group) in Service.Config.Get<UWUConfig>().BeyondLimits.Resolve(Raid))
            if (group >= 0)
                SoakOrder[group] = slot;
    }

    bool IsActive => !WorldState.Actors.Any(a => a.CastInfo is { } ci && (ci.IsSpell(AID.FeatherRain) || ci.IsSpell(AID.WeightOfTheLandAOE)));

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => IsActive ? Utils.ZeroOrOne(_position).Select(p => new AOEInstance(new AOEShapeCircle(1), p, Inverted: slot == _soaker)) : [];

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E39 && actor.HPMP.CurHP == 1 && actor.FindStatus(SID.Woken) != null)
        {
            switch ((OID)actor.OID)
            {
                case OID.Garuda:
                    _position = actor.Position;
                    _soaker = SoakOrder[0];
                    break;
                case OID.Ifrit:
                    _position = actor.Position;
                    _soaker = SoakOrder[1];
                    break;
                case OID.Titan:
                    _position = actor.Position;
                    _soaker = SoakOrder[2];
                    break;
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BeyondLimits)
            _position = actor.Position;
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.BeyondLimits)
        {
            _soaker = -1;
            _position = null;
        }
    }
}
