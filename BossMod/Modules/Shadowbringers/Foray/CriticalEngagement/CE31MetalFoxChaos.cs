namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE31MetalFoxChaos;

public enum OID : uint
{
    Boss = 0x2DB5, // R=8.0
    Helper = 0x233C, // R=0.500
    MagitekBit = 0x2DB6, // R=1.2
};

public enum AID : uint
{
    Attack = 6497, // Boss->player, no cast, single-target
    MagitektBitTeleporting = 20192, // 2DB6->location, no cast, ???
    DiffractiveLaser = 20138, // Boss->self, 7,0s cast, range 60 150-degree cone
    RefractedLaser = 20141, // 2DB6->self, no cast, range 100 width 6 rect
    LaserShower = 20136, // Boss->self, 3,0s cast, single-target
    LaserShower2 = 20140, // Helper->location, 5,0s cast, range 10 circle
    Rush = 20139, // Boss->player, 3,0s cast, width 14 rect charge
    SatelliteLaser = 20137, // Boss->self, 10,0s cast, range 100 circle
};

class MagitekBitLasers : Components.GenericAOEs
{
    private static readonly Angle[] rotations = [0.Degrees(), 90.Degrees(), 180.Degrees(), -90.Degrees()];
    private readonly List<DateTime> _times = [];
    private Angle startrotation;
    public enum Types { None, SatelliteLaser, DiffractiveLaser, LaserShower }
    public Types Type { get; private set; }
    private const float maxError = MathF.PI / 180;
    private static readonly AOEShapeRect rect = new(100, 3);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_times.Count > 0)
            foreach (var p in module.Enemies(OID.MagitekBit))
            {
                if (Type == Types.SatelliteLaser && module.WorldState.CurrentTime > _times[0])
                    yield return new(rect, p.Position, p.Rotation, _times[1]);
                if ((Type == Types.DiffractiveLaser && module.WorldState.CurrentTime > _times[0]) || Type == Types.LaserShower)
                {
                    if (NumCasts < 5 && p.Rotation.AlmostEqual(startrotation, maxError))
                        yield return new(rect, p.Position, p.Rotation, _times[1], ArenaColor.Danger);
                    if (NumCasts < 5 && (p.Rotation.AlmostEqual(startrotation + 90.Degrees(), maxError) || p.Rotation.AlmostEqual(startrotation - 90.Degrees(), maxError)))
                        yield return new(rect, p.Position, p.Rotation, _times[2]);
                    if (NumCasts >= 5 && NumCasts < 9 && (p.Rotation.AlmostEqual(startrotation + 90.Degrees(), maxError) || p.Rotation.AlmostEqual(startrotation - 90.Degrees(), maxError)))
                        yield return new(rect, p.Position, p.Rotation, _times[2], ArenaColor.Danger);
                    if (NumCasts >= 5 && NumCasts < 9 && p.Rotation.AlmostEqual(startrotation + 180.Degrees(), maxError))
                        yield return new(rect, p.Position, p.Rotation, _times[3]);
                    if (NumCasts >= 9 && p.Rotation.AlmostEqual(startrotation + 180.Degrees(), maxError))
                        yield return new(rect, p.Position, p.Rotation, _times[3], ArenaColor.Danger);
                }
            }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var _time = module.WorldState.CurrentTime;
        if ((AID)spell.Action.ID == AID.SatelliteLaser)
        {
            Type = Types.SatelliteLaser;
            _times.Add(_time.AddSeconds(2.5f));
            _times.Add(_time.AddSeconds(12.3f));
        }
        if ((AID)spell.Action.ID == AID.DiffractiveLaser)
        {
            DateTime[] times = [_time.AddSeconds(2), _time.AddSeconds(8.8f), _time.AddSeconds(10.6f), _time.AddSeconds(12.4f)];
            startrotation = rotations.FirstOrDefault(r => spell.Rotation.AlmostEqual(r, maxError)) + 180.Degrees();
            Type = Types.DiffractiveLaser;
            _times.AddRange(times);
        }
        if ((AID)spell.Action.ID == AID.LaserShower2)
        {
            DateTime[] times = [_time, _time.AddSeconds(6.5f), _time.AddSeconds(8.3f), _time.AddSeconds(10.1f)];
            startrotation = rotations.FirstOrDefault(r => caster.Rotation.AlmostEqual(r, maxError));
            Type = Types.LaserShower;
            _times.AddRange(times);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RefractedLaser)
        {
            ++NumCasts;
            if (NumCasts == 14)
            {
                NumCasts = 0;
                _times.Clear();
                Type = Types.None;
            }
        }
    }
}

class Rush : Components.BaitAwayChargeCast
{
    public Rush() : base(ActionID.MakeSpell(AID.Rush), 7) { }
}

class LaserShower : Components.LocationTargetedAOEs
{
    public LaserShower() : base(ActionID.MakeSpell(AID.LaserShower2), 10) { }
}

class DiffractiveLaser : Components.SelfTargetedAOEs
{
    public DiffractiveLaser() : base(ActionID.MakeSpell(AID.DiffractiveLaser), new AOEShapeCone(60, 75.Degrees())) { }
}

class SatelliteLaser : Components.RaidwideCast
{
    public SatelliteLaser() : base(ActionID.MakeSpell(AID.SatelliteLaser), "Raidwide + all lasers fire at the same time") { }
}

class CE31MetalFoxChaosStates : StateMachineBuilder
{
    public CE31MetalFoxChaosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SatelliteLaser>()
            .ActivateOnEnter<DiffractiveLaser>()
            .ActivateOnEnter<LaserShower>()
            .ActivateOnEnter<MagitekBitLasers>()
            .ActivateOnEnter<Rush>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 735, NameID = 13)] // bnpcname=9424
public class CE31MetalFoxChaos : BossModule
{
    public CE31MetalFoxChaos(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-234, 262), 30.2f)) { }
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var s in Enemies(OID.Boss))
            Arena.Actor(s, ArenaColor.Enemy, false);
        foreach (var s in Enemies(OID.MagitekBit))
            Arena.Actor(s, ArenaColor.Vulnerable, true);
    }
}
