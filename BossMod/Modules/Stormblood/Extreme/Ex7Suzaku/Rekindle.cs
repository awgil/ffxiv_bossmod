namespace BossMod.Stormblood.Extreme.Ex7Suzaku;

abstract class Rekindle(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, (uint)AID.Rekindle, 6f, 5.1f);

sealed class RekindleP1(BossModule module) : Rekindle(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var feathers = Module.Enemies((uint)OID.ScarletTailFeather);
        if (feathers.Count != 0 || !IsSpreadTarget(actor))
            return;
        var scarletLadies = Module.Enemies((uint)OID.ScarletLady);
        var count = scarletLadies.Count;
        for (var i = 0; i < count; ++i)
        {
            hints.AddForbiddenZone(new SDCircle(scarletLadies[i].Position, 7.12f));
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        var feathers = Module.Enemies((uint)OID.ScarletTailFeather);
        if (feathers.Count != 0 || !IsSpreadTarget(pc))
            return;
        var birds = Module.Enemies((uint)OID.ScarletLady);
        var count = birds.Count;
        for (var i = 0; i < count; ++i)
        {
            var b = birds[i];
            if (b.IsDead)
                Arena.ZoneCircleOutline(b.Position, 1.12f, Colors.Vulnerable);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        var feathers = Module.Enemies((uint)OID.ScarletTailFeather);
        if (IsSpreadTarget(actor))
        {
            if (feathers.Count == 0)
            {
                hints.Add("Avoid intersecting bird hitboxes!");
            }
            else
            {
                hints.Add("Intersect the hitbox of a bird and pull it out of AOEs!");
            }
        }
    }
}

sealed class RekindleP2(BossModule module) : Rekindle(module);
