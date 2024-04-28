namespace BossMod.Endwalker.Savage.P1SErichthonios;

// state related to knockback + aoe mechanic
// TODO: i'm not quite happy with implementation, consider revising...
class Knockback : BossComponent
{
    public bool AOEDone { get; private set; }
    private readonly bool _isFlare; // true -> purge aka flare (stay away from MT), false -> grace aka holy (stack to MT)
    private readonly Actor? _knockbackTarget;
    private WPos _knockbackPos;

    private const float _kbDistance = 15;
    private const float _flareRange = 24; // max range is 50, but it has distance falloff - linear up to ~24, then constant ~3k
    private const float _holyRange = 6;
    private const uint _colorAOETarget = 0xff8080ff;

    public Knockback(BossModule module) : base(module)
    {
        _isFlare = Module.PrimaryActor.CastInfo?.IsSpell(AID.KnockbackPurge) ?? false;
        _knockbackTarget = WorldState.Actors.Find(Module.PrimaryActor.CastInfo?.TargetID ?? 0);
        if (_knockbackTarget == null)
            ReportError("Failed to determine knockback target");
    }

    public override void Update()
    {
        if (_knockbackTarget != null)
        {
            _knockbackPos = _knockbackTarget.Position;
            if (Module.PrimaryActor.CastInfo != null)
            {
                _knockbackPos = Components.Knockback.AwayFromSource(_knockbackPos, Module.PrimaryActor, _kbDistance);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Module.PrimaryActor.CastInfo != null && actor == _knockbackTarget && !Module.InBounds(_knockbackPos))
        {
            hints.Add("About to be knocked into wall!");
        }

        float aoeRange = _isFlare ? _flareRange : _holyRange;
        if (Module.PrimaryActor.TargetID == actor.InstanceID)
        {
            // i'm the current tank - i should gtfo from raid if i'll get the flare -or- if i'm vulnerable (assuming i'll pop invul not to die)
            if (RaidShouldStack(actor))
            {
                // check that raid is stacked on actor, except for vulnerable target (note that raid never stacks if knockback target is actor, since he is current aoe target)
                if (_knockbackTarget != null && actor.Position.InCircle(_knockbackPos, aoeRange))
                {
                    hints.Add("GTFO from co-tank!");
                }
                if (Raid.WithoutSlot().InRadiusExcluding(actor, aoeRange).Count() < 7)
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
                if (Raid.WithoutSlot().InRadiusExcluding(actor, aoeRange).Any())
                {
                    hints.Add("GTFO from raid!");
                }
            }
        }
        else
        {
            // i'm not the current tank - I should gtfo if tank is invul soaking, from flare or from holy if i'm vulnerable, otherwise stack to current tank
            var target = WorldState.Actors.Find(Module.PrimaryActor.TargetID);
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

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Module.PrimaryActor.CastInfo != null && pc == _knockbackTarget && pc.Position != _knockbackPos)
        {
            Arena.AddLine(pc.Position, _knockbackPos, ArenaColor.Danger);
            Arena.Actor(_knockbackPos, pc.Rotation, ArenaColor.Danger);
        }

        var target = WorldState.Actors.Find(Module.PrimaryActor.TargetID);
        if (target == null)
            return;

        var targetPos = target == _knockbackTarget ? _knockbackPos : target.Position;
        float aoeRange = _isFlare ? _flareRange : _holyRange;
        if (target == pc)
        {
            // there will be AOE around me, draw all players to help with positioning - note that we use position adjusted for knockback
            foreach (var player in Raid.WithoutSlot())
                Arena.Actor(player, player.Position.InCircle(targetPos, aoeRange) ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
        }
        else
        {
            // draw AOE source
            Arena.Actor(targetPos, target.Rotation, _colorAOETarget);
        }
        Arena.AddCircle(targetPos, aoeRange, ArenaColor.Danger);

        // draw vulnerable target
        if (_knockbackTarget != pc && _knockbackTarget != target)
            Arena.Actor(_knockbackTarget, ArenaColor.Vulnerable);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TrueHoly2 or AID.TrueFlare2)
            AOEDone = true;
    }

    // we assume that if boss target is the same as knockback target, it's a tank using invul, and so raid shouldn't stack
    private bool RaidShouldStack(Actor bossTarget) => !_isFlare && _knockbackTarget != bossTarget;
}
