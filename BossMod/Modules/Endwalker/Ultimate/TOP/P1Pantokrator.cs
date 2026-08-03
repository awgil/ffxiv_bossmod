namespace BossMod.Endwalker.Ultimate.TOP;

sealed class P1BallisticImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BallisticImpact, 5);

sealed class P1FlameThrower(BossModule module) : Components.GenericAOEs(module)
{
    private readonly TOPConfig _config = Service.Config.Get<TOPConfig>();
    private readonly P1Pantokrator? _pantokrator = module.FindComponent<P1Pantokrator>();
    public readonly List<AOEInstance> AOEs = [];
    private static readonly AOEShapeCone _shape = new(65f, 30f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(AOEs);
        var max = count > 2 ? 2 : count;
        var color = Colors.Danger;
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (count > 2)
            {
                aoe.Color = color;
            }
            aoe.Risky = true;
        }
        return aoes;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (AOEs.Count == 0 || NumCasts > 0)
            return;

        var group = _pantokrator != null ? _pantokrator.PlayerStates[pcSlot].Group : 0;
        if (group > 0)
        {
            var flame1Dir = AOEs.Ref(0).Rotation - Module.PrimaryActor.Rotation;
            // if ne/sw, set of safe cones is offset by 1 rotation
            if (_config.P1PantokratorNESW)
                flame1Dir += 60f.Degrees();

            var dir = flame1Dir.Normalized().Deg switch
            {
                > 15f and < 45f or > -165f and < -135f => -60f.Degrees(),
                > 45f and < 75f or > -135f and < -105f => -30f.Degrees(),
                > 75f and < 105f or > -105f and < -75f => default,
                > 105f and < 135f or > -75f and < -45f => 30f.Degrees(),
                > 135f and < 165f or > -45f and < -15f => 60f.Degrees(),
                _ => -90f.Degrees(), // assume groups go CW
            };
            // undo direction adjustment to correct target safe spot
            if (_config.P1PantokratorNESW)
                dir -= 60f.Degrees();
            var offset = 12f * (Module.PrimaryActor.Rotation + dir).ToDirection();
            var pos = group == 1 ? Arena.Center + offset : Arena.Center - offset;
            Arena.ZoneCircleOutline(pos, 1f, Colors.Safe);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FlameThrowerFirst or (uint)AID.FlameThrowerRest)
        {
            AOEs.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), risky: false));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FlameThrowerFirst or (uint)AID.FlameThrowerRest)
        {
            ++NumCasts;
            if (AOEs.Count != 0)
            {
                AOEs.RemoveAt(0);
            }
        }
    }
}

sealed class P1Pantokrator(BossModule module) : P1CommonAssignments(module)
{
    public int NumSpreadsDone;
    public int NumStacksDone;

    private const float _spreadRadius = 5;
    private static readonly AOEShapeRect _stackShape = new(50f, 3f);

    protected override (GroupAssignmentUnique assignment, bool global) Assignments()
    {
        var config = Service.Config.Get<TOPConfig>();
        return (config.P1PantokratorAssignments, config.P1PantokratorGlobalPriority);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        var ps = PlayerStates[slot];
        if (ps.Order == 0)
            return;

        var stackOrder = NextStackOrder();
        if (ps.Order == NextSpreadOrder())
        {
            hints.Add("Spread!", Raid.WithoutSlot(false, true, true).InRadiusExcluding(actor, _spreadRadius).Any());
        }
        else if (ps.Order != stackOrder)
        {
            var stackTargetSlot = Array.FindIndex(PlayerStates, s => s.Order == stackOrder && s.Group == ps.Group);
            var stackTarget = Raid[stackTargetSlot];
            if (stackTarget != null && !_stackShape.Check(actor.Position, Module.PrimaryActor.Position, Angle.FromDirection(stackTarget.Position - Module.PrimaryActor.Position)))
                hints.Add("Stack!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var spreadOrder = NextSpreadOrder();
        var stackOrder = NextStackOrder();

        foreach (var (i, p) in Raid.WithSlot(true, true, true))
        {
            var order = PlayerStates[i].Order;
            if (order == spreadOrder)
            {
                Arena.ZoneCircleOutline(p.Position, _spreadRadius, i == pcSlot ? Colors.Safe : Colors.Danger);
            }
            else if (order == stackOrder)
            {
                _stackShape.Outline(Arena, Module.PrimaryActor.Position, Angle.FromDirection(p.Position - Module.PrimaryActor.Position), i == pcSlot ? Colors.Safe : 0);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.GuidedMissileKyrios:
                ++NumSpreadsDone;
                break;
            case (uint)AID.CondensedWaveCannonKyrios:
                ++NumStacksDone;
                break;
        }
    }

    private int NextSpreadOrder(int skip = 0)
    {
        var index = NumSpreadsDone + skip * 2;
        return index < 8 ? (index >> 1) + 1 : 0;
    }

    private int NextStackOrder(int skip = 0)
    {
        var index = NumStacksDone + skip * 2;
        return index < 8 ? (index >> 1) + (index < 4 ? 3 : -1) : 0;
    }
}

class P1DiffuseWaveCannonKyrios : Components.GenericBaitAway
{
    private static readonly AOEShape _shape = new AOEShapeCone(60f, 60f.Degrees()); // TODO: verify angle

    public P1DiffuseWaveCannonKyrios(BossModule module) : base(module, (uint)AID.DiffuseWaveCannonKyrios)
    {
        ForbiddenPlayers = Raid.WithSlot(false, true, true).WhereActor(a => a.Role != Role.Tank).Mask();
    }

    public override void Update()
    {
        CurrentBaits.Clear();
        var party = Raid.WithoutSlot(false, true, true);
        Array.Sort(party, (a, b) =>
            {
                var distA = (a.Position - Arena.Center).LengthSq();
                var distB = (b.Position - Arena.Center).LengthSq();
                return distA.CompareTo(distB);
            });
        List<Bait> baits = [];
        var len = party.Length;
        for (var i = Math.Max(0, len - 2); i < len; ++i)
        {
            baits.Add(new(Module.PrimaryActor, party[i], _shape));
        }
        CurrentBaits.AddRange(baits);
    }
}

class P1WaveCannonKyrios(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect _shape = new(50, 3);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WaveCannonKyrios)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.WaveCannonKyrios)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, _shape));
    }
}
