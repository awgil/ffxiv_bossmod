namespace BossMod.RealmReborn.Trial.T02TitanN;

public enum OID : uint
{
    Boss = 0xF6, // x1
    Helper = 0x1B2, // x4
    TitansHeart = 0x5E3, // Part type, spawn during fight
    GraniteGaol = 0x5A4, // spawn during fight
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast
    Tumult = 642, // Boss->self, no cast, raidwide
    RockBuster = 641, // Boss->self, no cast, range 11.25 ?-degree cone cleave
    Geocrush = 651, // Boss->self, 4.5s cast, range 27 aoe with ? falloff
    Landslide = 650, // Boss->self, 3.0s cast, range 40.25 width 6 rect aoe with knockback 15
    RockThrow = 645, // Boss->player, no cast, visual for granite gaol spawn
    GraniteSepulchre = 28799, // GraniteGaol->self, 15.0s cast, oneshot target if gaol not killed
    EarthenFury = 652, // Boss->self, no cast, wipe if heart not killed, otherwise just a raidwide
    WeightOfTheLand = 644, // Boss->self, 3.0s cast, visual
    WeightOfTheLandAOE = 973, // Helper->location, 3.5s cast, range 6 puddle
}

class Hints(BossModule module) : BossComponent(module)
{
    private DateTime _heartSpawn;

    public override void AddGlobalHints(GlobalHints hints)
    {
        var heartExists = ((T02TitanN)Module).ActiveHeart.Any();
        if (_heartSpawn == default && heartExists)
        {
            _heartSpawn = WorldState.CurrentTime;
        }
        if (_heartSpawn != default && heartExists)
        {
            hints.Add($"Heart enrage in: {Math.Max(62 - (WorldState.CurrentTime - _heartSpawn).TotalSeconds, 0.0f):f1}s");
        }
    }
}

class RockBuster(BossModule module) : Components.Cleave(module, AID.RockBuster, new AOEShapeCone(11.25f, 60.Degrees())); // TODO: verify angle
class Geocrush(BossModule module) : Components.StandardAOEs(module, AID.Geocrush, new AOEShapeCircle(18)); // TODO: verify falloff
class Landslide(BossModule module) : Components.StandardAOEs(module, AID.Landslide, new AOEShapeRect(40, 3));
class WeightOfTheLand(BossModule module) : Components.StandardAOEs(module, AID.WeightOfTheLandAOE, 6);

class T02TitanNStates : StateMachineBuilder
{
    public T02TitanNStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hints>()
            .ActivateOnEnter<RockBuster>()
            .ActivateOnEnter<Geocrush>()
            .ActivateOnEnter<Landslide>()
            .ActivateOnEnter<WeightOfTheLand>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 57, NameID = 1801)]
public class T02TitanN : BossModule
{
    private readonly IReadOnlyList<Actor> _heart;
    public IEnumerable<Actor> ActiveHeart => _heart.Where(h => h.IsTargetable && !h.IsDead);

    public T02TitanN(WorldState ws, Actor primary) : base(ws, primary, new(-0, 0), new ArenaBoundsCircle(20)) // note: initial area is size 25, but it becomes smaller at 75%
    {
        _heart = Enemies(OID.TitansHeart);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.GraniteGaol => 3,
                OID.TitansHeart => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
