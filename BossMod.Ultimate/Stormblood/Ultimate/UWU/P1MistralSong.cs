namespace BossMod.Stormblood.Ultimate.UWU;

// TODO: this assumes everyone shares the cleave, OT is front; other strategies have people avoid it
class P1MistralSongBoss(BossModule module) : Components.GenericWildCharge(module, 50, AID.MistralSongBoss, 40)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MistralSong)
        {
            Source = Module.PrimaryActor;
            Activation = WorldState.FutureTime(5.2f);
            foreach (var (i, p) in Raid.WithSlot(true))
                PlayerRoles[i] = p == actor ? PlayerRole.TargetNotFirst : p.Role == Role.Tank && Module.PrimaryActor.TargetID != p.InstanceID ? PlayerRole.Share : PlayerRole.Avoid;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Source == null)
            return;

        if (PlayerRoles[slot] == PlayerRole.TargetNotFirst)
            hints.AddForbiddenZone(ShapeDistance.PrecisePosition(new(107.2f, 100), new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f), Activation);
        else if (PlayerRoles[slot] == PlayerRole.Share)
            hints.AddForbiddenZone(ShapeDistance.PrecisePosition(new(106.2f, 100), new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f), Activation);
        else
            hints.AddForbiddenZone(ShapeDistance.Rect(Source.Position, new WDir(1, 0), 100, 0, HalfWidth), Activation);
    }
}

class P1MistralSongAdds(BossModule module) : Components.CastCounter(module, AID.MistralSongAdds)
{
    public bool EnableHints;
    private readonly DateTime _activation = module.WorldState.FutureTime(6.9f);
    private readonly List<Actor> _sisters = [.. module.Enemies(OID.GarudaSister).OrderBy(s => s.InstanceID)];
    private readonly List<Actor> _targets = [];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_targets.Count == 0 || !EnableHints)
            return;

        foreach (var sister in _sisters)
        {
            if (Raid.WithoutSlot().Closest(sister.Position) is { } target)
            {
                if (target == actor && actor.Role != Role.Tank)
                    hints.Add("GTFO from add!");
                else if (target.Role != Role.Tank && actor.Role == Role.Tank)
                    hints.Add("Intercept add!");
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _targets.Contains(player) ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var sister in _sisters)
        {
            Arena.Actor(sister, ArenaColor.Object, true);
            if (Raid.WithoutSlot().Closest(sister.Position) is { } target)
            {
                if (Arena.Config.ShowOutlinesAndShadows)
                    Arena.AddLine(sister.Position, target.Position, 0xFF000000, 2);
                Arena.AddLine(sister.Position, target.Position, ArenaColor.Danger);
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MistralSong)
        {
            _targets.Add(actor);
            _targets.SortBy(t => t.InstanceID);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT)
        {
            if (_sisters.Count == 2 && EnableHints)
            {
                var mine = _sisters[(int)assignment];
                hints.AddForbiddenZone(ShapeDistance.InvertedRect(mine.Position, Arena.Center, 2), _activation);
            }
        }
        else
        {
            foreach (var s in _sisters)
                hints.AddForbiddenZone(ShapeDistance.Circle(s.Position, 19.5f), _activation);
        }

        BitMask tanks = new();
        foreach (var s in _sisters)
            tanks.Set(Raid.WithSlot().Closest(s.Position).Item1);

        hints.AddPredictedDamage(tanks, _activation, AIHints.PredictedDamageType.Tankbuster);
        hints.AddPredictedDamage(new BitMask(0xff & ~tanks.Raw), _activation, AIHints.PredictedDamageType.Raidwide);
    }
}

class P1GreatWhirlwind(BossModule module) : Components.StandardAOEs(module, AID.GreatWhirlwind, 8);
