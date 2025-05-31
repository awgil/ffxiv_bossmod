namespace BossMod.Dawntrail.Foray.CriticalEngagement.CrescentBerserker;

public enum OID : uint
{
    Boss = 0x4703, // R4.500, x1
    Helper = 0x233C, // R0.500, x18, Helper type
    DeathWallHelper = 0x4871, // R0.500, x1
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    BoilOverCast = 30788, // Boss->self, 5.0s cast, single-target
    BoilOver = 30789, // Helper->self, 6.0s cast, ???
    Torch = 37808, // DeathWallHelper->self, no cast, range 25?-30 donut, death wall
    ScathingSweep = 42691, // Boss->self, 6.0s cast, range 60 width 60 rect
    ChanneledRageCast = 30790, // Boss->self, 3.0s cast, single-target
    ChanneledRage = 30791, // Helper->self, 3.5s cast, ???
    HeightenedRageCast = 37809, // Boss->self, 3.0s cast, single-target
    HeightenedRage = 37815, // Helper->self, 3.5s cast, ???
    WhiteHotRage = 37800, // Boss->location, 4.0s cast, range 6 circle
    HeatedOutburst = 37804, // Helper->self, 6.0s cast, range 13 circle

    HoppingMadCast1 = 37306, // Boss->location, 7.0s cast, single-target
    HoppingMadCast2 = 30792, // Boss->location, 8.0s cast, single-target
    HoppingMadSingle = 37323, // Helper->location, 8.0s cast, range 8 circle
    BedrockUpliftLarge = 37805, // Helper->self, 1.5s cast, range 24-60 donut
    BedrockUpliftMedium = 37806, // Helper->self, 1.5s cast, range 16-60 donut
    BedrockUpliftSmall = 37807, // Helper->self, 1.5s cast, range 8-60 donut
    HoppingMadJump1 = 30870, // Boss->location, no cast, single-target
    HoppingMadJump2 = 30871, // Boss->location, no cast, single-target
    HoppingMadLarge = 30872, // Helper->self, 9.0s cast, range 24 circle
    HoppingMadMedium = 30873, // Helper->self, 15.5s cast, range 16 circle
    HoppingMadSmall = 37041, // Helper->self, 22.5s cast, range 8 circle
}

class BoilOver(BossModule module) : Components.RaidwideCast(module, AID.BoilOverCast);
class HeightenedRage(BossModule module) : Components.RaidwideCast(module, AID.HeightenedRageCast);
class ChanneledRage(BossModule module) : Components.RaidwideCast(module, AID.ChanneledRageCast);
class ScathingSweep(BossModule module) : Components.StandardAOEs(module, AID.ScathingSweep, new AOEShapeRect(60, 30));
class WhiteHotRage(BossModule module) : Components.StandardAOEs(module, AID.WhiteHotRage, 6);
class HeatedOutburst(BossModule module) : Components.StandardAOEs(module, AID.HeatedOutburst, 13)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        DateTime first = default;
        var aoes = base.ActiveAOEs(slot, actor).GetEnumerator();
        while (aoes.MoveNext())
        {
            var cur = aoes.Current;
            if (first == default)
                first = cur.Activation.AddSeconds(1);

            if (cur.Activation < first)
                yield return cur;
        }
    }
}

class HoppingMad(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HoppingMadSingle:
            case AID.HoppingMadSmall:
                Predict(spell, 8);
                break;
            case AID.HoppingMadMedium:
                Predict(spell, 16);
                break;
            case AID.HoppingMadLarge:
                Predict(spell, 24);
                break;
        }
    }

    private void Predict(ActorCastInfo spell, float radius)
    {
        _predicted.Add(new(new AOEShapeCircle(radius), spell.LocXZ, Activation: Module.CastFinishAt(spell)));
        _predicted.Add(new(new AOEShapeDonut(radius, 60), spell.LocXZ, Activation: Module.CastFinishAt(spell, 2.1f)));

        _predicted.SortBy(p => p.Activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HoppingMadSingle or AID.HoppingMadSmall or AID.HoppingMadMedium or AID.HoppingMadLarge)
        {
            NumCasts++;
            if (_predicted.Count > 0 && _predicted[0].Shape is AOEShapeCircle)
                _predicted.RemoveAt(0);
        }

        if ((AID)spell.Action.ID is AID.BedrockUpliftSmall or AID.BedrockUpliftLarge or AID.BedrockUpliftMedium)
        {
            NumCasts++;
            if (_predicted.Count > 0 && _predicted[0].Shape is AOEShapeDonut)
                _predicted.RemoveAt(0);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_predicted.Count > 0 && _predicted[0].Shape is AOEShapeCircle circle)
            if (!actor.Position.InCircle(_predicted[0].Origin, circle.Radius + 6))
                hints.Add("Stay close to puddle!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_predicted.Count > 0 && _predicted[0].Shape is AOEShapeCircle circle)
            hints.AddForbiddenZone(ShapeContains.Donut(_predicted[0].Origin, circle.Radius + 2, 60), _predicted[0].Activation.AddSeconds(2));
    }
}

class CrescentBerserkerStates : StateMachineBuilder
{
    public CrescentBerserkerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BoilOver>()
            .ActivateOnEnter<HeightenedRage>()
            .ActivateOnEnter<ChanneledRage>()
            .ActivateOnEnter<ScathingSweep>()
            .ActivateOnEnter<HeatedOutburst>()
            .ActivateOnEnter<WhiteHotRage>()
            .ActivateOnEnter<HoppingMad>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13759)]
public class CrescentBerserker(WorldState ws, Actor primary) : BossModule(ws, primary, new(620, 800), new ArenaBoundsCircle(24.5f))
{
    public override bool DrawAllPlayers => true;
}
