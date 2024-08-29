namespace BossMod.Components;

// component for breakable chains - Note that chainLength for AI considers the minimum distance needed for a chain-pair to be broken (assuming perfectly stacked at cast)
public class Chains(BossModule module, uint tetherID, ActionID aid = default, float chainLength = 0, bool spreadChains = true) : CastCounter(module, aid)
{
    public uint TID { get; init; } = tetherID;
    public bool TethersAssigned { get; private set; }
    protected readonly (Actor? Partner, float InitialDistance)[] _partner = new (Actor? Partner, float InitialDistance)[PartyState.MaxAllianceSize];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_partner[slot].Partner != null)
            hints.Add("Break the tether!");
        if (_partner[slot].Partner != null && !spreadChains)
            hints.Add("Stay with Partner!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _partner[pcSlot].Partner == player ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (slot < 0 || slot >= _partner.Length || chainLength == 0)
            return;
        var partner = _partner[slot];
        if (partner.Partner != null)
        {
            var forbiddenZone = spreadChains ? ShapeDistance.Circle(partner.Partner.Position, partner.InitialDistance + chainLength) : ShapeDistance.InvertedCircle(partner.Partner.Position, chainLength);
            hints.AddForbiddenZone(forbiddenZone);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_partner[pcSlot] is var partner && partner.Partner != null)
            Arena.AddLine(pc.Position, partner.Partner.Position, ArenaColor.Danger);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == TID)
        {
            TethersAssigned = true;
            var target = WorldState.Actors.Find(tether.Target);
            if (target != null)
            {
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
        var slot = Raid.FindSlot(source);
        if (slot >= 0)
        {
            _partner[slot].Partner = target;
            _partner[slot].InitialDistance = initialDistance;
        }
    }
}
