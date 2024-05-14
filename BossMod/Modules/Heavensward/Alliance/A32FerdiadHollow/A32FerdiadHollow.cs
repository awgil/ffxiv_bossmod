namespace BossMod.Heavensward.Alliance.A32FerdiadHollow;

class Blackbolt(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Blackbolt), 6, 8);

class Blackfire2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Blackfire2), 7); // expanding aoe circle

class JestersJig1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.JestersJig1), new AOEShapeCircle(9));

class JestersReap(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.JestersReap), new AOEShapeCone(13.4f, 60.Degrees()));
class JestersReward(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.JestersReward), new AOEShapeCone(31.4f, 90.Degrees()));

class JongleursX(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.JongleursX));
class JugglingSphere(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.JugglingSphere), 3);
class JugglingSphere2(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.JugglingSphere2), 3);

class LuckyPierrot1(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.LuckyPierrot1), 2.5f);
class LuckyPierrot2(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.LuckyPierrot2), 2.5f);

class PetrifyingEye(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.PetrifyingEye));

class Flameflow1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Flameflow1));
class Flameflow2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Flameflow2));
class Flameflow3(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Flameflow3));

class Unknown4(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.Unknown4), 3);
class Unknown6(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.Unknown6), 3);

class AtmosAOE1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AtmosAOE1), new AOEShapeCircle(20));
class AtmosAOE2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AtmosAOE2), new AOEShapeCircle(20));
class AtmosDonut(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AtmosDonut), new AOEShapeDonut(6, 20));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 220, NameID = 5509)]
public class A32FerdiadHollow(WorldState ws, Actor primary) : BossModule(ws, primary, new(-350, 225), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.FerdiadsFool), ArenaColor.Enemy);
    }
}