namespace BossMod.Stormblood.Ultimate.UCOB;

class P3TenstrikeMeteorStream : MeteorStream
{
    public bool HatchAssigned;
    int _numHatches;
    private readonly int[] _order = Utils.MakeArray(PartyState.MaxPartySize, -1);

    public P3TenstrikeMeteorStream(BossModule module) : base(module)
    {
        AddSpreads(Raid.WithoutSlot(true), WorldState.FutureTime(3.2f));
        foreach (var (slot, group) in Service.Config.Get<UCOBConfig>().P3QuickmarchTrioAssignments.Resolve(Raid))
            _order[slot] = group;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!HatchAssigned)
        {
            var order = _order[slot];
            if (order >= 0)
            {
                var sign = order > 3 ? -1 : 1;

                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center + (180 + (22.5f + 45 * (order % 4)) * sign).Degrees().ToDirection() * 9, 1));
            }

            return;
        }

        if (Module.FindComponent<Hatch>()?.IsTarget(slot) == true)
            return;

        if (_numHatches >= 6)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center, 8));

        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MeteorStream)
        {
            ++NumCasts;
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
            for (var i = 0; i < Spreads.Count; i++)
                Spreads.Ref(i).Activation = WorldState.FutureTime(1);
        }

        if ((AID)spell.Action.ID == AID.Hatch)
            _numHatches++;
    }
}
