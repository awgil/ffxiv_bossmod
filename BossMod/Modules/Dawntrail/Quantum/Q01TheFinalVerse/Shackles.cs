namespace BossMod.Dawntrail.Quantum.Q01TheFinalVerse;

class ShackleSpreadHint(BossModule module) : Components.GenericStackSpread(module, alwaysShowSpreads: true, raidwideOnResolve: false)
{
    public bool Shackles { get; private set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ShacklesOfGreaterSanctity)
        {
            foreach (var player in Raid.WithoutSlot())
            {
                if (player.Role == Role.Healer)
                    Spreads.Add(new(player, 21, Module.CastFinishAt(spell)));
                else if (player.Role != Role.Tank)
                    Spreads.Add(new(player, 8, Module.CastFinishAt(spell)));
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ShackledHealing)
        {
            Shackles = true;
            Spreads.Clear();
        }
    }
}

// only draw spread on healer to reduce visual clutter
// DPS will naturally avoid healer (because of defam) and tank (because of jail)
class ShackleHint(BossModule module) : BossComponent(module)
{
    private Actor? Healer;
    public bool Expired;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ShackledHealing)
            Healer = actor;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ShackledHealing)
        {
            Healer = null;
            Expired = true;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Healer != null)
            Arena.AddCircle(Healer.Position, 21, ArenaColor.Danger);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor == Healer)
        {
            var count = Raid.WithoutSlot().InRadiusExcluding(actor, 21).Count();
            hints.Add($"Allies in radius: {count}", false);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => player == Healer ? PlayerPriority.Interesting : base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
}

class ArcaneFont(BossModule module) : Components.Adds(module, (uint)OID.ArcaneFont)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var jail = actor.FindStatus(SID.HellishEarth) != null;

        foreach (var add in ActiveActors)
        {
            if (hints.FindEnemy(add) is { } enemy)
            {
                enemy.Priority = jail ? AIHints.Enemy.PriorityInvincible : 1;
                enemy.ForbidDOTs = true;
            }
        }
    }
}

class HellishEarthPull(BossModule module) : Components.Knockback(module, ignoreImmunes: true)
{
    private Actor? Caster;
    private Actor? Target;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (Caster != null)
        {
            var activation = Module.CastFinishAt(Caster.CastInfo);

            if (actor == Target)
                yield return new(Caster.Position, 60, activation, Kind: Kind.TowardsOrigin);
            else if (!PlayerImmunes[slot].ImmuneAt(activation))
                yield return new(Caster.Position, 10, activation, Kind: Kind.TowardsOrigin);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor == Target)
        {
            if (actor.Role != Role.Tank)
                hints.Add("Too far from boss!");
        }
        else if (Target != null && actor.Role == Role.Tank)
            hints.Add("Go far to bait tether!");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HellishEarthPullTether)
            Caster = caster;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HellishEarthPullTether or AID.HellishEarthPull)
        {
            Target = null;
            Caster = null;
            NumCasts++;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.HellishEarth && WorldState.Actors.Find(tether.Target) is { } tar)
            Target = tar;
    }
}

class ManifoldLashingsTower(BossModule module) : Components.GenericTowers(module, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ManifoldLashingsTower)
        {
            Towers.Add(new(caster.Position, 2, forbiddenSoakers: Raid.WithSlot().WhereActor(a => a.Role != Role.Tank).Mask(), activation: Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ManifoldLashingsTower or AID.ManifoldLashingsTowerRepeat)
            NumCasts++;
    }
}
class ManifoldLashingsTail(BossModule module) : Components.StandardAOEs(module, AID.ManifoldLashingsTail, new AOEShapeRect(42, 4.5f));
