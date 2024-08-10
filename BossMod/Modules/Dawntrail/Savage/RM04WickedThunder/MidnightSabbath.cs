namespace BossMod.Dawntrail.Savage.RM04WickedThunder;

class MidnightSabbath(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShape _shapeCannon = new AOEShapeRect(40, 5);
    private static readonly AOEShape _shapeBird = new AOEShapeDonut(5, 15);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Skip(NumCasts).Take(4);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID != OID.WickedReplica)
            return;
        var (shape, delay) = id switch
        {
            0x11D1 => (_shapeCannon, 8.1f),
            0x11D2 => (_shapeCannon, 12.1f),
            0x11D3 => (_shapeBird, 8.1f),
            0x11D4 => (_shapeBird, 12.1f),
            _ => default
        };
        if (shape != default)
        {
            AOEs.Add(new(shape, actor.Position, actor.Rotation, WorldState.FutureTime(delay)));
            AOEs.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.MidnightSabbathThundering or AID.MidnightSabbathWickedCannon)
            ++NumCasts;
    }
}

class ConcentratedScatteredBurst(BossModule module) : Components.UniformStackSpread(module, 5, 5)
{
    public int NumFinishedStacks;
    public int NumFinishedSpreads;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ConcentratedBurst:
                ShowStacks(Module.CastFinishAt(spell, 0.1f));
                break;
            case AID.ScatteredBurst:
                ShowSpreads(Module.CastFinishAt(spell, 0.1f));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.WickedSpark:
                ++NumFinishedSpreads;
                Spreads.Clear();
                if (NumFinishedStacks == 0)
                    ShowStacks(WorldState.FutureTime(3.1f));
                break;
            case AID.WickedFlare:
                ++NumFinishedStacks;
                Stacks.Clear();
                if (NumFinishedSpreads == 0)
                    ShowSpreads(WorldState.FutureTime(3.1f));
                break;
        }
    }

    private void ShowStacks(DateTime activation)
    {
        // TODO: can target any role
        AddStacks(Raid.WithoutSlot(true).Where(p => p.Class.IsSupport()), activation);
    }

    private void ShowSpreads(DateTime activation) => AddSpreads(Raid.WithoutSlot(true), activation);
}
