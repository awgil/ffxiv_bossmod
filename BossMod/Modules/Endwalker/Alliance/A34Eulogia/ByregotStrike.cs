namespace BossMod.Endwalker.Alliance.A34Eulogia;

sealed class ByregotStrikeJump(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ByregotStrike, 8f);
sealed class ByregotStrikeKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ByregotStrikeKnockback, 20f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
            {
                hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOrigin(Arena.Center, c.Origin, 20f, 29f), act);
            }
        }
    }
}
sealed class ByregotStrikeCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ByregotStrikeAOE, new AOEShapeCone(90f, 22.5f.Degrees()));
