namespace BossMod.Dawntrail.Extreme.Ex3Sphene;

class AbsoluteAuthorityPuddles(BossModule module) : Components.StandardAOEs(module, AID.AbsoluteAuthorityPuddlesAOE, 8);

class AbsoluteAuthorityExpansionBoot(BossModule module) : Components.UniformStackSpread(module, 6, 15, 4, alwaysShowSpreads: true) // TODO: verify falloff
{
    public int NumCasts;
    private readonly Ex3SpheneConfig _config = Service.Config.Get<Ex3SpheneConfig>();

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.AuthoritysExpansion:
                if (!_config.AbsoluteAuthorityIgnoreFlares)
                    AddSpread(actor, status.ExpireAt);
                break;
            case SID.AuthoritysBoot:
                AddStack(actor, status.ExpireAt);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AbsoluteAuthorityExpansion or AID.AbsoluteAuthorityBoot)
        {
            ++NumCasts;
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}

class AbsoluteAuthorityHeel(BossModule module) : Components.CastCounter(module, AID.AbsoluteAuthorityHeel)
{
    private BitMask _targets;

    private const float Radius = 3; // TODO: verify

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_targets[slot] && !Raid.WithoutSlot().InRadiusExcluding(actor, Radius).Any())
            hints.Add("Stack with someone!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_targets[pcSlot])
            Arena.AddCircle(pc.Position, Radius, ArenaColor.Safe);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.AuthoritysHeel)
        {
            _targets.Set(Raid.FindSlot(actor.InstanceID));
        }
    }
}

class AbsoluteAuthorityKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.AbsoluteAuthorityKnockback, 30, kind: Kind.DirForward);
