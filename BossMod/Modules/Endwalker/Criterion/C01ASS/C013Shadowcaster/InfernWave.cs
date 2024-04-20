namespace BossMod.Endwalker.Criterion.C01ASS.C013Shadowcaster;

class InfernWave(BossModule module, bool savage, bool showHints, int maxActive) : Components.CastCounter(module, ActionID.MakeSpell(savage ? AID.SInfernWaveAOE : AID.NInfernWaveAOE))
{
    private class Beacon(Actor source, DateTime activation)
    {
        public Actor Source = source;
        public DateTime Activation = activation;
        public List<(Actor target, Angle dir)> Targets = [];
    }

    public bool ShowHints = showHints;
    private readonly bool _savage = savage;
    private readonly int _maxActive = maxActive;
    private readonly List<Beacon> _beacons = [];

    private static readonly AOEShapeCone _shape = new(60, 45.Degrees());

    public override void Update()
    {
        // create entries for newly activated beacons
        foreach (var s in Module.Enemies(_savage ? OID.SBeacon : OID.NBeacon).Where(s => s.ModelState.AnimState1 == 1 && !_beacons.Any(b => b.Source == s)))
        {
            _beacons.Add(new(s, WorldState.FutureTime(17.1f)));
        }

        // update beacon targets
        if (ShowHints)
        {
            foreach (var b in ActiveBeacons())
            {
                b.Targets.Clear();
                foreach (var t in Raid.WithoutSlot().SortedByRange(b.Source.Position).Take(2))
                    b.Targets.Add((t, Angle.FromDirection(t.Position - b.Source.Position)));
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!ShowHints)
            return;

        bool clipping = false, clipped = false;
        int numBaits = 0;
        foreach (var b in ActiveBeacons())
        {
            foreach (var t in b.Targets)
            {
                if (t.target == actor)
                {
                    ++numBaits;
                    clipping |= Raid.WithoutSlot().Exclude(actor).InShape(_shape, b.Source.Position, t.dir).Any();
                }
                else
                {
                    clipped |= _shape.Check(actor.Position, b.Source.Position, t.dir);
                }
            }
        }

        if (numBaits > 1)
            hints.Add("Baiting mulitple cones!");
        if (clipping)
            hints.Add("GTFO from raid!");
        if (clipped)
            hints.Add("GTFO from other bait!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!ShowHints)
            return;

        foreach (var b in ActiveBeacons())
            foreach (var t in b.Targets)
                _shape.Draw(Arena, b.Source.Position, t.dir);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var b in ActiveBeacons())
            Arena.Actor(b.Source, ArenaColor.Object, true);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
        {
            var beacon = _beacons.Find(b => b.Source.Position.AlmostEqual(caster.Position, 1));
            if (beacon != null)
                beacon.Activation = new();
        }
    }

    private IEnumerable<Beacon> ActiveBeacons() => _beacons.Where(b => b.Activation != default).Take(_maxActive);
}

class NInfernWave1(BossModule module) : InfernWave(module, false, false, 2);
class SInfernWave1(BossModule module) : InfernWave(module, true, false, 2);
class NInfernWave2(BossModule module) : InfernWave(module, false, true, 1);
class SInfernWave2(BossModule module) : InfernWave(module, true, true, 1);
