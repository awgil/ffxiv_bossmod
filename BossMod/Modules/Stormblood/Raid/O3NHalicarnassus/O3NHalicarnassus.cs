namespace BossMod.Stormblood.Raid.O3NHalicarnassus;
public enum OID : uint
{
    Boss = 0x1E14, // R0.5
    SoulReaper = 0x1E15, // R2.0
    GreatDragon = 0x1E16, // R6.0
    Helper = 0x186D
}

public enum AID : uint
{
    AutoAttackDragon = 970, // GreatDragon->player, no cast, single-target
    AutoAttack = 871, // Boss->player, no cast, single-target

    SpellbladeHoly = 8943, // Boss->self, 3.0s cast, single-target
    SpellbladeBlizzardIII = 9313, // Boss->self, 3.0s cast, range 6, point blank AOE
    SpellbladeFireIII = 9312, // Boss->self, 3.0s cast, range ?-15 AOE Donut
    SpellbladeThunderIII = 9314, // Boss->self, 2.5s cast, range 60, width 6, frontal line AOE
    HolyBlur = 9316, //Helper->player, no cast, range 6
    Ribbit = 9318, //Boss->none, 4.0s cast, range 17, 95-degree cone, frontal AOE cone

    TheQueensWaltz1 = 9327, // Boss->self, 3.0s cast
    SwordDance = 9328, // Helper->self, 3.0s cast, range 34, 20-degree cone
    TheQueensWaltz2 = 9329, // Boss->self, 5.0s cast
    EarthlyDance = 9330, // Helper->self, no cast, range 80
    Uplift = 9331, // Helper->self, no cast, range 80

    ThePlayingField = 8960, // Boss->self, 3.0s cast
    PanelSwap = 8964, // Boss->self, 3.0s cast
    TheGame = 9325, // Boss->self, 5.0s cast, range 80

    PlaceToken = 8955, // Boss->self, 3.0s cast, spawns GreatDragon
    PlaceDarkToken = 8956, // Boss->self 3.0s cast, spawns SoulReapers
    CrossReaper = 9323, // SoulReaper->self, 8.5s cast, range8, point blank AOE
    FrostBreath = 9334, // GreatDragon->self, no cast, range 15, 8.5-degree cone, frontal cone AoE
}

class Ribbit(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Ribbit), new AOEShapeCone(19.5f, 75.Degrees()));
class SpellbladeThunderIII(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SpellbladeThunderIII), new AOEShapeRect(60, 3));
class SpellbladeBlizzardIII(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SpellbladeBlizzardIII), new AOEShapeCircle(9));
class SpellbladeFireIII(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SpellbladeFireIII), new AOEShapeDonut(4, 15));
class CrossReaper(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CrossReaper), new AOEShapeCircle(10));
class TheQueensWaltz1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TheQueensWaltz1), new AOEShapeCone(34, 10.Degrees()));
class TheQueensWaltz2(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            var aoeCount = 12;
            for (var i = aoeCount; i < _aoes.Count; i++)
                yield return _aoes[i];
            for (var i = 0; i < aoeCount; i++)
                yield return _aoes[i] with { Color = ArenaColor.AOE };
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TheQueensWaltz2)
        {
            int[] xOffsets = [-15, -5, 5, 15];
            int[] zOffsets = [-15, -5, 5, 15];

            foreach (var zOffset in zOffsets)
            {
                foreach (var xOffset in xOffsets)
                {
                    if (xOffset != zOffset)
                    {
                        _aoes.Add(new(rect, new(xOffset, zOffset)));
                    }
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.TheQueensWaltz2)
        {
            for (var i = 0; i < 12; i++)
                _aoes.RemoveAt(0);
        }
    }
}

class TheGame(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            var safeTiles = new List<int>();

            switch (actor.Role)
            {
                case Role.Tank:
                    safeTiles.Add(6);
                    break;
                case Role.Healer:
                    safeTiles.Add(9);
                    break;
                case Role.Melee:
                case Role.Ranged:
                    safeTiles.Add(5);
                    safeTiles.Add(10);
                    break;
            }

            var tiles = 16;
            for (var i = tiles; i < _aoes.Count; i++)
                yield return _aoes[i];

            for (var i = 0; i < tiles; i++)
            {
                if (!safeTiles.Contains(i))
                {
                    yield return _aoes[i] with { Color = ArenaColor.AOE };
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TheGame)
        {
            int[] xOffsets = [-15, -5, 5, 15];
            int[] zOffsets = [-15, -5, 5, 15];

            foreach (var zOffset in zOffsets)
            {
                foreach (var xOffset in xOffsets)
                {
                    _aoes.Add(new(rect, new(xOffset, zOffset)));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.TheGame)
        {
            for (var i = 0; i < 16; i++)
                _aoes.RemoveAt(0);
        }
    }
}
class O3NHalicarnassusStates : StateMachineBuilder
{
    public O3NHalicarnassusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Ribbit>()
            .ActivateOnEnter<SpellbladeThunderIII>()
            .ActivateOnEnter<SpellbladeBlizzardIII>()
            .ActivateOnEnter<SpellbladeFireIII>()
            .ActivateOnEnter<CrossReaper>()
            .ActivateOnEnter<TheQueensWaltz1>()
            .ActivateOnEnter<TheQueensWaltz2>()
            .ActivateOnEnter<TheGame>();

    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 254, NameID = 5633)]
public class O3NHalicarnassus(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsSquare(20));
