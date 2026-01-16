

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
class ExplosionKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Spell_Explosion, 23, ignoreImmunes: true, shape: new AOEShapeCircle(4))
{
    public override IEnumerable<Source> Sources(int slot, Actor actor) => base.Sources(slot, actor).Where(s => actor.Position.InCircle(s.Origin, 4));
}

class FireBreathMeteowrath(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly List<(Actor Source, Actor Target, int Color)> _tethers = [];
    private BitMask _prey;

    public static readonly AOEShape BreathShape = new AOEShapeRect(80, 3);
    public static readonly AOEShape WrathShape = new AOEShapeRect(80, 5);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var color = (TetherID)tether.ID switch
        {
            TetherID._Gen_Tether_chn_arrow01f => 1,
            TetherID._Gen_Tether_chn_tergetfix2k1 => 2,
            _ => 0
        };
        if (color > 0 && WorldState.Actors.Find(tether.Target) is { } target)
            _tethers.Add((source, target, color));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        _tethers.RemoveAll(t => t.Source == source);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID._Gen_Icon_lockon8_t0w)
            _prey.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_FireBreath)
        {
            foreach (var (src, target, _) in _tethers)
                CurrentBaits.Add(new(src, target, WrathShape, Module.CastFinishAt(spell, 0.8f)));
            foreach (var (_, player) in Raid.WithSlot().IncludedInMask(_prey))
                CurrentBaits.Add(new(Module.PrimaryActor, player, BreathShape, Module.CastFinishAt(spell, 0.8f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_FireBreath1:
                NumCasts++;
                CurrentBaits.RemoveAll(b => b.Shape == BreathShape);
                _prey.Reset();
                break;
            case AID._Spell_MajesticMeteowrath:
                NumCasts++;
                CurrentBaits.RemoveAll(b => b.Shape == WrathShape);
                break;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (var (s, t, c) in _tethers)
            if (t == pc)
                Arena.AddLine(s.Position, t.Position, c == 1 ? ArenaColor.Danger : ArenaColor.Safe);
    }
}

class MajesticMeteorain(BossModule module) : Components.GenericAOEs(module, AID._Spell_MajesticMeteorain)
{
    private readonly List<WPos> _sources = [];
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
        {
            foreach (var src in _sources)
                yield return new(new AOEShapeRect(60, 5), src, default, _activation);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x16 and <= 0x19 && state == 0x00200010)
        {
            var xpos = index switch
            {
                0x16 => -21,
                0x17 => -11,
                0x18 => 11,
                0x19 => 21,
                _ => 0
            };
            _sources.Add(new(100 + xpos, 75));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_FireBreath)
            _activation = Module.CastFinishAt(spell, 0.8f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
            _sources.Clear();
        }
    }
}

class MajesticMeteor(BossModule module) : Components.StandardAOEs(module, AID._Spell_MajesticMeteor1, 6);

// meteorain: mapeffect 16 (west) through 19 (east)
