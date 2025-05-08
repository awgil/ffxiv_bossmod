namespace BossMod.Endwalker.Quest.WhereEverythingBegins;

public enum OID : uint
{
    Boss = 0x39B7,
    FilthyShackle = 0x39BB,
    VarshahnShield = 0x1EB762,
}

public enum AID : uint
{
    BlightedSweep = 30052,
    CursedNoise = 30026,
    BlightedBuffet = 30032,
    VacuumWave = 30033,
    BlightedSwathe = 30044,
    VoidQuakeIII = 30046,
    // stack on varshahn
    VoidVortex = 30025, // 233C->players, 5.0s cast, range 6 circle
    RottenRampage = 30031, // 233C->location, 10.0s cast, range 6 circle
    RottenRampageSpread = 30056, // 233C->player/39C1/39BC/39BF/39BE, 10.0s cast, range 6 circle
}

class VoidVortex(BossModule module) : Components.StackWithCastTargets(module, AID.VoidVortex, 6, minStackSize: 1);
class RottenRampageSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.RottenRampageSpread, 6);
class RottenRampage(BossModule module) : Components.StandardAOEs(module, AID.RottenRampage, 6);
class BlightedSwathe(BossModule module) : Components.StandardAOEs(module, AID.BlightedSwathe, new AOEShapeCone(40, 90.Degrees()));
class BlightedSweep(BossModule module) : Components.StandardAOEs(module, AID.BlightedSweep, new AOEShapeCone(40, 90.Degrees()));
class BlightedBuffet(BossModule module) : Components.StandardAOEs(module, AID.BlightedBuffet, new AOEShapeCircle(9));
class VacuumWave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.VacuumWave, 5)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
            hints.AddForbiddenZone(new AOEShapeDonut(13, 60), c.Position, activation: Module.CastFinishAt(c.CastInfo));
    }
}
class VoidQuakeIII(BossModule module) : Components.StandardAOEs(module, AID.VoidQuakeIII, new AOEShapeCross(40, 5));
class DeathWall(BossModule module) : Components.GenericAOEs(module, AID.CursedNoise)
{
    private DateTime? WallActivate;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (WallActivate is DateTime dt)
            yield return new AOEInstance(new AOEShapeDonut(18, 100), Arena.Center, Activation: dt);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        // 5.7 seconds

        if (spell.Action == WatchedAction && NumCasts == 0)
            WallActivate = WorldState.FutureTime(5.7f);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (WallActivate != null)
            hints.Add("Raidwide + poison wall spawn", false);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 1 && state == 0x00020001)
        {
            WallActivate = null;
            Arena.Bounds = new ArenaBoundsCircle(18);
        }
    }
}

class Shield(BossModule module) : BossComponent(module)
{
    private const float ShieldRadius = 5;
    private Actor? ShieldObj => Module.Enemies(OID.VarshahnShield).FirstOrDefault();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ShieldObj != null)
            hints.AddForbiddenZone(new AOEShapeDonut(ShieldRadius, 30), ShieldObj.Position);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ShieldObj is Actor obj && (actor.Position - obj.Position).Length() > ShieldRadius)
            hints.Add("Go to safe zone!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (ShieldObj is Actor obj)
            Arena.AddCircleFilled(obj.Position, ShieldRadius, ArenaColor.SafeFromAOE);
    }
}

public class ScarmiglioneStates : StateMachineBuilder
{
    public ScarmiglioneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BlightedSweep>()
            .ActivateOnEnter<BlightedBuffet>()
            .ActivateOnEnter<BlightedSwathe>()
            .ActivateOnEnter<VacuumWave>()
            .ActivateOnEnter<VoidQuakeIII>()
            .ActivateOnEnter<DeathWall>()
            .ActivateOnEnter<RottenRampage>()
            .ActivateOnEnter<RottenRampageSpread>()
            .ActivateOnEnter<Shield>()
            .ActivateOnEnter<VoidVortex>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70130, NameID = 11407)]
public class Scarmiglione(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -148), new ArenaBoundsCircle(19.5f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = e.Actor.OID == (uint)OID.FilthyShackle ? 1 : 0;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
