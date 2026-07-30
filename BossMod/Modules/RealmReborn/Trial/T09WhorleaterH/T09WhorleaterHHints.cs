namespace BossMod.RealmReborn.Trial.T09WhorleaterH;

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        var converters = Module.Enemies((uint)OID.Converter);
        var converter = converters.Count != 0 ? converters[0] : null;
        if (converter != null && converter.IsTargetable)
            hints.Add($"Activate the {converter.Name} or wipe!");

        var wavetooth = Module.Enemies((uint)OID.WavetoothSahagin);
        var countW = wavetooth.Count;
        for (var i = 0; i < countW; ++i)
        {
            var w = wavetooth[i];
            if (w.IsTargetable && !w.IsDead)
            {
                hints.Add("Kill Sahagins or lose control!");
                break;
            }
        }

        var spumes = Module.Enemies((uint)OID.Spume);
        var countS = spumes.Count;
        for (var i = 0; i < countS; ++i)
        {
            var s = spumes[i];
            if (s.IsTargetable && !s.IsDead)
            {
                hints.Add("Destroy the spumes!");
                return;
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var tails = Module.Enemies((uint)OID.Tail);
        var tail = tails.Count != 0 ? tails[0] : null;
        // SID 2124 = Bluemage Tank Mimikry
        if (tail != null && tail.FindStatus((uint)SID.Invincibility) == null && tail.FindStatus((uint)SID.MantleOfTheWhorl) != null)
        {
            if ((actor.Class.GetClassCategory() is ClassCategory.Caster or ClassCategory.Healer || actor.Class is Class.BLU && actor.FindStatus(2124u) == null) && actor.TargetID == tail.InstanceID)
                hints.Add("Attack the head! (Attacking the tail will reflect damage onto you)");
            else if (actor.Class.GetClassCategory() is ClassCategory.PhysRanged && actor.TargetID == Module.PrimaryActor.InstanceID)
                hints.Add("Attack the tail! (Attacking the head will reflect damage onto you)");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var converters = Module.Enemies((uint)OID.Converter);
        var converter = converters.Count != 0 ? converters[0] : null;
        if (converter != null && converter.IsTargetable)
            Arena.ZoneCircleOutline(converter.Position, 1.4f, Colors.Safe);
    }
}
