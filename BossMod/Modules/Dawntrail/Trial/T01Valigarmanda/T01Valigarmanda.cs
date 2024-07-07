namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

class T01ValigarmandaStates : StateMachineBuilder
{
    public T01ValigarmandaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            //.ActivateOnEnter<StranglingCoil2>() Disabled for now, not displaying properly, showing on boss instead of helper
            .ActivateOnEnter<SlitheringStrike2>()
            .ActivateOnEnter<SusurrantBreath2>()
            .ActivateOnEnter<Skyruin2>()
            .ActivateOnEnter<Skyruin3>()
            //.ActivateOnEnter<ThunderousBreath2>()
            .ActivateOnEnter<HailOfFeathers3>()
            .ActivateOnEnter<ArcaneLightning>() // Need to be tied to spawn of ArcaneSphere
            .ActivateOnEnter<DisasterZone2>()
            .ActivateOnEnter<DisasterZone4>()
            .ActivateOnEnter<RuinfallTower>()
            .ActivateOnEnter<Ruinfall3>()
            .ActivateOnEnter<RuinfallAOE>()
            .ActivateOnEnter<NorthernCross1>() // Need to be tied to animation
            .ActivateOnEnter<NorthernCross2>() // Need to be tied to animation
            .ActivateOnEnter<FreezingDust>()
            .ActivateOnEnter<ChillingCataclysm2>()
            .ActivateOnEnter<CalamitousEcho>()
            .ActivateOnEnter<Tulidisaster1>()
            .ActivateOnEnter<Eruption2>();
    }
}
class StranglingCoil2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.StranglingCoil2), new AOEShapeDonut(7, 60));
class SlitheringStrike2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SlitheringStrike2), new AOEShapeCone(24, 90.Degrees()));
class SusurrantBreath2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SusurrantBreath2), new AOEShapeCone(50, 65.Degrees()));
class Skyruin2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Skyruin2));
class Skyruin3(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Skyruin3));
class ThunderousBreath2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderousBreath2), new AOEShapeCone(50, 67.5f.Degrees()));
class HailOfFeathers3(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HailOfFeathers3));
class ArcaneLightning(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ArcaneLightning), new AOEShapeRect(50, 2.5f));
class DisasterZone2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DisasterZone2));
class DisasterZone4(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DisasterZone4));
class RuinfallAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RuinfallAOE), new AOEShapeCircle(6));
class Ruinfall3(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Ruinfall3), 21, stopAtWall: true, kind: Kind.DirForward);
class RuinfallTower(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.RuinfallTower), 6, 2, 2);

class NorthernCross1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.NorthernCross1), new AOEShapeRect(60, 60, DirectionOffset: -90.Degrees()));
class NorthernCross2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.NorthernCross2), new AOEShapeRect(60, 60, DirectionOffset: 90.Degrees()));

class FreezingDust(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FreezingDust)
        {
            if (Raid.FindSlot(caster.TargetID) is var slot && slot >= 0 && slot < Requirements.Length)
                Requirements[slot] = Requirement.Move;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FreezingDust)
        {
            if (Raid.FindSlot(caster.TargetID) is var slot && slot >= 0 && slot < Requirements.Length)
                Requirements[slot] = Requirement.None;
        }
    }
}

class ChillingCataclysm2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ChillingCataclysm2), new AOEShapeCross(40, 2.5f));
class RuinForetold(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.RuinForetold));
class CalamitousEcho(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CalamitousEcho), new AOEShapeCone(40, 10.Degrees()));
class Tulidisaster1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Tulidisaster1));
class Eruption2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Eruption2), 6);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 832, NameID = 12854)]
public class T01Valigarmanda(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(20, 15))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.IceBoulder), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.FlameKissedBeacon), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.GlacialBeacon), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.ThunderousBeacon), ArenaColor.Enemy);
    }
}
