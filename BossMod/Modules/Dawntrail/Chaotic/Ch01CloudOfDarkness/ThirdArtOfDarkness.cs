namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class ThirdArtOfDarknessCleave(BossModule module) : Components.GenericAOEs(module)
{
    public enum Mechanic { None, Left, Right, Stack, Spread }

    public readonly Dictionary<Actor, List<(Mechanic mechanic, DateTime activation)>> Mechanics = [];

    private static readonly AOEShapeCone _shape = new(15, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (caster, m) in Mechanics)
        {
            var dir = m.Count == 0 ? default : m[0].mechanic switch
            {
                Mechanic.Left => 90.Degrees(),
                Mechanic.Right => -90.Degrees(),
                _ => default
            };
            if (dir != default)
                yield return new(_shape, caster.Position, caster.Rotation + dir, m[0].activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var (a, m) = Mechanics.FirstOrDefault(kv => kv.Key.InstanceID == actor.TargetID);
        if (a != null && m.Count > 0)
            hints.Add($"Order: {string.Join(" > ", m.Select(m => m.mechanic))}", false);
        base.AddHints(slot, actor, hints);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((OID)actor.OID == OID.StygianShadow)
        {
            var mechanic = (IconID)iconID switch
            {
                IconID.ThirdArtOfDarknessLeft => Mechanic.Left,
                IconID.ThirdArtOfDarknessRight => Mechanic.Right,
                IconID.ThirdArtOfDarknessStack => Mechanic.Stack,
                IconID.ThirdArtOfDarknessSpread => Mechanic.Spread,
                _ => Mechanic.None
            };
            if (mechanic != Mechanic.None)
                Mechanics.GetOrAdd(actor).Add((mechanic, WorldState.FutureTime(9.5f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var mechanic = (AID)spell.Action.ID switch
        {
            AID.ArtOfDarknessAOEL => Mechanic.Left,
            AID.ArtOfDarknessAOER => Mechanic.Right,
            AID.HyperFocusedParticleBeamAOE => Mechanic.Spread,
            AID.MultiProngedParticleBeamAOE => Mechanic.Stack,
            _ => Mechanic.None
        };
        if (mechanic != Mechanic.None)
        {
            var (a, m) = Mechanics.FirstOrDefault(kv => kv.Key.Position.AlmostEqual(caster.Position, 1) && kv.Value.Count > 0 && kv.Value[0].mechanic == mechanic);
            if (a != null)
            {
                m.RemoveAt(0);
                if (m.Count == 0)
                    Mechanics.Remove(a);
            }
        }
    }
}

class ThirdArtOfDarknessHyperFocusedParticleBeam(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly ThirdArtOfDarknessCleave? _main = module.FindComponent<ThirdArtOfDarknessCleave>();

    private static readonly AOEShapeRect _shape = new(22, 2.5f);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_main != null)
            foreach (var (a, m) in _main.Mechanics)
                if (m.Count > 0 && m[0].mechanic == ThirdArtOfDarknessCleave.Mechanic.Spread)
                    foreach (var p in Raid.WithoutSlot().SortedByRange(a.Position).Take(6))
                        CurrentBaits.Add(new(a, p, _shape, m[0].activation));
    }
}

class ThirdArtOfDarknessMultiProngedParticleBeam(BossModule module) : Components.UniformStackSpread(module, 3, 0, 2)
{
    private readonly ThirdArtOfDarknessCleave? _main = module.FindComponent<ThirdArtOfDarknessCleave>();

    public override void Update()
    {
        Stacks.Clear();
        if (_main != null)
            foreach (var (a, m) in _main.Mechanics)
                if (m.Count > 0 && m[0].mechanic == ThirdArtOfDarknessCleave.Mechanic.Stack)
                    foreach (var p in Raid.WithoutSlot().SortedByRange(a.Position).Take(3))
                        AddStack(p, m[0].activation);
        base.Update();
    }
}
