namespace BossMod.Endwalker.Ultimate.TOP;

class P6CosmoDive : Components.UniformStackSpread
{
    private Actor? _source;
    private DateTime _activation;

    public P6CosmoDive() : base(6, 8, 6, 6, true) { }

    public override void Update(BossModule module)
    {
        Spreads.Clear();
        Stacks.Clear();
        if (_source != null)
        {
            BitMask forbidden = new();
            foreach (var (slot, actor) in module.Raid.WithSlot().SortedByRange(_source.Position).Take(2))
            {
                AddSpread(actor, _activation);
                forbidden.Set(slot);
            }
            var farthest = module.Raid.WithoutSlot().Farthest(_source.Position);
            if (farthest != null)
            {
                AddStack(farthest, _activation, forbidden);
            }
        }
        base.Update(module);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CosmoDive)
        {
            _source = caster;
            _activation = spell.NPCFinishAt.AddSeconds(2.5f);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CosmoDiveTankbuster or AID.CosmoDiveStack)
        {
            _source = null;
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}
