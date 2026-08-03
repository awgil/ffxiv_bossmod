namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA4ProtoOzma;

sealed class BlackHole(BossModule module) : Components.GenericTowersOpenWorld(module, prioritizeEmpty: true)
{
    private const string Hint1 = "Stand inside a black hole buffer!";
    private const string Hint2 = "There are uncovered black hole buffers!";

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TransfigurationSphere1:
            case (uint)AID.TransfigurationSphere2:
            case (uint)AID.TransfigurationSphere3:
                var buffers = Module.Enemies((uint)OID.BlackHoleBuffer); // for some reason the buffers have slightly different coordinates than their collision data, not sure which is correct
                var count = buffers.Count;
                var soakers = Soakers(Module);
                for (var i = 0; i < count; ++i)
                {
                    var buffer = buffers[i].Position;
                    if (Arena.InBounds(buffer) && (int)buffer.Z != 44f) // filter out irrelevant actors, unfortunately OID is also used for other stuff on this map
                    {
                        Towers.Add(new(buffer, 2f, 1, 99, soakers, activation: WorldState.FutureTime(9.1d)));
                    }
                }
                break;

            case (uint)AID.BlackHole:
                Towers.Clear();
                break;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = Towers.Count;
        if (count == 0)
            return;
        base.DrawArenaForeground(pcSlot, pc);
        var towers = CollectionsMarshal.AsSpan(Towers);
        for (var i = 0; i < count; ++i)
        {
            var t = towers[i];
            if (t.NumInside(Module) == 0)
                Arena.ZoneCircleOutline(t.Position, 2f, Colors.Vulnerable, 3f);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = Towers.Count;
        if (count == 0)
            return;
        var uncovered = false;
        var isInside = false;
        var towers = CollectionsMarshal.AsSpan(Towers);
        for (var i = 0; i < count; ++i)
        {
            var t = towers[i];
            if (t.NumInside(Module) == 0)
                uncovered = true;
            else if (t.IsInside(actor))
                isInside = true;
        }

        if (uncovered)
            hints.Add(Hint2);
        hints.Add(Hint1, !isInside);
    }
}
