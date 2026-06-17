namespace BossMod.Dawntrail.Variant.V01NMerchantsTale.V07NStonePuppet;

public enum OID : uint
{
    Boss = 0x4ABC, // R24.000, x?
    Rock = 0x4ACD,
    Helper = 0x233C, // R0.500, x?, Helper type
}
public enum AID : uint
{
    MagneticRock = 45721 // 5f circ
}
public enum IconID : uint
{
    MagneticMarker = 315, // player->self
}
class ForbiddenGolem(BossModule module) : Components.GenericInvincible(module)
{
    private bool _rocksCharged;
    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        if (!_rocksCharged)
        {
            foreach (var boss in Module.Enemies(OID.Boss))
                yield return boss;
        }
    }
    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001)
        {
            switch (index)
            {
                case 0x29:
                    _rocksCharged = true;
                    break;
            }
        }
    }
}
class MagneticRock(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(5f), (uint)IconID.MagneticMarker, AID.MagneticRock, centerAtTarget: true);
class RockHints(BossModule module) : BossComponent(module)
{
    private IEnumerable<Actor> Rocks => Module.Enemies(OID.Rock);
    private Actor? MagneticTarget;

    private readonly List<Actor> _disabledRocks = [];

    private static readonly WPos Rock28 = new(179.41f, -648.23f);
    private static readonly WPos Rock27 = new(149.66f, -631.79f);
    private static readonly WPos Rock26 = new(128.56f, -637.55f);

    private IEnumerable<Actor> AvailableRocks => Rocks.Where(r => !_disabledRocks.Contains(r));

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID is IconID.MagneticMarker)
            MagneticTarget = actor;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor == MagneticTarget && AvailableRocks.Any())
        {
            var closestRock = AvailableRocks.MinBy(actor.DistanceToHitbox);
            if (closestRock != null)
                hints.AddForbiddenZone(new AOEShapeDonut(2f, 90f), closestRock.Position);
        }
        if (AvailableRocks.Count() == 3)
            hints.AddForbiddenZone(new AOEShapeRect(40f, 40f, 20f), Module.Center, -90f.Degrees());
        if (AvailableRocks.Count() == 2)
            hints.AddForbiddenZone(new AOEShapeRect(40f, 40f), Module.Center, 90f.Degrees());
        if (AvailableRocks.Count() == 1)
            hints.AddForbiddenZone(new AOEShapeRect(40f, 12f, 40f), Module.Center, -50f.Degrees());
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(AvailableRocks, ArenaColor.Object, true);

        if (pc == MagneticTarget)
        {
            foreach (var rock in AvailableRocks)
                Arena.AddCircle(rock.Position, 4f, ArenaColor.Safe);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.MagneticRock)
            MagneticTarget = null;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        base.OnMapEffect(index, state);
        if (state is 0x00020001)
        {

            WPos? removedRock = index switch
            {
                0x26 => Rock26,
                0x27 => Rock27,
                0x28 => Rock28,
                _ => null
            };

            if (removedRock != null)
            {
                var rock = Rocks.MinBy(r => (r.Position - removedRock.Value).LengthSq());
                if (rock != null && !_disabledRocks.Contains(rock))
                    _disabledRocks.Add(rock);
            }
        }
    }
}
class V07NStonePuppetStates : StateMachineBuilder
{
    public V07NStonePuppetStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagneticRock>()
            .ActivateOnEnter<RockHints>()
            .ActivateOnEnter<ForbiddenGolem>();
    }
}

[ModuleInfo(Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1066, NameID = 14353)]
public class V07NStonePuppet(WorldState ws, Actor primary) : BossModule(ws, primary, new(149.66f, -631.79f), CustomBounds)
{
    private static readonly List<WDir> vertices =
        [
            new(171.5f, -656.5f), new(181f, -650.64f), new(181f, -644.64f), new(152.34f, -619.77f), new(126.16f, -615.1f), new(120.31f, -649.2f), new(129.59f, -639.89f), new(134.96f, -625.99f), new(146f, -632.55f), new(149.08f, -634.55f)
        ];
    public static readonly ArenaBoundsCustom CustomBounds = new(30, new(vertices.Select(v => v - new WDir(149.66f, -631.79f))));
}
