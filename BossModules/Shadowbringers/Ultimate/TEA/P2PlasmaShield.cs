namespace BossMod.Shadowbringers.Ultimate.TEA;

class P2PlasmaShield(BossModule module) : Components.DirectionalParry(module, (uint)OID.PlasmaShield, forbiddenPriority: AIHints.Enemy.PriorityInvincible)
{
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.PlasmaShield)
            PredictParrySide(actor.InstanceID, Side.Left | Side.Right | Side.Back);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (id, _) in _actorStates)
        {
            var e = hints.Enemies.FirstOrDefault(d => d?.Actor.InstanceID == id);
            if (e == null)
                continue;

            // ranged, healers, and CC tank should prioritize shield, others should attack BJ
            if (actor.Class.GetRole() is Role.Ranged or Role.Healer || ((TEA)Module).CruiseChaser()?.TargetID == actor.InstanceID)
                e.Priority = 1;
        }

        // overwrite priority if player is not inside vuln angle
        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        Arena.Actors(Module.Enemies(OID.PlasmaShield), ArenaColor.Enemy);
    }
}

class P2CCInvincible(BossModule module) : Components.InvincibleStatus(module, (uint)SID.Invincibility);
