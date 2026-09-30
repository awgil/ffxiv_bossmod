namespace BossMod.Global.Crucible.PasDeSeul;

public enum OID : uint
{
    Boss = 0x4B90, // R3.000, x1
    Unk = 0x4DA2, // R1.000, x1
    SuccubusMage = 0x4B91, // R1.500, x0 (spawn during fight)
    SuccubusKnight = 0x4B92, // R1.500, x0 (spawn during fight)
    Pheromone = 0x4B93, // R1.000, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x11, Helper type
}

public enum AID : uint
{
    AutoAttack = 50396, // Boss/4B92->player, no cast, single-target
    BloodRainDonutCast = 46923, // Boss->self, 5.0+1.0s cast, single-target
    BloodRainDonut = 46924, // Helper->self, 6.0s cast, range 8-40 donut
    BloodRainCircleCast = 46925, // Boss->self, 5.4+0.6s cast, single-target
    BloodRainCircle = 46926, // Helper->self, 6.0s cast, range 8 circle
    Jump = 46931, // Boss->location, no cast, single-target
    VoidAeroIIBoss = 46932, // Boss->self, 4.0s cast, range 60 width 8 rect
    VoidAeroIIOrb = 46933, // Helper->self, 3.0s cast, range 60 20-degree cone
    ColdCaress = 46935, // Boss->player, 5.0s cast, single-target
    Summon = 46927, // Boss->self, 4.0s cast, single-target
    AeroAuto = 50746, // 4B91->player, no cast, single-target
    Fanaticism = 46928, // 4B91->Boss, 6.0s cast, single-target
    VoidFireII = 46929, // 4B91->location, 4.0s cast, range 10 circle
    SweetSteel = 46930, // 4B92->self, 4.0s cast, range 10 120-degree cone
    BloodSword = 46934, // Boss->player, 6.0s cast, single-target
    Lifeblood = 49508, // Helper->Boss, no cast, single-target
    BeguilingMist = 46936, // Boss->self, 5.0s cast, range 30 circle
    HeartShatterVisual = 46937, // 4B93->self, 1.0s cast, single-target
    HeartShatter = 46938, // Helper->self, 1.0s cast, range 24 circle
}

public enum TetherID : uint
{
    Pheromone = 162, // _Gen_Pheromone->_Gen_Pheromone
}

class BloodRainDonut(BossModule module) : Components.StandardAOEs(module, AID.BloodRainDonut, new AOEShapeDonut(8, 40));
class BloodRainCircle(BossModule module) : Components.StandardAOEs(module, AID.BloodRainCircle, 8);
class VoidAeroBoss(BossModule module) : Components.StandardAOEs(module, AID.VoidAeroIIBoss, new AOEShapeRect(60, 4));
class VoidAeroOrb(BossModule module) : Components.StandardAOEs(module, AID.VoidAeroIIOrb, new AOEShapeCone(60, 10.Degrees()));
class ColdCaress(BossModule module) : Components.SingleTargetCast(module, AID.ColdCaress, "Tankbuster + poison");
class Adds(BossModule module) : Components.AddsMulti(module, [OID.SuccubusKnight, OID.SuccubusMage]);
class Fanaticism(BossModule module) : Components.CastInterruptHint(module, AID.Fanaticism);
class VoidFireII(BossModule module) : Components.StandardAOEs(module, AID.VoidFireII, 10);
class SweetSteel(BossModule module) : Components.StandardAOEs(module, AID.SweetSteel, new AOEShapeCone(10, 60.Degrees()));
class BloodSword(BossModule module) : Components.SingleTargetCast(module, AID.BloodSword, "Tankbuster + lifesteal");
class BeguilingMist(BossModule module) : Components.RaidwideCast(module, AID.BeguilingMist, "Raidwide + sleep");

class HeartShatter(BossModule module) : Components.StandardAOEs(module, AID.HeartShatter, new AOEShapeCircle(24))
{
    readonly List<AOEInstance> _predicted = [];

    readonly List<(Actor, ulong)> _pending = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).Concat(_predicted);

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Pheromone)
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

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1088, NameID = 14541, BitmapType = BossModuleInfo.BitmapType.Enabled)]
public class PasDeSeul(ModuleInit init) : BossModule(init, new(520, -420), new ArenaBoundsRect(20, 24));

