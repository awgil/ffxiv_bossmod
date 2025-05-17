namespace BossMod.Stormblood.Quest.TortoiseInTime;

public enum OID : uint
{
    Boss = 0x2339,
    Helper = 0x233C,
    Soroban = 0x2351, // R0.500, x8
    MonkeyMagick = 0x23C2, // R1.000, x0 (spawn during fight)
    Font = 0x233B, // R4.000, x0 (spawn during fight)
}

public enum AID : uint
{
    Eddy1 = 11511, // 2351->location, 3.0s cast, range 6 circle
    GreatFlood1 = 11513, // 2351->self, no cast, range 60 circle
    SpiritBurst = 11706, // 23C2->self, 1.0s cast, range 6 circle
    WaterDrop = 11301, // 2351->234F, 8.0s cast, range 6 circle
    Whitewater1 = 11521, // 2351->self, 3.0s cast, range 40+R width 7 rect
    Upwell = 11515, // 233B->self, 3.0s cast, range 37+R ?-degree cone
}

class Whitewater(BossModule module) : Components.StandardAOEs(module, AID.Whitewater1, new AOEShapeRect(40.5f, 3.5f));
class Upwell(BossModule module) : Components.StandardAOEs(module, AID.Upwell, new AOEShapeCone(41, 15.Degrees()));
class SpiritBurst(BossModule module) : Components.StandardAOEs(module, AID.SpiritBurst, new AOEShapeCircle(6));
class WaterDrop(BossModule module) : Components.SpreadFromCastTargets(module, AID.WaterDrop, 6);

class ExplosiveTataru(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> Balls = [];
    private Actor? Tataru;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == 3)
        {
            Balls.Add(source);
            Tataru ??= WorldState.Actors.Find(tether.Target);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SpiritBurst)
        {
            Balls.Remove(caster);
            if (Balls.Count == 0)
                Tataru = null;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Tataru != null)
            Arena.AddCircle(Tataru.Position, 6, ArenaColor.Danger);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Tataru != null)
            hints.AddForbiddenZone(ShapeContains.Circle(Tataru.Position, 6));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Tataru != null && actor.Position.InCircle(Tataru.Position, 6))
            hints.Add("GTFO from Tataru!");
    }
}

class Eddy(BossModule module) : Components.StandardAOEs(module, AID.Eddy1, 6);

class ShieldHint(BossModule module) : BossComponent(module)
{
    private const float Radius = 7;
    private Actor? Shield;

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (actor.OID == 0x1EA9C7 && state == 2)
            Shield = actor;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Shield is Actor s)
            Arena.ZoneCircle(s.Position, Radius, ArenaColor.SafeFromAOE);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.GreatFlood1)
            Shield = null;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Shield is Actor s)
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(s.Position, Radius), Module.CastFinishAt(Module.PrimaryActor.CastInfo));
    }
}

class SorobanStates : StateMachineBuilder
{
    public SorobanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Eddy>()
            .ActivateOnEnter<ShieldHint>()
            .ActivateOnEnter<WaterDrop>()
            .ActivateOnEnter<ExplosiveTataru>()
            .ActivateOnEnter<SpiritBurst>()
            .ActivateOnEnter<Whitewater>()
            .ActivateOnEnter<Upwell>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68552, NameID = 7240)]
public class Soroban(WorldState ws, Actor primary) : BossModule(ws, primary, new(62, -372), new ArenaBoundsSquare(19));
