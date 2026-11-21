namespace BossMod.Stormblood.Alliance.A34UltimaTheHighSeraph;

public enum OID : uint
{
    Boss = 0x2604, // R10.400, x1
    Helper = 0x233C, // R0.500, x39 (spawn during fight)
    _Gen_DemiFamfrit = 0x2634, // R5.940, x1
    _Gen_DemiHashmal = 0x2637, // R5.400, x1
    _Gen_DemiBelias = 0x2636, // R4.590, x1
    _Gen_AuraciteShard = 0x2639, // R1.250, x0 (spawn during fight)
    _Gen_Aspersory = 0x2635, // R2.400, x0 (spawn during fight)
    _Gen_Ruination = 0x263A, // R3.900, x0 (spawn during fight), Part type
    UltimaP2 = 0x25A9, // R8.000, x0 (spawn during fight)

    EarthHammer = 0x1EAA61,
    ControlTower = 0x1EAA62,
    ControlTowerIndicator = 0x1EAA63,
}

public enum AID : uint
{
    _AutoAttack_Attack = 14499, // Boss->player, no cast, single-target
    _Weaponskill_HolyIV1 = 14490, // Helper->location, 3.0s cast, range 6 circle
    _Weaponskill_HolyIV = 14489, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Auralight1 = 14487, // Helper->self, 6.0s cast, range 70 width 10 rect
    _Weaponskill_Auralight = 14570, // Boss->self, 6.0s cast, single-target
    _Weaponskill_Auralight2 = 14488, // Helper->self, 6.0s cast, range 20 circle
    _Weaponskill_HolyIV2 = 14491, // Helper->players, 5.5s cast, range 6 circle
    _Weaponskill_GrandCross = 14508, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Plummet = 14509, // 2639->self, 3.5s cast, range 15 width 15 rect
    _Weaponskill_GrandCross1 = 14510, // 2639->self, 4.0s cast, range 60 width 15 cross
    _Ability_DemiAquarius = 14527, // Boss->self, 3.0s cast, single-target
    _Weaponskill_DarkEwer = 14538, // 2634->self, 3.0s cast, single-target
    _Spell_PrevailingCurrent = 14540, // Helper->self, no cast, range 6 circle
    _Spell_Materialize = 14539, // 2635->self, 2.0s cast, range 6 circle
    _Ability_DemiAries = 14528, // Boss->self, 3.0s cast, single-target
    _Weaponskill_TimeEruption = 14543, // 2636->self, 3.0s cast, single-target
    _Weaponskill_TimeEruption1 = 14544, // Helper->self, 6.0s cast, range 20 width 20 rect
    _Weaponskill_HolyIV3 = 15367, // Boss->self, 5.0s cast, single-target
    _Weaponskill_TimeEruption2 = 14545, // Helper->self, 9.0s cast, range 20 width 20 rect
    _Ability_DemiLeo = 14529, // Boss->self, 3.0s cast, single-target
    _Weaponskill_ControlTower = 14548, // 2637->self, 3.0s cast, range 80 circle
    _Weaponskill_ControlTower1 = 14549, // Helper->self, 4.0s cast, range 6 circle
    _Weaponskill_Sanction = 14550, // 2637->self, no cast, range 80 circle
    _Weaponskill_Towerfall = 14551, // Helper->self, no cast, range 40+R width 10 rect
    _Weaponskill_EarthHammer = 14552, // 2637->self, 5.0s cast, single-target
    _Weaponskill_DarkCannonade = 14541, // 2634->self, 5.0s cast, single-target
    _Weaponskill_DarkCannonade1 = 14542, // Helper->player, no cast, single-target
    _Weaponskill_Hammerfall = 14553, // Helper->self, 1.0s cast, range 40 circle, falloff radius is maybe 25
    _Weaponskill_Eruption = 15479, // 2636->self, 3.0s cast, single-target
    _Weaponskill_Eruption1 = 15480, // Helper->location, 3.0s cast, range 8 circle
    _Weaponskill_ExtremeEdge = 14554, // 2637->self, 6.0s cast, range 60+R width 36 rect
    _Weaponskill_ExtremeEdge1 = 14555, // _Gen_DemiHashmal->self, 6.0s cast, range 60+R width 36 rect

    _Weaponskill_UltimateIllusion = 14485, // Boss->self, 22.0s cast, single-target
    _Weaponskill_UltimateIllusion1 = 14486, // Helper->self, no cast, range 80 circle
    _Weaponskill_ = 14482, // Boss->self, no cast, single-target
    _Weaponskill_1 = 14483, // Boss->self, no cast, single-target
    _Weaponskill_2 = 14556, // Boss->self, no cast, single-target
    _Weaponskill_UltimateIllusion2 = 15003, // Helper->self, no cast, range 80 circle
    _Weaponskill_Tsunami = 14672, // 2634->self, 15.0s cast, single-target
    _Weaponskill_Landwaster = 14674, // 2637->self, 15.0s cast, single-target
    _Weaponskill_Hellfire = 14673, // 2636->self, 15.0s cast, single-target
}

public enum SID : uint
{
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_VulnerabilityUp = 202, // 2639/Helper->player, extra=0x1/0x2
    _Gen_GuardianSpirit = 1692, // none->262C/262A/262B, extra=0x0
    _Gen_FleshWound = 264, // 2637->player, extra=0x0
    _Gen_GuardiansAegis = 1748, // none->player, extra=0x0
    _Gen_TheEcho = 42, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_target_ae_s5f = 139, // player->self, spread
    _Gen_Icon_com_trg01_0c = 55, // player->self, stack together
    _Gen_Icon_m0406trg_a0n = 102, // player->self, eruption bait
}

class HolyIVPuddle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_HolyIV1, 6);
class HolyIVSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID._Weaponskill_HolyIV2, 6);
class Auralight1(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Auralight1, new AOEShapeRect(70, 5));
class Auralight2(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Auralight2, 20);
class Auralight(BossModule module) : BossComponent(module)
{
    readonly List<(WPos, Angle, AOEShape)> Obstacles = [];

    public override void OnActorTimelineStateChange(Actor actor, uint state)
    {
        if (actor.LayoutID != 0x7626AE)
            return;

        switch ((actor.TimelineState, state & 0xFFFF))
        {
            case (1, 0x0020):
                Obstacles.Add((new(600, -630), -45.Degrees(), new AOEShapeRect(70, 4.5f)));
                Obstacles.Add((new(600, -630), 15.Degrees(), new AOEShapeRect(70, 4.5f)));
                Obstacles.Add((new(600, -630), 45.Degrees(), new AOEShapeRect(70, 4.5f)));
                break;
            case (1, 0x0100):
                Obstacles.Add((new(600, -630), -15.Degrees(), new AOEShapeRect(70, 4.5f)));
                Obstacles.Add((new(600, -630), 15.Degrees(), new AOEShapeRect(70, 4.5f)));
                Obstacles.Add((new(600, -630), 75.Degrees(), new AOEShapeRect(70, 4.5f)));
                break;
            case (1, 0x0800):
                Obstacles.Add((new(600, -630), -45.Degrees(), new AOEShapeRect(70, 4.5f)));
                Obstacles.Add((new(600, -630), -15.Degrees(), new AOEShapeRect(70, 4.5f)));
                Obstacles.Add((new(600, -630), 45.Degrees(), new AOEShapeRect(70, 4.5f)));
                break;
            case (1, 0x4000):
                Obstacles.Add((new(600, -630), -75.Degrees(), new AOEShapeRect(70, 4.5f)));
                Obstacles.Add((new(600, -630), -45.Degrees(), new AOEShapeRect(70, 4.5f)));
                Obstacles.Add((new(600, -630), 15.Degrees(), new AOEShapeRect(70, 4.5f)));
                break;
            case (4, _):
                Obstacles.Clear();
                break;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (p, a, s) in Obstacles)
            s.Draw(Arena, p, a, ArenaColor.AOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (p, a, s) in Obstacles)
            hints.TemporaryObstacles.Add(s.GetSdf(p, a));
    }
}

class Plummet(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Plummet, new AOEShapeRect(15, 7.5f));
class GrandCross(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_GrandCross1)
{
    DateTime Activation;
    readonly List<Actor> Crosses = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Crosses.Count == 3)
            foreach (var c in Crosses)
                yield return new(new AOEShapeCross(60, 7.5f), c.Position, default, Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Plummet)
        {
            Crosses.Add(caster);
            if (Activation == default)
                Activation = Module.CastFinishAt(spell, 13.7f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (Crosses.Count > 0)
                Crosses.RemoveAt(0);
        }
    }
}

class Aspersory(BossModule module) : Components.Voidzone(module, 6, OID._Gen_Aspersory, moveHintLength: 20);
class TimeEruption(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_TimeEruption1, AID._Weaponskill_TimeEruption2], new AOEShapeRect(20, 10))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).TakeSpan(a => a.Activation, TimeSpan.FromSeconds(1));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (IDs.Contains(spell.Action))
            Casters.SortBy(c => Module.CastFinishAt(c.CastInfo));
    }
}

class ControlTowerRaidwide(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_ControlTower);
class ControlTowerSpawn(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_ControlTower1, 6);
class ControlTower(BossModule module) : BossComponent(module)
{
    readonly List<Actor> Towers = [];

    public override void OnActorTimelineStateChange(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.ControlTower && actor.TimelineState == 8)
            Towers.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Towerfall)
            Towers.Clear();
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var t in Towers)
            Arena.ZoneCircle(t.Position, 6, ArenaColor.AOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var t in Towers)
            hints.TemporaryObstacles.Add(ShapeDistance.Circle(t.Position, 6));
    }
}
class Towerfall(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_Towerfall)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnActorTimelineStateChange(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.ControlTower && actor.TimelineState == 0x10)
            _predicted.Add(new(new AOEShapeRect(45, 5), actor.Position, actor.Rotation, WorldState.FutureTime(6.8f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class DarkCannonade(BossModule module) : Components.StackTogether(module, (uint)IconID._Gen_Icon_com_trg01_0c, 5.1f, radius: 1.5f);

class Hammerfall(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_Hammerfall)
{
    AOEInstance? _predicted;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_predicted);

    public override void OnActorTimelineStateChange(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.EarthHammer && actor.TimelineState == 8)
            _predicted = new(new AOEShapeCircle(25), actor.Position, default, WorldState.FutureTime(16.2f));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_predicted is { Activation: var a })
            hints.AddPredictedDamage(new BitMask(~0ul), a);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _predicted = null;
        }
    }
}

class Eruption(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Eruption1, 8);
class ExtremeEdge(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_ExtremeEdge, AID._Weaponskill_ExtremeEdge1], new AOEShapeRect(65, 18));

class GuardiansAegis(BossModule module) : BossComponent(module)
{
    const float Radius = 7;
    DateTime Activation;

    public override void OnActorTimelineStateChange(Actor actor, uint state)
    {
        if (actor.LayoutID == 0x7626B2)
        {
            if (actor.TimelineState == 1)
                Activation = Module.CastFinishAt(Module.PrimaryActor.CastInfo);

            if (actor.TimelineState == 0x1000)
            {
                Arena.Center = actor.Position;
                Arena.Bounds = new ArenaBoundsCircle(7);
                Activation = default;
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Activation != default)
            Arena.ZoneCircle(new(600, -620), 7, ArenaColor.SafeFromAOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Activation != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(600, -620), Radius), Activation);
    }
}

class Ruination(BossModule module) : Components.Adds(module, (uint)OID._Gen_Ruination, forbidDots: true);

class A34UltimaTheHighSeraphStates : StateMachineBuilder
{
    public A34UltimaTheHighSeraphStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HolyIVPuddle>()
            .ActivateOnEnter<HolyIVSpread>()
            .ActivateOnEnter<Auralight1>()
            .ActivateOnEnter<Auralight2>()
            .ActivateOnEnter<Auralight>()
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<GrandCross>()
            .ActivateOnEnter<Aspersory>()
            .ActivateOnEnter<TimeEruption>()
            .ActivateOnEnter<ControlTowerRaidwide>()
            .ActivateOnEnter<ControlTowerSpawn>()
            .ActivateOnEnter<ControlTower>()
            .ActivateOnEnter<Towerfall>()
            .ActivateOnEnter<DarkCannonade>()
            .ActivateOnEnter<Hammerfall>()
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<ExtremeEdge>()
            .ActivateOnEnter<GuardiansAegis>()
            .ActivateOnEnter<Ruination>()
            .Raw.Update = () => Module.Enemies(OID.UltimaP2).Any();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7657)]
public class A34UltimaTheHighSeraph(ModuleInit init) : BossModule(init, new(600, -600), new ArenaBoundsSquare(29.5f));
