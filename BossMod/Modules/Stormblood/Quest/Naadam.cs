namespace BossMod.Stormblood.Quest.Naadam;

public enum OID : uint
{
    Boss = 0x1B31,
    Helper = 0x233C,
    MagnaiTheOlder = 0x1B38, // R0.500, x0 (spawn during fight)
    StellarChuluu = 0x1B3F, // R1.800, x0 (spawn during fight)
    StellarChuluu1 = 0x1B40, // R1.800, x0 (spawn during fight)
    Grynewaht = 0x1B3A, // R0.500, x0 (spawn during fight)
    Ovoo = 0x1EA4E1
}

public enum AID : uint
{
    ViolentEarth = 8389, // MagnaiTheOlder1->location, 3.0s cast, range 6 circle
    DispellingWind = 8394, // SaduHeavensflame->self, 3.0s cast, range 40+R width 8 rect
    Epigraph = 8339, // 1A58->self, 3.0s cast, range 45+R width 8 rect
    DiffractiveLaser = 9122, // ArmoredWeapon->location, 3.0s cast, range 5 circle
    AugmentedSuffering = 8492, // Grynewaht->self, 3.5s cast, range 6+R circle
    AugmentedUprising = 8493, // Grynewaht->self, 3.0s cast, range 8+R 120-degree cone
}

public enum SID : uint
{
    EarthenAccord = 778
}

class DiffractiveLaser(BossModule module) : Components.StandardAOEs(module, AID.DiffractiveLaser, 5);
class AugmentedSuffering(BossModule module) : Components.StandardAOEs(module, AID.AugmentedSuffering, new AOEShapeCircle(6.5f));
class AugmentedUprising(BossModule module) : Components.StandardAOEs(module, AID.AugmentedUprising, new AOEShapeCone(8.5f, 60.Degrees()));

class ViolentEarth(BossModule module) : Components.StandardAOEs(module, AID.ViolentEarth, 6);
class DispellingWind(BossModule module) : Components.StandardAOEs(module, AID.DispellingWind, new AOEShapeRect(40.5f, 4));
class Epigraph(BossModule module) : Components.StandardAOEs(module, AID.Epigraph, new AOEShapeRect(45, 4));

class DrawOvoo : BossComponent
{
    private Actor? Ovoo => WorldState.Actors.FirstOrDefault(o => o.OID == 0x1EA4E1);

    public DrawOvoo(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actor(Ovoo, ArenaColor.Object, true);
    }
}

class ActivateOvoo(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.MountId == 117)
            hints.WantDismount = true;

        var beingAttacked = false;

        foreach (var e in hints.PotentialTargets)
        {
            if (e.Actor.TargetID == actor.InstanceID)
                beingAttacked = true;
            else
                e.Priority = AIHints.Enemy.PriorityForbidden;
        }

        var ovoo = WorldState.Actors.FirstOrDefault(x => x.OID == 0x1EA4E1);
        if (!beingAttacked && (ovoo?.IsTargetable ?? false))
            hints.InteractWithTarget = ovoo;
    }
}

class ProtectOvoo(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            if (e.Actor.FindStatus(SID.EarthenAccord) != null)
                e.Priority = 5;
            else if (e.Actor.OID == (uint)OID.StellarChuluu)
                e.Priority = 1;
            else
                e.Priority = 0;
        }
    }
}

class ProtectSadu(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var chuluu = WorldState.Actors.Where(x => (OID)x.OID == OID.StellarChuluu1).Select(x => x.InstanceID).ToList();

        foreach (var e in hints.PotentialTargets)
        {
            if (chuluu.Contains(e.Actor.TargetID))
                e.Priority = 5;
            else if ((OID)e.Actor.OID == OID.Grynewaht)
                e.Priority = 1;
            else
                e.Priority = 0;
        }
    }
}

class OvooStates : StateMachineBuilder
{
    public OvooStates(BossModule module) : base(module)
    {
        bool DutyEnd() => module.WorldState.CurrentCFCID != 246;

        TrivialPhase()
            .ActivateOnEnter<ActivateOvoo>()
            .ActivateOnEnter<DrawOvoo>()
            .Raw.Update = () => Module.WorldState.Actors.Any(x => x.OID == (uint)OID.MagnaiTheOlder && x.IsTargetable) || DutyEnd();
        TrivialPhase(1)
            .ActivateOnEnter<ProtectOvoo>()
            .ActivateOnEnter<ActivateOvoo>()
            .ActivateOnEnter<ViolentEarth>()
            .ActivateOnEnter<DispellingWind>()
            .ActivateOnEnter<Epigraph>()
            .Raw.Update = () => Module.WorldState.Actors.Any(x => x.OID == (uint)OID.Grynewaht && x.IsTargetable) || DutyEnd();
        TrivialPhase(2)
            .ActivateOnEnter<DiffractiveLaser>()
            .ActivateOnEnter<AugmentedSuffering>()
            .ActivateOnEnter<AugmentedUprising>()
            .ActivateOnEnter<ProtectSadu>()
            .Raw.Update = DutyEnd;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68051, PrimaryActorOID = (uint)OID.Ovoo)]
public class Ovoo(WorldState ws, Actor primary) : BossModule(ws, primary, new(354, 296.5f), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => Raid.Player()?.Position.InCircle(PrimaryActor.Position, 15) ?? false;

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
