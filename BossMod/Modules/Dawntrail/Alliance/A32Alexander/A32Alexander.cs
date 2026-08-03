namespace BossMod.Dawntrail.Alliance.A32Alexander;

sealed class BanishgaIV(BossModule module) : Components.RaidwideCast(module, (uint)AID.BanishgaIV);
sealed class HolyII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HolyII, new AOEShapeCircle(6f));
sealed class BanishgaIVSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BanishgaIVSpread, 6f);

sealed class ImpartialRuling(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ImpartialRulingShort, (uint)AID.ImpartialRulingLong], new AOEShapeCone(60f, 90f.Degrees()), 1, 1);

sealed class RadiantSacrament(BossModule module) : Components.GenericAOEs(module, (uint)AID.RadiantSacrament1)
{
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count > 0)
        {
            var max = count > 15 ? 15 : count;
            var tiles = new AOEInstance[max];
            for (var i = 0; i < max; i++)
            {
                tiles[i] = _aoes[i];
            }
            return tiles;
        }
        return [];
    }

    public override void OnMapEffect(byte index, uint state) // credit for finding the way these related to the squares goes to Xan. 
    {
        if (index is >= 0x14 and <= 0x2C && state == 0x00020001)
        {
            var ix = index - 0x14;
            var row = ix % 5;
            var col = ix / 5;
            var wd = new WDir(10f * col, 10f * row) + (Arena.Center - new WDir(20f, 20f));
            _aoes.Add(new(new AOEShapeRect(5f, 5f, 5f), wd, default, WorldState.FutureTime(8.1d)));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _aoes.RemoveAt(0);
        }
    }
}

sealed class DivineSpear(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DivineSpear1, new AOEShapeTriCone(25f, 45.Degrees()));

sealed class MegaHoly(BossModule module) : Components.StackWithIcon(module, (uint)IconID.MegaHoly, (uint)AID.MegaHolyCast, 6f, 7f, maxCasts: 3)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    { }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.MegaHolyCast or (uint)AID.MegaHolySpam)
        {
            var count = Stacks.Count;
            var t = spell.MainTargetID;
            var stacks = CollectionsMarshal.AsSpan(Stacks);
            for (var i = 0; i < count; ++i)
            {
                if (stacks[i].Target.InstanceID == t)
                {
                    Stacks.RemoveAt(i);
                    return;
                }
            }
            Stacks.Clear(); // stack was not found, just clear all - this can happen if donut stacks is self targeted instead of player targeted
        }
    }
}

class GordiusSystem(BossModule module) : Components.Adds(module, (uint)OID.GordiusSystem, 1);

sealed class Activate(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Activate, 3f);
class PerfectDefense(BossModule module) : Components.InvincibleStatus(module, (uint)SID.PerfectDefense2);
class HolyFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HolyFlame, 5f);
class Shock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shock, 7f);
class CircuitShock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CircuitShock, new AOEShapeDonut(7, 18));
class DivineJudgment(BossModule module) : Components.RaidwideCast(module, (uint)AID.DivineJudgment);

class Electrify(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _tethers = [];
    private static readonly AOEShapeCircle _circle = new(18f);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _tethers.Count;
        if (count <= 0)
        {
            return [];
        }
        var max = (count > 2) ? 2 : count;
        var _aoes = new AOEInstance[max];
        for (var i = 0; i < max; i++)
        {
            _aoes[i] = _tethers[i];
        }
        return _aoes;
    }
    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Electrify)
            _tethers.Add(new(_circle, source.Position, default, WorldState.FutureTime(9)));
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Electrify)
        {
            NumCasts++;
            _tethers.RemoveAt(0);
        }
    }
}
sealed class DivineBolt(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(60f, 3f), (uint)IconID.DivineBolt, (uint)AID.DivineBolt1, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, HerStolenLight, Some logic borrowed from Xan", PrimaryActorOID = (uint)OID.AlexanderResurrected, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117u, NameID = 14529u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 3)]
public sealed class A32Alexander(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsSquare(25f))
{
    public static readonly WPos ArenaCenter = new(0f, 360f);
}
