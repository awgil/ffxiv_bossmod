namespace BossMod.Dawntrail.Extreme.Ex5Necron;

class JailHands(BossModule module) : Components.Adds(module, (uint)OID._Gen_IcyHands3, 1)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var a in ActiveActors)
        {
            var e = hints.FindEnemy(a);
            e?.Priority = 1;
            e?.ForbidDOTs = true;
        }
    }
}

class JailGrasp(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_ChokingGrasp3, new AOEShapeRect(24, 3));
