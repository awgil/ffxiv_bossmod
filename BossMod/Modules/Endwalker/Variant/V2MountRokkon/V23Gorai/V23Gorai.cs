namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V23Gorai;

sealed class Unenlightenment(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.Unenlightenment, (uint)AID.UnenlightenmentAOE, 0.5d);
sealed class SpikeOfFlameAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpikeOfFlameAOE, 5f);

sealed class StringSnap(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StringSnap1)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.StringSnap1 => 0,
                (uint)AID.StringSnap2 => 1,
                (uint)AID.StringSnap3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

sealed class TorchingTorment(BossModule module) : Components.BaitAwayIcon(module, 6f, (uint)IconID.Tankbuster, (uint)AID.TorchingTorment, 5.9d, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class PureShock(BossModule module) : Components.RaidwideCast(module, (uint)AID.PureShock);
sealed class HumbleHammer(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HumbleHammer, 3f);
sealed class FightingSpirits(BossModule module) : Components.RaidwideCast(module, (uint)AID.FightingSpirits);
sealed class BiwaBreaker(BossModule module) : Components.RaidwideCast(module, (uint)AID.BiwaBreakerFirst, "Raidwide x5");

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.Gorai, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945u, NameID = 12373u, SortOrder = 5, Category = BossModuleInfo.Category.VariantCriterion, Expansion = BossModuleInfo.Expansion.Endwalker)]
public sealed class V23Gorai(WorldState ws, Actor primary) : BossModule(ws, primary, new(741f, -190f), new ArenaBoundsSquare(22.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ShishuWhiteBaboon));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.ShishuWhiteBaboon => 1,
                _ => 0
            };
        }
    }
}
