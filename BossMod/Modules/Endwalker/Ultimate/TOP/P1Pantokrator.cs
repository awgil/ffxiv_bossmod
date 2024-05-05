namespace BossMod.Endwalker.Ultimate.TOP;

class P1BallisticImpact(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BallisticImpact), 5);

class P1FlameThrower(BossModule module) : Components.GenericAOEs(module)
{
    public List<Actor> Casters = [];
    private readonly TOPConfig _config = Service.Config.Get<TOPConfig>();
    private readonly P1Pantokrator? _pantokrator = module.FindComponent<P1Pantokrator>();

    private static readonly AOEShapeCone _shape = new(65, 30.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in Casters.Skip(2))
            yield return new(_shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt, ArenaColor.AOE, false);
        foreach (var c in Casters.Take(2))
            yield return new(_shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt, ArenaColor.Danger, true);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Casters.Count == 0 || NumCasts > 0)
            return;

        var group = _pantokrator != null ? _pantokrator.PlayerStates[pcSlot].Group : 0;
        if (group > 0)
        {
            var flame1Dir = Casters[0].CastInfo!.Rotation - Module.PrimaryActor.Rotation;
            // if ne/sw, set of safe cones is offset by 1 rotation
            if (_config.P1PantokratorNESW)
                flame1Dir += 60.Degrees();

            var dir = flame1Dir.Normalized().Deg switch
            {
                (> 15 and < 45) or (> -165 and < -135) => -60.Degrees(),
                (> 45 and < 75) or (> -135 and < -105) => -30.Degrees(),
                (> 75 and < 105) or (> -105 and < -75) => 0.Degrees(),
                (> 105 and < 135) or (> -75 and < -45) => 30.Degrees(),
                (> 135 and < 165) or (> -45 and < -15) => 60.Degrees(),
                _ => -90.Degrees(), // assume groups go CW
            };
            // undo direction adjustment to correct target safe spot
            if (_config.P1PantokratorNESW)
                dir -= 60.Degrees();
            var offset = 12 * (Module.PrimaryActor.Rotation + dir).ToDirection();
            var pos = group == 1 ? Module.Center + offset : Module.Center - offset;
            Arena.AddCircle(pos, 1, ArenaColor.Safe);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FlameThrowerFirst or AID.FlameThrowerRest)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FlameThrowerFirst or AID.FlameThrowerRest)
            Casters.Remove(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FlameThrowerFirst or AID.FlameThrowerRest)
            ++NumCasts;
    }
}

class P1Pantokrator(BossModule module) : P1CommonAssignments(module)
{
    public int NumSpreadsDone { get; private set; }
    public int NumStacksDone { get; private set; }

    private const float _spreadRadius = 5;
    private static readonly AOEShapeRect _stackShape = new(50, 3);

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
            hints.Add("Spread!", Raid.WithoutSlot().InRadiusExcluding(actor, _spreadRadius).Any());
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

        foreach (var (i, p) in Raid.WithSlot(true))
        {
            var order = PlayerStates[i].Order;
            if (order == spreadOrder)
            {
                Arena.AddCircle(p.Position, _spreadRadius, i == pcSlot ? ArenaColor.Safe : ArenaColor.Danger);
            }
            else if (order == stackOrder)
            {
                _stackShape.Outline(Arena, Module.PrimaryActor.Position, Angle.FromDirection(p.Position - Module.PrimaryActor.Position), i == pcSlot ? ArenaColor.Safe : ArenaColor.Danger);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.GuidedMissileKyrios:
                ++NumSpreadsDone;
                break;
            case AID.CondensedWaveCannonKyrios:
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
    private static readonly AOEShape _shape = new AOEShapeCone(60, 60.Degrees()); // TODO: verify angle

    public P1DiffuseWaveCannonKyrios(BossModule module) : base(module, ActionID.MakeSpell(AID.DiffuseWaveCannonKyrios))
    {
        ForbiddenPlayers = Raid.WithSlot().WhereActor(a => a.Role != Role.Tank).Mask();
    }

    public override void Update()
    {
        CurrentBaits.Clear();
        CurrentBaits.AddRange(Raid.WithoutSlot().SortedByRange(Module.PrimaryActor.Position).TakeLast(2).Select(t => new Bait(Module.PrimaryActor, t, _shape)));
    }
}

class P1WaveCannonKyrios(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect _shape = new(50, 3);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.WaveCannonKyrios)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.WaveCannonKyrios)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, _shape));
    }
}
