namespace BossMod.Dawntrail.Quantum.Q01TheFinalVerse;

class Neutralize(BossModule module) : BossComponent(module)
{
    private readonly Color[] _colors = new Color[4];
    public int NumIcons { get; private set; }

    // random guess
    private DateTime _resolve;

    public const float Radius = 1.5f; // todo verify

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var color = (IconID)iconID switch
        {
            IconID.RingLight => Color.Light,
            IconID.RingDark => Color.Dark,
            _ => Color.None
        };

        if (_resolve == default)
            _resolve = WorldState.FutureTime(5.1f);

        if (color != Color.None && Raid.TryFindSlot(actor, out var slot))
        {
            _colors[slot] = color;
            NumIcons++;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_colors[slot] != default)
        {
            var stacked = Raid.WithSlot().InRadiusExcluding(actor, Radius).ToList();

            if (stacked.Any(s => _colors[s.Item1] == _colors[slot]))
                hints.Add("GTFO from same color!");
            else
                hints.Add("Stack with opposite color!", !stacked.Any(s => _colors[s.Item1] != _colors[slot]));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_colors[slot] != default)
        {
            List<Func<WPos, bool>> _partners = [];

            foreach (var (s, a) in Raid.WithSlot().Exclude(actor))
            {
                if (_colors[s] != _colors[slot])
                    _partners.Add(ShapeContains.Donut(a.Position, Radius, 60));
                else
                    hints.AddForbiddenZone(ShapeContains.Circle(a.Position, Radius), _resolve);
            }

            if (_partners.Count > 0)
                hints.AddForbiddenZone(ShapeContains.Intersection(_partners), _resolve);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_colors[pcSlot] != default)
        {
            Arena.AddCircle(pc.Position, Radius, ArenaColor.Safe);

            foreach (var (slot, player) in Raid.WithSlot().Exclude(pc))
            {
                if (_colors[slot] == _colors[pcSlot])
                    Arena.AddCircle(player.Position, Radius, ArenaColor.Danger);
            }
        }
    }

    public override void Update()
    {
        if (_resolve != default && _resolve <= WorldState.CurrentTime)
        {
            _resolve = default;
            Array.Fill(_colors, default);
            NumIcons = 0;
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _colors[pcSlot] == default ? PlayerPriority.Normal : _colors[pcSlot] == _colors[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Interesting;
}
