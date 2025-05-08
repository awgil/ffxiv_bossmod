namespace BossMod.Stormblood.Quest.HopeOnTheWaves;

public enum OID : uint
{
    Boss = 0x21B1,
    Helper = 0x233C,
}

public enum AID : uint
{
    CermetPile = 9425, // Boss->self, 2.5s cast, range 40+R width 6 rect
    SelfDetonate = 10928, // Boss->self, 30.0s cast, range 100 circle
    CircleOfDeath = 9428, // 2115->self, 3.0s cast, range 6+R circle
    W2TonzeMagitekMissile = 10929, // 2115->location, 3.0s cast, range 6 circle
    SelfDetonate1 = 10930, // 21B6->self, 5.0s cast, range 6 circle
    MagitekMissile1 = 10893, // 21B7->location, 10.0s cast, range 60 circle
    AssaultCannon = 10823, // 21B5->self, 2.5s cast, range 75+R width 2 rect
}

class AssaultCannon(BossModule module) : Components.StandardAOEs(module, AID.AssaultCannon, new AOEShapeRect(75, 1));
class CircleOfDeath(BossModule module) : Components.StandardAOEs(module, AID.CircleOfDeath, new AOEShapeCircle(10.24f));
class TwoTonzeMagitekMissile(BossModule module) : Components.StandardAOEs(module, AID.W2TonzeMagitekMissile, 6);
class MagitekMissileProximity(BossModule module) : Components.StandardAOEs(module, AID.MagitekMissile1, 11.75f);
class CermetPile(BossModule module) : Components.StandardAOEs(module, AID.CermetPile, new AOEShapeRect(42, 3));
class SelfDetonate(BossModule module) : Components.CastHint(module, AID.SelfDetonate, "Kill before detonation!", true);
class MineSelfDetonate(BossModule module) : Components.StandardAOEs(module, AID.SelfDetonate1, new AOEShapeCircle(6));

class Adds(BossModule module) : BossComponent(module)
{
    private Actor? Alphinaud => WorldState.Actors.FirstOrDefault(a => a.OID == 0x21AC);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        WPos? lbCenter = Alphinaud?.CastInfo is { Action.ID: 10894 } castInfo
            ? castInfo.LocXZ
            : null;

        foreach (var e in hints.PotentialTargets)
        {
            if (lbCenter != null && e.Actor.OID == 0x2114)
            {
                e.ShouldBeTanked = true;
                e.DesiredPosition = lbCenter.Value;
                e.Priority = 5;
            }
            else if (e.Actor.CastInfo?.Action.ID == (uint)AID.SelfDetonate)
                e.Priority = 5;
            else
                e.Priority = 0;
        }
    }
}

class ImperialCenturionStates : StateMachineBuilder
{
    public ImperialCenturionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<CircleOfDeath>()
            .ActivateOnEnter<TwoTonzeMagitekMissile>()
            .ActivateOnEnter<MagitekMissileProximity>()
            .ActivateOnEnter<CermetPile>()
            .ActivateOnEnter<SelfDetonate>()
            .ActivateOnEnter<MineSelfDetonate>()
            .ActivateOnEnter<AssaultCannon>()
            .Raw.Update = () => module.WorldState.CurrentCFCID != 472;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68560, NameID = 4148)]
public class ImperialCenturion(WorldState ws, Actor primary) : BossModule(ws, primary, new(473.25f, 751.75f), BoundsP2)
{
    public static readonly ArenaBoundsCustom BoundsP2 = new(30, new(CurveApprox.Ellipse(34, 21, 0.05f).Select(p => p.Rotate(140.Degrees()))));

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
    }
}
