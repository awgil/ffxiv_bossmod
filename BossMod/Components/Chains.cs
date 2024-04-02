namespace BossMod.Components;

// component for breakable chains
public class Chains : CastCounter
{
    public uint TID { get; init; }
    public bool TethersAssigned { get; private set; }
    private Actor?[] _partner = new Actor?[PartyState.MaxAllianceSize];

    public Chains(uint tetherID, ActionID aid = default) : base(aid)
    {
        TID = tetherID;
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_partner[slot] != null)
            hints.Add("Break the tether!");
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _partner[pcSlot] == player ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_partner[pcSlot] is var partner && partner != null)
            arena.AddLine(pc.Position, partner.Position, ArenaColor.Danger);
    }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == TID)
        {
            TethersAssigned = true;
            var target = module.WorldState.Actors.Find(tether.Target);
            if (target != null)
            {
                SetPartner(module, source.InstanceID, target);
                SetPartner(module, target.InstanceID, source);
            }
        }
    }

    public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == TID)
        {
            SetPartner(module, source.InstanceID, null);
            SetPartner(module, tether.Target, null);
        }
    }

    private void SetPartner(BossModule module, ulong source, Actor? target)
    {
        var slot = module.Raid.FindSlot(source);
        if (slot >= 0)
            _partner[slot] = target;
    }
}
