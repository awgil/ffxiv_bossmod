namespace BossMod.Dawntrail.Variant.V01NMerchantsTale.V08NYawningMaw;

public enum OID : uint
{
    Boss = 0x4AB2,
    Gems = 0x4ACD,
    Helper = 0x233C, // R0.500, x?, Helper type
}
public enum AID : uint
{
    Inhale = 45719, // Boss->self, 4.0s cast, range 23 120-degree cone
}
class ForbiddenGoobue(BossModule module) : Components.GenericInvincible(module)
{
    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        var gems = Module.FindComponent<GemHints>();
        if (gems == null || !gems.AllGemsDisabled)
        {
            foreach (var boss in Module.Enemies(OID.Boss))
                yield return boss;
        }
    }
}
class Inhale(BossModule module) : Components.StandardAOEs(module, AID.Inhale, new AOEShapeCone(23f, 60.Degrees()));
class GemHints(BossModule module) : BossComponent(module)
{
    private IEnumerable<Actor> Gems => Module.Enemies(OID.Gems);
    private readonly List<Actor> _disabledGems = [];
    public bool AllGemsDisabled => !AvailableGems.Any();
    private bool _succ;

    private static readonly WPos Gems36 = new(295.17f, 691.57f);
    private static readonly WPos Gems37 = new(287.13f, 684.47f);
    private static readonly WPos Gems38 = new(297.10f, 670.08f);
    private static readonly WPos Gems39 = new(312.69f, 679.21f);

    private IEnumerable<Actor> AvailableGems => Gems.Where(r => !_disabledGems.Contains(r));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (AvailableGems.Any() && !_succ)
        {
            var closestGem = AvailableGems.MinBy(actor.DistanceToHitbox);
            if (closestGem != null)
                hints.AddForbiddenZone(new AOEShapeDonut(2f, 30f), closestGem.Position);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(AvailableGems, ArenaColor.Object, true);
        {
            foreach (var rock in AvailableGems)
                Arena.AddCircle(rock.Position, 4f, ArenaColor.Safe);
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Inhale)
        {
            _succ = true;
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Inhale)
        {
            _succ = false;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        base.OnMapEffect(index, state);
        if (state is 0x00020001)
        {

            WPos? removedGem = index switch
            {
                0x36 => Gems36,
                0x37 => Gems37,
                0x38 => Gems38,
                0x39 => Gems39,
                _ => null
            };

            if (removedGem != null)
            {
                var gem = Gems.MinBy(r => (r.Position - removedGem.Value).LengthSq());
                if (gem != null && !_disabledGems.Contains(gem))
                    _disabledGems.Add(gem);
            }
        }
    }
}
class V08NYawningMawStates : StateMachineBuilder
{
    public V08NYawningMawStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ForbiddenGoobue>()
            .ActivateOnEnter<Inhale>()
            .ActivateOnEnter<GemHints>();
    }
}

[ModuleInfo(Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1066, NameID = 14402)]
public class V08NYawningMaw(WorldState ws, Actor primary) : BossModule(ws, primary, new(299.77f, 681.98f), new ArenaBoundsCircle(15f));
