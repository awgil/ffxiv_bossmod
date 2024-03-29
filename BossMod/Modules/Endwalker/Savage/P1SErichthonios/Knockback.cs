namespace BossMod.Endwalker.Savage.P1SErichthonios;

// state related to knockback + aoe mechanic
// TODO: i'm not quite happy with implementation, consider revising...
class Knockback : BossComponent
{
    public bool AOEDone { get; private set; } = false;
    private bool _isFlare = false; // true -> purge aka flare (stay away from MT), false -> grace aka holy (stack to MT)
    private Actor? _knockbackTarget = null;
    private WPos _knockbackPos = new();

    private static readonly float _kbDistance = 15;
    private static readonly float _flareRange = 24; // max range is 50, but it has distance falloff - linear up to ~24, then constant ~3k
    private static readonly float _holyRange = 6;
    private static readonly uint _colorAOETarget = 0xff8080ff;

    public override void Init(BossModule module)
    {
        _isFlare = module.PrimaryActor.CastInfo?.IsSpell(AID.KnockbackPurge) ?? false;
        _knockbackTarget = module.WorldState.Actors.Find(module.PrimaryActor.CastInfo?.TargetID ?? 0);
        if (_knockbackTarget == null)
            module.ReportError(this, "Failed to determine knockback target");
    }

    public override void Update(BossModule module)
    {
        if (_knockbackTarget != null)
        {
            _knockbackPos = _knockbackTarget.Position;
            if (module.PrimaryActor.CastInfo != null)
            {
                _knockbackPos = Components.Knockback.AwayFromSource(_knockbackPos, module.PrimaryActor, _kbDistance);
            }
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (module.PrimaryActor.CastInfo != null && actor == _knockbackTarget && !module.Bounds.Contains(_knockbackPos))
        {
            hints.Add("About to be knocked into wall!");
        }

        float aoeRange = _isFlare ? _flareRange : _holyRange;
        if (module.PrimaryActor.TargetID == actor.InstanceID)
        {
            // i'm the current tank - i should gtfo from raid if i'll get the flare -or- if i'm vulnerable (assuming i'll pop invul not to die)
            if (RaidShouldStack(actor))
            {
                // check that raid is stacked on actor, except for vulnerable target (note that raid never stacks if knockback target is actor, since he is current aoe target)
                if (_knockbackTarget != null && actor.Position.InCircle(_knockbackPos, aoeRange))
                {
                    hints.Add("GTFO from co-tank!");
                }
                if (module.Raid.WithoutSlot().InRadiusExcluding(actor, aoeRange).Count() < 7)
                {
                    hints.Add("Stack with raid!");
                }
            }
            else
            {
                // check that raid is spread from actor
                if (actor == _knockbackTarget)
                {
                    hints.Add("Press invul!");
                }
                if (module.Raid.WithoutSlot().InRadiusExcluding(actor, aoeRange).Any())
                {
                    hints.Add("GTFO from raid!");
                }
            }
        }
        else
        {
            // i'm not the current tank - I should gtfo if tank is invul soaking, from flare or from holy if i'm vulnerable, otherwise stack to current tank
            var target = module.WorldState.Actors.Find(module.PrimaryActor.TargetID);
            if (target == null)
                return;

            if (RaidShouldStack(target) && actor != _knockbackTarget)
            {
                // check that actor is stacked with tank
                if (!actor.Position.InCircle(target.Position, aoeRange))
                {
                    hints.Add("Stack with target!");
                }
            }
            else
            {
                // check that actor stays away from tank
                var pos = actor == _knockbackTarget ? _knockbackPos : actor.Position;
                if (pos.InCircle(target.Position, aoeRange))
                {
                    hints.Add("GTFO from target!");
                }
            }
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (module.PrimaryActor.CastInfo != null && pc == _knockbackTarget && pc.Position != _knockbackPos)
        {
            arena.AddLine(pc.Position, _knockbackPos, ArenaColor.Danger);
            arena.Actor(_knockbackPos, pc.Rotation, ArenaColor.Danger);
        }

        var target = module.WorldState.Actors.Find(module.PrimaryActor.TargetID);
        if (target == null)
            return;

        var targetPos = target == _knockbackTarget ? _knockbackPos : target.Position;
        float aoeRange = _isFlare ? _flareRange : _holyRange;
        if (target == pc)
        {
            // there will be AOE around me, draw all players to help with positioning - note that we use position adjusted for knockback
            foreach (var player in module.Raid.WithoutSlot())
                arena.Actor(player, player.Position.InCircle(targetPos, aoeRange) ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
        }
        else
        {
            // draw AOE source
            arena.Actor(targetPos, target.Rotation, _colorAOETarget);
        }
        arena.AddCircle(targetPos, aoeRange, ArenaColor.Danger);

        // draw vulnerable target
        if (_knockbackTarget != pc && _knockbackTarget != target)
            arena.Actor(_knockbackTarget, ArenaColor.Vulnerable);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TrueHoly2 or AID.TrueFlare2)
            AOEDone = true;
    }

    // we assume that if boss target is the same as knockback target, it's a tank using invul, and so raid shouldn't stack
    private bool RaidShouldStack(Actor bossTarget) =>  !_isFlare && _knockbackTarget != bossTarget;
}
