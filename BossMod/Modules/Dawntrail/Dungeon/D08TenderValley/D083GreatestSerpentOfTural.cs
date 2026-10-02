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
    MightyBlorp1 = 39981,
    MightyBlorp2 = 39982,
    MightyBlorp3 = 39983
}

class DubiousTulidisaster(BossModule module) : Components.RaidwideCast(module, AID.DubiousTulidisaster);
class GreatestLabyrinth(BossModule module) : BossComponent(module)
{
    private static readonly Dictionary<uint, WPos> _tiles = new()
    {
        [0x04000200] = new(-128.0483f, -560.11017f),
        [0x10000800] = new(-131.8944f, -547.8202f),
        [0x01000080] = new(-124.125694f, -551.9294f),
        [0x00020001] = new(-135.9524f, -560.0384f),
    };

    private BitMask _cursed;
    private WPos? _tile;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_tile is { } tile && _cursed[slot] && !actor.Position.InRect(tile, default(Angle), 2, 2, 2))
            hints.Add("Go to marked tile!");
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (_tile is { } tile && _cursed[slot])
            movementHints.Add(actor.Position, tile, ArenaColor.Safe);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_tile is { } tile && _cursed[slot])
            hints.GoalZones.Add(p => p.InRect(tile, default(Angle), 2, 2, 2) ? 10 : 0);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_tile is { } tile && _cursed[pcSlot])
            Arena.ZoneRect(tile, default(Angle), 2, 2, 2, ArenaColor.SafeFromAOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_tile is { } tile && _cursed[pcSlot])
        {
            Arena.AddRect(tile, new WDir(1, 0), 2, 2, 2, ArenaColor.Safe, 2);
            Arena.AddLine(pc.Position, tile, ArenaColor.Safe);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.GreatestCurse && Raid.TryFindSlot(actor, out var slot))
            _cursed.Set(slot);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.GreatestCurse && Raid.TryFindSlot(actor, out var slot))
        {
            _cursed.Clear(slot);
            if (_cursed.None())
                _tile = null;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x01 && _tiles.TryGetValue(state, out var tile))
            _tile = tile;
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
class MightyBlorpPuddles(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.MightyBlorpPuddle1).Concat(m.Enemies(OID.MightyBlorpPuddle2)).Concat(m.Enemies(OID.MightyBlorpPuddle3)));

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
            .ActivateOnEnter<MightyBlorpPuddles>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "CerQ", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12709)]
public class D083GreatestSerpentOfTural(WorldState ws, Actor primary) : BossModule(ws, primary, new(-130, -554), new ArenaBoundsSquare(12));
