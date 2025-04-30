namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class WealOfStone : PlayActionAOEs
{
    public WealOfStone(BossModule module) : base(module, (uint)OID.WolfOfStoneWeal, 0x11D2, new AOEShapeRect(40, 3), AID.WealOfStone1, 5.7f, actorIsCaster: false)
    {
        Risky = false;
    }
}

class MoonbeamsBite(BossModule module) : Components.GroupedAOEs(module, [AID.MoonbeamsBiteLeft, AID.MoonbeamsBiteRight], new AOEShapeRect(40, 10), 2)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var i = 0;
        foreach (var aoe in base.ActiveAOEs(slot, actor))
        {
            yield return aoe with { Color = i == 0 ? ArenaColor.Danger : ArenaColor.AOE, Risky = i == 0 };
            i++;
        }
    }
}

class WealOfStone2 : PlayActionAOEs
{
    public WealOfStone2(BossModule module) : base(module, (uint)OID.WolfOfStoneWeal, 0x11D2, new AOEShapeRect(40, 3), AID.WealOfStone2, 5.7f, actorIsCaster: false)
    {
        Risky = false;
    }
}

class QuadHints(BossModule module) : Components.CastCounterMulti(module, [AID.MoonbeamsBiteLeft, AID.MoonbeamsBiteRight])
{
    private readonly RM08SHowlingBladeConfig _config = Service.Config.Get<RM08SHowlingBladeConfig>();
    private readonly ArcList Forbidden = new(module.Arena.Center, 11);
    private readonly List<Angle> SafeSpots = [];
    private int StartCounter;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        switch ((AID)spell.Action.ID)
        {
            case AID.MoonbeamPreLeft:
                StartCounter++;
                Forbidden.ForbidArcByLength(Angle.FromDirection(spell.TargetXZ - Arena.Center) - 90.Degrees(), 90.Degrees());
                break;
            case AID.MoonbeamPreRight:
                StartCounter++;
                Forbidden.ForbidArcByLength(Angle.FromDirection(spell.TargetXZ - Arena.Center) + 90.Degrees(), 90.Degrees());
                break;
        }

        if (StartCounter == 2)
        {
            foreach (var (min, max) in Forbidden.Allowed(default))
                SafeSpots.Add((min + max) * 0.5f);
            Forbidden.Forbidden.Clear();
            StartCounter = 0;
        }
    }

    public void Advance()
    {
        SafeSpots.RemoveAt(0);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_config.QuadMoonlightHints)
            return;

        foreach (var (i, spot) in Enumerable.Reverse(SafeSpots.Select((i, s) => (s, i))))
            Arena.AddCircle(Arena.Center + spot.ToDirection() * 11, 0.5f, i == 0 ? ArenaColor.Safe : ArenaColor.Object);
    }
}
