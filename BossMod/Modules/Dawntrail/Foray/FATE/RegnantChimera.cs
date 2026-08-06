namespace BossMod.Dawntrail.Foray.FATE.RegnantChimera;

public enum OID : uint
{
    Boss = 0x4C7D,
    Helper = 0x233C,
    GlacipotentOrb = 0x4C80, // R2.000, x0 (spawn during fight)
    FulmipotentOrb = 0x4C7F, // R2.000, x0 (spawn during fight)
    Cacophony = 0x4B71, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50856, // Boss->player, no cast, single-target

    TheRamsBreath = 48631, // Boss->self, 6.0s cast, range 30 120-degree cone
    TheRamsBreath1 = 48632, // Boss->self, no cast, range 30 120-degree cone
    TheRamsBreath2 = 49748, // Boss->self, no cast, range 30 120-degree cone
    TheDragonsBreath = 48629, // Boss->self, 6.0s cast, range 30 120-degree cone
    TheDragonsBreath1 = 48630, // Boss->self, no cast, range 30 120-degree cone
    TheDragonsBreath2 = 49747, // Boss->self, no cast, range 30 120-degree cone

    TheRamsVoice = 48633, // Boss->self, 4.0s cast, range 9 circle
    TheRamsVoice1 = 48635, // 4C80->location, 1.0s cast, range 12 circle
    TheDragonsVoice = 48634, // Boss->self, 4.0s cast, range 8-30 donut
    TheDragonsVoice1 = 48636, // 4C7F->location, 4.0s cast, range 8-30 donut

    Cacophony = 50113, // Boss->self, 4.0s cast, single-target
    ChaoticChorus = 50114, // 4B71->self, 1.5s cast, range 6 circle

    LeftDuobreath = 50111, // Boss->self, 5.0s cast, range 40 180-degree cone
    TheRamsBreath3 = 50116, // Boss->self, no cast, range 40 180-degree cone
    RightDuobreath = 50112, // Boss->self, 5.0s cast, range 40 180-degree cone
    TheDragonsBreath3 = 50115, // Boss->self, no cast, range 40 180-degree cone
}

public enum SID : uint
{
    Gen = 5196, // Boss/4C80->4C80/Boss, extra=0x0
    Gen1 = 5197, // Boss/4C7F->4C7F/Boss, extra=0x0
}

public enum IconID : uint
{
    TurnLeft = 547, // Boss->self
    TurnRight = 546, // Boss->self
}

class TheRamsVoice(BossModule module) : Components.StandardAOEs(module, AID.TheRamsVoice, 9, highlightImminent: true);
class TheDragonsVoice(BossModule module) : Components.GroupedAOEs(module, [AID.TheDragonsVoice, AID.TheDragonsVoice1], new AOEShapeDonut(8, 30));

class Breath(BossModule module) : Components.GenericRotatingAOE(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TheRamsBreath:
                Sequences.Add(new(new AOEShapeCone(30, 60.Degrees()), spell.LocXZ, spell.Rotation, 120.Degrees(), Module.CastFinishAt(spell), 2.7f, 3));
                break;
            case AID.TheDragonsBreath:
                Sequences.Add(new(new AOEShapeCone(30, 60.Degrees()), spell.LocXZ, spell.Rotation, -120.Degrees(), Module.CastFinishAt(spell), 2.7f, 3));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TheRamsBreath or AID.TheRamsBreath1 or AID.TheRamsBreath2 or AID.TheDragonsBreath or AID.TheDragonsBreath1 or AID.TheDragonsBreath2 && Sequences.Count > 0)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}

class GlacipotentOrb(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_predicted.Count == 0)
            yield break;

        var first = _predicted[0].Activation.AddSeconds(0.5f);

        foreach (var aoe in _predicted.Where(a => a.Activation > first && a.Activation < first.AddSeconds(2)))
            yield return aoe;

        foreach (var aoe in _predicted.Where(a => a.Activation <= first))
            yield return aoe with { Color = ArenaColor.Danger };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TheRamsVoice)
        {
            var allOrbs = Module.Enemies(OID.GlacipotentOrb).ToList();
            List<(WPos, float)> sources = [(spell.LocXZ, 9)];
            var next = Module.CastFinishAt(spell, 2);
            while (true)
            {
                var nextOrbs = allOrbs.Drain(o => sources.Any(s => o.Position.InCircle(s.Item1, s.Item2))).ToList();
                if (nextOrbs.Count == 0)
                    break;

                _predicted.AddRange(nextOrbs.Select(o => new AOEInstance(new AOEShapeCircle(12), o.Position, default, next)));
                next = next.AddSeconds(1);
                sources.Clear();
                sources.AddRange(nextOrbs.Select(o => (o.Position, _predicted.Count <= 2 ? 11f : 12f)));
            }
        }

        if ((AID)spell.Action.ID == AID.TheRamsVoice1)
        {
            var ix = _predicted.FindIndex(p => p.Origin.AlmostEqual(caster.Position, 0.5f));
            if (ix >= 0)
                _predicted.Ref(ix).Activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TheRamsVoice1)
            _predicted.RemoveAll(p => p.Origin.AlmostEqual(caster.Position, 0.5f));
    }
}

class Cacophony(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> orbs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => orbs.Select(o => new AOEInstance(new AOEShapeCircle(6), o.Position, default, WorldState.FutureTime(1.5f)));

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Cacophony)
            orbs.Add(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ChaoticChorus)
            orbs.Remove(caster);
    }
}

class ChaoticChorus(BossModule module) : Components.StandardAOEs(module, AID.ChaoticChorus, 6);

class Duobreath(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCone shape = new(40, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.LeftDuobreath or AID.RightDuobreath)
        {
            aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            aoes.Add(new(shape, spell.LocXZ, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 3)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.LeftDuobreath or AID.RightDuobreath or AID.TheRamsBreath3 or AID.TheDragonsBreath3 && aoes.Count > 0)
            aoes.RemoveAt(0);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (aoes.Count == 2)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(aoes[0].Origin, aoes[0].Rotation, 2, 2, 40), aoes[1].Activation);
    }
}

class RegnantChimeraStates : StateMachineBuilder
{
    public RegnantChimeraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Cacophony>()
            .ActivateOnEnter<ChaoticChorus>()
            .ActivateOnEnter<Breath>()
            .ActivateOnEnter<GlacipotentOrb>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<Duobreath>();
    }
}

[ModuleInfo(Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14767)]
public class RegnantChimera(WorldState ws, Actor primary) : BossModule(ws, primary, new(95, 470), new ArenaBoundsCircle(30));
