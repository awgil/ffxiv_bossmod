namespace BossMod.Global.Crucible.PasDeSeul;

public enum OID : uint
{
    Boss = 0x4B90, // R3.000, x1
    _Gen_ = 0x4DA2, // R1.000, x1
    Helper = 0x233C, // R0.500, x11, Helper type
    _Gen_SuccubusKnight = 0x4B92, // R1.500, x0 (spawn during fight)
    _Gen_SuccubusMage = 0x4B91, // R1.500, x0 (spawn during fight)
    _Gen_Pheromone = 0x4B93, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 50396, // Boss/4B92->player, no cast, single-target
    _Weaponskill_BloodRain = 46923, // Boss->self, 5.0+1.0s cast, single-target
    _Weaponskill_BloodRain1 = 46924, // Helper->self, 6.0s cast, range 8-40 donut
    _Ability_ = 46931, // Boss->location, no cast, single-target
    _Spell_VoidAeroII = 46932, // Boss->self, 4.0s cast, range 60 width 8 rect
    _Spell_VoidAeroII1 = 46933, // Helper->self, 3.0s cast, range 60 20-degree cone
    _Weaponskill_ColdCaress = 46935, // Boss->player, 5.0s cast, single-target
    _Ability_Summon = 46927, // Boss->self, 4.0s cast, single-target
    _Spell_Aero = 50746, // 4B91->player, no cast, single-target
    _Ability_Fanaticism = 46928, // 4B91->Boss, 6.0s cast, single-target
    _Spell_VoidFireII = 46929, // 4B91->location, 4.0s cast, range 10 circle
    _Weaponskill_SweetSteel = 46930, // 4B92->self, 4.0s cast, range 10 120-degree cone
    _Weaponskill_BloodRain2 = 46925, // Boss->self, 5.4+0.6s cast, single-target
    _Weaponskill_BloodRain3 = 46926, // Helper->self, 6.0s cast, range 8 circle
    _Weaponskill_BloodSword = 46934, // Boss->player, 6.0s cast, single-target
    _Spell_Lifeblood = 49508, // Helper->Boss, no cast, single-target
    _Weaponskill_BeguilingMist = 46936, // Boss->self, 5.0s cast, range 30 circle
    _Weaponskill_HeartShatter = 46937, // 4B93->self, 1.0s cast, single-target
    _Weaponskill_HeartShatter1 = 46938, // Helper->self, 1.0s cast, range 24 circle
}

public enum TetherID : uint
{
    _Gen_Tether_chn_m9994_dark001_r1 = 162, // _Gen_Pheromone->_Gen_Pheromone
}

class BloodRainDonut(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_BloodRain1, new AOEShapeDonut(8, 40));
class BloodRainCircle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_BloodRain3, 8);
class VoidAeroBoss(BossModule module) : Components.StandardAOEs(module, AID._Spell_VoidAeroII, new AOEShapeRect(60, 4));
class VoidAeroOrb(BossModule module) : Components.StandardAOEs(module, AID._Spell_VoidAeroII1, new AOEShapeCone(60, 10.Degrees()));
class ColdCaress(BossModule module) : Components.SingleTargetCast(module, AID._Weaponskill_ColdCaress, "Tankbuster + poison");
class Adds(BossModule module) : Components.AddsMulti(module, [OID._Gen_SuccubusKnight, OID._Gen_SuccubusMage]);
class Fanaticism(BossModule module) : Components.CastHint(module, AID._Ability_Fanaticism, "Boss is being buffed!", true);
class VoidFireII(BossModule module) : Components.StandardAOEs(module, AID._Spell_VoidFireII, 10);
class SweetSteel(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SweetSteel, new AOEShapeCone(10, 60.Degrees()));
class BloodSword(BossModule module) : Components.SingleTargetCast(module, AID._Weaponskill_BloodSword, "Tankbuster + lifesteal");
class BeguilingMist(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_BeguilingMist, "Raidwide + sleep");

class HeartShatter(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_HeartShatter1, new AOEShapeCircle(24))
{
    readonly List<AOEInstance> _predicted = [];

    readonly List<(Actor, ulong)> _pending = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).Concat(_predicted);

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_chn_m9994_dark001_r1)
            _pending.Add((source, tether.Target));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction && _predicted.Count > 0)
            _predicted.RemoveAt(0);
    }

    public override void Update()
    {
        for (var i = _pending.Count - 1; i >= 0; i--)
        {
            if (WorldState.Actors.Find(_pending[i].Item2) is { } target)
            {
                var center = WPos.Lerp(_pending[i].Item1.Position, target.Position, 0.5f);
                _predicted.Add(new(new AOEShapeCircle(24), center, default, WorldState.FutureTime(14.3f)));
            }
            _pending.RemoveAt(i);
        }
    }
}

class PasDeSeulStates : StateMachineBuilder
{
    public PasDeSeulStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BloodRainDonut>()
            .ActivateOnEnter<BloodRainCircle>()
            .ActivateOnEnter<VoidAeroBoss>()
            .ActivateOnEnter<VoidAeroOrb>()
            .ActivateOnEnter<ColdCaress>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<Fanaticism>()
            .ActivateOnEnter<VoidFireII>()
            .ActivateOnEnter<SweetSteel>()
            .ActivateOnEnter<BloodSword>()
            .ActivateOnEnter<BeguilingMist>()
            .ActivateOnEnter<HeartShatter>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1088, NameID = 14541, BitmapType = BossModuleInfo.BitmapType.Enabled)]
public class PasDeSeul(WorldState ws, Actor primary) : BossModule(ws, primary, new(520, -420), new ArenaBoundsRect(20, 24));

