namespace BossMod.Dawntrail.Dungeon.D07TenderValley.D074GreatestSerpentOfTural;

public enum OID : uint
{
    Boss = 0x4164, // R4.5
    LesserSerpentOfTural = 0x41DE, // R2.812
    GreatSerpentOfTural = 0x41E0, // R1.152-3.84
    SludgeVoidzone1 = 0x1EBA86, // R0.5
    SludgeVoidzone2 = 0x1EBA87, // R0.5
    SludgeVoidzone3 = 0x1EBA88, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 36747, // Boss->location, no cast, single-target

    DubiousTulidisaster = 36748, // Boss->self, 5.0s cast, range 40 circle

    BouncyCouncil = 36746, // Boss->self, 3.0s cast, single-target, spawns clones

    MisplacedMystery = 36750, // LesserSerpentOfTural->self, 7.0s cast, range 52 width 5 rect
    ExaltedWobble = 36749, // LesserSerpentOfTural->self, 7.0s cast, range 9 circle

    ScreesOfFuryVisual = 36744, // Boss->self, 4.5+0.5s cast, single-target, AOE tankbuster
    ScreesOfFury = 36757, // Helper->player, no cast, range 3 circle 

    GreatestLabyrinth = 36745, // Boss->self, 4.0s cast, range 40 circle

    MoistSummoning = 36743, // Boss->self, 3.0s cast, single-target, spawns great serpent of tural
    MightyBlorpVisual1 = 36753, // GreatSerpentOfTural->self, 4.5+0.5s cast, single-target, stack
    MightyBlorpVisual2 = 36752, // GreatSerpentOfTural->self, 4.5+0.5s cast, single-target, stack
    MightyBlorpVisual3 = 36751, // GreatSerpentOfTural->self, 4.5+0.5s cast, single-target, stack
    MightyBlorp1 = 39983, // GreatSerpentOfTural->players, no cast, range 6 circle
    MightyBlorp2 = 39982, // GreatSerpentOfTural->players, no cast, range 5 circle
    MightyBlorp3 = 39981, // GreatSerpentOfTural->players, no cast, range 4 circle

    GreatestFloodVisual = 36742, // Boss->self, 5.0s cast, single-target
    GreatestFlood = 36756, // Helper->self, 6.0s cast, range 40 circle, knockback 15, away from source

    GreatTorrentVisual = 36741, // Boss->self, 3.0s cast, single-target
    GreatTorrentAOE = 36754, // Helper->location, 6.0s cast, range 6 circle 
    GreatTorrentSpread = 36755 // Helper->player, no cast, range 6 circle
}

public enum IconID : uint
{
    ScreesOfFury = 341, // player
    MightyBlorp1 = 62, // player
    MightyBlorp2 = 542, // player
    MightyBlorp3 = 543, // player
    GreatTorrent = 139 // player
}

sealed class DubiousTulidisasterArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DubiousTulidisaster && Arena.Bounds.Radius > 13f)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, [new Square(center, 14.5f)], [new Square(center, 12f)]);
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell, 4.8d), shapeDistance: shape.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsSquare(12f);
            _aoe = [];
        }
    }
}

sealed class ScreesOfFury(BossModule module) : Components.BaitAwayIcon(module, 3f, (uint)IconID.ScreesOfFury, (uint)AID.ScreesOfFury, 5.3d, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class GreatestFlood(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.GreatestFlood, 15f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
            {
                hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOrigin(Arena.Center, c.Origin, 15f, 11f), act);
            }
        }
    }
}

sealed class GreatestLabyrinth(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(2)];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x01)
        {
            return;
        }

        void AddAOEs(WPos[] pair)
        {
            var center = Arena.Center;
            var p0 = new Square(pair[0], 2f);
            var p1 = new Square(pair[1], 2f);
            var forbiddenShape = new AOEShapeCustom(center, [new Square(center, 12f)], [new Square(center, 4f), p0, p1]);
            var safeShape = new AOEShapeCustom(center, [p0, p1], invertForbiddenZone: true);
            _aoes.Add(new(forbiddenShape, center, shapeDistance: forbiddenShape.Distance(center, default)));
            _aoes.Add(new(safeShape, center, default, WorldState.FutureTime(10d), Colors.SafeFromAOE, shapeDistance: safeShape.Distance(center, default)));
        }
        switch (state)
        {
            case 0x01000080u:
                AddAOEs([new(-124f, -552f), new(-140f, -564f)]);
                break;
            case 0x04000200u:
                AddAOEs([new(-128f, -560f), new(-120f, -544f)]);
                break;
            case 0x10000800u:
                AddAOEs([new(-132f, -548f), new(-120f, -564f)]);
                break;
            case 0x00020001u:
                AddAOEs([new(-136f, -556f), new(-140f, -544f)]);
                break;
            case 0x00100004u or 0x00200004u or 0x00400004u or 0x00080004u:
                _aoes.Clear();
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoes.Count == 0)
        {
            return;
        }
        hints.Add("Walk onto safe square!", !_aoes[1].Check(actor.Position));
    }
}

abstract class MightyBlorp(BossModule module, uint iconID, uint aid, float radius) : Components.StackWithIcon(module, iconID, aid, radius, 4.6f, 4, 4);
sealed class MightyBlorp1(BossModule module) : MightyBlorp(module, (uint)IconID.MightyBlorp1, (uint)AID.MightyBlorp1, 6f);
sealed class MightyBlorp2(BossModule module) : MightyBlorp(module, (uint)IconID.MightyBlorp2, (uint)AID.MightyBlorp2, 5f);
sealed class MightyBlorp3(BossModule module) : MightyBlorp(module, (uint)IconID.MightyBlorp3, (uint)AID.MightyBlorp3, 4f);

abstract class SludgeVoidzone(BossModule module, float radius, uint oid) : Components.Voidzone(module, radius, m => GetVoidzones(m, oid))
{
    private static Actor[] GetVoidzones(BossModule module, uint oid)
    {
        var enemies = module.Enemies(oid);
        if (enemies.Count != 0 && enemies[0].EventState != 7)
            return [enemies[0]];
        return [];
    }
}

sealed class SludgeVoidzone1(BossModule module) : SludgeVoidzone(module, 6f, (uint)OID.SludgeVoidzone1);
sealed class SludgeVoidzone2(BossModule module) : SludgeVoidzone(module, 5f, (uint)OID.SludgeVoidzone2);
sealed class SludgeVoidzone3(BossModule module) : SludgeVoidzone(module, 4f, (uint)OID.SludgeVoidzone3);

sealed class DubiousTulidisasterGreatestLabyrinthFlood(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.DubiousTulidisaster, (uint)AID.GreatestLabyrinth, (uint)AID.GreatestFlood]);
sealed class ExaltedWobble(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExaltedWobble, 9f);
sealed class MisplacedMystery(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MisplacedMystery, new AOEShapeRect(52f, 2.5f));
sealed class GreatTorrent(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GreatTorrentAOE, 6f, 10);
sealed class GreatTorrentSpread(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.GreatTorrent, (uint)AID.GreatTorrentSpread, 6f, 5.1d);

sealed class D074GreatestSerpentOfTuralStates : StateMachineBuilder
{
    public D074GreatestSerpentOfTuralStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DubiousTulidisasterArenaChange>()
            .ActivateOnEnter<DubiousTulidisasterGreatestLabyrinthFlood>()
            .ActivateOnEnter<ScreesOfFury>()
            .ActivateOnEnter<MightyBlorp1>()
            .ActivateOnEnter<MightyBlorp2>()
            .ActivateOnEnter<MightyBlorp3>()
            .ActivateOnEnter<SludgeVoidzone1>()
            .ActivateOnEnter<SludgeVoidzone2>()
            .ActivateOnEnter<SludgeVoidzone3>()
            .ActivateOnEnter<GreatestFlood>()
            .ActivateOnEnter<GreatestLabyrinth>()
            .ActivateOnEnter<ExaltedWobble>()
            .ActivateOnEnter<MisplacedMystery>()
            .ActivateOnEnter<GreatTorrent>()
            .ActivateOnEnter<GreatTorrentSpread>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834u, NameID = 12709u)]
public sealed class D074GreatestSerpentOfTural(WorldState ws, Actor primary) : BossModule(ws, primary, new(-130f, -554f), new ArenaBoundsSquare(14.5f));
