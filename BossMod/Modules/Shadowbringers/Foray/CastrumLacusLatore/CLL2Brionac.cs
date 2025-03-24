namespace BossMod.Shadowbringers.Foray.CastrumLacusLatore.CLL2Brionac;

public enum OID : uint
{
    Boss = 0x2ECC, // R20.000, x1
    Helper = 0x233C, // R0.500, x36, Helper type
    Brionac = 0x2ED6, // R0.500, x1
    MagitekCore = 0x2F9A, // R10.000, x0 (spawn during fight), Part type
    Lightsphere = 0x2ECD, // R1.000, x0 (spawn during fight)
    Shadowsphere = 0x2ECE, // R1.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 21446, // Boss->player, no cast, single-target
    ElectricAnvilBoss = 20956, // Boss->self, 4.0s cast, single-target
    ElectricAnvil = 20957, // Helper->player, 5.0s cast, single-target
    FalseThunder = 20943, // Boss->self, 8.0s cast, range 47 ?-degree cone
    AntiWarmachinaWeaponry = 20941, // Boss->self, 5.0s cast, single-target
    MagitekThunder = 20993, // Brionac->2ED7, no cast, single-target
    LightningShowerBoss = 21444, // Boss->self, 4.0s cast, single-target
    LightningShower = 21445, // Helper->self, 5.0s cast, range 60 circle
    EnergyGeneration = 20944, // Boss->self, 3.0s cast, single-target
    MagitekMissiles = 20991, // Helper->player, 5.0s cast, single-target
    Lightburst = 20945, // Lightsphere->self, 2.0s cast, range 5-20 donut
    InfraredBlast = 20974, // Helper->player, no cast, single-target
    ShadowBurst = 20946, // Shadowsphere->self, 2.0s cast, range 12 circle
    VoltstreamBoss = 20954, // Boss->self, 3.0s cast, single-target
    Voltstream = 20955, // Helper->self, 6.0s cast, range 40 width 10 rect
    PoleShiftBoss = 20947, // Boss->self, 8.0s cast, single-target
    PoleShift = 20948, // Helper->Lightsphere/Shadowsphere, no cast, single-target
}

class ElectricAnvil(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.ElectricAnvil));
class FalseThunder(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID.FalseThunder), new AOEShapeCone(47, 64.Degrees()));
class LightningShower(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.LightningShower));

class Balls(BossModule module) : Components.GenericAOEs(module)
{
    class Ball(Actor caster, DateTime creation, DateTime activation, WPos destination)
    {
        public Actor Caster = caster;
        public DateTime Creation = creation;
        public DateTime Activation = activation;
        public WPos Destination = destination;
    }

    private readonly List<Ball> Casters = [];

    private static readonly AOEShape Circle = new AOEShapeCircle(12);
    private static readonly AOEShape Donut = new AOEShapeDonut(5, 20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var b in Casters)
        {
            if ((b.Activation - WorldState.CurrentTime).TotalSeconds < 6)
                yield return new AOEInstance(b.Caster.OID == (uint)OID.Lightsphere ? Donut : Circle, b.Destination, Activation: b.Activation);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.Lightsphere or OID.Shadowsphere)
            Casters.Add(new(actor, WorldState.CurrentTime, WorldState.FutureTime(10.7f), actor.Position));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Lightburst or AID.ShadowBurst)
        {
            if (Casters.Find(c => c.Caster == caster) is { } b)
                b.Activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Lightburst or AID.ShadowBurst)
            Casters.RemoveAll(c => c.Caster == caster);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == 21 && WorldState.Actors.Find(tether.Target) is { } target)
        {
            foreach (var b in Casters)
            {
                if (b.Caster == source)
                    b.Destination = target.Position;
                else if (b.Caster == target)
                    b.Destination = source.Position;

                b.Activation = WorldState.FutureTime(12.1f);
            }
        }
    }
}

class MagitekCore(BossModule module) : Components.Adds(module, (uint)OID.MagitekCore, 1);
class Voltstream(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID.Voltstream), new AOEShapeRect(40, 5), maxCasts: 3);

class BrionacStates : StateMachineBuilder
{
    public BrionacStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectricAnvil>()
            .ActivateOnEnter<FalseThunder>()
            .ActivateOnEnter<LightningShower>()
            .ActivateOnEnter<Balls>()
            .ActivateOnEnter<MagitekCore>()
            .ActivateOnEnter<Voltstream>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 735, NameID = 9436)]
public class Brionac(WorldState ws, Actor primary) : BossModule(ws, primary, new(80, -222), new ArenaBoundsRect(29.5f, 14.5f));
