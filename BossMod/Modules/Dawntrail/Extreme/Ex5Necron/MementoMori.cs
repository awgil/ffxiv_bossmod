namespace BossMod.Dawntrail.Extreme.Ex5Necron;

class MementoMoriLine(BossModule module) : Components.GroupedAOEs(module, [AID.MementoMoriDarkRight, AID.MementoMoriDarkLeft], new AOEShapeRect(100, 6))
{
    public bool HighlightSafe = true;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        if (Casters.Count > 0 && HighlightSafe)
        {
            var isTank = pc.Role == Role.Tank;

            var tanksLeft = (AID)Casters[0].CastInfo!.Action.ID == AID.MementoMoriDarkLeft;

            WPos safeOrig = (isTank == tanksLeft) ? new(88, 85) : new(112, 85);

            Arena.ZoneRect(safeOrig, safeOrig + new WDir(0, 30), 6, ArenaColor.SafeFromAOE);
        }
    }
}

class MementoMoriVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    public bool Active { get; private set; }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Active)
            yield return new AOEInstance(new AOEShapeRect(30, 6), new(100, 85));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.MementoMoriDarkLeft or AID.MementoMoriDarkRight)
            Active = true;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x18 && state == 0x00080004)
            Active = false;
    }
}

class SmiteOfGloom(BossModule module) : Components.SpreadFromCastTargets(module, AID.SmiteOfGloom, 10);

class CenterChokingGrasp(BossModule module) : Components.GenericAOEs(module, AID.ChokingGraspCast1)
{
    private readonly List<(Actor, DateTime)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (c, predicted) in _casters)
        {
            var activation = c.CastInfo == null ? predicted : Module.CastFinishAt(c.CastInfo);

            yield return new AOEInstance(new AOEShapeRect(24, 3), c.Position, c.Rotation, activation);
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.IcyHandsP1 && id == 0x11D2)
            _casters.Add((actor, WorldState.FutureTime(5.7f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _casters.RemoveAll(c => c.Item1 == caster);
        }
    }
}
