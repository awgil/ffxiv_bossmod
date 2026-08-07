namespace BossMod.Dawntrail.Foray.CriticalEngagement.Pallmagia;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x46, Helper type
    Boss = 0x4D8F, // R3.504, x1
    Pallkeeper = 0x4D90, // R2.300, x4
    Pallmagia = 0x4D91, // R1.000, x1

    RouletteLarge = 0x1EC02B, // R0.500, x0 (spawn during fight), EventObj type
    RouletteSmall = 0x1EC02C, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    DeathWall = 49771, // 4D91->self, no cast, range 20-25 donut
    AutoAttack = 50494, // Boss->player, no cast, single-target
    BadBreathBossCast = 50490, // Boss->self, 4.3+0.7s cast, single-target
    BadBreathBoss = 50491, // Helper->self, 5.0s cast, range 50 100-degree cone
    PlaincrackerBossCast = 50492, // Boss->self, 4.3+0.7s cast, single-target
    PlaincrackerBoss = 50493, // Helper->self, 5.0s cast, range 15 circle
    Summon = 49772, // Boss->self, 3.0s cast, single-target
    // this one is cast for the first set (no swaps)
    EsotericInstructionFirst = 49773, // Boss->self, 13.0s cast, single-target
    // this one is cast for other sets (swaps)
    EsotericInstructionRest = 49774, // Boss->self, 13.0s cast, single-target
    ReversePolarity = 49775, // Boss->self, 5.0s cast, single-target
    BadBreathAddsCast = 49776, // 4D90->self, no cast, single-target
    BadBreathAdds = 49777, // Helper->self, 3.0s cast, range 50 100-degree cone
    PlaincrackerAddsCast = 49778, // 4D90->self, no cast, single-target
    PlaincrackerAdds = 49779, // Helper->self, 3.0s cast, range 30 circle
    GreatWhirlwindCast = 49798, // Boss->self, 4.3+0.7s cast, single-target
    GreatWhirlwind = 50450, // Helper->self, 5.0s cast, ???
    OccultMissileCast = 49795, // Boss->self, 3.3+0.7s cast, single-target
    OccultMissile = 49797, // Helper->location, 4.0s cast, range 6 circle
    LilliputianLyricCast = 49791, // Boss->self, 4.3+0.7s cast, single-target
    LilliputianLyric = 49792, // Helper->self, 5.0s cast, range 40 180-degree cone
    RouletteCast = 49787, // Boss->self, 4.0s cast, single-target
    RouletteSmall = 49788, // Helper->self, no cast, range 5 circle
    RouletteMedium = 49789, // Helper->self, no cast, range 5-12 120-degree donut cone
    RouletteLarge = 49790, // Helper->self, no cast, range 12-20 135?-degree donut cone
    Unk1 = 49799, // Helper->self, 5.0s cast, single-target
    Unk2 = 49784, // 4D90->location, no cast, single-target
    Unk3 = 49785, // 4D90->location, no cast, single-target
    Unk4 = 49786, // 4D90->location, no cast, single-target
    MagicHammerCast = 49793, // Boss->self, 3.0s cast, single-target
    MagicHammer = 49794, // Helper->location, 5.5s cast, range 8 circle
}

public enum SID : uint
{
    Unk2056 = 2056, // none->Boss/4D90, extra=0x485/0x486/0x490
}

public enum TetherID : uint
{
    Instruction = 14, // 4D90->Boss
    Swap = 207, // 4D90->4D90
}

class BadBreathBoss(BossModule module) : Components.StandardAOEs(module, AID.BadBreathBoss, new AOEShapeCone(50, 50.Degrees()));
class PlaincrackerBoss(BossModule module) : Components.StandardAOEs(module, AID.PlaincrackerBoss, 15);
class GreatWhirlwind(BossModule module) : Components.RaidwideCastDelay(module, AID.GreatWhirlwindCast, AID.GreatWhirlwind, 0.8f);

class EsotericInstruction(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEWithSource> _predicted = [];

    public static readonly AOEShapeCircle Circle = new(30);
    public static readonly AOEShapeCone Cone = new(50, 50.Degrees());

    record class AOEWithSource(AOEInstance AOE, Actor Source)
    {
        public AOEInstance AOE = AOE;
    }

    // 0 = first set, no tethers
    // 1 = other sets
    int mode;
    bool draw;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => draw ? _predicted.Select((p, i) => p.AOE with { Color = i == 0 ? ArenaColor.Danger : ArenaColor.AOE, Risky = i == 0 }).Take(2) : [];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var aoe in ActiveAOEs(slot, actor))
            hints.AddForbiddenZone(aoe.Distance, aoe.Activation);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EC02A && state is 0x00100020 or 0x00010002)
        {
            var caster = Module.Enemies(OID.Pallkeeper).FirstOrDefault(k => k.Position.AlmostEqual(actor.Position, 1));
            if (caster == null)
            {
                ReportError($"Pallkeeper missing for telegraph {actor} at {actor.Position}");
                return;
            }

            var delay = mode == 0 ? 18.5f : 25f;

            var activation = WorldState.FutureTime(delay + 1.5f * _predicted.Count);
            _predicted.Add(new(new(state == 0x00010002 ? Cone : Circle, caster.Position, caster.Rotation, activation), caster));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.EsotericInstructionFirst:
                mode = 0;
                draw = true;
                break;
            case AID.EsotericInstructionRest:
                mode = 1;
                draw = false;
                break;
            case AID.BadBreathAdds:
            case AID.PlaincrackerAdds:
                if (_predicted.Count > 0)
                    _predicted[0].AOE = _predicted[0].AOE with { Activation = Module.CastFinishAt(spell) };
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BadBreathAdds or AID.PlaincrackerAdds && _predicted.Count > 0)
        {
            _predicted.RemoveAt(0);
            if (_predicted.Count == 0)
                draw = false;
        }
    }

    int numTethers;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Swap)
        {
            var target = WorldState.Actors.Find(tether.Target)!;

            foreach (var p in _predicted)
            {
                if (p.Source == source)
                    p.AOE = p.AOE with { Origin = target.Position, Rotation = target.Rotation };
                else if (p.Source == target)
                    p.AOE = p.AOE with { Origin = source.Position, Rotation = source.Rotation };
            }

            numTethers++;
            if (numTethers % 2 == 0)
                draw = true;
        }
    }
}

class OccultMissile(BossModule module) : Components.StandardAOEs(module, AID.OccultMissile, 6);
class LilliputianLyric(BossModule module) : Components.StandardAOEs(module, AID.LilliputianLyric, new AOEShapeCone(40, 90.Degrees()));

class Roulette(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    static readonly AOEShapeDonutSector Outer = new(12, 20, 67.5f.Degrees(), 22.5f.Degrees());
    static readonly AOEShapeDonutSector Inner = new(5, 12, 60.Degrees(), -60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var active = false;
        foreach (var p in _predicted)
        {
            active = true;
            yield return p;
        }

        if (active)
            yield return new(new AOEShapeCircle(5), Arena.Center, default, _predicted[0].Activation);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID is OID.RouletteLarge or OID.RouletteSmall && state is 0x00040010 or 0x00040020)
        {
            var dt = WorldState.FutureTime(10);
            var isCW = state == 0x00040020;
            var (shape, diff) = (OID)actor.OID == OID.RouletteSmall ? (Inner, 120.Degrees()) : (Outer, 67.5f.Degrees());

            _predicted.Add(new(shape, Arena.Center, actor.Rotation + diff * (isCW ? -1 : 1), dt));
            _predicted.Add(new(shape, Arena.Center, actor.Rotation + 180.Degrees() + diff * (isCW ? -1 : 1), dt));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RouletteLarge)
            _predicted.Clear();
    }
}

class MagicHammer(BossModule module) : Components.StandardAOEs(module, AID.MagicHammer, 8, 8);

class PallmagiaStates : StateMachineBuilder
{
    public PallmagiaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BadBreathBoss>()
            .ActivateOnEnter<PlaincrackerBoss>()
            .ActivateOnEnter<EsotericInstruction>()
            .ActivateOnEnter<GreatWhirlwind>()
            .ActivateOnEnter<OccultMissile>()
            .ActivateOnEnter<LilliputianLyric>()
            .ActivateOnEnter<Roulette>()
            .ActivateOnEnter<MagicHammer>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14714)]
public class Pallmagia(WorldState ws, Actor primary) : CEModule(ws, primary, new(807, -562), new ArenaBoundsCircle(20));

