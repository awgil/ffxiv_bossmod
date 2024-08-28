namespace BossMod.Components;

// component for breakable chains - Note that chainLength for AI considers the minimum distance needed for a chain-pair to be broken (assuming perfectly stacked at cast)
public class Chains(BossModule module, uint tetherID, ActionID aid = default, float chainLength = 0, bool spreadChains = true) : CastCounter(module, aid)
{
    public uint TID { get; init; } = tetherID;
    public bool TethersAssigned { get; private set; }
    private readonly Actor?[] _partner = new Actor?[PartyState.MaxAllianceSize];
    private float _initialDistance;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_partner[slot] != null)
            hints.Add("Break the tether!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _partner[pcSlot] == player ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }
    
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (slot < 0 || slot >= _partner.Length || chainLength == 0)
            return;
        var partner = _partner[slot];
        if (partner != null)
        {
            var forbiddenZone = spreadChains ? ShapeDistance.Circle(partner.Position, _initialDistance + chainLength) : ShapeDistance.InvertedCircle(partner.Position, chainLength);
            hints.AddForbiddenZone(forbiddenZone);
        }
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
                 _initialDistance = (source.Position - target.Position).Length();
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
