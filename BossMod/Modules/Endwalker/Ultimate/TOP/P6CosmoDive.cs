namespace BossMod.Endwalker.Ultimate.TOP;

class P6CosmoDive(BossModule module) : Components.UniformStackSpread(module, 6, 8, 6, 6, true)
{
    private Actor? _source;
    private DateTime _activation;

    public override void Update()
    {
        Spreads.Clear();
        Stacks.Clear();
        if (_source != null)
        {
            BitMask forbidden = new();
            foreach (var (slot, actor) in Raid.WithSlot().SortedByRange(_source.Position).Take(2))
            {
                AddSpread(actor, _activation);
                forbidden.Set(slot);
            }
            var farthest = Raid.WithoutSlot().Farthest(_source.Position);
            if (farthest != null)
            {
                AddStack(farthest, _activation, forbidden);
            }
        }
        base.Update();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CosmoDive)
        {
            _source = caster;
            _activation = spell.NPCFinishAt.AddSeconds(2.5f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CosmoDiveTankbuster or AID.CosmoDiveStack)
        {
            _source = null;
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}
