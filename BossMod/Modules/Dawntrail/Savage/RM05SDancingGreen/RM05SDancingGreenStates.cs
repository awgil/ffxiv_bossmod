
namespace BossMod.Dawntrail.Savage.RM05SDancingGreen;

class DeepCut(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60, 30.Degrees()), 471, ActionID.MakeSpell(AID._Weaponskill_DeepCut1));
class ABSide(BossModule module) : BossComponent(module)
{
    public enum Mechanic { None, Roles, Parties }
    public Mechanic NextMechanic { get; private set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_FlipToASide:
                NextMechanic = Mechanic.Roles;
                break;
            case AID._Weaponskill_FlipToBSide:
                NextMechanic = Mechanic.Parties;
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        switch (NextMechanic)
        {
            case Mechanic.Roles:
                hints.Add("Next: roles");
                break;
            case Mechanic.Parties:
                hints.Add("Next: light parties");
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_PlayASide1:
            case AID._Weaponskill_PlayBSide1:
                NextMechanic = Mechanic.None;
                break;
        }
    }
}

class TwistNDrop(BossModule module) : Components.GroupedAOEs(module, [.. BossCasts, .. HelperCasts], new AOEShapeRect(20, 20))
{
    public bool Side1 { get; private set; }
    public bool Side2 { get; private set; }

    public static readonly AID[] BossCasts = [
        AID._Weaponskill_2SnapTwistDropTheNeedle, AID._Weaponskill_3SnapTwistDropTheNeedle, AID._Weaponskill_4SnapTwistDropTheNeedle, AID._Weaponskill_2SnapTwistDropTheNeedle6, AID._Weaponskill_2SnapTwistDropTheNeedle5, AID._Weaponskill_4SnapTwistDropTheNeedle8, AID._Weaponskill_4SnapTwistDropTheNeedle7, AID._Weaponskill_2SnapTwistDropTheNeedle7, AID._Weaponskill_2SnapTwistDropTheNeedle8, AID._Weaponskill_3SnapTwistDropTheNeedle6, AID._Weaponskill_3SnapTwistDropTheNeedle7, AID._Weaponskill_4SnapTwistDropTheNeedle6, AID._Weaponskill_3SnapTwistDropTheNeedle4, AID._Weaponskill_4SnapTwistDropTheNeedle9, AID._Weaponskill_3SnapTwistDropTheNeedle8, AID._Weaponskill_3SnapTwistDropTheNeedle9, AID._Weaponskill_3SnapTwistDropTheNeedle5, AID._Weaponskill_4SnapTwistDropTheNeedle5, AID._Weaponskill_2SnapTwistDropTheNeedle3, AID._Weaponskill_2SnapTwistDropTheNeedle4
    ];

    public static readonly AID[] HelperCasts = [
        AID._Weaponskill_2SnapTwistDropTheNeedle1, AID._Weaponskill_2SnapTwistDropTheNeedle2, AID._Weaponskill_3SnapTwistDropTheNeedle1, AID._Weaponskill_3SnapTwistDropTheNeedle2, AID._Weaponskill_3SnapTwistDropTheNeedle3, AID._Weaponskill_4SnapTwistDropTheNeedle1, AID._Weaponskill_4SnapTwistDropTheNeedle2, AID._Weaponskill_4SnapTwistDropTheNeedle3, AID._Weaponskill_4SnapTwistDropTheNeedle4,
    ];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.OrderBy(c => Module.CastFinishAt(c.CastInfo)).Take(1).Select(csr => new AOEInstance(Shape, csr.CastInfo!.LocXZ, csr.CastInfo.Rotation, Module.CastFinishAt(csr.CastInfo), Color, Risky));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID is AID._Weaponskill_2SnapTwistDropTheNeedle1 or AID._Weaponskill_3SnapTwistDropTheNeedle2 or AID._Weaponskill_4SnapTwistDropTheNeedle3)
            Side1 = true;

        if ((AID)spell.Action.ID is AID._Weaponskill_2SnapTwistDropTheNeedle2 or AID._Weaponskill_3SnapTwistDropTheNeedle3 or AID._Weaponskill_4SnapTwistDropTheNeedle4)
            Side2 = true;
    }
}

class PlayASide(BossModule module) : BossComponent(module)
{
    public bool Active { get; private set; }
    private ABSide? _ab;

    private DateTime Activation;

    enum Category { Tank, Healer, DPS }

    static Category Categorize(Actor pc) => pc.Role == Role.Tank ? Category.Tank : pc.Role == Role.Healer ? Category.Healer : Category.DPS;

    private IEnumerable<Actor> DifferentRole(Actor pc) => Raid.WithoutSlot().Where(r => Categorize(r) != Categorize(pc));

    public override void Update()
    {
        _ab ??= Module.FindComponent<ABSide>();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_2SnapTwistDropTheNeedle2 or AID._Weaponskill_3SnapTwistDropTheNeedle3 or AID._Weaponskill_4SnapTwistDropTheNeedle4)
        {
            Active = _ab?.NextMechanic == ABSide.Mechanic.Roles;
            if (Active)
                Activation = WorldState.FutureTime(5.3f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_PlayASide1)
            Active = false;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!Active)
            return;

        Arena.AddCone(Module.PrimaryActor.Position, 60, Module.PrimaryActor.AngleTo(pc), 22.5f.Degrees(), ArenaColor.Danger);

        var cat = Categorize(pc);
        if (cat != Category.DPS)
            DrawBait(Category.DPS);
        if (cat != Category.Healer)
            DrawBait(Category.Healer);
        if (cat != Category.Tank)
            DrawBait(Category.Tank);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Active)
            return;

        hints.PredictedDamage.Add((Raid.WithSlot().Mask(), Activation));

        var cones = DifferentRole(actor).Select(p => ShapeDistance.Cone(Module.PrimaryActor.Position, 60, Module.PrimaryActor.AngleTo(p), 22.5f.Degrees())).ToList();
        if (cones.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.Union(cones), Activation);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (!Active)
            return base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);

        return Categorize(pc) == Categorize(player) ? PlayerPriority.Normal : PlayerPriority.Danger;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Active)
            return;

        hints.Add("Bait away from raid!", DifferentRole(actor).Any(p => p.Position.InCone(Module.PrimaryActor.Position, Module.PrimaryActor.DirectionTo(actor), 22.5f.Degrees())));
    }

    private void DrawBait(Category c)
    {
        if (Raid.WithoutSlot().FirstOrDefault(r => Categorize(r) == c) is { } actor)
            Arena.ZoneCone(Module.PrimaryActor.Position, 0, 60, Module.PrimaryActor.AngleTo(actor), 22.5f.Degrees(), ArenaColor.AOE);
    }
}

class PlayBSide(BossModule module) : Components.GenericWildCharge(module, 4, ActionID.MakeSpell(AID._Weaponskill_PlayBSide1), 60)
{
    private ABSide? _ab;

    public override void Update()
    {
        _ab ??= Module.FindComponent<ABSide>();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_ab?.NextMechanic == ABSide.Mechanic.Parties && (AID)spell.Action.ID is AID._Weaponskill_2SnapTwistDropTheNeedle2 or AID._Weaponskill_3SnapTwistDropTheNeedle3 or AID._Weaponskill_4SnapTwistDropTheNeedle4)
        {
            Source = Module.PrimaryActor;
            Array.Fill(PlayerRoles, PlayerRole.Share);
            foreach (var (i, player) in Raid.WithSlot())
                if (player.Role == Role.Healer)
                    PlayerRoles[i] = PlayerRole.Target;
            Activation = WorldState.FutureTime(5.3f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            Source = null;
    }
}

class PlaySideCounter(BossModule module) : Components.CastCounterMulti(module, [ActionID.MakeSpell(AID._Weaponskill_PlayASide1), ActionID.MakeSpell(AID._Weaponskill_PlayBSide1)]);

class CelebrateGoodTimes(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_CelebrateGoodTimes));

class FloorCounter(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID._Weaponskill_FunkyFloor1));

// 03.00020001 - even numbered tiles (starting at 0 top left)
// 03.00200010 - odd numbered tiles
// 03.00800040 - reset maybe
// 03.00080004 - reset maybe

class DanceFloor(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID._Weaponskill_FunkyFloor1))
{
    private static readonly BitMask EvenTiles = new(0xAA55AA55AA55AA55ul);

    public BitMask Tiles;
    private DateTime NextActivation;

    public bool Active => Tiles.Any();

    public enum Pattern { None, Even, Odd }
    public Pattern CurPattern;

    private int NumActivations;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID._Weaponskill_FunkyFloor1)
        {
            NextActivation = WorldState.FutureTime(4.1f);
            Tiles = ~Tiles;
            NumActivations++;

            if (NumActivations >= 10)
            {
                Tiles.Reset();
                CurPattern = Pattern.None;
                NextActivation = default;
            }
        }
    }

    private void Activate(bool even)
    {
        NextActivation = WorldState.FutureTime(3.1f);
        Tiles = even ? EvenTiles : ~EvenTiles;
        CurPattern = even ? Pattern.Even : Pattern.Odd;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (!Active && index == 3)
        {
            if (state == 0x00020001)
                Activate(true);
            else if (state == 0x00200010)
                Activate(false);
        }
    }

    private int GetTile(WPos p)
    {
        var bit = (p - new WPos(80, 80)) / 5;
        return (int)bit.X + ((int)bit.Z << 3);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Active)
            return;

        hints.AddForbiddenZone(p => Tiles[GetTile(p)] ? -1 : 1, NextActivation);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        switch (CurPattern)
        {
            case Pattern.Even:
                hints.Add($"Outside: 1/3, inside: 2/4");
                break;
            case Pattern.Odd:
                hints.Add($"Outside: 2/4, inside: 1/3");
                break;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!Active)
            return;

        foreach (var bit in Tiles.SetBits())
        {
            var center = LocateTile(bit);
            Arena.ZoneRect(center - new WDir(2.5f, 0), center + new WDir(2.5f, 0), 2.5f, ArenaColor.AOE);
        }
    }

    public WPos LocateTile(int bit) => new(82.5f + (bit & 7) * 5, 82.5f + (bit >> 3) * 5);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Tiles[GetTile(actor.Position)])
            hints.Add("GTFO from tile!");
    }
}

class BurnBabyBurn(BossModule module) : BossComponent(module)
{
    public enum Order { None, Short, Long }
    public readonly Order[] Orders = Utils.MakeArray(8, Order.None);

    public readonly DateTime[] Timers = Utils.MakeArray(8, default(DateTime));

    public int NumShort => Orders.Count(o => o == Order.Short);
    public int NumLong => Orders.Count(o => o == Order.Long);

    private DanceFloor? _danceFloor;

    public override void Update()
    {
        _danceFloor ??= Module.FindComponent<DanceFloor>();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        switch (Orders[slot])
        {
            case Order.Short:
                hints.Add($"Short timer", false);
                break;
            case Order.Long:
                hints.Add("Long timer", false);
                break;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_BurnBabyBurn)
        {
            Timers[Raid.FindSlot(actor.InstanceID)] = status.ExpireAt;
            Orders[Raid.FindSlot(actor.InstanceID)] = status.ExpireAt > WorldState.FutureTime(30) ? Order.Long : Order.Short;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_BurnBabyBurn)
        {
            Timers[Raid.FindSlot(actor.InstanceID)] = default;
            Orders[Raid.FindSlot(actor.InstanceID)] = Order.None;
        }
    }

    private IEnumerable<int> GetSafeTiles(int slot)
    {
        var order = Orders[slot];
        if (order == Order.None || _danceFloor == null || _danceFloor.CurPattern == DanceFloor.Pattern.None)
            yield break;

        if (_danceFloor.CurPattern == DanceFloor.Pattern.Even)
        {
            yield return 9;
            yield return 54;
        }
        else
        {
            yield return 14;
            yield return 49;
        }

        switch ((order, _danceFloor.CurPattern))
        {
            case (Order.Short, DanceFloor.Pattern.Even):
                yield return 29;
                yield return 34;
                break;
            case (Order.Short, DanceFloor.Pattern.Odd):
                yield return 26;
                yield return 37;
                break;
            case (Order.Long, DanceFloor.Pattern.Even):
                yield return 20;
                yield return 43;
                break;
            case (Order.Long, DanceFloor.Pattern.Odd):
                yield return 19;
                yield return 44;
                break;
        }
    }

    private bool Imminent(int slot) => Orders[slot] != Order.None && Timers[slot] < WorldState.FutureTime(7);

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var t in GetSafeTiles(slot))
            movementHints.Add((actor.Position, _danceFloor!.LocateTile(t), Imminent(slot) ? ArenaColor.Safe : ArenaColor.Danger));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Imminent(slot))
        {
            var tiles = GetSafeTiles(slot).Select(t => ShapeDistance.Circle(_danceFloor!.LocateTile(t), 2.5f)).ToList();
            var all = ShapeDistance.Union(tiles);
            hints.AddForbiddenZone(p => -all(p), Timers[slot]);
        }
    }
}

class InsideOutside(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly AOEShapeDonut Donut = new(4.998f, 60);
    public static readonly AOEShapeCircle Circle = new(7);

    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_InsideOut1)
        {
            _aoes.Add(new(Circle, caster.Position, default, Module.CastFinishAt(spell)));
            _aoes.Add(new(Donut, caster.Position, default, Module.CastFinishAt(spell, 2.5f)));
        }
        if ((AID)spell.Action.ID == AID._Weaponskill_OutsideIn1)
        {
            _aoes.Add(new(Donut, caster.Position, default, Module.CastFinishAt(spell)));
            _aoes.Add(new(Circle, caster.Position, default, Module.CastFinishAt(spell, 2.5f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_InsideOut or AID._Weaponskill_InsideOut2 or AID._Weaponskill_OutsideIn or AID._Weaponskill_OutsideIn2)
        {
            NumCasts++;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}

class DancingGreenStates : StateMachineBuilder
{
    public DancingGreenStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<ABSide>()
            .ActivateOnEnter<DanceFloor>()
            .ActivateOnEnter<BurnBabyBurn>()
            .ActivateOnEnter<InsideOutside>()
            ;
    }

    private void SinglePhase(uint id)
    {
        DeepCut(id, 9.2f);

        id += 0x10000;
        Flip(id, 7.7f);
        TwistNDrop(id + 0x10, 2.25f);

        id += 0x10000;
        Flip(id, 2.9f);
        TwistNDrop(id + 0x10, 2.2f);
        Cast(id + 0x20, AID._Weaponskill_CelebrateGoodTimes, 0.9f, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x40, AID._Weaponskill_DiscoInfernal, 8.4f, 4, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        id += 0x10000;
        CastStart(id, AID._Weaponskill_FunkyFloor, 2.1f)
            .ActivateOnEnter<FloorCounter>();
        ComponentCondition<FloorCounter>(id + 0x10, 3.1f, c => c.NumCasts > 0, "Floor start");

        CastMulti(id + 0x20, [AID._Weaponskill_InsideOut1, AID._Weaponskill_OutsideIn1], 3.1f, 5, "In/out");
        ComponentCondition<InsideOutside>(id + 0x22, 2.5f, i => i.NumCasts >= 2, "Out/in");

        ComponentCondition<BurnBabyBurn>(id + 0x30, 8.3f, b => b.NumShort == 0, "Debuffs 1");
        ComponentCondition<BurnBabyBurn>(id + 0x40, 7.9f, b => b.NumLong == 0, "Debuffs 2");

        TwistNDrop(id + 0x100, 2.8f);

        SimpleState(id + 0xFF0000, 10000, "???")
            .ActivateOnEnter<Wavelength>()
            .ActivateOnEnter<CelebrateGoodTimes>();
    }

    private void DeepCut(uint id, float delay)
    {
        Timeout(id, 0).ActivateOnEnter<DeepCut>();

        Cast(id + 2, AID._Weaponskill_DeepCut, delay, 5);
        ComponentCondition<DeepCut>(id + 4, 0.7f, d => d.NumCasts >= 2, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Flip(uint id, float delay)
    {
        CastMulti(id, [AID._Weaponskill_FlipToASide, AID._Weaponskill_FlipToBSide], delay, 4);
    }

    private void TwistNDrop(uint id, float delay)
    {
        CastStartMulti(id, RM05SDancingGreen.TwistNDrop.BossCasts, delay)
            .ActivateOnEnter<TwistNDrop>()
            .ActivateOnEnter<PlayASide>()
            .ActivateOnEnter<PlayBSide>()
            .ActivateOnEnter<PlaySideCounter>();

        CastEnd(id + 0x02, 5);

        ComponentCondition<TwistNDrop>(id + 0x20, 3.5f, t => t.Side2, "Left/right");
        ComponentCondition<PlaySideCounter>(id + 0x22, 1.8f, p => p.NumCasts >= 2, "Stack/spread")
            .DeactivateOnExit<PlaySideCounter>()
            .DeactivateOnExit<PlayASide>()
            .DeactivateOnExit<PlayBSide>()
            .DeactivateOnExit<TwistNDrop>();
    }

    //private void XXX(uint id, float delay)
}
