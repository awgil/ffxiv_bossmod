namespace BossMod.Dawntrail.Savage.RM11STheTyrant;

class Flatliner(BossModule module) : Components.Knockback(module, AID.Flatliner, ignoreImmunes: true)
{
    private DateTime _activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Flatliner)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Flatliner)
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

class FlatlinerArena(BossModule module) : Components.GenericAOEs(module, AID.Flatliner)
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

class ExplosionTower(BossModule module) : Components.CastTowers(module, AID.ExplosionTower, 4, minSoakers: 2, maxSoakers: 2)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
        {
            for (var i = 0; i < Towers.Count; i++)
            {
                Towers.Ref(i).ForbiddenSoakers = Raid.WithSlot().WhereActor(a => (a.Position.X > 100) != (Towers[i].Position.X > 100)).Mask();
            }
        }
    }
}
class ExplosionKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.ExplosionTower, 23, ignoreImmunes: true, shape: new AOEShapeCircle(4))
{
    public override IEnumerable<Source> Sources(int slot, Actor actor) => base.Sources(slot, actor).Where(s => actor.Position.InCircle(s.Origin, 4));

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (base.DestinationUnsafe(slot, actor, pos))
            return true;

        if (Module.PrimaryActor.CastInfo is { } ci && (AID)ci.Action.ID is AID.ArcadionAvalancheBoss1 or AID.ArcadionAvalancheBoss2 or AID.ArcadionAvalancheBoss3 or AID.ArcadionAvalancheBoss4)
            return pos.InRect(ci.LocXZ, ci.Rotation, 40, 0, 40);

        return false;
    }
}

class FireBreathMeteowrath : Components.GenericBaitAway
{
    private readonly List<(Actor Source, Actor Target, int Color)> _tethers = [];
    private BitMask _prey;

    public static readonly AOEShape BreathShape = new AOEShapeRect(80, 3);
    public static readonly AOEShape WrathShape = new AOEShapeRect(80, 5);

    // estimate
    public const float StretchDistance = 48f;

    public bool PreyAssigned { get; private set; }

    public FireBreathMeteowrath(BossModule module) : base(module)
    {
        EnableHints = false;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (var (s, t, c) in _tethers)
            if (t == pc)
                Arena.AddLine(s.Position, t.Position, c == 1 ? ArenaColor.Border : ArenaColor.Safe);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (EnableHints && ActiveBaitsOn(actor).FirstOrNull(b => b.Shape == WrathShape) is { } tetherBait)
            hints.Add("Stretch tether!", actor.Position.InCircle(tetherBait.Source.Position, StretchDistance));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (EnableHints && ActiveBaitsOn(actor).FirstOrNull(b => b.Shape == WrathShape) is { } tetherBait)
            hints.AddForbiddenZone(ShapeContains.Circle(tetherBait.Source.Position, 48), tetherBait.Activation);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var color = (TetherID)tether.ID switch
        {
            TetherID.Unstretched => 1,
            TetherID.Stretched => 2,
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
        if ((IconID)iconID == IconID.FireBreath)
        {
            PreyAssigned = true;
            _prey.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FireBreathBoss)
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
            case AID.FireBreathRect:
                NumCasts++;
                CurrentBaits.RemoveAll(b => b.Shape == BreathShape);
                _prey.Reset();
                break;
            case AID.MajesticMeteowrathRect:
                NumCasts++;
                CurrentBaits.RemoveAll(b => b.Shape == WrathShape);
                break;
        }
    }
}

class MajesticMeteorain(BossModule module) : Components.GenericAOEs(module, AID.MajesticMeteorainRect)
{
    private readonly List<WPos> _sources = [];
    private DateTime _activation;
    public bool Risky;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
        {
            foreach (var src in _sources)
                yield return new(new AOEShapeRect(60, 5), src, default, _activation, Risky: Risky);
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
        if ((AID)spell.Action.ID == AID.FireBreathBoss)
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

class MajesticMeteor(BossModule module) : Components.StandardAOEs(module, AID.MajesticMeteorAOE, 6)
{
    public bool BaitsDone { get; private set; }

    private DateTime _firstCastStart;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
        {
            if (_firstCastStart == default)
                _firstCastStart = WorldState.CurrentTime;
            else if (_firstCastStart.AddSeconds(3) < WorldState.CurrentTime)
                BaitsDone = true;
        }
    }
}

class FireBreathMeteowrathHints(BossModule module) : BossComponent(module)
{
    private readonly MajesticMeteorain _meteorLines = module.FindComponent<MajesticMeteorain>()!;
    private readonly Actor?[] _tetheredTo = new Actor?[8];
    private BitMask _prey;
    public bool Safe;

    private readonly List<WPos>[] _destination = Utils.GenArray<List<WPos>>(8, () => []);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.TheTyrant && Raid.TryFindSlot(tether.Target, out var slot))
            _tetheredTo[slot] = source;
    }
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.FireBreath)
            _prey.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FireBreathBoss)
            Redraw();
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var spot in _destination[pcSlot])
            Arena.AddCircle(spot, 0.75f, Safe ? ArenaColor.Safe : ArenaColor.Danger);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var spot in _destination[slot])
            movementHints.Add((actor.Position, spot, Safe ? ArenaColor.Safe : ArenaColor.Danger));
    }

    void Redraw()
    {
        foreach (var (slot, player) in Raid.WithSlot())
        {
            _destination[slot].Clear();

            if (_tetheredTo[slot] is { } tetherSource)
            {
                var safeX = tetherSource.Position.X > 100 ? 84 : 116;
                var safeZ = tetherSource.Position.Z > 100 ? 81 : 119;
                AddDest(slot, player, new(safeX, safeZ));
            }
            else if (_prey[slot])
            {
                var safeX = player.Position.X < 100 ? 84 : 116;
                // TODO: tweak Z coordinates based on buddy's tether source? inner tether buddies can stand further out
                AddDest(slot, player, new(safeX, 87.75f));
                AddDest(slot, player, new(safeX, 112.25f));
            }
        }
    }

    void AddDest(int slot, Actor player, WPos dest)
    {
        _destination[slot].Add(new WPos(dest.X - 1, dest.Z));
        _destination[slot].Add(new WPos(dest.X + 1, dest.Z));
        _destination[slot].RemoveAll(d => _meteorLines.ActiveAOEs(slot, player).Any(i => i.Check(d)));
    }
}

class MassiveMeteor(BossModule module) : Components.StackWithIcon(module, (uint)IconID.MultiStack, AID.MassiveMeteorStack, 6, 6.2f, minStackSize: 4)
{
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == StackAction)
        {
            NumCasts++;
            if (NumCasts >= Stacks.Count * 5)
            {
                Stacks.Clear();
                NumFinishedStacks += 2;
            }
        }
    }
}

class ArcadionAvalancheRect(BossModule module) : Components.GroupedAOEs(module, [AID.ArcadionAvalanchePlatform4, AID.ArcadionAvalanchePlatform2, AID.ArcadionAvalanchePlatform1, AID.ArcadionAvalanchePlatform3], new AOEShapeRect(40, 20));

class ArcadionAvalancheBoss(BossModule module) : Components.GroupedAOEs(module, [AID.ArcadionAvalancheBoss1, AID.ArcadionAvalancheBoss2, AID.ArcadionAvalancheBoss3, AID.ArcadionAvalancheBoss4], new AOEShapeRect(40, 20))
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (IDs.Contains(spell.Action))
        {
            Arena.Bounds = new ArenaBoundsRect(10, 20);
            if (spell.Rotation.Deg < 0)
                Arena.Center = new(116, 100);
            else
                Arena.Center = new(84, 100);
        }
    }
}

class CrownOfArcadiaArena(BossModule module) : Components.GenericAOEs(module, AID.CrownOfArcadia)
{
    private DateTime _resolve;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_resolve != default)
        {
            yield return new(new AOEShapeRect(6, 20), new(120, 100), 90.Degrees(), _resolve);
            yield return new(new AOEShapeRect(6, 20), new(80, 100), -90.Degrees(), _resolve);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _resolve = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _resolve = default;
            Arena.Bounds = new ArenaBoundsSquare(20);
            Arena.Center = new(100, 100);
        }
    }
}
