namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class DecisiveBattle(BossModule module) : BossComponent(module)
{
    enum Buff
    {
        None,
        Epic,
        Fated,
        Vaunted
    }

    private readonly Actor?[] _bosses = new Actor?[4];
    private readonly Buff[] _buffs = new Buff[PartyState.MaxAllianceSize];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);

        switch ((SID)status.ID)
        {
            case SID.EpicHero:
                if (slot >= 0)
                    _buffs[slot] = Buff.Epic;
                break;
            case SID.FatedHero:
                if (slot >= 0)
                    _buffs[slot] = Buff.Fated;
                break;
            case SID.VauntedHero:
                if (slot >= 0)
                    _buffs[slot] = Buff.Vaunted;
                break;
            case SID.EpicVillain:
                _bosses[(int)Buff.Epic] = actor;
                break;
            case SID.FatedVillain:
                _bosses[(int)Buff.Fated] = actor;
                break;
            case SID.VauntedVillain:
                _bosses[(int)Buff.Vaunted] = actor;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.EpicHero:
            case SID.FatedHero:
            case SID.VauntedHero:
                var slot = Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    _buffs[slot] = Buff.None;
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var assigned = AssignedBoss(slot);
        if (assigned != null)
        {
            var target = WorldState.Actors.Find(actor.TargetID);
            if (target != null && !target.IsAlly && target != assigned)
                hints.Add("Target correct boss!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var buff = GetBuff(slot);
        if (buff == Buff.None)
            return;

        for (var b = Buff.Epic; b <= Buff.Vaunted; b++)
            if (b != buff)
                hints.SetPriority(_bosses[(int)b], AIHints.Enemy.PriorityInvincible);
    }

    private Buff GetBuff(int slot) => slot < _buffs.Length ? _buffs[slot] : Buff.None;
    private Actor? AssignedBoss(int slot) => _bosses[(int)GetBuff(slot)];
}
