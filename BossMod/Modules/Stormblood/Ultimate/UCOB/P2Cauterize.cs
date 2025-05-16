namespace BossMod.Stormblood.Ultimate.UCOB;

class P2Cauterize(BossModule module) : Components.GenericAOEs(module)
{
    public int[] BaitOrder = new int[PartyState.MaxPartySize];
    public int NumBaitsAssigned;
    public List<Actor> Casters = [];
    private readonly List<(Actor actor, int position)> _dragons = []; // position 0 is N, then CW

    private static readonly AOEShapeRect _shape = new(52, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return Casters.Select(c => new AOEInstance(_shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo)));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (BaitOrder[slot] >= NextBaitOrder)
            hints.Add($"Bait {BaitOrder[slot]}", false);
        base.AddHints(slot, actor, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (BaitOrder[pcSlot] >= NextBaitOrder)
        {
            foreach (var d in DragonsForOrder(BaitOrder[pcSlot]))
            {
                Arena.Actor(d, ArenaColor.Object, true);
                _shape.Outline(Arena, d.Position, Angle.FromDirection(pc.Position - d.Position));
            }
            // TODO: safe spots
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

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID is IconID.Cauterize && Raid.TryFindSlot(actor.InstanceID, out var slot))
        {
            BaitOrder[slot] = ++NumBaitsAssigned;
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

class P2Hypernova(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID.Hypernova, m => m.Enemies(OID.VoidzoneHypernova).Where(z => z.EventState != 7), 1.4f);
