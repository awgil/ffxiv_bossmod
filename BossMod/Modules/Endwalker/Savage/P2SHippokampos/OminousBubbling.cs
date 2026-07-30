namespace BossMod.Endwalker.Savage.P2SHippokampos;

class OminousBubbling(BossModule module) : Components.CastCounter(module, (uint)AID.OminousBubblingAOE)
{
    private const float _radius = 6;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var healersInRange = Raid.WithoutSlot(false, true, true).Where(a => a.Role == Role.Healer).InRadius(actor.Position, _radius).Count();
        if (healersInRange > 1)
            hints.Add("Hit by two aoes!");
        else if (healersInRange == 0)
            hints.Add("Stack with healer!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var player in Raid.WithoutSlot(false, true, true))
        {
            if (player.Role == Role.Healer)
            {
                Arena.Actor(player, Colors.Danger);
                Arena.ZoneCircleOutline(player.Position, _radius, Colors.Danger);
            }
            else
            {
                Arena.Actor(player, Colors.PlayerGeneric);
            }
        }
    }
}
