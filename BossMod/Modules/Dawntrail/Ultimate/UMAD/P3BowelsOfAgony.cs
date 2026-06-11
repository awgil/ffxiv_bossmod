namespace BossMod.Dawntrail.Ultimate.UMAD;

class P3AeroIIIAssault(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Ability_AeroIIIAssault, 15, true);

class P3ThunderIII(BossModule module) : Components.StandardAOEs(module, AID._Ability_ThunderIII, 14.8f);

class P3Firewall(BossModule module) : Components.GenericInvincible(module)
{
    enum Color
    {
        None,
        Epic, // chaos
        Fated // exdeath
    }

    readonly Color[] _colors = new Color[8];

    Actor? _epicBoss;
    Actor? _fatedBoss;

    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        var otherBoss = _colors[slot] switch
        {
            Color.Fated => _epicBoss,
            Color.Epic => _fatedBoss,
            _ => null
        };

        if (otherBoss != null)
            yield return otherBoss;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID._Gen_EpicHero:
                if (Raid.TryFindSlot(actor, out var slot))
                    _colors[slot] = Color.Epic;
                break;
            case SID._Gen_FatedHero:
                if (Raid.TryFindSlot(actor, out var slot2))
                    _colors[slot2] = Color.Fated;
                break;
            case SID._Gen_EpicVillain:
                _epicBoss = actor;
                break;
            case SID._Gen_FatedVillain:
                _fatedBoss = actor;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID._Gen_EpicHero:
            case SID._Gen_FatedHero:
                if (Raid.TryFindSlot(actor, out var slot))
                    _colors[slot] = default;
                break;
            case SID._Gen_EpicVillain:
                _epicBoss = null;
                break;
            case SID._Gen_FatedVillain:
                _fatedBoss = null;
                break;
        }
    }
}
