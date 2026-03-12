namespace BossMod.Dawntrail.Criterion.C01MerchantsTale.C011DaryaTheSeaMaid;

public enum OID : uint
{
    Boss = 0x4A99, // R5.000, x1
    Helper = 0x233C, // R0.500, x15-35, Helper type
    SeabornSteed = 0x4A9A, // R2.200, x0 (spawn during fight), horse
    SeabornSteward = 0x4A9B, // R2.200, x0 (spawn during fight), turtle
    SeabornSoldier = 0x4A9C, // R2.200, x0 (spawn during fight), crab
    SeabornServant = 0x4A9D, // R1.600, x0 (spawn during fight), bomb
    SeabornShrike = 0x4A9E, // R1.600, x0 (spawn during fight), chicken
}

public enum AID : uint
{
    AutoAttack = 45838, // Boss->player, no cast, single-target
    PiercingPlunge = 45870, // Boss->self, 5.0s cast, range 70 circle
    Jump = 45770, // Boss->location, no cast, single-target
    FamiliarCall = 45771, // Boss->self, 3.0+1.0s cast, single-target
    EchoedSerenade = 45773, // Boss->self, 8.5+0.5s cast, range 60 circle
    WatersongHorse = 45839, // SeabornSteed->self, 1.0s cast, range 40 width 8 rect
    WatersongTurtle = 45840, // SeabornSteward->self, 1.0s cast, range 40 width 8 rect
    WatersongCrab = 45841, // SeabornSoldier->self, 1.0s cast, range 40 width 8 rect
    WatersongBomb = 45842, // SeabornServant->self, 1.0s cast, range 20 180-degree cone
    WatersongChicken = 45843, // SeabornShrike->self, 1.0s cast, range 45 60-degree cone
    HydrobulletBoss = 45847, // Boss->self, no cast, single-target
    HydrobulletSpread = 45848, // Helper->player, 3.0s cast, range 15 circle
    SurgingCurrentCast = 45865, // Boss->self, 5.0+1.0s cast, single-target
    SurgingCurrent = 45866, // Helper->self, 6.0s cast, range 60 90-degree cone
    AlluringOrder = 45861, // Boss->self, 4.0s cast, range 70 circle
    SwimmingInTheAir = 45845, // Boss->self, 4.0s cast, single-target
    Hydrofall = 45846, // Helper->location, 1.0s cast, range 12 circle
    Tidalspout = 45858, // Helper->players, no cast, range 6 circle
    CeaselessCurrentCast = 45862, // Boss->self, 4.0+1.0s cast, single-target
    CeaselessCurrentFirst = 45863, // Helper->self, 5.0s cast, range 8 width 40 rect
    CeaselessCurrentRest = 45864, // Helper->self, no cast, range 8 width 40 rect
    Unk = 45859, // Helper->player, 6.0s cast, single-target
    CrossCurrent = 45860, // Helper->self, no cast, range 36 width 8 cross
    SunkenTreasure = 45849, // Boss->self, 3.0+1.0s cast, single-target
    SphereShatterDonut = 45851, // Helper->self, no cast, range 4-20 donut
    SphereShatterCircle = 45850, // Helper->self, no cast, range 18 circle
    AquaSpear = 45852, // Boss->self, 3.0s cast, single-target
    AquaSpearPlayer = 45854, // Helper->player, 6.0s cast, single-target
    AquaSpearTileCast = 45853, // Helper->self, 6.0s cast, range 8 width 8 rect
    AquaSpearTileInstant = 45855, // Helper->self, no cast, range 8 width 8 rect
    AquaBallBoss = 45867, // Boss->self, 2.0+1.0s cast, single-target
    AquaBall = 45868, // Helper->location, 3.0s cast, range 5 circle
    EchoedReprise = 45844, // Boss->self, 4.0+0.5s cast, range 60 circle
    SeaShackles = 45856, // Boss->self, 4.0+1.0s cast, range 70 circle
    HydrobulletStack = 47088, // Helper->players, no cast, range 15 circle
    Explosion = 45857, // Helper->player, no cast, single-target, tether failure
    PiercingPlungeEnrageCast = 45872, // Boss->self, 10.0s cast, range 70 circle
    PiercingPlungeEnrage = 45873, // Boss->self, no cast, range 70 circle
}

public enum SID : uint
{
    MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    ForwardMarch = 2161, // none->player, extra=0x0
    AboutFace = 2162, // none->player, extra=0x0
    LeftFace = 2163, // none->player, extra=0x0
    RightFace = 2164, // none->player, extra=0x0
    ForcedMarch = 1257, // none->player, extra=0x4/0x1/0x2/0x8
    NearShoreShackles = 4724, // none->player, extra=0x0
    FarShoreShackles = 4725, // none->player, extra=0x0
    TidalspoutTarget = 4726, // none->player, extra=0x0
    HydrobulletTarget = 4968, // none->player, extra=0x0

}

public enum VfxID : uint
{
    Horse = 2741,
    Turtle = 2743,
    Crab = 2744,
    Bomb = 2745,
    Chicken = 2746,
}

public enum IconID : uint
{
    Hydrobullet = 22, // player->self
    CrossCurrent = 20, // player->self
    Tile = 185, // player->self
}

public enum TetherID : uint
{
    Active = 3, // player->player
    Far = 129, // player->player
    Near = 130, // player->player
}

class PiercingPlunge(BossModule module) : Components.RaidwideCast(module, AID.PiercingPlunge);
class AlluringOrder(BossModule module) : Components.RaidwideCast(module, AID.AlluringOrder);

class FamiliarCall1(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly AOEShapeRect Rect = new(40, 4);
    public static readonly AOEShapeCone Bomb = new(20, 90.Degrees());
    public static readonly AOEShapeCone Bird = new(45, 30.Degrees());

    public uint ColorImminent = ArenaColor.Danger;
    public int NumShown = 2;

    protected record struct Caster(Actor Source, AOEShape Shape, DateTime Activation);

    protected readonly List<List<Caster>> _sources = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var i = 0;
        foreach (var srcs in _sources.Take(NumShown))
        {
            foreach (var src in srcs)
                yield return new(src.Shape, src.Source.Position, src.Source.Rotation, src.Activation, Risky: i == 0, Color: i == 0 ? ColorImminent : ArenaColor.AOE);
            i++;
        }
    }

    public override void OnEventVFX(Actor actor, uint vfxID, ulong targetID)
    {
        if ((OID)actor.OID == OID.Boss)
        {
            var (caster, shape) = (VfxID)vfxID switch
            {
                VfxID.Chicken => (OID.SeabornShrike, (AOEShape)Bird),
                VfxID.Bomb => (OID.SeabornServant, Bomb),
                VfxID.Turtle => (OID.SeabornSteward, Rect),
                VfxID.Horse => (OID.SeabornSteed, Rect),
                VfxID.Crab => (OID.SeabornSoldier, Rect),
                _ => (0, null)
            };
            if (shape != null)
            {
                var activation = _sources.Count > 0 ? _sources[^1][0].Activation.AddSeconds(3) : WorldState.FutureTime(11.6f);
                _sources.Add([.. Module.Enemies(caster).Select(c => new Caster(c, shape, activation))]);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WatersongHorse or AID.WatersongBomb or AID.WatersongCrab or AID.WatersongChicken or AID.WatersongTurtle)
        {
            if (_sources.Count > 0)
            {
                _sources[0].RemoveAll(c => c.Source == caster);
                if (_sources[0].Count == 0)
                {
                    NumCasts++;
                    _sources.RemoveAt(0);
                }
            }
        }
    }
}

class Hydrobullet(BossModule module) : Components.SpreadFromCastTargets(module, AID.HydrobulletSpread, 15);

class SurgingCurrent(BossModule module) : Components.StandardAOEs(module, AID.SurgingCurrent, new AOEShapeCone(60, 45.Degrees()), maxCasts: 2)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
            Casters.SortBy(c => Module.CastFinishAt(c.CastInfo));
    }
}

class Hydrofall(BossModule module) : Components.GenericAOEs(module, AID.Hydrofall)
{
    DateTime _activation;

    public bool Risky;
    readonly List<WPos> _sources = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _sources.Select(s => new AOEInstance(new AOEShapeCircle(12), s, default, _activation, Risky: Risky));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SwimmingInTheAir)
            _activation = WorldState.FutureTime(17.6f);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == 0x1EBF1B && _activation != default)
            _sources.Add(actor.Position);
    }
}

class ForcedMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    public bool EnableHints = true;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (EnableHints)
            base.AddHints(slot, actor, hints);
    }
}

class Tidalspout(BossModule module) : Components.UniformStackSpread(module, 6, 0, 2, 2)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.TidalspoutTarget)
            AddStack(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Tidalspout)
        {
            Stacks.Clear();
        }
    }
}

class CeaselessCurrent(BossModule module) : Components.Exaflare(module, new AOEShapeRect(8, 20))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (c, t, r) in FutureAOEs())
            yield return new(Shape, c, r, t, FutureColor, Risky: false);
        foreach (var (c, t, r) in ImminentAOEs())
            yield return new(Shape, c, r, t, ImminentColor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CeaselessCurrentFirst)
        {
            Lines.Add(new()
            {
                Next = spell.LocXZ,
                Advance = spell.Rotation.ToDirection() * 8,
                Rotation = spell.Rotation.ToDirection().Rounded().ToAngle(),
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.1f,
                ExplosionsLeft = 5,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CeaselessCurrentFirst or AID.CeaselessCurrentRest)
        {
            NumCasts++;
            var ix = Lines.FindIndex(l => l.Rotation.AlmostEqual(caster.Rotation, 0.1f));
            if (ix >= 0)
                AdvanceLine(Lines[ix], Lines[ix].Next);
        }
    }
}

// 6.1s delay
class CrossCurrent(BossModule module) : Components.GenericAOEs(module, AID.CrossCurrent, "GTFO from bait!")
{
    BitMask _targets;
    DateTime _activation;

    public static WPos TileCenter(Actor a)
    {
        WPos arenaCenter = new(375, 530);
        var dir = (a.Position - arenaCenter) / 8;
        return arenaCenter + dir.Rounded() * 8;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (_, p) in Raid.WithSlot().IncludedInMask(_targets).Exclude(actor))
            yield return new(new AOEShapeCross(36, 4), TileCenter(p), default, _activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_targets[pcSlot])
            new AOEShapeCross(36, 4).Outline(Arena, TileCenter(pc), default, ArenaColor.Danger);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.CrossCurrent)
        {
            _targets.Set(Raid.FindSlot(targetID));
            _activation = WorldState.FutureTime(6.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.CrossCurrent)
        {
            NumCasts++;
            _targets.Reset();
        }
    }
}

class SphereShatter(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly AOEShape Donut = new AOEShapeDonut(4, 20);
    public static readonly AOEShape Circle = new AOEShapeCircle(18);

    readonly List<AOEInstance> _predicted = [];

    public bool Risky;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Select(p => p with { Risky = Risky }).TakeSpan(TimeSpan.FromSeconds(2));

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00100020)
        {
            switch (actor.OID)
            {
                case 0x1EBF1C:
                    _predicted.Add(new(Circle, actor.Position, default, WorldState.FutureTime(10.2f)));
                    break;
                case 0x1EBF1D:
                    _predicted.Add(new(Donut, actor.Position, default, WorldState.FutureTime(10.2f)));
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SphereShatterDonut or AID.SphereShatterCircle)
        {
            NumCasts++;
            _predicted.RemoveAll(p => p.Origin.AlmostEqual(caster.Position, 1));
        }
    }
}

class AquaSpearX(BossModule module) : Components.StandardAOEs(module, AID.AquaSpearTileCast, new AOEShapeRect(8, 4));
class AquaSpearPlayer(BossModule module) : Components.GenericAOEs(module, AID.AquaSpearPlayer)
{
    DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _activation == default ? [] : Raid.WithoutSlot().Exclude(actor).Select(r => new AOEInstance(new AOEShapeRect(4, 4, 4), CrossCurrent.TileCenter(r), default, _activation));

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (_activation != default)
            Arena.AddRect(CrossCurrent.TileCenter(pc), new(0, 1), 4, 4, 4, ArenaColor.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Tile)
            _activation = WorldState.FutureTime(6);
    }
}

class AquaSpearTile(BossModule module) : Components.GenericAOEs(module)
{
    public bool Active => _tiles.Count > 0;

    readonly List<(Actor Actor, DateTime Spawn)> _tiles = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _tiles.Where(t => t.Actor.EventState != 7).Select(t => new AOEInstance(new AOEShapeRect(4, 4, 4), t.Actor.Position, default, t.Spawn.AddSeconds(2)));

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == 0x1EBF1E)
            _tiles.Add((actor, WorldState.CurrentTime));
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008)
            _tiles.RemoveAll(t => t.Actor == actor);
    }
}

class FamiliarCall2 : FamiliarCall1
{
    public FamiliarCall2(BossModule module) : base(module)
    {
        NumShown = 1;
        ColorImminent = ArenaColor.AOE;
    }
}

// 256.828 - 249.277
class EchoedSerenade(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<(VfxID, DateTime)> _order = [];

    bool _active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_active)
        {
            foreach (var (v, d) in _order.Take(1))
            {
                foreach (var enemy in Module.Enemies(v switch
                {
                    VfxID.Turtle => OID.SeabornSteward,
                    VfxID.Horse => OID.SeabornSteed,
                    VfxID.Crab => OID.SeabornSoldier,
                    _ => (OID)0
                }))
                    yield return new(FamiliarCall1.Rect, enemy.Position, enemy.Rotation, d);
            }
        }
    }

    public void Activate()
    {
        _active = true;

        for (var i = 0; i < _order.Count; i++)
        {
            var (v, _) = _order[i];
            _order[i] = (v, WorldState.FutureTime(7.6f + 3.2f * i));
        }
    }

    public override void OnEventVFX(Actor actor, uint vfxID, ulong targetID)
    {
        if ((OID)actor.OID == OID.Boss)
            _order.Add(((VfxID)vfxID, default));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (!_active)
            return;

        if ((AID)spell.Action.ID is AID.WatersongHorse or AID.WatersongCrab or AID.WatersongTurtle)
        {
            if (_order.Count > 0 && Math.Abs((WorldState.CurrentTime - _order[0].Item2).TotalSeconds) < 1)
            {
                NumCasts++;
                _order.RemoveAt(0);
            }
        }
    }
}

class AquaBall(BossModule module) : Components.StandardAOEs(module, AID.AquaBall, 5)
{
    public int NumBaits;

    DateTime _lastStarted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
        {
            if (_lastStarted.AddSeconds(1) < WorldState.CurrentTime)
                NumBaits++;
            _lastStarted = WorldState.CurrentTime;
        }
    }
}

class Shackles(BossModule module) : BossComponent(module)
{
    enum Assignment
    {
        None,
        Near,
        Far
    }

    record struct PlayerState(Assignment Assignment, int Partner);

    readonly PlayerState[] _assignments = Utils.MakeArray<PlayerState>(4, new(Assignment.None, -1));

    public bool Active => _assignments.Any(s => s.Assignment != Assignment.None);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var a = (TetherID)tether.ID switch
        {
            TetherID.Near => Assignment.Near,
            TetherID.Far => Assignment.Far,
            _ => default
        };

        if (a == default)
            return;

        if (Raid.TryFindSlot(source, out var fromSlot) && Raid.TryFindSlot(tether.Target, out var toSlot))
        {
            _assignments[fromSlot] = new(a, toSlot);
            _assignments[toSlot] = new(a, fromSlot);
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Active)
            Array.Fill(_assignments, new(Assignment.None, -1));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_assignments[pcSlot].Partner >= 0)
            Arena.AddLine(pc.Position, Raid[_assignments[pcSlot].Partner]!.Position, ArenaColor.Danger);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _assignments[pcSlot].Partner == playerSlot ? PlayerPriority.Danger : PlayerPriority.Normal;
}

class HydrobulletStack(BossModule module) : Components.UniformStackSpread(module, 15, 0, maxStackSize: 2)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.HydrobulletTarget)
            AddStack(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.HydrobulletStack)
            Stacks.Clear();
    }
}

class C011DaryaTheSeaMaidStates : StateMachineBuilder
{
    public C011DaryaTheSeaMaidStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        FamiliarCall1(id, 7.2f);
        AlluringOrder1(id + 0x10000, 6.2f);
        CeaselessCurrent(id + 0x20000, 7.2f);
        AlluringOrder2(id + 0x30000, 7.2f);
        AquaSpear(id + 0x40000, 6.1f);
        SeaShackles(id + 0x50000, 5.3f);
        Cast(id + 0x60000, AID.PiercingPlungeEnrageCast, 9.6f, 10, "Enrage");
    }

    void PiercingPlunge(uint id, float delay)
    {
        Cast(id, AID.PiercingPlunge, delay, 5, "Raidwide")
            .ActivateOnEnter<PiercingPlunge>()
            .DeactivateOnExit<PiercingPlunge>();
    }

    void FamiliarCall1(uint id, float delay)
    {
        PiercingPlunge(id, delay);

        Cast(id + 0x100, AID.FamiliarCall, 10.3f, 3)
            .ActivateOnEnter<FamiliarCall1>()
            .ActivateOnEnter<Hydrobullet>()
            .ActivateOnEnter<SurgingCurrent>()
            .ExecOnEnter<SurgingCurrent>(c => c.Risky = false);
        Cast(id + 0x110, AID.EchoedSerenade, 5.1f, 8.5f);
        ComponentCondition<FamiliarCall1>(id + 0x120, 3.6f, c => c.NumCasts > 0, "Adds 1");
        ComponentCondition<FamiliarCall1>(id + 0x121, 3.1f, c => c.NumCasts > 1, "Adds 2");
        ComponentCondition<Hydrobullet>(id + 0x122, 0.1f, h => h.NumFinishedSpreads > 0, "Spreads");
        ComponentCondition<FamiliarCall1>(id + 0x123, 3f, c => c.NumCasts > 2, "Adds 3");
        ComponentCondition<FamiliarCall1>(id + 0x124, 3.1f, c => c.NumCasts > 3, "Adds 4");
        ComponentCondition<Hydrobullet>(id + 0x125, 0.1f, h => h.NumFinishedSpreads > 4, "Spreads")
            .DeactivateOnExit<FamiliarCall1>()
            .DeactivateOnExit<Hydrobullet>()
            .ExecOnExit<SurgingCurrent>(c => c.Risky = true);

        ComponentCondition<SurgingCurrent>(id + 0x200, 4.9f, s => s.NumCasts > 0, "Diagonals 1");
        ComponentCondition<SurgingCurrent>(id + 0x201, 3.1f, s => s.NumCasts > 2, "Diagonals 2")
            .DeactivateOnExit<SurgingCurrent>();
    }

    void AlluringOrder1(uint id, float delay)
    {
        Cast(id, AID.AlluringOrder, delay, 4, "Raidwide")
            .ActivateOnEnter<AlluringOrder>()
            .ActivateOnEnter<ForcedMarch>()
            .ActivateOnEnter<Tidalspout>()
            .DeactivateOnExit<AlluringOrder>()
            .ExecOnEnter<Tidalspout>(t => t.EnableHints = false);

        Cast(id + 0x10, AID.SwimmingInTheAir, 5.5f, 4)
            .ActivateOnEnter<Hydrofall>();

        ComponentCondition<ForcedMarch>(id + 0x20, 9.7f, f => f.NumActiveForcedMarches > 0)
            .ExecOnExit<Tidalspout>(t => t.EnableHints = true)
            .ExecOnExit<Hydrofall>(t => t.Risky = true);

        ComponentCondition<Hydrofall>(id + 0x30, 3.9f, h => h.NumCasts > 0, "Puddles")
            .DeactivateOnExit<Hydrofall>();
        ComponentCondition<Tidalspout>(id + 0x31, 0.2f, t => !t.Active, "Stacks")
            .DeactivateOnExit<Tidalspout>()
            .DeactivateOnExit<ForcedMarch>();
    }

    void CeaselessCurrent(uint id, float delay)
    {
        Cast(id, AID.CeaselessCurrentCast, delay, 4)
            .ActivateOnEnter<CrossCurrent>()
            .ActivateOnEnter<CeaselessCurrent>()
            .ActivateOnEnter<SurgingCurrent>();

        ComponentCondition<CeaselessCurrent>(id + 0x10, 8.1f, c => c.NumCasts > 0, "Waves start");
        ComponentCondition<CrossCurrent>(id + 0x11, 4.2f, c => c.NumCasts > 0, "Crosses + diagonals")
            .DeactivateOnExit<CrossCurrent>();
        ComponentCondition<SurgingCurrent>(id + 0x12, 2.9f, s => s.NumCasts > 2, "Diagonals 2")
            .DeactivateOnExit<SurgingCurrent>();
        ComponentCondition<CeaselessCurrent>(id + 0x20, 1.3f, c => c.NumCasts >= 10, "Waves end")
            .DeactivateOnExit<CeaselessCurrent>();

        PiercingPlunge(id + 0x100, 7);
    }

    void AlluringOrder2(uint id, float delay)
    {
        Cast(id, AID.AlluringOrder, delay, 4, "Raidwide")
            .ActivateOnEnter<AlluringOrder>()
            .ActivateOnEnter<ForcedMarch>()
            .ActivateOnEnter<Tidalspout>()
            .DeactivateOnExit<AlluringOrder>()
            .ExecOnEnter<ForcedMarch>(m => m.EnableHints = false)
            .ExecOnEnter<Tidalspout>(m => m.EnableHints = false);

        Cast(id + 0x10, AID.SunkenTreasure, 5.2f, 3)
            .ActivateOnEnter<SphereShatter>()
            .ActivateOnEnter<SurgingCurrent>()
            .ExecOnEnter<SurgingCurrent>(c => c.Risky = false);

        ComponentCondition<ForcedMarch>(id + 0x20, 13, f => f.NumActiveForcedMarches > 0)
            .ExecOnEnter<ForcedMarch>(m => m.EnableHints = true)
            .ExecOnEnter<SurgingCurrent>(c => c.Risky = true)
            .ExecOnExit<SphereShatter>(s => s.Risky = true)
            .ExecOnExit<Tidalspout>(s => s.EnableHints = true);

        ComponentCondition<Tidalspout>(id + 0x21, 4.1f, t => !t.Active, "Stacks")
            .DeactivateOnExit<Tidalspout>()
            .DeactivateOnExit<ForcedMarch>();
        ComponentCondition<SphereShatter>(id + 0x22, 0.1f, s => s.NumCasts > 0, "Diagonals 1 + orbs");
        ComponentCondition<SurgingCurrent>(id + 0x23, 3.1f, s => s.NumCasts > 2, "Diagonals 2")
            .DeactivateOnExit<SurgingCurrent>();
        ComponentCondition<SphereShatter>(id + 0x24, 3.4f, s => s.NumCasts > 5, "Orb")
            .DeactivateOnExit<SphereShatter>();
    }

    void AquaSpear(uint id, float delay)
    {
        Cast(id, AID.AquaSpear, delay, 3)
            .ActivateOnEnter<AquaSpearX>()
            .ActivateOnEnter<AquaSpearPlayer>()
            .ActivateOnEnter<AquaSpearTile>();

        ComponentCondition<AquaSpearTile>(id + 0x10, 7, t => t.Active, "Tiles")
            .DeactivateOnExit<AquaSpearX>()
            .DeactivateOnExit<AquaSpearPlayer>();

        Cast(id + 0x20, AID.FamiliarCall, 4.3f, 3)
            .ActivateOnEnter<FamiliarCall2>()
            .ActivateOnEnter<EchoedSerenade>()
            .ActivateOnEnter<AquaBall>()
            .ActivateOnEnter<CrossCurrent>();

        Cast(id + 0x30, AID.EchoedSerenade, 5.2f, 8.5f);

        ComponentCondition<FamiliarCall2>(id + 0x40, 3.7f, f => f.NumCasts > 0, "Adds 1");
        ComponentCondition<FamiliarCall2>(id + 0x41, 3.1f, f => f.NumCasts > 1, "Adds 2");
        ComponentCondition<FamiliarCall2>(id + 0x42, 3.1f, f => f.NumCasts > 2, "Adds 3");
        ComponentCondition<FamiliarCall2>(id + 0x43, 3.1f, f => f.NumCasts > 3, "Adds 4")
            .DeactivateOnExit<FamiliarCall2>();

        ComponentCondition<CrossCurrent>(id + 0x50, 5.8f, c => c.NumCasts > 0, "Crosses")
            .DeactivateOnExit<CrossCurrent>();

        Cast(id + 0x100, AID.FamiliarCall, 8.1f, 3);
        Cast(id + 0x110, AID.EchoedReprise, 6.2f, 4)
            .ExecOnEnter<EchoedSerenade>(s => s.Activate());

        ComponentCondition<EchoedSerenade>(id + 0x200, 3.6f, s => s.NumCasts > 0, "Repeat 1");
        ComponentCondition<EchoedSerenade>(id + 0x201, 3.1f, s => s.NumCasts > 1, "Repeat 2");
        ComponentCondition<EchoedSerenade>(id + 0x202, 3.1f, s => s.NumCasts > 2, "Repeat 3");
        ComponentCondition<EchoedSerenade>(id + 0x203, 3.1f, s => s.NumCasts > 3, "Repeat 4")
            .DeactivateOnExit<EchoedSerenade>()
            .DeactivateOnExit<AquaBall>();

        ComponentCondition<SurgingCurrent>(id + 0x210, 2.8f, s => s.NumCasts > 0, "Diagonals 1")
            .ActivateOnEnter<SurgingCurrent>();
        ComponentCondition<SurgingCurrent>(id + 0x211, 3, s => s.NumCasts > 2, "Diagonals 2")
            .DeactivateOnExit<SurgingCurrent>();
    }

    void SeaShackles(uint id, float delay)
    {
        Cast(id, AID.SeaShackles, delay, 4)
            .ActivateOnEnter<AquaBall>()
            .ActivateOnEnter<HydrobulletStack>()
            .ActivateOnEnter<SphereShatter>()
            .ActivateOnEnter<Shackles>()
            .ExecOnExit<HydrobulletStack>(s => s.EnableHints = false)
            .DeactivateOnExit<AquaSpearTile>();

        ComponentCondition<Shackles>(id + 0x2, 1.1f, s => s.Active, "Tethers appear");

        Cast(id + 0x10, AID.SunkenTreasure, 2.1f, 3);

        ComponentCondition<AquaBall>(id + 0x20, 11.1f, b => b.Casters.Count > 0, "Puddle baits start");

        ComponentCondition<AquaBall>(id + 0x100, 4.1f, b => b.NumBaits == 3)
            .ExecOnExit<SphereShatter>(s => s.Risky = true);

        ComponentCondition<SphereShatter>(id + 0x110, 1.9f, s => s.NumCasts > 0, "Orbs 1");
        ComponentCondition<SphereShatter>(id + 0x111, 6.5f, s => s.NumCasts > 6, "Orbs 2")
            .DeactivateOnExit<SphereShatter>()
            .DeactivateOnExit<AquaBall>();

        PiercingPlunge(id + 0x200, 4.7f);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1079, NameID = 14291, PlanLevel = 100)]
public class C011DaryaTheSeaMaid(WorldState ws, Actor primary) : BossModule(ws, primary, new(375, 530), new ArenaBoundsSquare(20));
