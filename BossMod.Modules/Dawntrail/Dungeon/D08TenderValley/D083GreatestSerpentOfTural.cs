namespace BossMod.Dawntrail.Dungeon.D08TenderValley.D083GreatestSerpentOfTural;

public enum OID : uint
{
    Boss = 0x4164,
    LesserSerpentOfTural = 0x41DE,
    GreatSerpentOfTural = 0x41E0,
    MightyBlorpPuddle1 = 0x1EBA86,
    MightyBlorpPuddle2 = 0x1EBA87,
    MightyBlorpPuddle3 = 0x1EBA88,
    Helper = 0x233C
}

public enum SID : uint
{
    Bind = 2518,
    GreatestCurse = 3825
}

public enum AID : uint
{
    AutoAttack = 872,

    GreatTorrentVisual = 36741,
    GreatestFloodVisual = 36742,
    MoistSummoning = 36743,
    ScreesOfFuryVisual = 36744,
    GreatestLabyrinth = 36745,
    BouncyCouncil = 36746,
    BouncyCouncilResolve = 36747,
    DubiousTulidisaster = 36748,
    ExaltedWobble = 36749,
    MisplacedMystery = 36750,
    MightyBlorpVisual1 = 36751,
    MightyBlorpVisual2 = 36752,
    MightyBlorpVisual3 = 36753,
    GreatTorrentAOE = 36754,
    GreatTorrentSpread = 36755,
    GreatestFloodAOE = 36756,
    ScreesOfFury = 36757,
    MightyBlorp1 = 39981, // Helper->player, no cast, range 4 circle stack (third)
    MightyBlorp2 = 39982, // Helper->player, no cast, range 5 circle stack (second)
    MightyBlorp3 = 39983 // Helper->player, no cast, range 6 circle stack (first)
}

public enum IconID : uint
{
    MightyBlorpFirst = 62, // player
    MightyBlorpSecond = 542, // player
    MightyBlorpThird = 543, // player
}

class MightyBlorpStack1(BossModule module) : Components.StackWithIcon(module, (uint)IconID.MightyBlorpFirst, AID.MightyBlorp3, 6, 4.7f);
class MightyBlorpStack2(BossModule module) : Components.StackWithIcon(module, (uint)IconID.MightyBlorpSecond, AID.MightyBlorp2, 5, 4.7f);
class MightyBlorpStack3(BossModule module) : Components.StackWithIcon(module, (uint)IconID.MightyBlorpThird, AID.MightyBlorp1, 4, 4.7f);
class DubiousTulidisaster(BossModule module) : Components.RaidwideCast(module, AID.DubiousTulidisaster);
class GreatestLabyrinth(BossModule module) : BossComponent(module)
{
    private static readonly Dictionary<uint, WPos> _entries = new()
    {
        [0x04000200] = new(-128, -560),
        [0x10000800] = new(-132, -548),
        [0x01000080] = new(-124, -552),
        [0x00020001] = new(-136, -556),
    };
    private const float TileHalf = 2;
    private const float MiddleHalf = 4;

    private BitMask _cursed;
    private WPos? _entry;

    private bool OnFoot(WPos p) => p.InRect(Module.Center, default(Angle), MiddleHalf, MiddleHalf, MiddleHalf) || _entry is { } e && p.InRect(e, default(Angle), TileHalf, TileHalf, TileHalf);

    private WPos TileAt(WPos p)
    {
        var corner = Module.Center - new WDir(12, 12);
        static float snap(float v) => Math.Clamp(MathF.Floor(v / (2 * TileHalf)), 0, 5) * 2 * TileHalf + TileHalf;
        return corner + new WDir(snap(p.X - corner.X), snap(p.Z - corner.Z));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_entry == null)
            return;
        if (_cursed[slot] && OnFoot(actor.Position))
            hints.Add("Step on the marked arrow!");
        else if (!_cursed[slot])
            hints.Add("Stay put until the maze is gone!", false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_entry is not { } entry)
            return;
        var middle = ShapeDistance.Rect(Module.Center, new WDir(0, 1), MiddleHalf, MiddleHalf, MiddleHalf);
        if (!_cursed[slot])
        {
            hints.AddForbiddenZone(actor.Position.InRect(Module.Center, default(Angle), MiddleHalf, MiddleHalf, MiddleHalf)
                ? ShapeDistance.InvertedRect(Module.Center, new WDir(0, 1), MiddleHalf, MiddleHalf, MiddleHalf)
                : ShapeDistance.InvertedRect(TileAt(actor.Position), new WDir(0, 1), TileHalf, TileHalf, TileHalf));
            return;
        }
        if (!OnFoot(actor.Position))
            return;
        var tile = ShapeDistance.Rect(entry, new WDir(0, 1), TileHalf, TileHalf, TileHalf);
        hints.AddForbiddenZone(p => -MathF.Min(middle(p), tile(p)));
        hints.AddForbiddenZone(middle, WorldState.FutureTime(1));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_entry is { } entry && _cursed[pcSlot])
            Arena.ZoneRect(entry, new WDir(0, 1), TileHalf, TileHalf, TileHalf, ArenaColor.SafeFromAOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_entry is { } entry && _cursed[pcSlot] && OnFoot(pc.Position))
        {
            Arena.AddRect(entry, new WDir(0, 1), TileHalf, TileHalf, TileHalf, ArenaColor.Safe, 2);
            Arena.AddLine(pc.Position, entry, ArenaColor.Safe);
        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.GreatestCurse && Raid.TryFindSlot(actor, out var slot))
            _cursed.Set(slot);
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.GreatestCurse && Raid.TryFindSlot(actor, out var slot))
        {
            _cursed.Clear(slot);
            if (_cursed.None())
                _entry = null;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x01 && _entries.TryGetValue(state, out var entry))
            _entry = entry;
    }
}

class MoistSummoning(BossModule module) : Components.CastHint(module, AID.MoistSummoning, "Summon jumps + puddles");
class ScreesOfFury(BossModule module) : Components.SingleTargetCastDelay(module, AID.ScreesOfFuryVisual, AID.ScreesOfFury, 0.6f);
class GreatTorrentAOEs(BossModule module) : Components.StandardAOEs(module, AID.GreatTorrentAOE, 6);
class GreatTorrentSpread(BossModule module) : Components.UniformStackSpread(module, 0, 6, alwaysShowSpreads: true)
{
    private int _numAOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GreatTorrentVisual)
        {
            _numAOEs = 0;
            Spreads.Clear();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.GreatTorrentAOE:
                if (++_numAOEs == 12)
                    AddSpreads(Raid.WithoutSlot(true), WorldState.FutureTime(2.8f));
                break;
            case AID.GreatTorrentSpread:
                Spreads.Clear();
                break;
        }
    }
}
class ExaltedWobble(BossModule module) : Components.StandardAOEs(module, AID.ExaltedWobble, 9);
class MisplacedMystery(BossModule module) : Components.StandardAOEs(module, AID.MisplacedMystery, new AOEShapeRect(52, 2.5f));
class GreatestFlood(BossModule module) : Components.KnockbackFromCastTarget(module, AID.GreatestFloodAOE, 20, shape: new AOEShapeCircle(40));
class MightyBlorpPuddles1(BossModule module) : Components.Voidzone(module, 6, (uint)OID.MightyBlorpPuddle1);
class MightyBlorpPuddles2(BossModule module) : Components.Voidzone(module, 6, (uint)OID.MightyBlorpPuddle2);
class MightyBlorpPuddles3(BossModule module) : Components.Voidzone(module, 6, (uint)OID.MightyBlorpPuddle3);

class D083GreatestSerpentOfTuralStates : StateMachineBuilder
{
    public D083GreatestSerpentOfTuralStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DubiousTulidisaster>()
            .ActivateOnEnter<GreatestLabyrinth>()
            .ActivateOnEnter<MoistSummoning>()
            .ActivateOnEnter<ScreesOfFury>()
            .ActivateOnEnter<GreatTorrentAOEs>()
            .ActivateOnEnter<GreatTorrentSpread>()
            .ActivateOnEnter<ExaltedWobble>()
            .ActivateOnEnter<MisplacedMystery>()
            .ActivateOnEnter<GreatestFlood>()
            .ActivateOnEnter<MightyBlorpStack1>()
            .ActivateOnEnter<MightyBlorpStack2>()
            .ActivateOnEnter<MightyBlorpStack3>()
            .ActivateOnEnter<MightyBlorpPuddles1>()
            .ActivateOnEnter<MightyBlorpPuddles2>()
            .ActivateOnEnter<MightyBlorpPuddles3>();
    }
}

[ModuleInfo(Contributors = "CerQ, Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12709)]
public class D083GreatestSerpentOfTural(ModuleInit init) : BossModule(init, new(-130, -554), new ArenaBoundsSquare(12));
