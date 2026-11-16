namespace BossMod.Dawntrail.Unreal.Un2Shinryu;

class TailMarker(BossModule module) : BossComponent(module)
{
    private static readonly WPos _center = new(0, 0);
    private static readonly WPos _southWest = new(-20, 20);
    private static readonly WPos _northEast = new(20, -20);
    private static readonly WPos _northWest = new(-20, -20);
    public Actor? Target { get; private set; }
    private DateTime _dive;

    private bool CenterCracked => Module.FindComponent<ArenaChanges>()?.Cracked(_center) == true;
    private bool Before(Actor actor) => actor == Target && _dive != default && WorldState.CurrentTime < _dive;
    private static bool OnCenter(Actor actor) => Math.Abs(actor.Position.X) < 10 && Math.Abs(actor.Position.Z) < 10;

    private WPos? Spot(Actor actor)
    {
        var arena = Module.FindComponent<ArenaChanges>();
        bool Clean(WPos tile) => arena == null || arena.Intact(tile) && !arena.Cracked(tile);
        var spot = actor.Role switch
        {
            Role.Healer => _southWest,
            Role.Tank => _northWest,
            _ => !Clean(_northEast) && Clean(_center) ? _center : _northEast
        };
        return arena == null || arena.Intact(spot) ? spot : null;
    }

    private static string SpotName(WPos spot) => spot == _southWest ? "south-west" : spot == _northWest ? "north-west" : spot == _northEast ? "north-east" : "center";

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.TailMarker)
        {
            Target = actor;
            _dive = WorldState.FutureTime(8.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TailDiveNorth or AID.TailDiveSouth)
            _dive = default;
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Tail)
            Target = null;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Before(actor) && Spot(actor) is { } spot)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(spot, new WDir(0, 1), 8, 8, 8), _dive);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Before(actor))
            return;
        if (CenterCracked && OnCenter(actor))
            hints.Add("Tail marker: get off the cracked center tile, breaking it drops the whole floor!");
        else if (Spot(actor) is { } spot)
            hints.Add($"Tail marker: drop it on the {SpotName(spot)} tile", false);
        else
            hints.Add("Tail marker: drop it away from the party, not on a cracked center", false);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Before(pc) && Spot(pc) is { } spot)
            Arena.ZoneRect(spot, new WDir(0, 1), 10, 10, 10, ArenaColor.SafeFromAOE);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => player == Target ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
}
