using Dalamud.Bindings.ImGui;

namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

class Roar1(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_Roar);
class Roar2(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_Roar1);
class Roar3(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_Roar2);

class ChainbladeBlow(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_ChainbladeBlow1, AID._Weaponskill_ChainbladeBlow2, AID._Weaponskill_ChainbladeBlow7, AID._Weaponskill_ChainbladeBlow8, AID._Weaponskill_ChainbladeBlow13, AID._Weaponskill_ChainbladeBlow14, AID._Weaponskill_ChainbladeBlow19, AID._Weaponskill_ChainbladeBlow20], new AOEShapeRect(40, 2));

class ChainbladeRadiance(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_WyvernsRadiance, AID._Weaponskill_WyvernsRadiance7, AID._Weaponskill_WyvernsRadiance19, AID._Weaponskill_WyvernsRadiance23], new AOEShapeRect(80, 14));

class ChainbladeRepeat(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AID[] TailsCast = [AID._Weaponskill_ChainbladeBlow1, AID._Weaponskill_ChainbladeBlow2, AID._Weaponskill_ChainbladeBlow7, AID._Weaponskill_ChainbladeBlow8, AID._Weaponskill_ChainbladeBlow13, AID._Weaponskill_ChainbladeBlow14, AID._Weaponskill_ChainbladeBlow19, AID._Weaponskill_ChainbladeBlow20];
    private static readonly AID[] BossCast = [AID._Weaponskill_WyvernsRadiance, AID._Weaponskill_WyvernsRadiance7, AID._Weaponskill_WyvernsRadiance19, AID._Weaponskill_WyvernsRadiance23];

    private static readonly AID[] TailsFast = [AID._Weaponskill_ChainbladeBlow4, AID._Weaponskill_ChainbladeBlow5, AID._Weaponskill_ChainbladeBlow10, AID._Weaponskill_ChainbladeBlow11, AID._Weaponskill_ChainbladeBlow16, AID._Weaponskill_ChainbladeBlow17, AID._Weaponskill_ChainbladeBlow22, AID._Weaponskill_ChainbladeBlow23];
    private static readonly AID[] BossFast = [AID._Weaponskill_WyvernsRadiance1, AID._Weaponskill_WyvernsRadiance8, AID._Weaponskill_WyvernsRadiance21, AID._Weaponskill_WyvernsRadiance24];

    private static readonly AOEShape TailShape = new AOEShapeRect(40, 2);
    private static readonly AOEShape CleaveShape = new AOEShapeRect(80, 14);

    public bool Draw;

    private readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Draw ? _predicted : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = (AID)spell.Action.ID;
        var shape = TailsCast.Contains(id)
            ? TailShape
            : BossCast.Contains(id)
                ? CleaveShape
                : null;

        if (shape == null)
            return;

        var n = Module.PrimaryActor.Rotation.ToDirection().OrthoR();
        var d0 = spell.Rotation.ToDirection();
        var angle = d0 - d0.Dot(n) * n * 2;

        var d1 = spell.LocXZ - Module.PrimaryActor.Position;
        var src = d1 - d1.Dot(n) * n * 2;

        _predicted.Add(new(shape, Module.PrimaryActor.Position + src, angle.ToAngle(), Module.CastFinishAt(spell, 4)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = (AID)spell.Action.ID;

        if (BossCast.Contains(id))
            Draw = true;

        if (TailsFast.Contains(id))
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);

        if (BossFast.Contains(id))
        {
            NumCasts++;
            Draw = false;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class WhiteFlash(BossModule module) : Components.StackWithCastTargets(module, AID._Weaponskill_WhiteFlash, 6, maxStackSize: 4);
class Dragonspark(BossModule module) : Components.StackWithCastTargets(module, AID._Weaponskill_Dragonspark, 6, maxStackSize: 4);

class WhiteFlashDragonspark(BossModule module) : Components.CastCounterMulti(module, [AID._Weaponskill_WhiteFlash, AID._Weaponskill_Dragonspark]);

class BossSiegeflight(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_GuardianSiegeflight, AID._Weaponskill_GuardianSiegeflight2, AID._Weaponskill_WyvernsSiegeflight, AID._Weaponskill_WyvernsSiegeflight2], new AOEShapeRect(40, 2));
class HelperSiegeflight(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_GuardianSiegeflight1, AID._Weaponskill_GuardianSiegeflight3, AID._Weaponskill_WyvernsSiegeflight1, AID._Weaponskill_WyvernsSiegeflight3], new AOEShapeRect(40, 4));

class GuardianResonance(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_GuardianResonance, AID._Weaponskill_GuardianResonance4], new AOEShapeRect(40, 8));

class GuardianResonancePuddle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GuardianResonance1, 6)
{
    public int NumStarted { get; private set; }
    private DateTime _lastCastStarted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction && _lastCastStarted.AddSeconds(2) < WorldState.CurrentTime)
        {
            _lastCastStarted = WorldState.CurrentTime;
            NumStarted++;
        }
    }
}

abstract class ResonanceTower : Components.CastTowers
{
    private DateTime _lastBaitIn;

    protected ResonanceTower(BossModule module, AID aid, float radius, bool tankbuster) : base(module, aid, radius, maxSoakers: 2, damageType: tankbuster ? AIHints.PredictedDamageType.Tankbuster : AIHints.PredictedDamageType.Raidwide)
    {
        EnableHints = false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_lastBaitIn != default)
            foreach (var t in Towers)
                // prevent baits from dropping in the middle 1.5y of each tower - small tower is only 2y and large tower tanks should have plenty of room
                hints.AddForbiddenZone(ShapeContains.Circle(t.Position, 7.5f), _lastBaitIn);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_lastBaitIn != default && Towers.Any(t => actor.Position.InCircle(t.Position, 7.5f)))
            hints.Add("Bait away from towers!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (Towers.Count > 0 && !EnableHints)
            Arena.AddCircle(pc.Position, 6, ArenaColor.Danger);
    }

    public void EnableBaitHints(float deadline)
    {
        _lastBaitIn = WorldState.FutureTime(deadline);
    }

    public void DisableBaitHints()
    {
        _lastBaitIn = default;
        EnableHints = true;
    }
}

class GuardianResonanceTowerSmall(BossModule module) : ResonanceTower(module, AID._Weaponskill_GuardianResonance2, 2, false);

class GuardianResonanceTowerLarge(BossModule module) : ResonanceTower(module, AID._Weaponskill_GuardianResonance3, 4, true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
            foreach (ref var t in Towers.AsSpan())
                t.ForbiddenSoakers |= Raid.WithSlot().WhereActor(a => a.Role != Role.Tank).Mask();
    }
}

class WyvernsRadiancePuddle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_WyvernsRadiance3, 6);
class WyvernsRadianceExawave(BossModule module) : Components.Exaflare(module, new AOEShapeRect(8, 20))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_WyvernsRadiance2)
        {
            Lines.Add(new()
            {
                Next = spell.LocXZ,
                Advance = caster.Rotation.ToDirection() * 8,
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.5f,
                ExplosionsLeft = 5,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_WyvernsRadiance2 or AID._Weaponskill_WyvernsRadiance4 && Lines.Count > 0)
        {
            AdvanceLine(Lines[0], caster.Position);
            Lines.RemoveAll(l => l.ExplosionsLeft <= 0);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (c, t, r) in ImminentAOEs())
            hints.AddForbiddenZone(Shape, c, r, t);
    }
}

class WyvernsRadianceSides(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_WyvernsRadiance5, AID._Weaponskill_WyvernsRadiance6, AID._Weaponskill_WyvernsRadiance25, AID._Weaponskill_WyvernsRadiance26], new AOEShapeRect(40, 9));

class WyvernsRadianceQuake(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(8), new AOEShapeDonut(8, 14), new AOEShapeDonut(14, 20), new AOEShapeDonut(20, 26)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_WyvernsRadiance9)
            AddSequence(caster.Position, Module.CastFinishAt(spell), default);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID._Weaponskill_WyvernsRadiance9 => 0,
            AID._Weaponskill_WyvernsRadiance10 => 1,
            AID._Weaponskill_WyvernsRadiance11 => 2,
            AID._Weaponskill_WyvernsRadiance12 => 3,
            _ => -1
        };
        if (order >= 0)
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(2));
    }
}

class Rush(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_Rush)
{
    private readonly List<(WPos From, WPos To, DateTime Activation)> _charges = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _charges)
            yield return new(new AOEShapeRect((c.To - c.From).Length(), 6), c.From, (c.To - c.From).ToAngle(), c.Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_3)
        {
            _charges.Add((caster.Position, spell.LocXZ, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_Rush or AID._Weaponskill_Rush1)
        {
            NumCasts++;
            _charges.RemoveAll(c => c.To.AlmostEqual(spell.TargetXZ, 1));
        }
    }
}

class WyvernsOuroblade(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_WyvernsOuroblade1, AID._Weaponskill_WyvernsOuroblade3, AID._Weaponskill_WyvernsOuroblade5, AID._Weaponskill_WyvernsOuroblade7], new AOEShapeCone(40, 90.Degrees()));

class WildEnergy(BossModule module) : Components.SpreadFromCastTargets(module, AID._Weaponskill_WildEnergy, 6);

class SteeltailThrust(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_SteeltailThrust1, AID._Weaponskill_SteeltailThrust3], new AOEShapeRect(60, 3));

class ChainbladeCharge(BossModule module) : Components.StackWithIcon(module, (uint)IconID._Gen_Icon_com_share2i, AID._Weaponskill_ChainbladeCharge2, 6, 8.3f, minStackSize: 5);

class WyvernsVengeance(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_WyvernsVengeance)
        {
            Lines.Add(new()
            {
                Next = spell.LocXZ,
                Advance = caster.Rotation.ToDirection() * 8,
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 1.1f,
                ExplosionsLeft = 3,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_WyvernsVengeance or AID._Weaponskill_WyvernsVengeance1)
        {
            NumCasts++;
            var ix = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 1));
            if (ix >= 0)
                AdvanceLine(Lines[ix], caster.Position);
        }
    }
}

class WyvernsRadianceCrystal(BossModule module) : Components.GenericAOEs(module)
{
    private bool _active;

    private readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var nextActive = DateTime.MinValue;
        foreach (var item in _predicted)
        {
            if (nextActive == default)
                nextActive = item.Activation;

            var highlight = item.Activation < nextActive.AddSeconds(1);

            yield return item with { Color = highlight ? ArenaColor.Danger : ArenaColor.AOE };
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_active)
            return;

        if ((AID)spell.Action.ID == AID._Weaponskill_WyvernsVengeance)
        {
            _active = true;

            foreach (var big in Module.Enemies(OID._Gen_CrackedCrystal))
            {
                var first = big.Position.InCircle(Arena.Center, 11);
                var activate = Module.CastFinishAt(spell, first ? 1.6f : 2.7f);
                _predicted.Add(new(new AOEShapeCircle(12), big.Position, big.Rotation, activate));

                foreach (var small in Module.Enemies(OID._Gen_CrackedCrystal1).Where(c => c.Position.InCircle(big.Position, 14)))
                {
                    var sameTime = small.Position.InCircle(big.Position, 12);
                    _predicted.Add(new(new AOEShapeCircle(6), small.Position, small.Rotation, sameTime ? activate : activate.AddSeconds(1.2f)));
                }
            }

            _predicted.SortBy(p => p.Activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_WyvernsRadiance15 or AID._Weaponskill_WyvernsRadiance16)
        {
            NumCasts++;
            _predicted.RemoveAll(p => p.Origin.AlmostEqual(caster.Position, 0.5f));
        }
    }
}

class ForgedFury(BossModule module) : Components.CastHint(module, null, "Raidwide")
{
    private readonly List<DateTime> _activations = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_ForgedFury1 or AID._Weaponskill_ForgedFury2 or AID._Weaponskill_ForgedFury3)
        {
            _activations.Add(Module.CastFinishAt(spell));
            _activations.Sort();
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_activations.Count > 0)
            hints.Add(Hint);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activations.Count > 0)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), _activations[0]);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_ForgedFury1 or AID._Weaponskill_ForgedFury2 or AID._Weaponskill_ForgedFury3)
        {
            NumCasts++;
            if (_activations.Count > 0)
                _activations.RemoveAt(0);
        }
    }
}

class ClamorousJump(BossModule module) : Components.CastCounterMulti(module, [AID._Weaponskill_ClamorousChase1, AID._Weaponskill_ClamorousChase11]);

class ClamorousCleave(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _predicted;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_predicted);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var angle = (spell.TargetXZ - caster.Position).ToAngle();
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_ClamorousChase1:
                _predicted = new(new AOEShapeCone(40, 90.Degrees()), spell.TargetXZ, angle - 90.Degrees(), WorldState.FutureTime(2));
                break;
            case AID._Weaponskill_ClamorousChase11:
                _predicted = new(new AOEShapeCone(40, 90.Degrees()), spell.TargetXZ, angle + 90.Degrees(), WorldState.FutureTime(2));
                break;
            case AID._Weaponskill_ClamorousChase2:
            case AID._Weaponskill_ClamorousChase21:
                NumCasts++;
                _predicted = null;
                break;
        }
    }
}

class ClamorousBait(BossModule module) : Components.CastCounterMulti(module, [AID._Weaponskill_ClamorousChase2, AID._Weaponskill_ClamorousChase21])
{
    private readonly int[] _order = Utils.MakeArray(8, -1);
    private readonly Actor?[] _targets = Utils.MakeArray<Actor?>(8, null);
    private int _side = 0;
    private int _nextBait = -1;
    private WPos _source;
    private DateTime _nextJump;

    // TODO: figure out actual tether length
    public const int TetherLength = 22;

    private DateTime NextCleave => _nextJump.AddSeconds(2);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var order = (int)iconID - (int)IconID._Gen_Icon_m0361trg_b1t;
        if (order is >= 0 and < 8 && Raid.TryFindSlot(actor, out var slot))
        {
            _order[slot] = order;
            _targets[order] = actor;
            if (_order.All(o => o >= 0))
                Activate();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_ClamorousChase:
                _side = 1;
                _nextJump = Module.CastFinishAt(spell, 0.2f);
                Activate();
                break;
            case AID._Weaponskill_ClamorousChase3:
                _side = -1;
                _nextJump = Module.CastFinishAt(spell, 0.2f);
                Activate();
                break;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (GetNextBait() is var (cleaveSrc, rotation))
        {
            if (cleaveSrc == pc)
            {
                Arena.AddCone(cleaveSrc.Position, 40, rotation, 90.Degrees(), ArenaColor.Danger);
                Arena.AddCircle(cleaveSrc.Position, 6, ArenaColor.Danger);
            }
            else
            {
                Arena.ZoneCone(cleaveSrc.Position, 0, 40, rotation, 90.Degrees(), ArenaColor.AOE);
                Arena.ZoneCircle(cleaveSrc.Position, 6, ArenaColor.AOE);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (WatchedActions.Contains(spell.Action))
            NumCasts++;

        if ((AID)spell.Action.ID is AID._Weaponskill_ClamorousChase1 or AID._Weaponskill_ClamorousChase11)
        {
            _nextBait++;
            _source = spell.TargetXZ;
            _nextJump = WorldState.FutureTime(3);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_nextBait >= 0 && _order[slot] >= _nextBait)
            hints.Add($"Order: {_order[slot] + 1}", false);

        if (GetNextBait() is var (cleaveSrc, rotation))
        {
            var hint = _nextBait == 0 ? "Get away from boss!" : "Get away from buddy!";
            if (cleaveSrc == actor)
            {
                hints.Add(hint, actor.Position.InCircle(_source, TetherLength));

                if (Raid.WithoutSlot().Exclude(actor).Any(a => !a.Position.AlmostEqual(_source, 0.5f) && a.Position.InCone(cleaveSrc.Position, rotation, 90.Degrees())))
                    hints.Add("Bait away from raid!");
            }
            else
            {
                if (_order[slot] == _nextBait + 1)
                    hints.Add("Get away from buddy!", actor.Position.InCircle(cleaveSrc.Position, TetherLength));

                if (actor.Position.InCircle(cleaveSrc.Position, 6))
                    hints.Add("GTFO from bait!");
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (GetNextBait() is var (cleaveSrc, rotation))
        {
            if (cleaveSrc == actor)
            {
                // TODO: do we need to add a hint to prevent cleaving the party...
                hints.AddForbiddenZone(ShapeContains.Circle(_source, TetherLength), _nextJump);
                foreach (var p in Raid.WithoutSlot().Exclude(actor))
                    hints.AddForbiddenZone(ShapeContains.Circle(p.Position, 6), _nextJump);
            }
            else
            {
                hints.AddForbiddenZone(ShapeContains.Circle(cleaveSrc.Position, 6), _nextJump);
                hints.AddForbiddenZone(ShapeContains.Cone(cleaveSrc.Position, 40, rotation, 90.Degrees()), NextCleave);

                if (_order[slot] == _nextBait + 1)
                    hints.AddForbiddenZone(ShapeContains.Circle(cleaveSrc.Position, TetherLength), NextCleave.AddSeconds(1));
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (_nextBait >= 0)
        {
            if (_order[playerSlot] == _nextBait)
                return PlayerPriority.Danger;

            if (_order[playerSlot] == _nextBait + 1)
                return PlayerPriority.Interesting;
        }

        return base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
    }

    private void Activate()
    {
        if (_targets.All(t => t != null) && _side != 0)
        {
            _source = Module.PrimaryActor.Position;
            _nextBait = 0;
        }
    }

    private (Actor Target, Angle Angle)? GetNextBait()
    {
        return _targets.BoundSafeAt(_nextBait) is { } cleaveSrc
            ? (cleaveSrc, (_source - cleaveSrc.Position).ToAngle() + 90.Degrees() * _side)
            : null;
    }
}

class WyvernsWealCast(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_WyvernsWeal2, AID._Weaponskill_WyvernsWeal3], new AOEShapeRect(60, 3));

class WyvernsWeal(BossModule module) : Components.GenericAOEs(module, null)
{
    public class Bait(Actor t)
    {
        public Actor Target = t;
        public DateTime NextCast;
        public DateTime FinalCast { get; init; }
        public Angle LastRotation;
        public int NumCasts;
        public Angle StartRotation;
    }

    private readonly List<Bait> _baits = [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID._Gen_Icon_lockon8_line_1v)
        {
            _baits.Add(new(actor)
            {
                NextCast = WorldState.FutureTime(10.1f),
                FinalCast = WorldState.FutureTime(16.1f)
            });
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_WyvernsWeal2 or AID._Weaponskill_WyvernsWeal3 && _baits.Count > 0)
        {
            _baits[0].LastRotation = spell.Rotation;
            _baits[0].NextCast = Module.CastFinishAt(spell, 0.7f);
            if ((AID)spell.Action.ID == AID._Weaponskill_WyvernsWeal2)
                _baits[0].StartRotation = spell.Rotation;
            if (++_baits[0].NumCasts >= 9)
                _baits.RemoveAt(0);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_WyvernsWeal2 or AID._Weaponskill_WyvernsWeal3)
            NumCasts++;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var b in _baits.Take(1).Where(b => b.Target != actor))
            foreach (var (i, angle) in PredictedAngles(b))
                yield return new AOEInstance(new AOEShapeRect(60, 3), Module.PrimaryActor.Position, angle, b.NextCast.AddSeconds(0.7f * i));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (var bait in _baits)
        {
            if (bait.Target == pc)
            {
                // we are the target and the laser hasn't activated, draw a cone approximating the full range of the laser rotation (it seems to be 90 degrees or slightly wider)
                if (bait.LastRotation == default)
                {
                    var fakeSrc = Module.PrimaryActor.Position - new WDir(3, 3).Rotate(-45.Degrees());
                    var side = MathF.Sign(pc.Position.X - Arena.Center.X);
                    Arena.AddCone(fakeSrc, 60, Module.PrimaryActor.AngleTo(pc) - (45 * side).Degrees(), 45.Degrees(), ArenaColor.Danger);
                }
                else
                {
                    // just draw baits, TOP-style
                    foreach (var (_, angle) in PredictedAngles(bait))
                        Arena.AddRect(Module.PrimaryActor.Position, angle.ToDirection(), 60, 0, 3, ArenaColor.Danger);
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // forbid the side of the arena we started on
        if (_baits.FirstOrDefault() is { } b && b.Target == actor && b.StartRotation != default)
            hints.AddForbiddenZone(ShapeContains.Rect(Module.PrimaryActor.Position, b.StartRotation, 60, 3, 60), b.FinalCast);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var ix = _baits.FindIndex(b => b.Target == player);
        return ix == 0
            ? PlayerPriority.Danger
            : ix > 0
                ? PlayerPriority.Interesting
                : base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
    }

    // enumerate the remaining rotations of the laser given the starting angle
    // this completely breaks if a laser target dies early, but there's nothing we can do about that with no way to know who the new target is
    private IEnumerable<(int, Angle)> PredictedAngles(Bait b)
    {
        var actual = Module.PrimaryActor.AngleTo(b.Target);
        if (b.LastRotation == default)
        {
            yield return (0, actual);
            yield break;
        }

        var rotateDist = actual - b.LastRotation;
        for (var i = 1; i <= (9 - b.NumCasts); i++)
        {
            var moveDeg = Math.Clamp(rotateDist.Deg, -11.25f * i, 11.25f * i);
            var predicted = moveDeg.Degrees();
            yield return (i - 1, b.LastRotation + predicted);
            // break to avoid drawing multiple AOEs on top of each other
            if (moveDeg == rotateDist.Deg)
                yield break;
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1044, NameID = 14237, DevOnly = true)]
public class Ex6GuardianArkveld(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
