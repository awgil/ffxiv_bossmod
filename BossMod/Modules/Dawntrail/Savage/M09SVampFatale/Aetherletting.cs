namespace BossMod.Dawntrail.Savage.M09SVampFatale;

sealed class AetherlettingCone : Components.SimpleAOEs
{
    private static readonly M09SVampFataleConfig _config = Service.Config.Get<M09SVampFataleConfig>();

    public AetherlettingCone(BossModule module) : base(module, (uint)AID.AetherlettingCone, new AOEShapeCone(40f, 22.5f.Degrees()), 4)
    {
        MaxDangerColor = 2;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_config.EnableStaticAetherlettingPuddle)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        // if enabled avoid only imminent cones so it doesn't go too far from expected position
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length > 2 ? 2 : aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var c = ref aoes[i];
            if (c.Risky)
            {
                hints.AddForbiddenZone(c.ShapeDistance ?? (c.Shape.Distance(c.Origin, c.Rotation)), c.Activation);
            }
        }
    }
}
// TODO: draw bait areas for each role (every 45 degrees, edges of cone)
sealed class AetherlettingPuddle(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AetherlettingPuddle, 6f)
{
    private static readonly M09SVampFataleConfig _config = Service.Config.Get<M09SVampFataleConfig>();
    private static readonly PartyRolesConfig _partyConfig = Service.Config.Get<PartyRolesConfig>();
    // CW -> R2 H2 M2 OT M1 H1 R1 MT starting N
    private readonly WPos[] _hectorPos =
    [
        module.Arena.Center + 157.5f.Degrees().ToDirection() * 18f,
        module.Arena.Center + 112.5f.Degrees().ToDirection() * 18f,
        module.Arena.Center + 67.5f.Degrees().ToDirection() * 18f,
        module.Arena.Center + 22.5f.Degrees().ToDirection() * 18f,
        module.Arena.Center - 22.5f.Degrees().ToDirection() * 18f,
        module.Arena.Center - 67.5f.Degrees().ToDirection() * 18f,
        module.Arena.Center - 112.5f.Degrees().ToDirection() * 18f,
        module.Arena.Center - 157.5f.Degrees().ToDirection() * 18f
    ];
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        var spreads = CollectionsMarshal.AsSpan(Spreads);
        var len = spreads.Length;

        for (var i = 0; i < len; i++)
        {
            if (spreads[i].Target.InstanceID == actor.InstanceID)
                hints.Add("Bait puddle to edge", false);
        }
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (Spreads.Count == 0)
            return;

        if (!_config.ShowStaticAetherletting)
            return;

        var radius = 1.5f;
        var baitPos = GetBaitPosition(pcSlot);

        // show all if bait position not found (party not configured or index error)
        if (baitPos == default)
        {
            var party = Raid.WithoutSlot(false, true, true);
            var len = party.Length;

            for (var i = 0; i < _hectorPos.Length; i++)
            {
                var pos = _hectorPos[i];
                var inside = false;

                for (var j = 0; j < len; j++)
                {
                    var actor = party[j];
                    var actorPos = actor.Position;
                    inside = actor.InstanceID != pc.InstanceID && actorPos.InCircle(pos, radius);

                    if (inside)
                        break;
                }

                if (inside)
                    Arena.ZoneCircle(pos, radius, Colors.AOE);
                else
                    Arena.ZoneCircleOutline(pos, radius, Colors.Safe);
            }
            return;
        }

        // only draw player bait position when spreading
        var spreads = CollectionsMarshal.AsSpan(Spreads);
        var lenSpread = spreads.Length;
        var isTarget = false;

        for (var i = 0; i < lenSpread; i++)
        {
            ref var s = ref spreads[i];
            isTarget = s.Target.InstanceID == pc.InstanceID;

            if (isTarget)
                break;
        }

        if (isTarget)
            Arena.ZoneCircleOutline(baitPos, radius, Colors.Safe);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (!_config.EnableStaticAetherlettingPuddle)
            return;

        if (Spreads.Count == 0)
            return;

        var spreads = CollectionsMarshal.AsSpan(Spreads);
        var lenSpread = spreads.Length;
        var isTarget = false;

        for (var i = 0; i < lenSpread; i++)
        {
            ref var s = ref spreads[i];
            isTarget = s.Target.InstanceID == actor.InstanceID;

            if (isTarget)
                break;
        }

        if (!isTarget)
            return;

        var baitPos = GetBaitPosition(slot);

        // if it can't get bait position for some reason, go to the edge to have a chance of salvaging the run
        if (baitPos == default)
        {
            hints.AddForbiddenZone(new AOEShapeCircle(18.5f), Arena.Center);
            return;
        }

        hints.GoalZones.Add(AIHints.GoalSingleTarget(baitPos, 1f, 9f));
    }

    private WPos GetBaitPosition(int pcSlot)
    {
        if (_partyConfig.SlotsPerAssignment(Raid).Length == 0)
            return default;

        var assignment = _partyConfig[Raid.Members[pcSlot].ContentId];
        var index = assignment switch
        {
            PartyRolesConfig.Assignment.MT => 4,
            PartyRolesConfig.Assignment.OT => 3,
            PartyRolesConfig.Assignment.H1 => 6,
            PartyRolesConfig.Assignment.H2 => 1,
            PartyRolesConfig.Assignment.M1 => 7,
            PartyRolesConfig.Assignment.M2 => 2,
            PartyRolesConfig.Assignment.R1 => 5,
            PartyRolesConfig.Assignment.R2 => 0,
            _ => -1
        };

        // couldn't find index
        if (index == -1)
            return default;

        return _hectorPos[index];
    }
}
sealed class AetherlettingCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherlettingCross, new AOEShapeCross(40f, 5f), 2)
{
    private readonly AetherlettingPuddle _puddles = module.FindComponent<AetherlettingPuddle>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_puddles.NumFinishedSpreads < 8)
            return [];
        return base.ActiveAOEs(slot, actor);
    }
}
