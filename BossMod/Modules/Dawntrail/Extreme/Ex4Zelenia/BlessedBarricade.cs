namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

sealed class RosebloodDrop(BossModule module) : Components.Adds(module, (uint)OID.RosebloodDrop2);

sealed class Towers2(BossModule module) : Components.GenericTowers(module)
{
    private BitMask forbidden;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Explosion2:
                Towers.Add(new(spell.LocXZ, 3f, 3, 3, forbidden, Module.CastFinishAt(spell)));
                break;
            case (uint)AID.SpearpointPush1:
            case (uint)AID.SpearpointPush2:
                forbidden = default;
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.SpearpointPush)
        {
            forbidden.Set(Raid.FindSlot(targetID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Explosion2)
        {
            Towers.Clear();
        }
    }
}

sealed class SpearpointPushAOE(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [with(2)];
    private readonly AOEShapeRect rect = new(33f, 37f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SpearpointPush1 or (uint)AID.SpearpointPush2)
        {
            AOEs.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SpearpointPush1 or (uint)AID.SpearpointPush2)
        {
            AOEs.Clear();
        }
    }
}

sealed class SpearpointPushBait(BossModule module) : Components.GenericBaitAway(module, onlyShowOutlines: true)
{
    private static readonly AOEShapeRect rect = new(32f, 37f, 1f);
    private Angle offset;
    private static readonly Angle a90 = 90f.Degrees();
    private static readonly WDir dir = new(default, 8f);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.ZeleniasShade)
        {
            offset = id switch
            {
                0x0C90 => -a90,
                0x0C91 => a90,
                _ => default
            };
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.SpearpointPush)
        {
            var target = WorldState.Actors.Find(tether.Target);
            if (target is Actor t)
            {
                CurrentBaits.Add(new(source, t, rect, WorldState.FutureTime(6.7d)));
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SpearpointPush1 or (uint)AID.SpearpointPush2)
        {
            CurrentBaits.Clear();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        var count = CurrentBaits.Count;
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < count; ++i)
        {
            ref var b = ref baits[i];
            if (b.Target == pc)
            {
                var pos = b.Source.Position;
                Arena.AddLine(pos, pc.Position);
                var offsetDir = pos.X < 100f ? offset == -a90 ? 1f : -1f : offset == a90 ? 1f : -1f;
                Arena.ZoneCircleOutline(pos + offsetDir * dir, 1f, Colors.Safe);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var baits = ActiveBaitsOn(actor);
        var count = baits.Count;
        if (count == 0)
            base.AddAIHints(slot, actor, assignment, hints);
        else
        {
            ref var b = ref baits.Ref(0);
            var pos = b.Source.Position;
            var offsetDir = pos.X < 100f ? offset == -a90 ? 1f : -1f : offset == a90 ? 1f : -1f;
            hints.AddForbiddenZone(new SDInvertedCircle(pos + offsetDir * dir, 1f));
        }
    }

    public override void Update()
    {
        var count = CurrentBaits.Count;
        if (count == 0)
            return;
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < count; ++i)
        {
            ref var b = ref baits[i];
            b.CustomRotation = b.Source.Rotation + offset;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var baits = ActiveBaitsOn(actor);
        var count = baits.Count;
        if (count != 0)
        {
            hints.Add("Bait away at marked spot!");
        }
    }
}
