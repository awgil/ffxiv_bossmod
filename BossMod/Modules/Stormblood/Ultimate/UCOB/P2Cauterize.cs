namespace BossMod.Stormblood.Ultimate.UCOB;

class P2Cauterize : Components.GenericAOEs
{
    public int[] BaitOrder = new int[PartyState.MaxPartySize];
    public int NumBaitsAssigned;
    public List<Actor> Casters = new();
    private List<(Actor actor, int position)> _dragons = new(); // position 0 is N, then CW

    private static readonly AOEShapeRect _shape = new(52, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        return Casters.Select(c => new AOEInstance(_shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt));
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (BaitOrder[slot] >= NextBaitOrder)
            hints.Add($"Bait {BaitOrder[slot]}", false);
        base.AddHints(module, slot, actor, hints, movementHints);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (BaitOrder[pcSlot] >= NextBaitOrder)
        {
            foreach (var d in DragonsForOrder(BaitOrder[pcSlot]))
            {
                arena.Actor(d, ArenaColor.Object, true);
                _shape.Outline(arena, d.Position, Angle.FromDirection(pc.Position - d.Position));
            }
            // TODO: safe spots
        }
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID is OID.Firehorn or OID.Iceclaw or OID.Thunderwing or OID.TailOfDarkness or OID.FangOfLight)
        {
            var dir = 180.Degrees() - Angle.FromDirection(actor.Position - module.Bounds.Center);
            var pos = (int)MathF.Round(dir.Deg / 45) & 7;
            _dragons.Add((actor, pos));
            if (_dragons.Count == 5)
            {
                // sort by direction
                _dragons.SortBy(d => d.position);
            }
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Cauterize1 or AID.Cauterize2 or AID.Cauterize3 or AID.Cauterize4 or AID.Cauterize5)
        {
            Casters.Add(caster);
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Cauterize1 or AID.Cauterize2 or AID.Cauterize3 or AID.Cauterize4 or AID.Cauterize5)
        {
            Casters.Remove(caster);
            ++NumCasts;
        }
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if ((IconID)iconID is IconID.Cauterize && module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
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

class P2Hypernova : Components.PersistentVoidzoneAtCastTarget
{
    public P2Hypernova() : base(5, ActionID.MakeSpell(AID.Hypernova), m => m.Enemies(OID.VoidzoneHypernova).Where(z => z.EventState != 7), 1.4f) { }
}
