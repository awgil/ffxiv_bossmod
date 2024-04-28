namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to fledgling flight & death toll mechanics
class FledglingFlight(BossModule module) : BossComponent(module)
{
    public bool PlacementDone { get; private set; }
    public bool CastsDone { get; private set; }
    private readonly List<(Actor, Angle)> _sources = []; // actor + rotation
    private readonly int[] _playerDeathTollStacks = new int[8];
    private readonly int[] _playerAOECount = new int[8];

    private static readonly Angle _coneHalfAngle = 45.Degrees();
    private const float _eyePlacementOffset = 10;

    public override void Update()
    {
        if (_sources.Count == 0)
            return;

        foreach ((int i, var player) in Raid.WithSlot())
        {
            _playerDeathTollStacks[i] = player.FindStatus((uint)SID.DeathsToll)?.Extra ?? 0; // TODO: use status events here...
            _playerAOECount[i] = _sources.Where(srcRot => player.Position.InCone(srcRot.Item1.Position, srcRot.Item2, _coneHalfAngle)).Count();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_sources.Count == 0)
            return;

        var eyePos = GetEyePlacementPosition(slot, actor);
        if (eyePos != null && !actor.Position.InCircle(eyePos.Value, 5))
        {
            hints.Add("Get closer to eye placement position!");
        }

        if (_playerAOECount[slot] < _playerDeathTollStacks[slot])
        {
            hints.Add($"Enter more aoes ({_playerAOECount[slot]}/{_playerDeathTollStacks[slot]})!");
        }
        else if (_playerAOECount[slot] > _playerDeathTollStacks[slot])
        {
            hints.Add($"GTFO from eyes ({_playerAOECount[slot]}/{_playerDeathTollStacks[slot]})!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_sources.Count == 0)
            return;

        // draw all players
        foreach ((int i, var player) in Raid.WithSlot())
            Arena.Actor(player, _playerAOECount[i] != _playerDeathTollStacks[i] ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);

        var eyePos = GetEyePlacementPosition(pcSlot, pc);
        if (eyePos != null)
            Arena.AddCircle(eyePos.Value, 1, ArenaColor.Safe);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach ((var source, var dir) in _sources)
        {
            Arena.ZoneIsoscelesTri(source.Position, dir, _coneHalfAngle, 50, ArenaColor.AOE);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AshenEye)
        {
            if (!PlacementDone)
            {
                PlacementDone = true;
                _sources.Clear();
            }
            _sources.Add((caster, caster.Rotation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AshenEye)
        {
            _sources.RemoveAll(x => x.Item1 == caster);
            CastsDone = _sources.Count == 0;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID is >= 296 and <= 299)
        {
            if (PlacementDone)
            {
                ReportError($"Unexpected icon after eyes started casting");
                return;
            }

            var dir = iconID switch
            {
                296 => 90.Degrees(), // E
                297 => 270.Degrees(), // W
                298 => 0.Degrees(), // S
                299 => 180.Degrees(), // N
                _ => 0.Degrees()
            };
            _sources.Add((actor, dir));
        }
    }

    private WPos? GetEyePlacementPosition(int slot, Actor player)
    {
        if (PlacementDone)
            return null;

        (var src, Angle rot) = _sources.Find(srcRot => srcRot.Item1 == player);
        if (src == null)
            return null;

        var offset = rot.ToDirection() * _eyePlacementOffset;
        return _playerDeathTollStacks[slot] > 0 ? Module.Center - offset : Module.Center + offset;
    }
}
