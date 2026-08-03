namespace BossMod.Stormblood.Ultimate.UWU;

// TODO: this assumes everyone shares the cleave, OT is front; other strategies have people avoid it
// TODO: verify width
class P1MistralSongBoss(BossModule module) : Components.GenericWildCharge(module, 5, (uint)AID.MistralSongBoss, 40)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MistralSong)
        {
            Source = Module.PrimaryActor;
            Activation = WorldState.FutureTime(5.2f);
            foreach (var (i, p) in Raid.WithSlot(true, true, true))
                PlayerRoles[i] = p == actor ? PlayerRole.TargetNotFirst : p.Role == Role.Tank && Module.PrimaryActor.TargetID != p.InstanceID ? PlayerRole.Share : PlayerRole.ShareNotFirst;
        }
    }
}

// TODO: generalize wild-charge to support this
// TODO: for adds version, there doesn't seem to be an indication of which sister targets which player - so we just guess... it doesn't matter for usual strat where targets stack
// TODO: verify width
class P1MistralSongAdds(BossModule module) : Components.CastCounter(module, (uint)AID.MistralSongAdds)
{
    private readonly UWUConfig _config = Service.Config.Get<UWUConfig>();
    private readonly List<Actor> _sisters = module.Enemies((uint)OID.GarudaSister);
    private readonly List<Actor> _targets = [];

    private static readonly AOEShapeRect _shape = new(40, 5);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.Role == Role.Tank && _targets.Count != 0)
        {
            hints.Add("Intercept charge!", !IsClosest(actor));
        }
        else if (_targets.Contains(actor))
        {
            var isClosest = ActiveAOEs().Any(aoe => Raid.WithoutSlot(false, true, true).InShape(_shape, aoe.origin, aoe.rotation).Closest(aoe.origin) == actor);
            hints.Add("Hide behind tank!", IsClosest(actor));
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _targets.Contains(player) ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var aoe in ActiveAOEs())
            _shape.Draw(Arena, aoe.origin, aoe.rotation, Colors.SafeFromAOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_sisters, Colors.Object, true);
        if (_config.P1MistralSongFixedLocation && pc.Role != Role.Tank)
        {
            var loc = new WPos(107, 107);
            var circle = new SDCircle(loc, 2f);
            Arena.ZoneCircleOutline(loc, 2f, circle.Contains(pc.Position) ? Colors.Safe : Colors.Danger);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MistralSong)
        {
            _targets.Add(actor);
        }
    }

    IEnumerable<(WPos origin, Angle rotation)> ActiveAOEs() => _sisters.Zip(_targets).Select(st => (st.First.Position, Angle.FromDirection(st.Second.Position - st.First.Position)));
    bool IsClosest(Actor actor) => ActiveAOEs().Any(aoe => Raid.WithoutSlot(false, true, true).InShape(_shape, aoe.origin, aoe.rotation).Closest(aoe.origin) == actor);
}

class P1GreatWhirlwind(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GreatWhirlwind, 8f);
