namespace BossMod.Stormblood.Ultimate.UCOB;

class P2Cauterize(BossModule module) : Components.GenericAOEs(module)
{
    public record struct Assignment(int Order, DateTime Deadline);

    public Assignment[] BaitOrder = new Assignment[PartyState.MaxPartySize];
    public int NumBaitsAssigned;
    private int _numHypernovas;
    public List<Actor> Casters = [];
    private readonly List<(Actor actor, int position)> _dragons = []; // position 0 is N, then CW

    private static readonly AOEShapeRect _shape = new(52, 10);

    // todo: make static
    public readonly WPos[] StandardBaits = [
        new(18.149f, -9.531f),
        new(8, 18.874f),
        new(-17.667f, 10.398f)
    ];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return Casters.Select(c => new AOEInstance(_shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo)));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (BaitOrder[slot].Order >= NextBaitOrder)
            hints.Add($"Bait {BaitOrder[slot].Order}", false);
        base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        var bo = BaitOrder[slot].Order;

        if (bo >= NextBaitOrder)
        {
            if (_numHypernovas >= Math.Min(4, bo * 2 - 1))
            {
                var dir = StandardBaits[bo - 1] - actor.Position;
                hints.ForcedMovement = dir.LengthSq() > 0.1f ? dir.ToVec3() : new(0);
                // TODO: goal cell is too close to the arena border so preciseposition can't handle it, what do we do here 
                //hints.AddForbiddenZone(ShapeDistance.PrecisePosition(StandardBaits[bo - 1], new(0, 1), 0.5f, actor.Position, 0.1f), BaitOrder[slot].Deadline);
            }
            else
                hints.AddForbiddenZone(Sdf.Continuous(ShapeDistance.Donut(StandardBaits[bo - 1], 5, 7)).Inverted(), BaitOrder[slot].Deadline);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (BaitOrder[pcSlot].Order >= NextBaitOrder)
        {
            foreach (var d in DragonsForOrder(BaitOrder[pcSlot].Order))
            {
                Arena.ActorInsideBounds(d.Position, d.Rotation, ArenaColor.Object);
                _shape.Outline(Arena, d.Position, Angle.FromDirection(pc.Position - d.Position));
            }

            Arena.AddCircle(StandardBaits[BaitOrder[pcSlot].Order - 1], 0.5f, ArenaColor.Safe);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.Firehorn or OID.Iceclaw or OID.Thunderwing or OID.TailOfDarkness or OID.FangOfLight)
        {
            var dir = 180.Degrees() - Angle.FromDirection(actor.Position - Module.Center);
            var pos = (int)MathF.Round(dir.Deg / 45) & 7;
            _dragons.Add((actor, pos));
            if (_dragons.Count == 5)
            {
                // sort by direction
                _dragons.SortBy(d => d.position);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Cauterize1 or AID.Cauterize2 or AID.Cauterize3 or AID.Cauterize4 or AID.Cauterize5)
        {
            Casters.Add(caster);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Cauterize1 or AID.Cauterize2 or AID.Cauterize3 or AID.Cauterize4 or AID.Cauterize5)
        {
            Casters.Remove(caster);
            ++NumCasts;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID.Hypernova)
            _numHypernovas++;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID is IconID.Cauterize && Raid.TryFindSlot(actor.InstanceID, out var slot))
        {
            BaitOrder[slot] = new(++NumBaitsAssigned, WorldState.FutureTime(7.2f));
        }
    }

    private int NextBaitOrder => (Casters.Count + NumCasts) switch
    {
        0 => 1,
        1 or 2 => 2,
        3 => 3,
        _ => 4
    };

    private IEnumerable<Actor> DragonsForOrder(int order)
    {
        if (_dragons.Count != 5)
            yield break;
        switch (order)
        {
            case 1:
                yield return _dragons[0].actor;
                yield return _dragons[1].actor;
                break;
            case 2:
                yield return _dragons[2].actor;
                break;
            case 3:
                yield return _dragons[3].actor;
                yield return _dragons[4].actor;
                break;
        }
    }
}

class P2Hypernova(BossModule module) : Components.VoidzoneAtCastTarget(module, 5, AID.Hypernova, OID.VoidzoneHypernova, 1.4f, activationDelay: 2.1f);
