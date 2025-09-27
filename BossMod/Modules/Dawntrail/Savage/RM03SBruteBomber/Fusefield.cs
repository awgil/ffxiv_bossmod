namespace BossMod.Dawntrail.Savage.RM03SBruteBomber;

class Fusefield(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor spark, Actor target, int order)> _sparks = [];
    private readonly int[] _orders = new int[PartyState.MaxPartySize];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_orders[slot] > 0)
            hints.Add($"Order: {_orders[slot]}", false);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var s in _sparks)
        {
            if (s.order == _orders[pcSlot])
            {
                Arena.AddLine(s.spark.Position, s.target.Position, ArenaColor.Safe);
                Arena.Actor(s.spark, ArenaColor.Object, true);
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Bombarium && Raid.TryFindSlot(actor.InstanceID, out var slot))
            _orders[slot] = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds < 30 ? 1 : 2;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Bombarium && Raid.TryFindSlot(actor.InstanceID, out var slot))
            _orders[slot] = 0;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.SinisterSpark && tether.ID == (uint)TetherID.Fusefield && WorldState.Actors.Find(tether.Target) is var target && target != null)
            _sparks.Add((source, target, (source.Position - target.Position).LengthSq() < 55 ? 1 : 2));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ManaExplosion or AID.ManaExplosionKill)
        {
            _sparks.RemoveAll(s => s.spark == caster);
        }
    }
}

class FusefieldVoidzone(BossModule module) : Components.PersistentVoidzone(module, 5, m => m.Enemies(OID.Boss))
{
    public bool Active;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 19 && state is 0x00200010 or 0x00080004)
            Active = state == 0x00200010;
    }
}
