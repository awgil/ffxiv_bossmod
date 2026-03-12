
namespace BossMod.Dawntrail.Criterion.C01MerchantsTale.C012LoneSwordmaster;

public enum OID : uint
{
    Boss = 0x4B1A, // R5.000, x1
    Helper = 0x233C, // R0.500, x26, Helper type
    _Gen_ForceOfWill = 0x4B1C, // R0.500, x0 (spawn during fight)
    _Gen_ForceOfWill1 = 0x4B1E, // R1.000, x0 (spawn during fight)
    _Gen_ForceOfWill2 = 0x4B1B, // R1.250, x0 (spawn during fight)
    _Gen_ForceOfWill3 = 0x4C3E, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 45128, // Boss->player, no cast, single-target
    _Weaponskill_SteelsbreathRelease = 46686, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_HeavensConfluence = 46692, // Helper->player, no cast, range 8-60 donut
    _Weaponskill_HeavensConfluence1 = 46691, // Helper->players, no cast, range 8 circle
    _Weaponskill_MaleficQuartering = 46693, // Boss->self, 3.0s cast, single-target
    _Weaponskill_MaleficInfluence = 46694, // Helper->player, no cast, single-target
    _Weaponskill_MaleficInfluence1 = 46695, // Helper->player, no cast, single-target
    _Ability_ = 46607, // Boss->location, no cast, single-target
    _Weaponskill_MaleficInfluence2 = 46696, // Helper->player, no cast, single-target
    _Weaponskill_MaleficInfluence3 = 46697, // Helper->player, no cast, single-target
    _Weaponskill_LashOfLight = 46701, // Helper->self, 4.0s cast, range 40 90-degree cone
    _Weaponskill_ShiftingHorizon = 46700, // Boss->self, 4.0s cast, single-target
    _Weaponskill_MaleficPortent = 46698, // Boss->self, 6.0s cast, single-target
    _Weaponskill_MaleficPortent1 = 46699, // Helper->player, no cast, single-target
    _Weaponskill_UnyieldingWill = 48653, // 4B1C->4B1E, 7.9s cast, width 4 rect charge
    _Weaponskill_UnyieldingWill1 = 46702, // 4B1C->location, no cast, width 4 rect charge
    _Weaponskill_UnyieldingWill2 = 46703, // 4B1C->player, no cast, width 4 rect charge
    _Weaponskill_EchoingEight = 46707, // Boss->self, 4.0s cast, single-target
    _Weaponskill_EchoingHush = 46705, // Helper->location, 4.0s cast, range 8 circle
    _Weaponskill_EchoingEight1 = 46708, // Helper->self, 3.0s cast, range 40 width 8 cross
    _Weaponskill_WaitingWounds = 46713, // Boss->self, 5.0s cast, single-target
    _Weaponskill_WaitingWounds1 = 46714, // Helper->location, 5.0s cast, range 10 circle
    _Weaponskill_NearToHeavenOneCast = 47566, // Boss->self, 5.0s cast, single-target
    _Weaponskill_FarFromHeavenOneCast = 47567, // Boss->self, 5.0s cast, single-target
    _Weaponskill_NearToHeavenTwoCast = 47568, // Boss->self, 5.0s cast, single-target
    _Weaponskill_FarFromHeavenTwoCast = 47569, // Boss->self, 5.0s cast, single-target
    _Weaponskill_NearToHeavenOne = 46687, // Boss->player, no cast, range 5 circle
    _Weaponskill_FarFromHeavenOne = 46688, // Boss->player, no cast, range 5 circle
    _Weaponskill_NearToHeavenTwo = 46689, // Boss->players, no cast, range 5 circle
    _Weaponskill_FarFromHeavenTwo = 46690, // Boss->players, no cast, range 5 circle
    _Weaponskill_SilentEight = 46711, // Boss->self, 4.0s cast, single-target
    _Weaponskill_ResoundingSilence = 46712, // Helper->player, no cast, range 8 circle
    _Weaponskill_MawOfTheWolf = 46715, // Boss->self, 3.4+1.6s cast, single-target
    _Weaponskill_MawOfTheWolf1 = 46716, // Helper->self, 5.0s cast, range 80 width 80 rect
    _Weaponskill_FangsOfTheUnderworld = 46717, // Boss->self, 5.0s cast, single-target
    _Weaponskill_FangsOfTheUnderworld1 = 46719, // Helper->self, no cast, range 60 width 10 rect
    _Weaponskill_FangsOfTheUnderworld2 = 46718, // Boss->self, no cast, single-target
    _Weaponskill_SteelsbreathRelease1 = 46720, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_SteelHorizon = 46721, // Boss->self, 4.0s cast, single-target
    _Weaponskill_SilentHeat = 46725, // Boss->self, 4.0s cast, single-target
    _Weaponskill_WillOfTheUnderworld = 46722, // 4B1B->self, 8.0s cast, range 40 width 10 rect
    _Weaponskill_CardinalHorizons = 46733, // Boss->self, 4.0s cast, single-target
    _Weaponskill_WillOfTheUnderworld1 = 46749, // 4B1B->self, 4.0s cast, range 40 width 10 rect
    _Weaponskill_MaleficAlignment = 46723, // Boss->self, 3.0+1.0s cast, single-target
    _Weaponskill_MaleficAlignment1 = 46724, // Helper->self, 4.0s cast, range 40 90-degree cone
    _Weaponskill_Plummet = 46728, // Helper->location, 3.0s cast, range 5 circle
    _Weaponskill_Plummet1 = 46729, // Helper->location, 4.0s cast, range 10 circle
    _Weaponskill_Plummet2 = 46730, // Helper->location, 10.0s cast, range 60 circle
    _Weaponskill_SteelsbreathBonds = 46731, // Boss->self, no cast, single-target
    _Weaponskill_Plummet3 = 46727, // Helper->location, 3.0s cast, range 3 circle
    _Weaponskill_WillOfTheUnderworld2 = 47763, // 4C3E->self, 4.5s cast, range 40 width 20 rect
    _Weaponskill_EchoingOrbit = 46704, // Boss->self, 4.0s cast, single-target
    _Weaponskill_EchoingOrbit1 = 46706, // Helper->location, 3.0s cast, range 8-60 donut
    _Weaponskill_SteelScream = 46732, // Helper->player, no cast, single-target
    _Weaponskill_MortalFate = 46734, // Boss->self, 12.0+1.0s cast, single-target
    _Weaponskill_MortalFate1 = 46735, // Helper->self, 13.0s cast, range 40 ?-degree cone
}

public enum SID : uint
{
    _Gen_PhysicalVulnerabilityUp = 2940, // Boss/Helper->player, extra=0x0
    _Gen_MaleficE = 4773, // none->player, extra=0x0
    _Gen_MaleficEW = 4775, // none->player, extra=0x0
    _Gen_MaleficSEW = 4779, // none->player, extra=0x0
    _Gen_MaleficNEW = 4783, // none->player, extra=0x0
    _Gen_Bind = 2518, // none->player, extra=0x0
    _Gen_MaleficNSEW = 4787, // none->player, extra=0x0
    _Gen_MaleficS = 4776, // none->player, extra=0x0
    _Gen_MaleficNS = 4784, // none->player, extra=0x0
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_MaleficW = 4774, // none->player, extra=0x0
    _Gen_MaleficSW = 4778, // none->player, extra=0x0
    _Gen_MaleficSE = 4777, // none->player, extra=0x0
    _Gen_MaleficNSW = 4786, // none->player, extra=0x0
    _Gen_MaleficNSE = 4785, // none->player, extra=0x0
    _Gen_DamageDown = 2911, // Helper/Boss->player, extra=0x0
    _Gen_MaleficNE = 4781, // none->player, extra=0x0
    _Gen_MaleficNW = 4782, // none->player, extra=0x0
    _Gen_Incurable = 2398, // none->4B1D, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_sph_lockon2_num01_s5p = 332, // player->self
    _Gen_Icon_sph_lockon2_num02_s5p = 333, // player->self
    _Gen_Icon_lockon5_line_1p = 652, // player->self
    _Gen_Icon_m0489trg_a0c = 136, // player->self
    _Gen_Icon_loc08sp_05a_se_c2 = 499, // player->self
    _Gen_Icon_share_laser_5s_c0w = 572, // Boss->player
    _Gen_Icon_d1086_f_cn_t0p = 653, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_ambd_n_p = 357, // player->Boss
    _Gen_Tether_chn_ambd_s_p = 360, // player->Boss
    _Gen_Tether_chn_d1086_gkn_p = 371, // 4B1C/4B1E->4B1E/player
    _Gen_Tether_chn_ambd_w_p = 359, // player->Boss
    _Gen_Tether_chn_ambd_e_p = 358, // player->Boss
    _Gen_Tether_chn_m0731_0m1 = 163, // player->player
}

[Flags]
enum Side
{
    None = 0,
    E = 1,
    W = 2,
    S = 4,
    N = 8,
    All = N | E | S | W
}

static class SideHelpers
{
    extension(Side a)
    {
        public bool Matches(Side b) => (a & b) != Side.None;

        public Side Inverted() => (~a) & Side.All;

        public WDir? Direction() => a switch
        {
            Side.S => new(0, 1),
            Side.N => new(0, -1),
            Side.E => new(1, 0),
            Side.W => new(-1, 0),
            _ => null
        };

        public Side RotateR()
        {
            var @new = Side.None;
            if (a.HasFlag(Side.E))
                @new |= Side.S;
            if (a.HasFlag(Side.S))
                @new |= Side.W;
            if (a.HasFlag(Side.W))
                @new |= Side.N;
            if (a.HasFlag(Side.N))
                @new |= Side.E;
            return @new;
        }

        public Side RotateL()
        {
            var @new = Side.None;
            if (a.HasFlag(Side.E))
                @new |= Side.N;
            if (a.HasFlag(Side.N))
                @new |= Side.W;
            if (a.HasFlag(Side.W))
                @new |= Side.S;
            if (a.HasFlag(Side.S))
                @new |= Side.E;
            return @new;
        }
    }

    public static Side FromDirection(WDir d)
    {
        var abs = d.Abs();
        if (abs.X < abs.Z)
            d.X = 0;
        else
            d.Z = 0;
        return d.Sign() switch
        {
            (0, 1) => Side.S,
            (0, -1) => Side.N,
            (1, 0) => Side.E,
            (-1, 0) => Side.W,
            _ => throw new InvalidOperationException("unreachable")
        };
    }
    public static Side FromAngle(Angle a) => FromDirection(a.ToDirection());
}

class SteelsbreathRelease(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_SteelsbreathRelease);

class FromHeaven(BossModule module) : Components.UniformStackSpread(module, 5, 5, maxStackSize: 2)
{
    Actor? _target;
    int _count;
    BitMask _forbidden;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_NearToHeavenOneCast:
            case AID._Weaponskill_FarFromHeavenOneCast:
                _count = 1;
                Init();
                break;
            case AID._Weaponskill_NearToHeavenTwoCast:
            case AID._Weaponskill_FarFromHeavenTwoCast:
                _count = 2;
                Init();
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID._Gen_Icon_lockon5_line_1p:
                _target = actor;
                Init();
                break;
            case IconID._Gen_Icon_sph_lockon2_num02_s5p:
            case IconID._Gen_Icon_sph_lockon2_num01_s5p:
                _forbidden.Set(Raid.FindSlot(actor.InstanceID));
                Init();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_NearToHeavenOne or AID._Weaponskill_NearToHeavenTwo or AID._Weaponskill_FarFromHeavenOne or AID._Weaponskill_FarFromHeavenTwo)
        {
            Stacks.Clear();
            Spreads.Clear();
        }
    }

    void Init()
    {
        if (Active || _count == 0 || _target == null || _forbidden.NumSetBits() < 2)
            return;

        if (_count == 1)
            AddSpread(_target, WorldState.FutureTime(5.2f));

        if (_count == 2)
            AddStack(_target, WorldState.FutureTime(5.2f), _forbidden);
    }
}

class HeavensConfluence(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    enum Order
    {
        None,
        NearFirst,
        FarFirst
    }

    bool _initialized;
    Order _nextOrder = Order.None;
    readonly int[] _playerOrder = Utils.MakeArray(4, -1);

    public static readonly AOEShape Donut = new AOEShapeDonut(8, 60);
    public static readonly AOEShape Circle = new AOEShapeCircle(8);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_FarFromHeavenOneCast:
            case AID._Weaponskill_FarFromHeavenTwoCast:
                _nextOrder = Order.FarFirst;
                Init();
                break;
            case AID._Weaponskill_NearToHeavenOneCast:
            case AID._Weaponskill_NearToHeavenTwoCast:
                _nextOrder = Order.NearFirst;
                Init();
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID._Gen_Icon_sph_lockon2_num02_s5p:
                _playerOrder[Raid.FindSlot(actor.InstanceID)] = 1;
                Init();
                break;
            case IconID._Gen_Icon_sph_lockon2_num01_s5p:
                _playerOrder[Raid.FindSlot(actor.InstanceID)] = 0;
                Init();
                break;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        // it's impossible to tell circle and donut baits apart just from the outline
        if (CurrentBaits is [{ Target: var t, Shape: var s }, ..] && t == pc && s is AOEShapeDonut)
            s.Draw(Arena, t, ArenaColor.AOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!EnableHints || CurrentBaits.Count == 0)
            return;

        var nextBait = CurrentBaits[0];
        if (nextBait.Target == actor && PlayersClippedBy(nextBait).Any())
            hints.Add("Bait away from raid!");

        if (nextBait.Target != actor && IsClippedBy(actor, nextBait))
            hints.Add("GTFO from baited aoe!");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_HeavensConfluence or AID._Weaponskill_HeavensConfluence1)
        {
            NumCasts++;
            if (CurrentBaits.Count > 0)
                CurrentBaits.RemoveAt(0);
        }
    }

    void Init()
    {
        if (_initialized)
            return;

        if (_nextOrder == Order.None)
            return;

        if (Raid[_playerOrder.IndexOf(0)] is not { } first)
            return;
        if (Raid[_playerOrder.IndexOf(1)] is not { } second)
            return;

        var (shape1, shape2) = _nextOrder == Order.NearFirst ? (Circle, Donut) : (Donut, Circle);
        CurrentBaits.Add(new(Module.PrimaryActor, first, shape1, WorldState.FutureTime(5.2f)));
        CurrentBaits.Add(new(Module.PrimaryActor, second, shape2, WorldState.FutureTime(7.3f)));
        _initialized = true;
    }
}

class LashOfLight(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_LashOfLight, new AOEShapeCone(40, 45.Degrees()), maxCasts: 2);

class Malefic(BossModule module) : BossComponent(module)
{
    public readonly Side[] PlayerStates = new Side[4];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID is >= 4773 and <= 4787 && Raid.TryFindSlot(actor, out var slot))
            PlayerStates[slot] = (Side)(status.ID - 4772);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is >= 4773 and <= 4787 && Raid.TryFindSlot(actor, out var slot))
        {
            var side = (Side)(status.ID - 4772);
            if (PlayerStates[slot] == side)
                PlayerStates[slot] = Side.None;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (i, p) in Raid.WithSlot())
            DrawSides(p, PlayerStates[i]);
    }

    public static void DrawSides(MiniArena arena, Actor actor, Side state, uint color = 0)
    {
        if (state.HasFlag(Side.E))
            DrawSide(arena, actor, 90, color);
        if (state.HasFlag(Side.W))
            DrawSide(arena, actor, -90, color);
        if (state.HasFlag(Side.N))
            DrawSide(arena, actor, 180, color);
        if (state.HasFlag(Side.S))
            DrawSide(arena, actor, 0, color);
    }

    private void DrawSides(Actor actor, Side state, uint color = 0)
    {
        if (state.HasFlag(Side.E))
            DrawSide(actor, 90, color);
        if (state.HasFlag(Side.W))
            DrawSide(actor, -90, color);
        if (state.HasFlag(Side.N))
            DrawSide(actor, 180, color);
        if (state.HasFlag(Side.S))
            DrawSide(actor, 0, color);
    }

    private void DrawSide(Actor actor, float deg, uint color = 0) => DrawSide(Arena, actor, deg, color);

    private static void DrawSide(MiniArena arena, Actor actor, float deg, uint color = 0)
    {
        arena.PathArcTo(actor.Position, 1.5f, (deg - 35).Degrees().Rad, (deg + 35).Degrees().Rad);
        arena.PathStroke(false, color == 0 ? ArenaColor.Enemy : color);
    }
}

class MaleficPortent(BossModule module) : Components.CastCounter(module, AID._Weaponskill_MaleficPortent1)
{
    readonly Side[] _sides = new Side[4];
    BitMask _targets;
    BitMask _forceOfWillTargets;
    readonly Malefic _malefic = module.FindComponent<Malefic>()!;

    public bool Active => _targets.Any();

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_chn_d1086_gkn_p)
            _forceOfWillTargets.Set(Raid.FindSlot(tether.Target));

        var side = (TetherID)tether.ID switch
        {
            TetherID._Gen_Tether_chn_ambd_e_p => Side.E,
            TetherID._Gen_Tether_chn_ambd_n_p => Side.N,
            TetherID._Gen_Tether_chn_ambd_s_p => Side.S,
            TetherID._Gen_Tether_chn_ambd_w_p => Side.W,
            _ => Side.None
        };

        if (side != Side.None && Raid.TryFindSlot(source, out var slot))
        {
            _sides[slot] = side;
            _targets.Set(slot);
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (Raid.TryFindSlot(source, out var slot))
        {
            _sides[slot] = Side.None;
            _targets.Clear(slot);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (i, p) in Raid.WithSlot())
            if (_targets[i])
            {
                var shouldTake = ShouldTake(pcSlot, i);

                Arena.AddLine(Module.PrimaryActor.Position, p.Position, shouldTake ? ArenaColor.Safe : ArenaColor.Danger, shouldTake && !_targets[pcSlot] ? 2 : 1);
                Malefic.DrawSides(Arena, p, _sides[i], ArenaColor.Danger);
            }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_targets.Any() && _forceOfWillTargets.Any() && !HasCorrectTether(slot))
            hints.Add(_targets[slot] ? "Pass tether!" : "Take tether!");
    }

    bool HasCorrectTether(int slot)
    {
        return _targets[slot] != _forceOfWillTargets[slot] && !_malefic.PlayerStates[slot].Matches(_sides[slot]);
    }

    bool ShouldTake(int slot, int targetSlot)
    {
        return !_targets[slot] && !_forceOfWillTargets[slot] && !HasCorrectTether(targetSlot) && !_malefic.PlayerStates[slot].Matches(_sides[targetSlot]);
    }
}

class ForceOfWill(BossModule module) : Components.GenericAOEs(module)
{
    readonly Dictionary<ulong, Actor> WallSource = [];
    readonly Malefic _malefic = module.FindComponent<Malefic>()!;
    readonly List<(Actor From, Actor To, int Order)> AllTethers = [];
    DateTime _appearedAt;
    bool _bound;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        ulong parentID = 0;

        if (WallSource.TryGetValue(actor.InstanceID, out var force))
        {
            parentID = force.InstanceID;
            if (true)
            {
                var bindAt = _appearedAt.AddSeconds(6.8f);

                var sides = _malefic.PlayerStates[slot];
                var effCenter = SideHelpers.FromAngle(force.Rotation + 180.Degrees());
                if (sides.HasFlag(effCenter))
                    yield return new(new AOEShapeRect(40, 2), force.Position, force.Rotation, bindAt);
                if (sides.HasFlag(effCenter.RotateL()))
                    yield return new(new AOEShapeRect(40, 40, -2), force.Position, force.Rotation + 90.Degrees(), bindAt);
                if (sides.HasFlag(effCenter.RotateR()))
                    yield return new(new AOEShapeRect(40, 40, -2), force.Position, force.Rotation - 90.Degrees(), bindAt);
            }
        }

        foreach (var (from, to, i) in AllTethers)
        {
            // skip any charges not targeting us
            if (to == actor || from.InstanceID == parentID)
                continue;

            var dir = to.Position - from.Position;
            var activation = _appearedAt.AddSeconds(8 + 0.5 * i);
            yield return new(new AOEShapeRect(dir.Length(), 2), from.Position, dir.ToAngle(), activation);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        // TODO: add hints for baiting this away from other players?
        if (AllTethers.FirstOrNull(t => t.To == pc) is { From: var src } && (OID)src.OID != OID._Gen_ForceOfWill)
        {
            var dir = pc.Position - src.Position;
            Arena.AddRect(src.Position, dir.Normalized(), dir.Length(), 0, 2, ArenaColor.Danger);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_chn_d1086_gkn_p)
        {
            if (_appearedAt == default)
                _appearedAt = WorldState.CurrentTime;

            AllTethers.RemoveAll(t => t.From == source);
            AllTethers.Add((source, WorldState.Actors.Find(tether.Target)!, (OID)source.OID == OID._Gen_ForceOfWill ? 0 : 1));
            WallSource[tether.Target] = WallSource.TryGetValue(source.InstanceID, out var parent) ? parent : source;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_Bind)
            _bound = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_UnyieldingWill:
                NumCasts++;
                AllTethers.RemoveAll(t => (OID)t.From.OID == OID._Gen_ForceOfWill);
                WallSource.Clear();
                break;
            case AID._Weaponskill_UnyieldingWill2:
                NumCasts++;
                AllTethers.RemoveAll(t => t.To.OID == 0);
                break;
        }
    }
}

class LavaRect(BossModule module) : Components.GenericAOEs(module)
{
    bool _active;

    public static readonly AOEShape Rect = new AOEShapeRect(10, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => [];

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EBF72 && state == 0x00010002)
        {
            var rect = CurveApprox.Rect(new WDir(10, -5), new WDir(10, 0), new WDir(0, 5));
            var clipper = Arena.Bounds.Clipper;
            Arena.Bounds = new ArenaBoundsCustom(20, clipper.UnionAll(new(rect), new PolygonClipper.Operand(rect.Select(r => r.OrthoR())), new PolygonClipper.Operand(rect.Select(r => r.OrthoL())), new PolygonClipper.Operand(rect.Select(r => -r))));
        }
    }
}

class EchoesBait(BossModule module) : BossComponent(module)
{
    DateTime _activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_EchoingEight or AID._Weaponskill_EchoingOrbit)
            _activation = Module.CastFinishAt(spell);

        if ((AID)spell.Action.ID is AID._Weaponskill_EchoingHush)
            _activation = default;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation != default)
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(Arena.Center, 1), _activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_activation != default)
            Arena.AddCircle(pc.Position, 8, ArenaColor.Danger);
    }
}

class EchoingHush(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_EchoingHush, 8);
class EchoingEight(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_EchoingEight1, new AOEShapeCross(40, 4));
class EchoingOrbit(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_EchoingOrbit1, new AOEShapeDonut(8, 60));
class EchoPredict(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly AOEShape Donut = new AOEShapeDonut(8, 60);
    public static readonly AOEShape Cross = new AOEShapeCross(40, 4);

    AOEShape? _next;
    readonly List<AOEInstance> _predicted = [];
    bool _draw;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _draw ? _predicted : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_EchoingEight:
                _next = Cross;
                break;
            case AID._Weaponskill_EchoingOrbit:
                _next = Donut;
                break;
            case AID._Weaponskill_EchoingEight1:
            case AID._Weaponskill_EchoingOrbit1:
                _predicted.Clear();
                _next = null;
                break;
            case AID._Weaponskill_EchoingHush:
                _predicted.Add(new(_next!, spell.LocXZ, default, Module.CastFinishAt(spell, 3)));
                if (_next is AOEShapeCross)
                    _predicted.Add(new(_next, spell.LocXZ, 45.Degrees(), Module.CastFinishAt(spell, 3)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_EchoingHush:
                _draw = true;
                break;
            case AID._Weaponskill_EchoingEight1:
            case AID._Weaponskill_EchoingOrbit1:
                NumCasts++;
                break;
        }
    }
}

class WaitingWounds(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_WaitingWounds1, 10, 6, highlightImminent: true);

class SilentEightSpread(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID._Gen_Icon_loc08sp_05a_se_c2, AID._Weaponskill_ResoundingSilence, 8, 5)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (IsSpreadTarget(actor))
            hints.AddForbiddenZone(_ => true, DateTime.MaxValue);
    }
}
class MawOfTheWolf(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_MawOfTheWolf1, new AOEShapeRect(80, 40));

class C012LoneSwordmasterStates : StateMachineBuilder
{
    public C012LoneSwordmasterStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    void SinglePhase(uint id)
    {
        Malefic1(id, 9.2f);
        Echoes(id + 0x10000, 5.5f);

        SimpleState(id + 0xFF0000, 10000, "???");
    }

    void Malefic1(uint id, float delay)
    {
        SteelsbreathRelease(id, delay);
        FromHeaven(id + 0x100, 7.2f);

        Cast(id + 0x200, AID._Weaponskill_MaleficQuartering, 2, 3)
            .ActivateOnEnter<Malefic>();

        Cast(id + 0x210, AID._Weaponskill_ShiftingHorizon, 3.5f, 4, "Diagonals")
            .ActivateOnEnter<LashOfLight>()
            .ActivateOnEnter<ForceOfWill>()
            .ActivateOnEnter<MaleficPortent>();

        ComponentCondition<MaleficPortent>(id + 0x212, 3, p => p.Active, "Tethers appear")
            .DeactivateOnExit<LashOfLight>();
        Cast(id + 0x220, AID._Weaponskill_MaleficPortent, 0.1f, 6);
        ComponentCondition<MaleficPortent>(id + 0x222, 1, m => m.NumCasts > 0, "Tethers resolve")
            .DeactivateOnExit<MaleficPortent>();
        ComponentCondition<ForceOfWill>(id + 0x230, 1.3f, w => w.NumCasts > 0, "Charges 1");
        ComponentCondition<ForceOfWill>(id + 0x231, 0.5f, w => w.NumCasts > 2, "Charges 2")
            .DeactivateOnExit<ForceOfWill>()
            .DeactivateOnExit<Malefic>();
    }

    void Echoes(uint id, float delay)
    {
        CastMulti(id, [AID._Weaponskill_EchoingEight, AID._Weaponskill_EchoingOrbit], delay, 4)
            .ActivateOnEnter<EchoesBait>()
            .ActivateOnEnter<EchoingHush>()
            .ActivateOnEnter<EchoPredict>()
            .ActivateOnEnter<EchoingEight>()
            .ActivateOnEnter<EchoingOrbit>();

        ComponentCondition<EchoingHush>(id + 0x10, 0.1f, h => h.Casters.Count > 0, "Puddle bait")
            .DeactivateOnExit<EchoesBait>();
        ComponentCondition<EchoingHush>(id + 0x11, 4, h => h.NumCasts > 0, "Puddles")
            .DeactivateOnExit<EchoingHush>();
        ComponentCondition<EchoPredict>(id + 0x12, 3, p => p.NumCasts > 0, "Stored AOEs")
            .DeactivateOnExit<EchoPredict>()
            .DeactivateOnExit<EchoingEight>()
            .DeactivateOnExit<EchoingOrbit>();

        ComponentCondition<WaitingWounds>(id + 0x100, 4.4f, w => w.NumCasts > 0, "Puddles 1")
            .ActivateOnEnter<WaitingWounds>();
        ComponentCondition<WaitingWounds>(id + 0x101, 1, w => w.NumCasts > 3, "Puddles 2");

        FromHeaven(id + 0x200, 1, id => ComponentCondition<WaitingWounds>(id, 0, w => w.NumCasts > 6, "Puddles 3"));

        Cast(id + 0x300, AID._Weaponskill_SilentEight, 30, 4)
            .ActivateOnEnter<SilentEightSpread>()
            .ActivateOnEnter<EchoingEight>()
            .ActivateOnEnter<MawOfTheWolf>();
        ComponentCondition<SilentEightSpread>(id + 0x310, 5, s => s.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<SilentEightSpread>();
        ComponentCondition<EchoingEight>(id + 0x311, 3.2f, e => e.NumCasts > 0, "Crosses")
            .DeactivateOnExit<EchoingEight>();
        ComponentCondition<MawOfTheWolf>(id + 0x312, 3.2f, m => m.NumCasts > 0, "Half-room")
            .DeactivateOnExit<MawOfTheWolf>();
    }

    void SteelsbreathRelease(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_SteelsbreathRelease, delay, 5, "Raidwide")
           .ActivateOnEnter<SteelsbreathRelease>()
           .DeactivateOnExit<SteelsbreathRelease>();
    }

    void FromHeaven(uint id, float delay, Action<uint>? extra = null)
    {
        CastStartMulti(id, [AID._Weaponskill_NearToHeavenOneCast, AID._Weaponskill_FarFromHeavenOneCast, AID._Weaponskill_NearToHeavenTwoCast, AID._Weaponskill_FarFromHeavenTwoCast], delay)
            .ActivateOnEnter<HeavensConfluence>()
            .ActivateOnEnter<FromHeaven>();
        extra?.Invoke(id + 1);
        CastEnd(id + 2, 5);

        ComponentCondition<HeavensConfluence>(id + 0x10, 0.3f, c => c.NumCasts > 0, "In/out 1");
        ComponentCondition<HeavensConfluence>(id + 0x11, 2.1f, c => c.NumCasts > 1, "In/out 2")
            .DeactivateOnExit<HeavensConfluence>()
            .DeactivateOnExit<FromHeaven>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1079, NameID = 14323, DevOnly = true)]
public class C012LoneSwordmaster(WorldState ws, Actor primary) : BossModule(ws, primary, new(170, -815), new ArenaBoundsSquare(20));

