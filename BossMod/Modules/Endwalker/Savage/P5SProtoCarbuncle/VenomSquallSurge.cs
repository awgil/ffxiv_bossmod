namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

class VenomDrops : Components.LocationTargetedAOEs
{
    public VenomDrops() : base(ActionID.MakeSpell(AID.VenomDrops), 5) { }
}

class VenomSquallSurge : BossComponent
{
    public enum Mechanic { None, Rain, Drops, Pool }

    public int Progress { get; private set; }
    public bool _reverse;

    private static readonly float _radius = 5;

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        switch (NextMechanic)
        {
            case Mechanic.Rain:
                if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _radius).Any())
                    hints.Add("Spread!");
                break;
            case Mechanic.Pool:
                if (module.Raid.WithoutSlot().InRadius(actor.Position, _radius).Count(p => p.Role == Role.Healer) != 1)
                    hints.Add("Stack with healer!");
                break;
        }
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        hints.Add(_reverse ? "Order: stack -> mid -> spread" : "Order: spread -> mid -> stack");
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return NextMechanic == Mechanic.Pool && player.Role == Role.Healer ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        switch (NextMechanic)
        {
            case Mechanic.Rain: // spreads
                arena.AddCircle(pc.Position, _radius, ArenaColor.Danger);
                break;
            case Mechanic.Drops: // bait
                foreach (var p in module.Raid.WithoutSlot())
                    arena.AddCircle(p.Position, _radius, ArenaColor.Danger);
                break;
            case Mechanic.Pool: // party stacks
                foreach (var p in module.Raid.WithoutSlot().Where(p => p.Role == Role.Healer))
                    arena.AddCircle(p.Position, _radius, ArenaColor.Danger);
                break;
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.VenomSurge:
                _reverse = true;
                break;
            case AID.VenomDrops:
                if (NextMechanic == Mechanic.Drops)
                    ++Progress;
                break;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.VenomRain:
                if (NextMechanic == Mechanic.Rain)
                    ++Progress;
                break;
            case AID.VenomPool:
                if (NextMechanic == Mechanic.Pool)
                    ++Progress;
                break;
        }
    }

    private Mechanic NextMechanic => Progress switch
    {
        0 => _reverse ? Mechanic.Pool : Mechanic.Rain,
        1 => Mechanic.Drops,
        2 => _reverse ? Mechanic.Rain : Mechanic.Pool,
        _ => Mechanic.None
    };
}
