namespace BossMod.Global.Crucible.SphinxPiece;

public enum OID : uint
{
    Boss = 0x4CFE, // R5.300, x1
    Helper = 0x233C, // R0.500, x8, Helper type
    _Gen_DodoPiece = 0x4CFF, // R0.600-1.620, x0 (spawn during fight)
    _Gen_PugilPiece = 0x4D00, // R0.600-1.950, x0 (spawn during fight)
    _Gen_OpoOpoPiece = 0x4D01, // R0.600-1.400, x0 (spawn during fight)
    _Gen_PukPiece = 0x4D02, // R0.600-1.500, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 49356, // Boss->player, no cast, single-target
    _Weaponskill_Banish = 49340, // Boss->self, 5.0s cast, single-target
    _Weaponskill_Banish1 = 49341, // Helper->self, 6.0s cast, range 60 180-degree cone
    _Weaponskill_NumericRiddle = 49352, // Boss->self, 4.0s cast, single-target
    _Weaponskill_NumericRiddle1 = 49353, // Helper->self, 5.0s cast, range 40 circle
    _Weaponskill_Assignment = 49354, // Boss->self, 3.0s cast, single-target
    _Ability_ = 49357, // Boss->location, no cast, single-target
    _Weaponskill_MnemonicRiddle = 49343, // Boss->self, 4.0s cast, single-target
    _Weaponskill_MnemonicRiddle1 = 49344, // Helper->self, 5.0s cast, range 40 circle
    _Weaponskill_Transfigure = 49345, // Boss->self, 2.0s cast, single-target
    _Ability_1 = 49346, // 4CFF/4D01/4D02/4D00->location, no cast, single-target
    _Ability_2 = 49347, // 4CFF/4D00/4D01/4D02->self, no cast, single-target
    _Weaponskill_RiddleSolved = 49348, // Boss->self, 2.0s cast, single-target
    _Weaponskill_RiddleSolved1 = 49349, // Helper->self, 3.0s cast, range 50 circle
    _Weaponskill_Banish2 = 49334, // Boss->self, 5.0s cast, single-target
    _Weaponskill_Banish3 = 49335, // Helper->self, 6.0s cast, range 10-60 donut
    _Weaponskill_Banish4 = 49338, // Boss->self, 5.0s cast, single-target
    _Weaponskill_Banish5 = 49339, // Helper->self, 6.0s cast, range 60 180-degree cone
    _Weaponskill_LostHope = 49342, // Boss->player, 3.0s cast, single-target
    _Weaponskill_Banish6 = 49336, // Boss->self, 5.0s cast, single-target
    _Weaponskill_Banish7 = 49337, // Helper->self, 6.0s cast, range 18 circle
}

public enum SID : uint
{
    _Gen_VulnerabilityDown = 2198, // Boss->Boss, extra=0x0
    _Gen_Transfiguration = 1433, // none->4CFF/4D01/4D02/4D00, extra=0x172
    _Gen_TemporaryMisdirection = 3909, // Boss->player, extra=0x2D0
    _Gen_DamageUp = 5147, // Helper->player, extra=0x0
    _Gen_AllEvens = 5148, // none->player, extra=0x0
    _Gen_AllOdds = 5149, // none->player, extra=0x0
    _Gen_AllPrime = 5150, // none->player, extra=0x0
    _Gen_AllThree = 5151, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_m0146_count3_t0j2 = 718, // player->self
    _Gen_Icon_m0489trg_a0c = 503, // player->self
}

class NumericRiddle(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_NumericRiddle1);
class BanishSide(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_Banish1, AID._Weaponskill_Banish5], new AOEShapeCone(60, 90.Degrees()));
class BanishCircle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Banish7, 18);
class BanishRing(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Banish3, new AOEShapeDonut(10, 60));

class VulnerabilityDown(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.PrimaryActor.FindStatus(SID._Gen_VulnerabilityDown) != null)
            hints.SetPriority(Module.PrimaryActor, AIHints.Enemy.PriorityPointless);
    }
}

class Assignment(BossModule module) : BossComponent(module)
{
    enum Ass
    {
        Even,
        Odd,
        Prime,
        Three
    }

    readonly List<(Ass, DateTime)> _assignments = [];

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        Ass? a = (SID)status.ID switch
        {
            SID._Gen_AllEvens => Ass.Even,
            SID._Gen_AllOdds => Ass.Odd,
            SID._Gen_AllPrime => Ass.Prime,
            SID._Gen_AllThree => Ass.Three,
            _ => null
        };

        if (a.HasValue)
        {
            _assignments.Add((a.Value, status.ExpireAt));
            _assignments.SortBy(a => a.Item2);
        }
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        Ass? a = (SID)status.ID switch
        {
            SID._Gen_AllEvens => Ass.Even,
            SID._Gen_AllOdds => Ass.Odd,
            SID._Gen_AllPrime => Ass.Prime,
            SID._Gen_AllThree => Ass.Three,
            _ => null
        };

        if (a.HasValue)
        {
            var index = _assignments.FindIndex(b => b.Item1 == a.Value);
            if (index >= 0)
                _assignments.RemoveAt(index);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var (p, _) in _assignments.Take(1))
            hints.Add($"Stand on correct tile!", !WorldState.Actors.Any(t => Check(t, p) && actor.Position.AlmostEqual(t.Position, 6)));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (p, a) in _assignments.Take(1))
        {
            var tiles = WorldState.Actors.Where(t => Check(t, p)).Select(t => ShapeDistance.InvertedRect(t.Position, new WDir(0, 1), 6, 6, 6)).ToList();
            if (tiles.Count > 0)
                hints.AddForbiddenZone(ShapeDistance.Intersection(tiles), a);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (p, _) in _assignments.Take(1))
            foreach (var tile in WorldState.Actors.Where(t => Check(t, p)))
                Arena.ZoneRect(tile.Position, new WDir(0, 1), 6, 6, 6, ArenaColor.SafeFromAOE);
    }

    static bool IsTile(Actor tile) => tile.OID is >= 0x1EC105 and <= 0x1EC10D;

    bool Check(Actor tile, Ass a)
    {
        if (!IsTile(tile))
            return false;

        var val = tile.OID - 0x1EC104;

        return a switch
        {
            Ass.Odd => val % 2 == 1,
            Ass.Even => val % 2 == 0,
            Ass.Prime => val is 2 or 3 or 5 or 7,
            Ass.Three => val % 3 == 0,
            _ => false,
        };
    }
}

class MnemonicRiddle(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_MnemonicRiddle1);

class BeastShuffle(BossModule module) : BossComponent(module)
{
    uint _targetID;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_targetID > 0 && WorldState.Actors.FirstOrDefault(a => a.OID == _targetID) is { } target)
            Arena.AddCircle(target.Position, 1, ArenaColor.Safe);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_targetID == 0)
            return;

        if (WorldState.Actors.FirstOrDefault(a => a.OID == _targetID) is { } target)
        {
            hints.GoalZones.Add(AIHints.GoalSingleTarget(target.Position, 3, 0.1f));
            hints.InteractWithTarget = WorldState.Actors.Where(a => a.OID == 0x1EC0E3).Closest(target.Position);
        }
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x80000027)
        {
            switch (param1)
            {
                case 3:
                    _targetID = (uint)OID._Gen_DodoPiece;
                    break;
                case 4:
                    _targetID = (uint)OID._Gen_PukPiece;
                    break;
                case 5:
                    _targetID = (uint)OID._Gen_OpoOpoPiece;
                    break;
                case 6:
                    _targetID = (uint)OID._Gen_PukPiece;
                    break;
            }
        }
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_Transfiguration)
            _targetID = 0;
    }
}

class SphinxPieceStates : StateMachineBuilder
{
    public SphinxPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NumericRiddle>()
            .ActivateOnEnter<BanishSide>()
            .ActivateOnEnter<BanishCircle>()
            .ActivateOnEnter<BanishRing>()
            .ActivateOnEnter<VulnerabilityDown>()
            .ActivateOnEnter<Assignment>()
            .ActivateOnEnter<MnemonicRiddle>()
            .ActivateOnEnter<BeastShuffle>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1092, NameID = 14665)]
public class SphinxPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsSquare(20));

