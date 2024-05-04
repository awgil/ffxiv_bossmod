namespace BossMod.RealmReborn.Dungeon.D16Amdapor.D161Psycheflayer;

public enum OID : uint
{
    BossP1 = 0x5D1, // x1
    BossP2 = 0x5D0, // spawn during fight
    StoneMarionette = 0x5D2, // x1
    MarbleMarionette = 0x5D3, // x1
    MegalithMarionette = 0x5D4, // x1
}

public enum AID : uint
{
    Thunder = 968, // BossP1->player, 1.0s cast, single-target, "autoattack"
    VoidFireCleave = 1083, // BossP1->player, 2.0s cast, range 5 circle cleave
    VoidFireAOE = 1084, // BossP1->location, 3.0s cast, range 5 circle puddle
    VoidCall = 1082, // BossP1->self, 7.0s cast, single-target, phase transition

    Water = 971, // BossP2->player, 1.0s cast (interruptible), single-target, "autoattack"
    VoidThunder = 1085, // BossP2->player, 4.0s cast (interruptible), single-target, tankbuster
    MindMelt = 1078, // BossP2->self, 3.0s cast (interruptible), raidwide
    Canker = 1079, // BossP2->player, 3.0s cast (interruptible), single-target debuff
    Reanimate = 1080, // BossP2->StoneMarionette/MarbleMarionette/MegalithMarionette, no cast, single-target, visual
    AutoAttack = 872, // StoneMarionette/MarbleMarionette->player, no cast, single-target
    Rockslide = 1086, // StoneMarionette->self, 2.5s cast, range 11+R width 8 rect
    Obliterate = 1088, // MarbleMarionette->self, 3.0s cast, raidwide
    Plaincracker = 1087, // MegalithMarionette->self, 7.0s cast, range 25+R circle
}

class VoidFireCleave(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.VoidFireCleave), new AOEShapeCircle(5), originAtTarget: true);
class VoidFireAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.VoidFireAOE), 5);
class VoidThunder(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.VoidThunder), "Interruptible tankbuster");
class MindMelt(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MindMelt), "Interruptible raidwide");
class Canker(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.Canker), "Interruptible debuff");
class Rockslide(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Rockslide), new AOEShapeRect(12.76f, 4));
class Obliterate(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Obliterate));
class Plaincracker(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Plaincracker), new AOEShapeCircle(30.5f));

class D161PsycheflayerStates : StateMachineBuilder
{
    public D161PsycheflayerStates(D161Psycheflayer module) : base(module)
    {
        SimplePhase(0, id => { SimpleState(id, 10000, "Enrage"); }, "Boss death")
            .ActivateOnEnter<VoidFireCleave>()
            .ActivateOnEnter<VoidFireAOE>()
            .ActivateOnEnter<VoidThunder>()
            .ActivateOnEnter<MindMelt>()
            .ActivateOnEnter<Canker>()
            .ActivateOnEnter<Rockslide>()
            .ActivateOnEnter<Obliterate>()
            .ActivateOnEnter<Plaincracker>()
            .Raw.Update = () => module.MainBoss().IsDead || module.MainBoss().IsDestroyed;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 14, NameID = 1689)]
public class D161Psycheflayer(WorldState ws, Actor primary) : BossModule(ws, primary, new(-29, 0), new ArenaBoundsCircle(40))
{
    private Actor? _bossP2;
    public Actor MainBoss() => _bossP2 ?? PrimaryActor;

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.MarbleMarionette => 4,
                OID.StoneMarionette => 3,
                OID.BossP1 => 2,
                OID.BossP2 => 1,
                _ => 0
            };
        }
    }

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bossP2 ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.BossP2).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_bossP2, ArenaColor.Enemy);
    }
}
