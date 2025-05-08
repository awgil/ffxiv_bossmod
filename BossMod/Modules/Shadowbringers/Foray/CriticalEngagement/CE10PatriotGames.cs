namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE10PatriotGames;

public enum OID : uint
{
    Boss = 0x2E14, // R4.500, x1
    Helper = 0x233C, // R0.500, x8, Helper type
    MagitekBartizan = 0x2F02, // R1.000, x6
}

public enum AID : uint
{
    PlasmaGun = 20451, // Boss->player, 4.0s cast, single-target
    MagitekBartizan = 20441, // Boss->self, 5.0s cast, single-target
    ElectrochemicalTransfer = 20442, // Boss->self, 7.0s cast, single-target
    ElectrochemicalReaction = 20443, // 2F02->self, 1.0s cast, range 50 width 25 rect
    LightningRod = 20447, // Boss->self, 5.0s cast, single-target
    LightningRod1 = 20448, // Helper->self, no cast, single-target
    Explosion = 20449, // Helper->self, no cast, range 8 circle
    FiringOrders = 20445, // Boss->self, 5.0s cast, single-target
    Neutralization = 20444, // Boss->self, 4.0s cast, range 30 120-degree cone
    OrderedFire = 20446, // Helper->location, 8.0s cast, range 10 circle
    ElectrifyingConduction = 20453, // Helper->self, 4.0s cast, range 40 circle
    ElectrifyingConduction1 = 20452, // Boss->self, 4.0s cast, single-target
    SearingConduction = 20454, // Boss->self, 3.0s cast, single-target
    SearingConduction1 = 20455, // Helper->self, 3.0s cast, range 15 circle
    MassiveExplosion = 20450, // Helper->self, no cast, range 40 circle
}

public enum TetherID : uint
{
    TetherOne = 114, // 2F02->Boss
    TetherTwo = 115, // 2F02->Boss
}

class Neutralization(BossModule module) : Components.StandardAOEs(module, AID.Neutralization, new AOEShapeCone(30, 60.Degrees()));
class OrderedFire(BossModule module) : Components.StandardAOEs(module, AID.OrderedFire, 10);
class ElectrifyingConduction(BossModule module) : Components.RaidwideCast(module, AID.ElectrifyingConduction);
class SearingConduction(BossModule module) : Components.StandardAOEs(module, AID.SearingConduction1, new AOEShapeCircle(15));

class ElectrochemicalReaction(BossModule module) : Components.GenericAOEs(module, AID.ElectrochemicalReaction)
{
    private readonly Dictionary<WPos, int> Charges = [];
    private readonly List<AOEInstance> AOEs = [];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.TetherOne or TetherID.TetherTwo)
            Charge(source, 1);
    }

    private void Charge(Actor actor, int amount)
    {
        var pos = actor.Position.Rounded();
        Charges.TryAdd(pos, 0);
        Charges[pos] += amount;
        if (Charges[pos] >= 2)
            AOEs.Add(new(new AOEShapeRect(50, 12.5f), actor.Position, actor.Rotation, WorldState.FutureTime(11.1f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            for (var i = 0; i < AOEs.Count; i++)
            {
                var a = AOEs.Ref(i);
                if (a.Origin.AlmostEqual(caster.Position, 1))
                {
                    a.Origin = spell.LocXZ;
                    a.Rotation = spell.Rotation;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            AOEs.RemoveAll(t => t.Origin.AlmostEqual(caster.Position, 1));
            Charges[caster.Position.Rounded()] = 0;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;
}

class LightningRod(BossModule module) : BossComponent(module)
{
    private readonly List<WPos> Mines = [];
    private DateTime Activation;

    public const float Radius = 8;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LightningRod1)
        {
            Mines.Add(caster.Position);
            Activation = WorldState.FutureTime(12);
        }

        if ((AID)spell.Action.ID == AID.Explosion)
            Mines.RemoveAll(m => m.AlmostEqual(caster.Position, 1));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var m in Mines)
            Arena.ZoneCircle(m, Radius, ArenaColor.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Mines.Count > 0)
            hints.Add($"Detonate mines! {(Activation - WorldState.CurrentTime).TotalSeconds:f1}s remaining", false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Mines.Count > 0 && actor.Role == Role.Tank)
            hints.GoalZones.Add(p => Mines.Any(m => p.InCircle(m, Radius)) ? 0.5f : 0);
    }
}

class PatriotStates : StateMachineBuilder
{
    public PatriotStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectrochemicalReaction>()
            .ActivateOnEnter<LightningRod>()
            .ActivateOnEnter<Neutralization>()
            .ActivateOnEnter<OrderedFire>()
            .ActivateOnEnter<ElectrifyingConduction>()
            .ActivateOnEnter<SearingConduction>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 735, NameID = 10)] // bnpcname=9417
public class Patriot(WorldState ws, Actor primary) : BossModule(ws, primary, new(-240, 414), new ArenaBoundsSquare(24.5f));

