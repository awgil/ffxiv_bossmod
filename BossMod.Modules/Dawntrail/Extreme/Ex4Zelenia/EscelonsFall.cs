using System.Text;

namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

// TODO: add "you should bait now!" hints
class EscelonsFall : Components.GenericBaitAway
{
    enum Proximity
    {
        Close,
        Far
    }

    public bool Active => Order.Count > 0;
    public int NumVolleys { get; private set; }

    private readonly List<Proximity> Order = [];
    private readonly List<Proximity> HintOrder = []; // not cleared, component should be reloaded instead

    private readonly DateTime[] Vulns = new DateTime[PartyState.MaxPartySize];
    private DateTime NextActivation; // 13.9s from boss cast, then every 3.1s
    private bool VolleyPending;
    private uint VolleySequence;

    public EscelonsFall(BossModule module) : base(module, AID.EscelonsFall)
    {
        AllowDeadTargets = false;
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((OID)actor.OID == OID.Boss && status.ID == (uint)SID.WitchHunt)
        {
            Order.Add(status.Extra == 0x2F6 ? Proximity.Close : Proximity.Far);
            HintOrder.Add(Order[^1]);
        }

        if (status.ID == (uint)SID.SlashingResistanceDown && Raid.TryFindSlot(actor.InstanceID, out var slot))
            Vulns[slot] = status.ExpireAt;
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        if (!Active)
            return;

        var baiters = Raid.WithSlot().SortedByRange(Module.PrimaryActor.Position);
        var ordered = Order[0] == Proximity.Close ? baiters.Take(4) : baiters.Reverse().Take(4);

        foreach (var (slot, actor) in ordered)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, new AOEShapeCone(24, 22.5f.Degrees()), NextActivation));

        ForbiddenPlayers = Raid.WithSlot().Where(p => Vulns[p.Item1] > NextActivation).Mask();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.EscelonsFallVisual)
            NextActivation = WorldState.FutureTime(13.9f);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (HintOrder.Count > 0)
        {
            var hintstr = new StringBuilder();

            foreach (var (order, ix) in HintOrder.Select((a, b) => (a, b)))
            {
                if (ix > 0)
                    hintstr.Append(" -> ");
                hintstr.Append(order.ToString());
            }

            hints.Add(hintstr.ToString());
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (caster == Module.PrimaryActor && (AID)spell.Action.ID is AID.EscelonsFallVisual or AID.EscelonsFallVisualRepeat && IsAfterVolleyMarker(spell.GlobalSequence))
        {
            VolleyPending = true;
            VolleySequence = spell.GlobalSequence;
        }
        else if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            if (VolleyPending && Order.Count > 0 && IsAfterVolleyMarker(spell.GlobalSequence))
            {
                VolleyPending = false;
                ++NumVolleys;
                Order.RemoveAt(0);
                if (Order.Count > 0)
                    NextActivation = WorldState.FutureTime(3.1f);
            }
        }
    }

    // Late hits from the previous volley must not consume a new marker.
    private bool IsAfterVolleyMarker(uint sequence) => sequence == 0 || VolleySequence == 0 || unchecked((int)(sequence - VolleySequence)) > 0;
}

class PowerBreak(BossModule module) : Components.GroupedAOEs(module, [AID.PowerBreak1, AID.PowerBreak2], new AOEShapeRect(24, 32));
