namespace BossMod.Dawntrail.Criterion.C01AMT.C011DaryaTheSeaMaid;

class AlluringOrder(BossModule module) : Components.RaidwideCast(module, (uint)AID.AlluringOrder);

class AlluringOrderForcedMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3.0f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace);

// TODO add priority order to this - DPS flex, but add configuration to module so it can be picked by the player
//  - No need to decide who goes with what stack, just highlight their safe spot? - always 2 spots -> this way we can just check if one side has two stacks or not
class Tidalspout(BossModule module) : Components.StatusStackSpread(module, (uint)SID.TidalspoutTarget, 0, 6, 0)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.TidalspoutTarget)
        {
            AddStack(actor, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.TidalspoutTarget)
        {
            Stacks.Clear();
        }
    }
}

// TODO refer to Tidalspout TODO 
class SwimmingInTheAir(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    public int activeAOEs = 0; // Used for the timeline

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BlueOrb)
        {
            aoes.Add(new(new AOEShapeCircle(12f), actor.Position, default, default, Colors.AOE));
            activeAOEs++;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Hydrofall)
        {
            NumCasts++;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return CollectionsMarshal.AsSpan(aoes);
    }
}

class SunkenTreasure(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly List<Actor> blueObjects = [];
    public int maxShow = 5;

    private readonly AOEShapeCircle SphereAOE = new(18f);
    private readonly AOEShapeDonut DonutAOE = new(6f, 20f);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        aoes.Clear();

        var shown = 0;
        foreach (var blueObject in blueObjects)
        {
            if (shown >= maxShow)
            {
                break;
            }

            AOEShape shape = ((OID)blueObject.OID == OID.BlueSphere) ? SphereAOE : DonutAOE;
            aoes.Add(new AOEInstance(shape, blueObject.Position));
            shown++;
        }

        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID is (uint)OID.BlueSphere or (uint)OID.DonutSphere)
        {
            if (state == (uint)STATE.FirstState)
            {
                blueObjects.Add(actor);
            }

            if (state == (uint)STATE.ThirdState)
            {
                blueObjects.Remove(actor);
                NumCasts++;
            }
        }
    }
}

class SunkenTreasure2 : SunkenTreasure
{
    public SunkenTreasure2(BossModule module) : base(module)
    {
        maxShow = 6;
    }
}