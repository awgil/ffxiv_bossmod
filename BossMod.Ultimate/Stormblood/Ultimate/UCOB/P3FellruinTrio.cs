namespace BossMod.Stormblood.Ultimate.UCOB;

class P3AethericProfusion : Components.CastCounter
{
    public bool Active;
    private DateTime _deadline;
    private WPos RelativeNorth;

    // OT -> party -> MT (using clockwise order)
    private readonly List<Actor> _neurolinksSorted = [];
    private PartyRolesConfig.Assignment[] _assignments = [];

    public P3AethericProfusion(BossModule module) : base(module, AID.AethericProfusion)
    {
        _deadline = WorldState.FutureTime(14);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AethericProfusion)
            _deadline = Module.CastFinishAt(spell);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var myLink = _assignments.Length == 0 ? -1 : _assignments[pcSlot] switch
        {
            PartyRolesConfig.Assignment.MT => 2,
            PartyRolesConfig.Assignment.OT => 0,
            _ => 1
        };
        for (var i = 0; i < _neurolinksSorted.Count; i++)
            Arena.AddCircle(_neurolinksSorted[i].Position, 2, i == myLink ? ArenaColor.Safe : ArenaColor.Danger);

        var ucob = (UCOB)Module;
        if (ucob.BahamutPrime() is { } baha)
            Arena.Actor(baha, ArenaColor.Enemy, true);

        if (ucob.Nael() is { } nael)
            Arena.Actor(nael, ArenaColor.Object, true);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Active)
            return;

        var myLink = _neurolinksSorted[assignment switch
        {
            PartyRolesConfig.Assignment.MT => 2,
            PartyRolesConfig.Assignment.OT => 0,
            _ => 1
        }];

        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(myLink.Position, 2), _deadline);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.BahamutPrime && id == 0x1E43 && RelativeNorth == default)
        {
            RelativeNorth = actor.Position;
            _neurolinksSorted.AddRange(Module.Enemies(OID.Neurolink).ClockOrder(actor, Arena.Center));
            _assignments = Service.Config.Get<PartyRolesConfig>().AssignmentsPerSlot(Raid);

            if (Module.FindComponent<P3BahamutPositioning>() is { } bp)
            {
                bp.DesiredPosition = actor.Position;
                bp.DesiredRotation = (actor.Position - Arena.Center).ToAngle();
            }
        }
    }
}

class P3DynamoTetherHelper(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.FindComponent<Quote>() is { PendingMechanics: [AID.LunarDynamo, ..], NextActivation: var nextActivation } && assignment is not (PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT) && ((UCOB)Module).BahamutPrime() is { } baha && ((UCOB)Module).Nael() is { } nael)
        {
            hints.AddForbiddenZone(ShapeDistance.Circle(baha.Position, (baha.Position - nael.Position).Length()), nextActivation);
            // if squishies dodge outside the donut with tether that's game over
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(nael.Position, 10), nextActivation);
        }
    }
}
