namespace BossMod.Endwalker.Dungeon.D10Troia.D103Scarmiglione;

public enum OID : uint
{
    Boss = 0x39C5,
    Necroserf = 0x39C7,
    Helper = 0x233C,
}

public enum AID : uint
{
    CursedEcho = 30257, // Boss->self, 4.0s cast, range 40 circle
    RottenRampageAOE = 30232, // Helper->location, 10.0s cast, range 6 circle
    RottenRampageSpread = 30233, // Helper->player, 10.0s cast, range 6 circle
    BlightedBedevilment = 30235, // Boss->self, 4.9s cast, range 9 circle
    VacuumWave = 30236, // Helper->self, 5.4s cast, range 40 circle
    BlightedBladework = 30260, // Helper->self, 11.0s cast, range 25 circle
    BlightedSweep = 30261, // Boss->self, 7.0s cast, range 40 180-degree cone
    Firedamp = 30263, // Helper->player, 5.4s cast, range 5 circle
    Nox = 30241, // Helper->self, 5.0s cast, range 10 circle
    VoidVortex = 30243, // Helper->player, 5.0s cast, range 6 circle
    VoidGravity = 30242, // Helper->players, 5.0s cast, range 6 circle
}

class VoidGravity(BossModule module) : Components.SpreadFromCastTargets(module, AID.VoidGravity, 6);
class Firedamp(BossModule module) : Components.BaitAwayCast(module, AID.Firedamp, new AOEShapeCircle(5), true);
class Nox(BossModule module) : Components.StandardAOEs(module, AID.Nox, new AOEShapeCircle(10));
class VoidVortex(BossModule module) : Components.StackWithCastTargets(module, AID.VoidVortex, 6);
class BlightedBladework(BossModule module) : Components.StandardAOEs(module, AID.BlightedBladework, new AOEShapeCircle(25));
class BlightedSweep(BossModule module) : Components.StandardAOEs(module, AID.BlightedSweep, new AOEShapeCone(40, 90.Degrees()));
class BlightedBedevilment(BossModule module) : Components.StandardAOEs(module, AID.BlightedBedevilment, new AOEShapeCircle(9));
class CursedEcho(BossModule module) : Components.RaidwideCast(module, AID.CursedEcho);
class RottenRampage(BossModule module) : Components.StandardAOEs(module, AID.RottenRampageAOE, 6);
class RottenRampagePlayer(BossModule module) : Components.SpreadFromCastTargets(module, AID.RottenRampageSpread, 6);

class Necroserf(BossModule module) : Components.Adds(module, (uint)OID.Necroserf, forbidDots: true);

class VacuumWave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.VacuumWave, 30)
{
    BitMask _activeWalls = new(0xfff);

    static readonly float WallHalfAngle = MathF.Atan2(4.5f, 19.05f);

    static Angle AngleToWall(int bit)
    {
        var group = bit / 3;
        var ix = bit % 3;
        var baseDir = 135 - 90 * group;
        return (baseDir + (1 - ix) * 26).Degrees();
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (var bit in _activeWalls.SetBits())
        {
            var realDir = AngleToWall(bit).ToDirection();
            // wall center at 20 units away from arena center, with a depth of 0.45 units, minus 0.5 units for player hitbox
            Arena.AddLine(Arena.Center + realDir * 19.05f + realDir.OrthoL() * 4.5f, Arena.Center + realDir * 19.05f + realDir.OrthoR() * 4.5f, ArenaColor.Border);
        }
    }

    float DistanceToWall(WPos player, WPos origin)
    {
        var dist = 30f;
        var dir = player - origin;
        var angle = dir.ToAngle();
        foreach (var bit in _activeWalls.SetBits())
        {
            var wa = AngleToWall(bit);
            if (angle.AlmostEqual(wa, WallHalfAngle))
            {
                var wd = wa.ToDirection();
                dist = MathF.Min(dist, Intersect.RaySegment(dir, dir.Normalized(), wd * 19.05f + wd.OrthoL() * 4.5f, wd * 19.05f + wd.OrthoR() * 4.5f));
            }
        }

        return dist;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 5 and < 17)
        {
            if (state == 0x00020001)
                _activeWalls.Clear(index - 5);
            if (state == 0x00040001)
                _activeWalls.Set(index - 5);
        }
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var caster in Casters)
        {
            var activation = Module.CastFinishAt(caster.CastInfo);
            if (!IsImmune(slot, activation))
            {
                var origin = caster.CastInfo!.LocXZ;
                yield return new(origin, DistanceToWall(actor.Position, origin), activation);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
        {
            List<Func<WPos, bool>> safeCones = [];
            foreach (var bit in _activeWalls.SetBits())
                // we need to aim for the center of a wall, since they're flat and aiming at the edge will cause you to slide off and land in the voidzone
                safeCones.Add(ShapeContains.InvertedCone(Arena.Center, 30, AngleToWall(bit), (WallHalfAngle * 0.5f).Radians()));

            if (safeCones.Count > 0)
                hints.AddForbiddenZone(ShapeContains.Intersection(safeCones), src.Activation);
        }
    }
}

class D103ScarmiglioneStates : StateMachineBuilder
{
    public D103ScarmiglioneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VacuumWave>()
            .ActivateOnEnter<RottenRampage>()
            .ActivateOnEnter<RottenRampagePlayer>()
            .ActivateOnEnter<BlightedBedevilment>()
            .ActivateOnEnter<CursedEcho>()
            .ActivateOnEnter<BlightedBladework>()
            .ActivateOnEnter<BlightedSweep>()
            .ActivateOnEnter<Firedamp>()
            .ActivateOnEnter<Nox>()
            .ActivateOnEnter<Necroserf>()
            .ActivateOnEnter<VoidVortex>()
            .ActivateOnEnter<VoidGravity>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 869, NameID = 11372)]
public class D103Scarmiglione(WorldState ws, Actor primary) : BossModule(ws, primary, new(-35, -298), new ArenaBoundsCircle(20));
