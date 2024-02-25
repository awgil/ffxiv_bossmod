namespace BossMod.Components
{
    // generic unavoidable raidwide cast
    public class RaidwideCast : CastHint
    {
        public RaidwideCast(ActionID aid, string hint = "Raidwide") : base(aid, hint) { }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            foreach (var c in Casters)
                hints.PredictedDamage.Add((module.Raid.WithSlot().Mask(), c.CastInfo!.NPCFinishAt));
        }
    }

    // generic unavoidable single-target damage cast (typically tankbuster, but not necessary)
    public class SingleTargetCast : CastHint
    {
        public SingleTargetCast(ActionID aid, string hint = "Tankbuster") : base(aid, hint) { }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            foreach (var c in Casters)
            {
                BitMask targets = new();
                targets.Set(module.Raid.FindSlot(c.CastInfo!.TargetID));
                hints.PredictedDamage.Add((targets, c.CastInfo!.NPCFinishAt));
            }
        }
    }
}
