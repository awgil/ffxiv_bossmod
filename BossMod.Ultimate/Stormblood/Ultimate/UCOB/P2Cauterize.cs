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

    public bool Cursed { get; private set; }

    public static readonly WPos[] StandardBaits = [
        new(19.035f, -10),
        new(8.39f, 19.795f),
        new(-18.532f, 10.901f)
    ];

    public WPos[] CurrentBaits = [];

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
            // bait is imminent
            if (bo == 1 && _numHypernovas >= 1 || bo == 2 && _numHypernovas >= 4 || bo == 3)
            {
                hints.PathfindMapBounds = UCOB.PathfindHugBorderBounds;
                hints.AddForbiddenZone(ShapeDistance.PrecisePosition(CurrentBaits[bo - 1], new(0, 1), 0.5f, actor.Position, 0.1f), BaitOrder[slot].Deadline);
            }
            else
            {
                hints.AddForbiddenZone(Sdf.Continuous(ShapeDistance.Donut(CurrentBaits[bo - 1], 5, 7)).Inverted(), BaitOrder[slot].Deadline.AddSeconds(-2));

                // hypernova drop timing is not actually consistent pull to pull, because whoever designed this mechanic is a genius
                // thus if you drop h3 5 units away, and h4 directly under the dragon, as recommended, h4 may kill you especially at higher latency (or rather, the player would have to move before receiving the cast start event from the server to survive it, which would be fine if the cast start timings were predictable, etc etc)
                // instead we do an awkward L shape where baiter 2 spends most of their dodge trajectory along the edge of the arena, which should hopefully be enough to not kill baiter 3
                if (bo == 2 && _numHypernovas == 3)
                {
                    hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 20), BaitOrder[slot].Deadline.AddSeconds(-2));
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(0, 22), 9), BaitOrder[slot].Deadline.AddSeconds(-2));
                }
            }
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

            Arena.AddCircle(CurrentBaits[BaitOrder[pcSlot].Order - 1], 0.5f, ArenaColor.Safe);
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

            if (NumBaitsAssigned == 1)
            {
                if (_dragons.Count(d => d.actor.Position.InCone(Arena.Center, 112.5f.Degrees(), 90.Degrees())) == 1)
                {
                    // cursed pattern: second dragon is true S; flipping standard baits horizontally will resolve the mechanic hopefully without killing anyone
                    CurrentBaits = [StandardBaits[2], StandardBaits[1], StandardBaits[0]];
                    for (var i = 0; i < CurrentBaits.Length; i++)
                        CurrentBaits[i].X *= -1;
                    Cursed = true;
                }
                else
                {
                    CurrentBaits = StandardBaits;
                }
            }
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

class P2Hypernova(BossModule module) : Components.VoidzoneAtCastTarget(module, 5, AID.Hypernova, OID.VoidzoneHypernova, 1.4f, activationDelay: 2);
