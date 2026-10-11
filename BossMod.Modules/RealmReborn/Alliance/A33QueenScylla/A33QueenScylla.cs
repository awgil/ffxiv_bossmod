namespace BossMod.RealmReborn.Alliance.A33QueenScylla;

public enum OID : uint
{
    Boss = 0xDA0, // R3.250, Queen Scylla
    ForbiddenGate = 0xE73, // R1.500
    ForbiddenGateLarge = 0xE74, // R1.500
    Thor = 0xDA1, // R1.000
    Jormungand = 0xDA2, // R2.300
    XandesClone = 0xDA3, // R3.000
    FakeChest = 0x1E9709, // R0.500
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/Thor/Jormungand/XandesClone->player, no cast, single-target
    Unholy = 2362, // Boss->location, no cast, range 40 circle
    Firewalker = 2329, // Boss->self, no cast, range 5+R cone
    VoidCall = 3439, // ForbiddenGate/ForbiddenGateLarge->self, 15.0s cast, single-target
    AncientFlare = 2347, // Boss->self, 6.0s cast, range 81 circle
    LightningBolt = 2368, // Thor->location, 2.5s cast, range 5 circle
    Tremblor = 2381, // Jormungand->location, 3.0s cast, range 6 circle
    DoubleAxeHandle = 2354, // XandesClone->player, no cast, single-target
    AncientQuake = 3413, // XandesClone->self, no cast, range 60 circle
    AncientQuaga = 3412, // XandesClone->self, 5.0s cast, range 60 circle
    AuraCannon = 2358, // XandesClone->self, 3.0s cast, range 60+R width 10 rect
}

class AncientFlare(BossModule module) : Components.RaidwideCast(module, AID.AncientFlare, "");
class AncientFlareInterrupt(BossModule module) : Components.CastInterruptHint(module, AID.AncientFlare);
class AncientQuaga(BossModule module) : Components.RaidwideCast(module, AID.AncientQuaga, "");
class AncientQuagaInterrupt(BossModule module) : Components.CastInterruptHint(module, AID.AncientQuaga);
class LightningBolt(BossModule module) : Components.StandardAOEs(module, AID.LightningBolt, 5);
class Tremblor(BossModule module) : Components.StandardAOEs(module, AID.Tremblor, 6);
class AuraCannon(BossModule module) : Components.StandardAOEs(module, AID.AuraCannon, new AOEShapeRect(63, 5));

class ForbiddenGates(BossModule module) : Components.AddsMulti(module, [OID.ForbiddenGate, OID.ForbiddenGateLarge], 2);
class Adds(BossModule module) : Components.AddsMulti(module, [OID.Thor, OID.Jormungand], 1);
class Bosses(BossModule module) : Components.AddsMulti(module, [OID.Boss, OID.XandesClone], 0);

class FalseChest(BossModule module) : BossComponent(module)
{
    private Actor? ActiveChest => Module.Enemies(OID.FakeChest).FirstOrDefault(c => c.IsTargetable);

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (ActiveChest != null)
            hints.Add("Open chest to summon Xande");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveChest is not { } chest)
            return;
        hints.InteractWithTarget = chest;
        hints.GoalZones.Add(AIHints.GoalSingleTarget(chest.Position, 3));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (ActiveChest is { } chest)
            Arena.AddCircle(chest.Position, 1.5f, ArenaColor.Safe);
    }
}

class A33QueenScyllaStates : StateMachineBuilder
{
    public A33QueenScyllaStates(BossModule module) : base(module)
    {
        var gatesSeen = false;
        var xandeSeen = false;
        var addsSeen = false;
        TrivialPhase()
            .ActivateOnEnter<AncientFlare>()
            .ActivateOnEnter<AncientFlareInterrupt>()
            .ActivateOnEnter<AncientQuaga>()
            .ActivateOnEnter<AncientQuagaInterrupt>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<Tremblor>()
            .ActivateOnEnter<AuraCannon>()
            .ActivateOnEnter<ForbiddenGates>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<Bosses>()
            .ActivateOnEnter<FalseChest>()
            .Raw.Update = () =>
            {
                var xande = Module.Enemies(OID.XandesClone);
                var jorm = Module.Enemies(OID.Jormungand);
                var thor = Module.Enemies(OID.Thor);
                var gates = Module.Enemies(OID.ForbiddenGate).Concat(Module.Enemies(OID.ForbiddenGateLarge));
                if (gates.Any())
                    gatesSeen = true;
                if (xande.Count > 0)
                    xandeSeen = true;
                if (jorm.Count + thor.Count > 0)
                    addsSeen = true;

                var xandeCleared = xandeSeen && xande.All(x => x.IsDeadOrDestroyed);
                var gatesCleared = gatesSeen && gates.All(g => g.IsDeadOrDestroyed);
                var addsCleared = addsSeen && jorm.All(j => j.IsDeadOrDestroyed) && thor.All(t => t.IsDeadOrDestroyed);
                var secondaryCleared = gatesCleared || addsCleared;
                var addsDone = !addsSeen || addsCleared;
                return Module.PrimaryActor.IsDeadOrDestroyed && xandeCleared && (gatesSeen || addsSeen) && secondaryCleared && addsDone;
            };
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 111, NameID = 3247)]
public class A33QueenScylla(ModuleInit init) : BossModule(init, new(130, 265), new ArenaBoundsCircle(30));
