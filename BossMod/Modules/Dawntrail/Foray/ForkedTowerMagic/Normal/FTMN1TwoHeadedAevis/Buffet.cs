namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

sealed class Buffet(BossModule module) : BossComponent(module)
{
    private readonly Actor?[] AssignedBoss = new Actor?[PartyState.MaxPartySize];
    private readonly TwoHeadedAevis bossModule = (TwoHeadedAevis)module;
    private readonly TwoHeadedAevisConfig _config = Service.Config.Get<TwoHeadedAevisConfig>();

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Buffet && Raid.FindSlot(source.InstanceID) is var slot && slot >= 0)
        {
            AssignedBoss[slot] = WorldState.Actors.Find(tether.Target);
        }
    }

    // fall back since players outside arena bounds do not get tethered but will still receive status effects
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var boss = status.ID switch
        {
            (uint)SID.EpicHero => Module.PrimaryActor,
            (uint)SID.FatedHero => bossModule.BlueHead(),
            _ => null
        };
        if (boss != null && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            AssignedBoss[slot] = boss;
        }
    }

    // if player joins fight late, statemachine won't reset this component properly
    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.EpicVillain or (uint)SID.FatedVillain)
        {
            Array.Clear(AssignedBoss);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (slot < PartyState.MaxPartySize && AssignedBoss[slot] is var assignedSlot && assignedSlot != null && WorldState.Actors.Find(actor.TargetID) is Actor target)
        {
            if (target != assignedSlot)
            {
                hints.Add($"Target {assignedSlot?.Name}!");
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (slot < PartyState.MaxAllianceSize && AssignedBoss[slot] is var assignedSlot && assignedSlot != null && WorldState.Actors.Find(actor.TargetID) is Actor target)
        {
            var count = hints.PotentialTargets.Count;
            for (var i = 0; i < count; ++i)
            {
                var enemy = hints.PotentialTargets[i];
                if (enemy.Actor != assignedSlot)
                {
                    enemy.Priority = AIHints.Enemy.PriorityInvincible;
                }
            }
            // also ignore forced targeting if current target is a PC
            if (_config.ForceTargeting && (target == null || target.Type != ActorType.Player))
            {
                if (assignedSlot == null)
                {
                    // one boss is dead, target healthier boss
                    var green = Module.PrimaryActor;
                    var blue = bossModule.BlueHead();
                    hints.ForcedTarget = green.HPMP.CurHP > blue?.HPMP.CurHP ? green : blue;
                }
                else if (target != assignedSlot)
                {
                    hints.ForcedTarget = assignedSlot;
                }
            }
        }
    }
}
