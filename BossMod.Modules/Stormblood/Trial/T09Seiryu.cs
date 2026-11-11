namespace BossMod.Stormblood.Trial.WreathOfSnakes.T09Seiryu;

public enum OID : uint
{
    Boss = 0x25F4, // R4.400, x1
    Helper = 0x233C, // R0.500, Helper type
    AkaNoShiki = 0x2786, // R2.600, red wheel, charges its tether target
    AoNoShiki = 0x2787, // R3.000, blue wheel, line stack on its tether target
    IwaNoShiki = 0x2788, // R4.000, jumps to the edge, then circle and tethered cone
    TenNoShiki = 0x25F8, // R2.700, wide line
    NumaNoShiki = 0x25F6, // R2.400, big add, explodes for about 9k on the party
    DoroNoShiki = 0x25F7, // R1.440, small add, explodes for about 1k on the party
    YamaNoShiki = 0x25F9, // R12.000, phase 2 fists
    BlueOrochi = 0x2672, // R1.000, x4, idle during the add phase
    BlueOrochi1 = 0x25F5, // R1.000, Coursing River visual
    BlueOrochi2 = 0x2658, // R1.000, Coursing River visual
    BlueOrochi3 = 0x2659, // R1.000, Coursing River visual
    SerpentTower = 0x1EAA3C, // EventObj type, soak tower, x4
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttackAdds = 872, // NumaNoShiki/DoroNoShiki->player, no cast, single-target
    FifthElement = 14334, // Boss->self, 4.0s cast, range 100 circle, raidwide
    InfirmSoul = 14333, // Boss->player, 5.0s cast, range 4 circle, tankbuster
    SerpentAscending = 14300, // Boss->self, no cast, visual before spreads
    SerpentAscendingCast = 15397, // Boss->self, 4.0s cast, single-target, visual, soak towers appear about 2.6s later
    SerpentsFang = 14988, // Helper->self, no cast, range 3 circle, tower with a soaker
    SerpentsJaws = 14989, // Helper->self, no cast, range 80 circle, raidwide and Damage Down for each empty tower
    SerpentDescending = 14340, // Helper->player, 6.0s cast, range 5 circle spread
    Teleport = 14293, // Boss->location, no cast, single-target, sigil position
    OnmyoSigilVisual = 14338, // Boss->self, no cast, visual
    SerpentEyeSigilVisual = 14339, // Boss->self, no cast, visual
    OnmyoSigil = 14855, // Helper->self, 3.0s cast, range 12 circle
    SerpentEyeSigil = 14856, // Helper->self, 3.0s cast, range 7-30 donut
    KujiKiri = 14305, // Boss->self, 4.0s cast, single-target, visual
    FortuneBladeSigil = 14342, // Helper->self, 6.5s cast, range 50+R width 4 rect
    SummonShikiWheels = 14286, // Boss->self, 3.0s cast, single-target, visual
    SummonShikiIwa = 14285, // Boss->self, 3.0s cast, single-target, visual
    SummonShikiYama = 14287, // Boss->self, 3.0s cast, single-target, visual
    SummonShikiOrochi = 14288, // Boss->self, 5.0s cast, single-target, visual
    BlueBoltTarget = 14319, // Helper->player, no cast, selects the line stack target
    BlueBolt = 15388, // AoNoShiki->self, no cast, range 80+R width 5 rect, line stack
    RedRush = 15389, // AkaNoShiki->self, no cast, range 80+R width 5 rect, knocks back the tether target
    HundredTonzeSwing = 15390, // IwaNoShiki->self, 4.0s cast, range 16 circle
    KanaboVisual = 15392, // IwaNoShiki->self, 6.0s cast, single-target, visual
    Kanabo = 15391, // IwaNoShiki->self, 3.0s cast, range 40+R 60-degree cone
    YamaKagura = 14355, // TenNoShiki->self, 5.0s cast, range 60+R width 6 rect
    ExplosionNuma = 14347, // NumaNoShiki->self, no cast, range 80 circle, on death
    ExplosionDoro = 14348, // DoroNoShiki->self, no cast, range 80 circle, on death
    StrengthOfSpirit = 14281, // Boss->self, 5.0s cast, range 80 circle, damage scales with adds left alive
    DragonsWakeVisual = 14282, // Boss->self, no cast, single-target, visual
    DragonsWake = 14336, // Helper->self, 24.0s cast, range 80 circle, raidwide, phase change
    GreatTyphoon1 = 14352, // Helper->self, 3.0s cast, donut out to 28, only reaches the water
    GreatTyphoon2 = 14353, // Helper->self, 3.0s cast, donut out to 34, only reaches the water
    GreatTyphoon3 = 14354, // Helper->self, 3.0s cast, donut out to 40, only reaches the water
    CoursingRiverVisual1 = 14325, // BlueOrochi1->self, 5.0s cast, single-target, visual
    CoursingRiverVisual2 = 14626, // BlueOrochi2->self, 5.0s cast, single-target, visual
    CoursingRiverVisual3 = 14627, // BlueOrochi3->self, 5.0s cast, single-target, visual
    CoursingRiverDamage = 14349, // Helper->self, 7.5s cast, range 21 circle, raidwide
    CoursingRiver = 14350, // Helper->self, 7.5s cast, range 90 width 90 rect, knockback 25
    ForbiddenArtsTarget = 14277, // Helper->player, no cast, selects the line stack target
    ForbiddenArtsFirst = 15490, // Boss->self, no cast, range 80+R width 8 rect, line stack
    ForbiddenArtsSecond = 15474, // Boss->self, no cast, range 80+R width 8 rect, line stack
    HandprintVisual1 = 14309, // YamaNoShiki->self, no cast, visual
    HandprintVisual2 = 14310, // YamaNoShiki->self, no cast, visual
    HandprintNear = 14343, // Helper->self, 4.5s cast, range 20 180-degree cone
    HandprintFar = 14344, // Helper->self, 4.5s cast, range 40 180-degree cone
    ForceOfNatureVisual = 14313, // YamaNoShiki->self, no cast, visual
    ForceOfNatureCenter = 14345, // Helper->self, 5.0s cast, range 5 circle, lethal
    ForceOfNatureKnockback = 14346, // Helper->self, 5.0s cast, range 21 circle, knockback 10
}

public enum TetherID : uint
{
    Shiki = 17, // AkaNoShiki/AoNoShiki/IwaNoShiki->player
}

class Phase2Water(BossModule module) : BossComponent(module)
{
    public bool Active { get; private set; }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DragonsWake)
        {
            Active = true;
            Arena.Bounds = new ArenaBoundsCircle(T09Seiryu.ArenaRadius);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
            Arena.ZoneDonut(Arena.Center, T09Seiryu.IslandRadius, T09Seiryu.ArenaRadius, ArenaColor.AOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Active && !actor.Position.InCircle(Arena.Center, T09Seiryu.IslandRadius))
            hints.Add("Return to the island!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Active)
            hints.AddForbiddenZone(ShapeDistance.Donut(Arena.Center, T09Seiryu.IslandRadius, T09Seiryu.ArenaRadius));
    }
}

class Sigils(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Onmyo = new(12);
    private static readonly AOEShapeDonut SerpentEye = new(7, 30);

    private WPos _origin;
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor == Module.PrimaryActor && modelState is 32 or 33)
            _aoe = new(modelState == 32 ? Onmyo : SerpentEye, _origin, default, WorldState.FutureTime(5.6f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Teleport:
                _origin = spell.TargetXZ;
                break;
            case AID.OnmyoSigil:
            case AID.SerpentEyeSigil:
                ++NumCasts;
                _aoe = null;
                break;
        }
    }
}

class ShikiTether(BossModule module, OID oid, AOEShape shape, AID aid, float delay) : Components.BaitAwayTethers(module, shape, (uint)TetherID.Shiki, aid)
{
    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID == (uint)oid && tether.ID == TID && WorldState.Actors.Find(tether.Target) is { } target)
            CurrentBaits.Add(new(source, target, Shape, WorldState.FutureTime(delay)));
    }
}

class RedRush(BossModule module) : ShikiTether(module, OID.AkaNoShiki, new AOEShapeRect(82.6f, 2.5f), AID.RedRush, 6.2f);

class KanaboBait(BossModule module) : ShikiTether(module, OID.IwaNoShiki, new AOEShapeCone(44, 30.Degrees()), AID.Kanabo, 9.3f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var b in ActiveBaitsOn(actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 18), b.Activation.AddSeconds(-3));
    }
}

// TODO: distance is estimated from two knockbacks into the water, phase 1 walls hide it
class RedRushKnockback(BossModule module) : Components.Knockback(module, AID.RedRush, stopAtWall: true)
{
    private readonly RedRush? _rush = module.FindComponent<RedRush>();
    private readonly Phase2Water? _water = module.FindComponent<Phase2Water>();

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var b in _rush?.ActiveBaitsOn(actor) ?? [])
            yield return new(b.Source.Position, 15, b.Activation);
    }

    public override PointTest DestinationUnsafe(int slot, Actor actor)
    {
        if (_water is not { Active: true })
            return PointTest.Never;

        return ShapeDistance.InvertedCircle(Arena.Center, T09Seiryu.IslandRadius).Discretize();
    }
}

class BlueBolt(BossModule module) : Components.SimpleLineStack(module, 2.5f, 83, AID.BlueBoltTarget, AID.BlueBolt, 6.1f)
{
    private readonly RedRush? _rush = module.FindComponent<RedRush>();

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.BlueBoltTarget)
            foreach (var b in _rush?.ActiveBaits ?? [])
                if (Raid.TryFindSlot(b.Target, out var slot))
                    PlayerRoles[slot] = PlayerRole.Avoid;
    }
}

class SerpentDescending(BossModule module) : Components.SpreadFromCastTargets(module, AID.SerpentDescending, 5)
{
    public int NumWaves { get; private set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SerpentDescending && Spreads.Count == 0)
            ++NumWaves;
        base.OnCastStarted(caster, spell);
    }
}

class SerpentTowers(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.SerpentTower)
            Towers.Add(new(actor.Position, 3, activation: WorldState.FutureTime(7.6f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SerpentsFang or AID.SerpentsJaws)
        {
            ++NumCasts;
            Towers.RemoveAll(t => t.IsInside(caster.Position));
        }
    }
}

class ForbiddenArts(BossModule module) : Components.GenericWildCharge(module, 4, AID.ForbiddenArtsSecond, 84.4f)
{
    private DateTime _hideAt;

    public override void Update()
    {
        if (_hideAt != default && WorldState.CurrentTime >= _hideAt)
        {
            _hideAt = default;
            Source = null;
            Array.Fill(PlayerRoles, PlayerRole.Ignore);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ForbiddenArtsTarget:
                Source = caster;
                Hits = 2;
                Activation = WorldState.FutureTime(5.4f);
                foreach (var (i, p) in Raid.WithSlot(true))
                    PlayerRoles[i] = p.InstanceID == spell.MainTargetID ? PlayerRole.Target : PlayerRole.Share;
                break;
            case AID.ForbiddenArtsFirst:
                Hits = 1;
                Activation = WorldState.FutureTime(2.1f);
                foreach (var (i, p) in Raid.WithSlot())
                {
                    if (PlayerRoles[i] == PlayerRole.Share && p.Role == Role.Healer)
                    {
                        PlayerRoles[Array.IndexOf(PlayerRoles, PlayerRole.Target)] = PlayerRole.Share;
                        PlayerRoles[i] = PlayerRole.Target;
                        break;
                    }
                }
                break;
            case AID.ForbiddenArtsSecond:
                ++NumCasts;
                _hideAt = WorldState.FutureTime(1);
                break;
        }
    }
}

class CoursingRiver(BossModule module) : Components.KnockbackFromCastTarget(module, AID.CoursingRiver, 25, shape: new AOEShapeRect(90, 45), kind: Kind.DirForward)
{
    public override PointTest DestinationUnsafe(int slot, Actor actor) => ShapeDistance.InvertedCircle(Arena.Center, T09Seiryu.IslandRadius).Discretize();
}

class ForceOfNatureKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.ForceOfNatureKnockback, 10, shape: new AOEShapeCircle(21))
{
    public override PointTest DestinationUnsafe(int slot, Actor actor) => ShapeDistance.InvertedCircle(Arena.Center, T09Seiryu.IslandRadius).Discretize();
}

class FifthElement(BossModule module) : Components.RaidwideCast(module, AID.FifthElement);
class StrengthOfSpirit(BossModule module) : Components.RaidwideCast(module, AID.StrengthOfSpirit);
class DragonsWake(BossModule module) : Components.RaidwideCast(module, AID.DragonsWake);
class CoursingRiverDamage(BossModule module) : Components.RaidwideCast(module, AID.CoursingRiverDamage);
class InfirmSoul(BossModule module) : Components.BaitAwayCast(module, AID.InfirmSoul, new AOEShapeCircle(4), centerAtTarget: true, endsOnCastEvent: true);
class KujiKiri(BossModule module) : Components.StandardAOEs(module, AID.FortuneBladeSigil, new AOEShapeRect(50.5f, 2));
class HundredTonzeSwing(BossModule module) : Components.StandardAOEs(module, AID.HundredTonzeSwing, 16);
class Kanabo(BossModule module) : Components.StandardAOEs(module, AID.Kanabo, new AOEShapeCone(44, 30.Degrees()));
class YamaKagura(BossModule module) : Components.StandardAOEs(module, AID.YamaKagura, new AOEShapeRect(62.7f, 3));
class Adds(BossModule module) : Components.AddsMulti(module, [OID.DoroNoShiki, OID.NumaNoShiki], 1);
class Handprint(BossModule module) : Components.StandardAOEs(module, AID.HandprintFar, new AOEShapeCone(40, 90.Degrees()));
class ForceOfNatureCenter(BossModule module) : Components.StandardAOEs(module, AID.ForceOfNatureCenter, 5);

class T09SeiryuStates : StateMachineBuilder
{
    public T09SeiryuStates(BossModule module) : base(module)
    {
        DeathPhase(0, Fight)
            .ActivateOnEnter<Phase2Water>()
            .ActivateOnEnter<RedRush>()
            .ActivateOnEnter<RedRushKnockback>()
            .ActivateOnEnter<BlueBolt>()
            .ActivateOnEnter<KanaboBait>()
            .ActivateOnEnter<Kanabo>()
            .ActivateOnEnter<CoursingRiver>()
            .ActivateOnEnter<CoursingRiverDamage>()
            .ActivateOnEnter<ForceOfNatureKnockback>()
            .ActivateOnEnter<ForceOfNatureCenter>()
            .ActivateOnEnter<FifthElement>()
            .ActivateOnEnter<StrengthOfSpirit>()
            .ActivateOnEnter<DragonsWake>()
            .ActivateOnEnter<InfirmSoul>()
            .ActivateOnEnter<SerpentDescending>()
            .ActivateOnEnter<SerpentTowers>()
            .ActivateOnEnter<Sigils>()
            .ActivateOnEnter<KujiKiri>()
            .ActivateOnEnter<HundredTonzeSwing>()
            .ActivateOnEnter<YamaKagura>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<ForbiddenArts>()
            .ActivateOnEnter<Handprint>();
    }

    private void Fight(uint id)
    {
        Raidwide(id, 8.2f);
        Tankbuster(id + 0x10000, 7.2f);
        Sigil(id + 0x20000, 14.8f, 1);
        Spreads(id + 0x30000, 13.2f, 1);
        Sigil(id + 0x40000, 7.8f, 2);
        Cast(id + 0x50000, AID.KujiKiri, 4.6f, 4, "Grid start");
        Grid(id + 0x50010, 2.6f, 1);
        AddPhase(id + 0x60000, 13.8f);
        Orochi(id + 0x70000, 4.3f);
        Yama(id + 0x80000, 2.2f);
        Sigil(id + 0x90000, 17.6f, 3);
        Spreads(id + 0xA0000, 8.2f, 2);
        Grid(id + 0xB0000, 3.0f, 2);
        Raidwide(id + 0xC0000, 0.8f);
        Tankbuster(id + 0xD0000, 8.1f);
        Raidwide(id + 0xE0000, 2.1f);
        Mixed(id + 0xF0000, 8.2f);
        Overtime(id + 0x100000, 14.2f);
        Towers(id + 0x110000, 7.9f);
        Spreads(id + 0x120000, 21.3f, 4);
        Grid(id + 0x130000, 2.2f, 4);
        Sigil(id + 0x140000, 7.3f, 5);
        Tankbuster(id + 0x150000, 4.2f);
        Raidwide(id + 0x160000, 2.1f);
        OrochiRaidwide(id + 0x170000, 12.6f);
        Raidwide(id + 0x180000, 2.1f);
        Raidwide(id + 0x190000, 3.2f);
        Cast(id + 0x1A0000, AID.SummonShikiIwa, 12.1f, 3, "Summon");

        SimpleState(id + 0xFF0000, 10000, "???");
    }

    private void Raidwide(uint id, float delay)
    {
        Cast(id, AID.FifthElement, delay, 4, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Tankbuster(uint id, float delay)
    {
        Cast(id, AID.InfirmSoul, delay, 5, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Sigil(uint id, float delay, int count)
    {
        ComponentCondition<Sigils>(id, delay, comp => comp.NumCasts >= count, "In/out");
    }

    private void Spreads(uint id, float delay, int count)
    {
        ComponentCondition<SerpentDescending>(id, delay, comp => comp.NumWaves >= count && comp.Spreads.Count == 0, "Spreads");
    }

    private void Grid(uint id, float delay, int count)
    {
        ComponentCondition<KujiKiri>(id, delay, comp => comp.NumCasts >= count * 9, "Grid");
    }

    private void AddPhase(uint id, float delay)
    {
        Cast(id, AID.SummonShikiWheels, delay, 3, "Summon");
        Targetable(id + 0x10, false, 2.1f, "Boss disappears");
        ComponentCondition<BlueBolt>(id + 0x20, 11.0f, comp => comp.NumCasts >= 1, "Line stack + charges");
        ComponentCondition<HundredTonzeSwing>(id + 0x30, 4.8f, comp => comp.NumCasts >= 2, "Circles");
        ComponentCondition<YamaKagura>(id + 0x40, 6.1f, comp => comp.NumCasts >= 4, "Lines");
        ComponentCondition<Kanabo>(id + 0x50, 6.2f, comp => comp.NumCasts >= 2, "Cones");
        Cast(id + 0x60, AID.StrengthOfSpirit, 27.0f, 5, "Adds end")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DragonsWake>(id + 0x70, 27.1f, comp => comp.NumCasts >= 1, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        Targetable(id + 0x80, true, 2.6f, "Boss returns");
    }

    private void Orochi(uint id, float delay)
    {
        Cast(id, AID.SummonShikiOrochi, delay, 5, "Summon");
        ComponentCondition<CoursingRiver>(id + 0x10, 10.5f, comp => comp.NumCasts >= 1, "Knockback")
            .SetHint(StateMachine.StateHint.Knockback);
        Cast(id + 0x20, AID.SummonShikiOrochi, 2.7f, 5, "Summon");
        ComponentCondition<CoursingRiver>(id + 0x30, 10.5f, comp => comp.NumCasts >= 2, "Knockback")
            .SetHint(StateMachine.StateHint.Knockback);
        ComponentCondition<ForbiddenArts>(id + 0x40, 6.1f, comp => comp.NumCasts >= 1, "Line stack x2");
    }

    private void Yama(uint id, float delay)
    {
        Cast(id, AID.SummonShikiYama, delay, 3, "Summon");
        ComponentCondition<Handprint>(id + 0x10, 14.5f, comp => comp.NumCasts >= 1, "Fist");
        ComponentCondition<Handprint>(id + 0x20, 8.2f, comp => comp.NumCasts >= 2, "Fist");
        ComponentCondition<FifthElement>(id + 0x30, 3.4f, comp => comp.NumCasts >= 2, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Handprint>(id + 0x40, 4.8f, comp => comp.NumCasts >= 3, "Fist");
        ComponentCondition<ForceOfNatureCenter>(id + 0x50, 8.7f, comp => comp.NumCasts >= 1, "Center + knockback")
            .SetHint(StateMachine.StateHint.Knockback);
    }

    private void Mixed(uint id, float delay)
    {
        Cast(id, AID.SummonShikiIwa, delay, 3, "Summon");
        ComponentCondition<HundredTonzeSwing>(id + 0x10, 11.8f, comp => comp.NumCasts >= 4, "Circles");
        Sigil(id + 0x20, 2.1f, 4);
        ComponentCondition<Kanabo>(id + 0x30, 10.2f, comp => comp.NumCasts >= 4, "Cones");
        ComponentCondition<BlueBolt>(id + 0x40, 9.9f, comp => comp.NumCasts >= 2, "Line stack + charges");
        Spreads(id + 0x50, 8.0f, 3);
        ComponentCondition<YamaKagura>(id + 0x60, 0.9f, comp => comp.NumCasts >= 8, "Lines");
        Grid(id + 0x70, 3.0f, 3);
    }

    private void Overtime(uint id, float delay)
    {
        ComponentCondition<Handprint>(id, delay, comp => comp.NumCasts >= 4, "Fist");
        ComponentCondition<Handprint>(id + 0x10, 8.2f, comp => comp.NumCasts >= 5, "Fist");
        ComponentCondition<CoursingRiver>(id + 0x20, 2.9f, comp => comp.NumCasts >= 3, "Knockback")
            .SetHint(StateMachine.StateHint.Knockback);
        ComponentCondition<Handprint>(id + 0x30, 5.3f, comp => comp.NumCasts >= 6, "Fist");
        Cast(id + 0x40, AID.SerpentAscendingCast, 2.3f, 4, "Serpent Ascending");
        ComponentCondition<ForceOfNatureCenter>(id + 0x50, 2.3f, comp => comp.NumCasts >= 2, "Center + knockback")
            .SetHint(StateMachine.StateHint.Knockback);
    }

    private void Towers(uint id, float delay)
    {
        ComponentCondition<SerpentTowers>(id, delay, comp => comp.NumCasts > 0, "Towers");
        ComponentCondition<FifthElement>(id + 0x10, 4.0f, comp => comp.NumCasts >= 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void OrochiRaidwide(uint id, float delay)
    {
        Cast(id, AID.SummonShikiOrochi, delay, 5, "Summon");
        ComponentCondition<CoursingRiver>(id + 0x10, 10.5f, comp => comp.NumCasts >= 4, "Knockback")
            .SetHint(StateMachine.StateHint.Knockback);
        ComponentCondition<FifthElement>(id + 0x20, 0.6f, comp => comp.NumCasts >= 7, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x30, AID.SummonShikiOrochi, 2.1f, 5, "Summon");
        ComponentCondition<CoursingRiver>(id + 0x40, 10.5f, comp => comp.NumCasts >= 5, "Knockback")
            .SetHint(StateMachine.StateHint.Knockback);
        ComponentCondition<ForbiddenArts>(id + 0x50, 5.9f, comp => comp.NumCasts >= 2, "Line stack x2");
    }
}

[ModuleInfo(Contributors = "skmagiik & Nonu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 637, NameID = 7922)]
public class T09Seiryu(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(Phase1Radius))
{
    public const float Phase1Radius = 19.5f;
    public const float IslandRadius = 20;
    public const float ArenaRadius = 40;
}
