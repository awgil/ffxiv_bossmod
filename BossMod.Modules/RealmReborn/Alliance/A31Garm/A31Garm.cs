namespace BossMod.RealmReborn.Alliance.A31Garm;

public enum OID : uint
{
    Boss = 0xD9C, // R3.700, x3
    Helper = 0x1B2, // R0.500
    TwoHeadedDragon = 0xD9D, // R4.000
    SacrificedNinja = 0xD9F, // R0.500
    SacrificedKunoichi = 0xD9E, // R0.500
    BurnPuddle = 0x1E9701, // R0.500, EventObj type, (EventState 7 = inactive)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/SacrificedNinja/SacrificedKunoichi->player, no cast, single-target
    TheDragonsVoice = 3344, // Boss->self, 4.5s cast, range 30 circle
    TheRamsVoice = 3343, // Boss->self, 3.0s cast, range 6+R circle
    MarrowDrainLeft = 3340, // Boss->self, 3.0s cast, range 6+R 120-degree cone
    MarrowDrainMiddle = 3341, // Boss->self, 3.0s cast, range 6+R 120-degree cone
    MarrowDrainRight = 3342, // Boss->self, 3.0s cast, range 6+R 120-degree cone
    BurnPuddle = 3410, // Helper->location, 10.0s cast, range 14 circle

    MeanThrash = 3345, // TwoHeadedDragon->self, 2.0s cast, range 6+R 120-degree cone
    AutoAttackDragon = 682, // TwoHeadedDragon->players, no cast, range 6+R cone
    Diarchy = 3346, // TwoHeadedDragon->self, 0.5s cast, range 9+R cone
    BallOfFire = 3347, // TwoHeadedDragon->location, 3.0s cast, range 6 circle
    BallOfIce = 3348, // TwoHeadedDragon->location, 3.0s cast, range 6 circle
}

class MarrowDrain(BossModule module) : Components.GroupedAOEs(module, [AID.MarrowDrainLeft, AID.MarrowDrainMiddle, AID.MarrowDrainRight], new AOEShapeCone(9.7f, 60.Degrees()));
class TheDragonsVoice(BossModule module) : Components.StandardAOEs(module, AID.TheDragonsVoice, new AOEShapeDonut(8, 30));
class TheDragonsVoiceInterrupt(BossModule module) : Components.CastInterruptHint(module, AID.TheDragonsVoice);
class TheRamsVoice(BossModule module) : Components.StandardAOEs(module, AID.TheRamsVoice, new AOEShapeCircle(9));
class TheRamsVoiceInterrupt(BossModule module) : Components.CastInterruptHint(module, AID.TheRamsVoice);
class BurnPuddle(BossModule module) : Components.VoidzoneAtCastTarget(module, 14, AID.BurnPuddle, OID.BurnPuddle, 0);
class MeanThrash(BossModule module) : Components.StandardAOEs(module, AID.MeanThrash, new AOEShapeCone(10, 60.Degrees()));
class Diarchy(BossModule module) : Components.StandardAOEs(module, AID.Diarchy, new AOEShapeCone(13, 45.Degrees()));
class BallOfFire(BossModule module) : Components.StandardAOEs(module, AID.BallOfFire, 6);
class BallOfIce(BossModule module) : Components.StandardAOEs(module, AID.BallOfIce, 6);
class Adds(BossModule module) : Components.AddsMulti(module, [OID.TwoHeadedDragon, OID.SacrificedNinja, OID.SacrificedKunoichi]);

class A31GarmStates : StateMachineBuilder
{
    public A31GarmStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MarrowDrain>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<TheDragonsVoiceInterrupt>()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<TheRamsVoiceInterrupt>()
            .ActivateOnEnter<BurnPuddle>()
            .ActivateOnEnter<MeanThrash>()
            .ActivateOnEnter<Diarchy>()
            .ActivateOnEnter<BallOfFire>()
            .ActivateOnEnter<BallOfIce>()
            .ActivateOnEnter<Adds>()
            .Raw.Update = () =>
            {
                var bossesDead = Module.Enemies(OID.Boss).All(b => b.IsDeadOrDestroyed);
                var dragon = Module.Enemies(OID.TwoHeadedDragon);
                var ninjas = Module.Enemies(OID.SacrificedNinja);
                var kunoichi = Module.Enemies(OID.SacrificedKunoichi);
                var addsSpawned = dragon.Count + ninjas.Count + kunoichi.Count > 0;
                return bossesDead && addsSpawned && dragon.All(a => a.IsDeadOrDestroyed) && ninjas.All(a => a.IsDeadOrDestroyed) && kunoichi.All(a => a.IsDeadOrDestroyed);
            };
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 111, NameID = 3243)]
public class A31Garm(ModuleInit init) : BossModule(init, new(-77, 383), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.Boss), ArenaColor.Enemy);
    }
}
