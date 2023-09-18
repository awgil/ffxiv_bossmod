namespace BossMod.Endwalker.Criterion.C02AMR.C022Gorai
{
    class ImpurePurgationBait : Components.BaitAwayEveryone
    {
        public ImpurePurgationBait() : base(new AOEShapeCone(60, 22.5f.Degrees())) { }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.NImpurePurgationBait or AID.SImpurePurgationBait)
            {
                ++NumCasts;
                CurrentBaits.Clear();
            }
        }
    }

    class ImpurePurgationAOE : Components.SelfTargetedAOEs
    {
        public ImpurePurgationAOE(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(60, 22.5f.Degrees())) { }
    }
    class NImpurePurgationAOE : ImpurePurgationAOE { public NImpurePurgationAOE() : base(AID.NImpurePurgationAOE) { } }
    class SImpurePurgationAOE : ImpurePurgationAOE { public SImpurePurgationAOE() : base(AID.SImpurePurgationAOE) { } }
}
