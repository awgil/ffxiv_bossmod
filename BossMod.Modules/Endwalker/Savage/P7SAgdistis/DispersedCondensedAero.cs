namespace BossMod.Endwalker.Savage.P7SAgdistis;

// TODO: currently we don't expose aggro to components, so we just assume tanks are doing their job...
class DispersedCondensedAero(BossModule module) : BossComponent(module)
{
    public bool Done { get; private set; }
    private bool _condensed;

    private const float _radiusDispersed = 8;
    private const float _radiusCondensed = 6;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_condensed)
        {
            if (Module.PrimaryActor.TargetID == actor.InstanceID)
            {
                hints.Add("Stack with other tank or press invuln!", Raid.WithoutSlot().InRadiusExcluding(actor, _radiusCondensed).Any(a => a.Role != Role.Tank));
            }
            else
            {
                var tank = WorldState.Actors.Find(Module.PrimaryActor.TargetID);
                if (tank != null && actor.Position.InCircle(tank.Position, _radiusCondensed))
                {
                    hints.Add("GTFO from tank!");
                }
            }
        }
        else
        {
            if (actor.Role == Role.Tank)
            {
                hints.Add("GTFO from raid!", Raid.WithoutSlot().InRadiusExcluding(actor, _radiusDispersed).Any());
            }
            else if (Raid.WithoutSlot().Where(a => a.Role == Role.Tank).InRadius(actor.Position, _radiusDispersed).Any())
            {
                hints.Add("GTFO from tanks!");
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => (_condensed ? Module.PrimaryActor.TargetID == player.InstanceID : player.Role == Role.Tank) ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_condensed)
        {
            var tank = WorldState.Actors.Find(Module.PrimaryActor.TargetID);
            if (tank != null)
                Arena.AddCircle(tank.Position, _radiusCondensed, ArenaColor.Danger);
        }
        else
        {
            foreach (var tank in Raid.WithoutSlot().Where(a => a.Role == Role.Tank))
                Arena.AddCircle(tank.Position, _radiusDispersed, ArenaColor.Danger);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CondensedAero)
            _condensed = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CondensedAeroAOE or AID.DispersedAeroAOE)
            Done = true;
    }
}
