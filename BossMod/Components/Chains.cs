namespace BossMod.Components;

// component for breakable chains
public class Chains(BossModule module, uint tetherID, ActionID aid = default) : CastCounter(module, aid)
{
    public uint TID { get; init; } = tetherID;
    public bool TethersAssigned { get; private set; }
    private readonly Actor?[] _partner = new Actor?[PartyState.MaxAllianceSize];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_partner[slot] != null)
            hints.Add("Break the tether!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _partner[pcSlot] == player ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_partner[pcSlot] is var partner && partner != null)
            Arena.AddLine(pc.Position, partner.Position, ArenaColor.Danger);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == TID)
        {
            TethersAssigned = true;
            var target = WorldState.Actors.Find(tether.Target);
            if (target != null)
            {
                SetPartner(source.InstanceID, target);
                SetPartner(target.InstanceID, source);
            }
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == TID)
        {
            SetPartner(source.InstanceID, null);
            SetPartner(tether.Target, null);
        }
    }

    private void SetPartner(ulong source, Actor? target)
    {
        var slot = Raid.FindSlot(source);
        if (slot >= 0)
            _partner[slot] = target;
    }
}
