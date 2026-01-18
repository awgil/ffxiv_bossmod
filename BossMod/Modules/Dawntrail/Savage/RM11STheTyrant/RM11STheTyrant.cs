namespace BossMod.Dawntrail.Savage.RM11STheTyrant;

class CrownOfArcadia(BossModule module) : Components.RaidwideCast(module, AID.CrownOfArcadia);

class DanceOfDomination1(BossModule module) : Components.RaidwideCastDelay(module, AID.DanceOfDominationTrophy, AID.DanceOfDominationFirst, 6.6f);
class DanceOfDomination2(BossModule module) : Components.RaidwideInstant(module, AID.DanceOfDominationLast, 4.2f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DanceOfDominationFirst)
            Activation = WorldState.FutureTime(Delay);
        if ((AID)spell.Action.ID == AID.DanceOfDominationLast)
        {
            NumCasts++;
            Activation = default;
        }
    }
}

class HurricaneExplosion(BossModule module) : Components.StandardAOEs(module, AID.HurricaneExplosion, new AOEShapeRect(60, 5));
class EyeOfTheHurricane(BossModule module) : Components.StackWithCastTargets(module, AID.EyeOfTheHurricane, 6, 2, 2);
class Maelstrom(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.Maelstrom).Where(e => e.EventState != 7));

class PowerfulGust : Components.GenericBaitAway
{
    public DateTime Activation;

    public PowerfulGust(BossModule module) : base(module)
    {
        Activation = WorldState.FutureTime(6.3f);
    }

    public override void Update()
    {
        CurrentBaits.Clear();
        if (Activation == default)
            return;

        foreach (var t in Module.Enemies(OID.Maelstrom))
        {
            foreach (var player in Raid.WithoutSlot().SortedByRange(t.Position).Take(2))
            {
                CurrentBaits.Add(new(t, player, new AOEShapeCone(60, 45.Degrees()), Activation));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.PowerfulGust)
        {
            Activation = default;
            NumCasts++;
        }
    }
}

class OneAndOnly(BossModule module) : Components.RaidwideCast(module, AID.OneAndOnlyRaidwide);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1073, NameID = 14305, PlanLevel = 100)]
public class RM11STheTyrant(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (PrimaryActor.IsTargetable)
            Arena.ActorInsideBounds(PrimaryActor.Position, PrimaryActor.Rotation, ArenaColor.Enemy);
    }
}
