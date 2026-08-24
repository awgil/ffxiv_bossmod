namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

class VenomDrops(BossModule module) : Components.StandardAOEs(module, AID.VenomDrops, 5);

class VenomSquallSurge(BossModule module) : BossComponent(module)
{
    public enum Mechanic { None, Rain, Drops, Pool }

    public int Progress { get; private set; }
    public bool _reverse;

    private const float _radius = 5;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        switch (NextMechanic)
        {
            case Mechanic.Rain:
                if (Raid.WithoutSlot().InRadiusExcluding(actor, _radius).Any())
                    hints.Add("Spread!");
                break;
            case Mechanic.Pool:
                if (Raid.WithoutSlot().InRadius(actor.Position, _radius).Count(p => p.Role == Role.Healer) != 1)
                    hints.Add("Stack with healer!");
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add(_reverse ? "Order: stack -> mid -> spread" : "Order: spread -> mid -> stack");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => NextMechanic == Mechanic.Pool && player.Role == Role.Healer ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        switch (NextMechanic)
        {
            case Mechanic.Rain: // spreads
                Arena.AddCircle(pc.Position, _radius, ArenaColor.Danger);
                break;
            case Mechanic.Drops: // bait
                foreach (var p in Raid.WithoutSlot())
                    Arena.AddCircle(p.Position, _radius, ArenaColor.Danger);
                break;
            case Mechanic.Pool: // party stacks
                foreach (var p in Raid.WithoutSlot().Where(p => p.Role == Role.Healer))
                    Arena.AddCircle(p.Position, _radius, ArenaColor.Danger);
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
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

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
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
