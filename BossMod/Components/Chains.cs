namespace BossMod.Components;

// component for breakable chains - Note that chainLength for AI considers the minimum distance needed for a chain-pair to be broken (assuming perfectly stacked at cast)
public class Chains(BossModule module, uint tetherID, Enum? aid = default, float chainLength = 0, bool spreadChains = true, float activationDelay = 30) : CastCounter(module, aid)
{
    public uint TID { get; init; } = tetherID;
    public bool TethersAssigned { get; private set; }
    protected readonly (Actor? Partner, float InitialDistance)[] Partners = new (Actor? Partner, float InitialDistance)[PartyState.MaxAllies];
    private DateTime _activation;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Partners[slot].Partner != null)
            hints.Add(spreadChains ? "Break the tether!" : "Stay with partner!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return Partners[pcSlot].Partner == player ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (chainLength == 0)
            return;
        var partner = Partners[slot];
        if (partner.Partner != null)
        {
            var forbiddenZone = spreadChains ? ShapeContains.Circle(partner.Partner.Position, partner.InitialDistance + chainLength) : ShapeContains.InvertedCircle(partner.Partner.Position, chainLength);
            hints.AddForbiddenZone(forbiddenZone, _activation);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var partner = Partners[pcSlot];
        if (partner.Partner != null)
            Arena.AddLine(pc.Position, partner.Partner.Position, spreadChains ? ArenaColor.Danger : ArenaColor.Safe);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == TID)
        {
            TethersAssigned = true;
            var target = WorldState.Actors.Find(tether.Target);
            if (target != null)
            {
                _activation = WorldState.FutureTime(activationDelay);
                var initialDistance = (source.Position - target.Position).Length();
                SetPartner(source.InstanceID, target, initialDistance);
                SetPartner(target.InstanceID, source, initialDistance);
            }
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == TID)
        {
            SetPartner(source.InstanceID, null, 0);
            SetPartner(tether.Target, null, 0);
        }
    }

    private void SetPartner(ulong source, Actor? target, float initialDistance)
    {
        if (Raid.TryFindSlot(source, out var slot))
            Partners[slot] = (target, initialDistance);
    }
}
