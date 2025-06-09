namespace BossMod.Dawntrail.Foray.ForkedTower.FT04Magitaur;

class UnsealedAura(BossModule module) : Components.RaidwideCastDelay(module, AID._Ability_UnsealedAura, AID._Ability_UnsealedAura1, 0.8f);

class AssassinsDagger(BossModule module) : Components.GenericAOEs(module)
{
    class Dagger(Angle angle, DateTime d1, DateTime d2)
    {
        public Angle Angle = angle;
        public DateTime Activation1 = d1;
        public DateTime Activation2 = d2;
        public int NumCasts;
    }

    private readonly List<Dagger> _daggers = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        DateTime next = default;
        foreach (var dagger in _daggers)
        {
            if (next == default)
                next = dagger.Activation1.AddSeconds(0.5f);
            if (dagger.Activation1 < next)
                yield return new AOEInstance(new AOEShapeRect(32, 3), Arena.Center, dagger.Angle, dagger.NumCasts > 0 ? dagger.Activation2 : dagger.Activation1, Color: ArenaColor.Danger);
            else if (dagger.Activation1 < next.AddSeconds(4))
                yield return new AOEInstance(new AOEShapeRect(32, 3), Arena.Center, dagger.Angle, dagger.Activation1, Risky: false);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_AssassinsDagger1)
        {
            var orientation = Angle.FromDirection(spell.LocXZ - caster.Position);
            var next = Module.CastFinishAt(spell);
            for (var i = 0; i < 6; i++)
            {
                _daggers.Add(new(orientation, next, next.AddSeconds(2)));
                next = next.AddSeconds(4);
                orientation += 70.Degrees();
            }
            _daggers.SortBy(d => d.Activation1);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_AssassinsDagger1 or AID._Ability_AssassinsDagger2 or AID._Ability_AssassinsDagger3)
        {
            NumCasts++;
            var dir = Angle.FromDirection(spell.TargetXZ - caster.Position);
            if (spell.TargetXZ.AlmostEqual(Arena.Center, 1))
                dir += 180.Degrees();

            var ix = _daggers.FindIndex(d => d.Angle.AlmostEqual(dir, 0.1f));
            if (ix >= 0)
            {
                if (_daggers[ix].NumCasts == 1)
                    _daggers.RemoveAt(ix);
                else
                    _daggers[ix].NumCasts++;
            }
        }
    }
}

// 12 hits, 10 degrees of rotation between each
class Dagger1(BossModule module) : Components.ChargeAOEs(module, AID._Ability_AssassinsDagger1, 3);
class Dagger2(BossModule module) : Components.DebugCasts(module, [AID._Ability_AssassinsDagger1, AID._Ability_AssassinsDagger2], new AOEShapeRect(0, 3, 32));

class CriticalAxeblow(BossModule module) : Components.StandardAOEs(module, AID._Ability_CriticalAxeblow, 20);
class CriticalAxeblowFloor(BossModule module) : Components.StandardAOEs(module, AID._Ability_CriticalAxeblow, FT04Magitaur.NotPlatforms)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).Select(s => s with { Rotation = default });
}
class CriticalLanceblow(BossModule module) : Components.StandardAOEs(module, AID._Ability_CriticalLanceblow, new AOEShapeDonut(10, 40));
class CriticalLanceblowFloor(BossModule module) : Components.StandardAOEs(module, AID._Ability_CriticalLanceblow, 0)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var aoe in base.ActiveAOEs(slot, actor))
        {
            yield return new AOEInstance(new AOEShapeRect(10, 10, 10), Arena.Center + new WDir(0, 14.5f), 45.Degrees(), aoe.Activation);
            yield return new AOEInstance(new AOEShapeRect(10, 10, 10), Arena.Center + new WDir(0, 14.5f).Rotate(120.Degrees()), 165.Degrees(), aoe.Activation);
            yield return new AOEInstance(new AOEShapeRect(10, 10, 10), Arena.Center + new WDir(0, 14.5f).Rotate(-120.Degrees()), -75.Degrees(), aoe.Activation);
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13947)]
public class FT04Magitaur(WorldState ws, Actor primary) : BossModule(ws, primary, new(700, -674), new ArenaBoundsCircle(31.5f))
{
    public override bool DrawAllPlayers => true;

    public static readonly AOEShapeCustom NotPlatforms = MakeNotPlatforms();

    private static AOEShapeCustom MakeNotPlatforms()
    {
        IEnumerable<WDir> rect(Angle offset) => CurveApprox.Rect(new(10, 0), new(0, 10)).Select(d => d.Rotate(45.Degrees() + offset) + new WDir(0, 14.5f).Rotate(offset));

        var clipper = new PolygonClipper();
        RelSimplifiedComplexPolygon arena = new(CurveApprox.Circle(31.5f, 1 / 90f));

        arena = clipper.Difference(new(arena), new(rect(default)));
        arena = clipper.Difference(new(arena), new(rect(120.Degrees())));
        arena = clipper.Difference(new(arena), new(rect(-120.Degrees())));
        return new(arena);
    }
}
