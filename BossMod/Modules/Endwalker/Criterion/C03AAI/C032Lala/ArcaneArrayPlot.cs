namespace BossMod.Endwalker.Criterion.C03AAI.C032Lala;

class ArcaneArrayPlot : Components.GenericAOEs
{
    public List<AOEInstance> AOEs = [];
    public List<WPos> SafeZoneCenters = [];

    public static readonly AOEShapeRect Shape = new(4, 4, 4);

    public ArcaneArrayPlot(BossModule module) : base(module)
    {
        for (int z = -16; z <= 16; z += 8)
            for (int x = -16; x <= 16; x += 8)
                SafeZoneCenters.Add(Module.Center + new WDir(x, z));
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NBrightPulseFirst or AID.NBrightPulseRest or AID.SBrightPulseFirst or AID.SBrightPulseRest)
            ++NumCasts;
    }

    public void AddAOE(WPos pos, DateTime activation)
    {
        AOEs.Add(new(Shape, pos, default, activation));
        SafeZoneCenters.RemoveAll(c => Shape.Check(c, pos, default));
    }

    protected void Advance(ref WPos pos, ref DateTime activation, WDir offset)
    {
        AddAOE(pos, activation);
        activation = activation.AddSeconds(1.2f);
        pos += offset;
    }
}

class ArcaneArray(BossModule module) : ArcaneArrayPlot(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.ArrowBright)
        {
            var activation = WorldState.FutureTime(4.6f);
            var pos = actor.Position;
            var offset = 8 * actor.Rotation.ToDirection();
            for (int i = 0; i < 5; ++i)
            {
                Advance(ref pos, ref activation, offset);
            }
            pos -= offset;
            pos += Module.InBounds(pos + offset.OrthoL()) ? offset.OrthoL() : offset.OrthoR();
            for (int i = 0; i < 5; ++i)
            {
                Advance(ref pos, ref activation, -offset);
            }

            if (AOEs.Count > 10)
                AOEs.SortBy(aoe => aoe.Activation);
        }
    }
}

class ArcanePlot(BossModule module) : ArcaneArrayPlot(module)
{
    public override void OnActorCreated(Actor actor)
    {
        switch ((OID)actor.OID)
        {
            case OID.ArrowBright:
                AddLine(actor, WorldState.FutureTime(4.6f), false);
                break;
            case OID.ArrowDim:
                AddLine(actor, WorldState.FutureTime(8.2f), true);
                break;
        }
    }

    private void AddLine(Actor actor, DateTime activation, bool preAdvance)
    {
        var pos = actor.Position;
        var offset = 8 * actor.Rotation.ToDirection();
        if (preAdvance)
            pos += offset;

        do
        {
            Advance(ref pos, ref activation, offset);
        }
        while (Module.InBounds(pos));

        AOEs.SortBy(aoe => aoe.Activation);
    }
}
