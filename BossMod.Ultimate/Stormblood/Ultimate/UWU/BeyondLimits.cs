namespace BossMod.Stormblood.Ultimate.UWU;

class BeyondLimits : Components.GenericAOEs
{
    int _soaker;
    WPos? _position;

    readonly PartyRolesConfig.Assignment[] Assignments;

    public BeyondLimits(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;

        Assignments = Service.Config.Get<PartyRolesConfig>().AssignmentsPerSlot(Raid);
        if (Assignments.Length == 0)
            Assignments = new PartyRolesConfig.Assignment[8];
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
                    _soaker = Assignments.IndexOf(PartyRolesConfig.Assignment.H1);
                    break;
                case OID.Ifrit:
                    _position = actor.Position;
                    _soaker = Assignments.IndexOf(PartyRolesConfig.Assignment.R2);
                    break;
                case OID.Titan:
                    _position = actor.Position;
                    _soaker = Assignments.IndexOf(PartyRolesConfig.Assignment.M1);
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
