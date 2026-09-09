namespace BossMod.Endwalker.Criterion.C02AMR.C022Gorai;

class ImpurePurgationBait(BossModule module) : Components.BaitAwayEveryone(module, module.PrimaryActor, new AOEShapeCone(60, 22.5f.Degrees()))
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NImpurePurgationBait or AID.SImpurePurgationBait)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

class ImpurePurgationAOE(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeCone(60, 22.5f.Degrees()));
class NImpurePurgationAOE(BossModule module) : ImpurePurgationAOE(module, AID.NImpurePurgationAOE);
class SImpurePurgationAOE(BossModule module) : ImpurePurgationAOE(module, AID.SImpurePurgationAOE);
