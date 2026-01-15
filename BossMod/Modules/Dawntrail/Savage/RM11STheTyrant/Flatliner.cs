namespace BossMod.Dawntrail.Savage.RM11STheTyrant;

class Flatliner(BossModule module) : Components.Knockback(module, AID._Weaponskill_Flatliner1, ignoreImmunes: true)
{
    private DateTime _activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Flatliner1)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Flatliner1)
        {
            NumCasts++;
            _activation = default;
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => base.DestinationUnsafe(slot, actor, pos) || pos.InRect(Arena.Center, default(Angle), 20, 20, 6);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(Arena.Center, 15, _activation);
    }
}

class FlatlinerArena(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_Flatliner1)
{
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(new AOEShapeRect(20, 6, 20), Arena.Center, default, _activation, Risky: false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_activation != default)
            hints.TemporaryObstacles.Add(ShapeContains.InvertedRect(Arena.Center, default(Angle), 20, 20, 20));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _activation = Module.CastFinishAt(spell);
            Arena.Bounds = new ArenaBoundsRect(26, 20);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            var plat = CurveApprox.Rect(new WDir(0, 20), new WDir(10, 0));

            var clipper = Arena.Bounds.Clipper;
            var p1 = new RelSimplifiedComplexPolygon(plat.Select(p => p + new WDir(16, 0)));
            var p2 = new RelSimplifiedComplexPolygon(plat.Select(p => p - new WDir(16, 0)));

            Arena.Bounds = new ArenaBoundsCustom(26, clipper.Union(new(p1), new(p2)));
        }
    }
}

class ExplosionTower(BossModule module) : Components.CastTowers(module, AID._Spell_Explosion, 4, minSoakers: 2, maxSoakers: 2);
class ExplosionKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Spell_Explosion, 23, ignoreImmunes: true, shape: new AOEShapeCircle(4));

class MeteowrathTether(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor Source, Actor Target)> _tethers = [];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_chn_arrow01f && WorldState.Actors.Find(tether.Target) is { } target)
            _tethers.Add((source, target));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        _tethers.RemoveAll(t => t.Source == source);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (s, t) in _tethers)
            Arena.AddLine(s.Position, t.Position, ArenaColor.Danger);
    }
}
