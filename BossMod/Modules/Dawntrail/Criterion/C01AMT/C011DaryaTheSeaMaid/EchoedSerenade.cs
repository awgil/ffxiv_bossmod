namespace BossMod.Dawntrail.Criterion.C01AMT.C011DaryaTheSeaMaid;

class EchoedSerenade(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    public int maxShow = 2;

    private readonly List<uint> orderVFX = [];
    private int indexVFX = 0;
    private DateTime lastVFX = DateTime.MinValue;
    private readonly TimeSpan cooldown = TimeSpan.FromSeconds(0.5);

    private record VfxInfo(string Name, uint Oid, AOEShape Shape);
    private static readonly Dictionary<uint, VfxInfo> vfxInfo = new()
    {
        [2741u] = new VfxInfo("horse", (uint)OID.SeabornSteed, new AOEShapeRect(40f, 4f)),
        [2746u] = new VfxInfo("bird", (uint)OID.SeabornShrike, new AOEShapeCone(45f, 30f.Degrees())),
        [2744u] = new VfxInfo("crab", (uint)OID.SeabornSoldier, new AOEShapeRect(40f, 4f)),
        [2745u] = new VfxInfo("bomb", (uint)OID.SeabornServant, new AOEShapeCone(20f, 180f.Degrees())),
        [2743u] = new VfxInfo("turtle", (uint)OID.SeabornSteward, new AOEShapeRect(40f, 4f)),
    };

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EchoedReprise)
        {
            indexVFX = 0;
            NumCasts = 0;
        }
    }

    public override void OnEventVFX(Actor actor, uint vfxID, ulong targetID)
    {
        if (actor.OID == (uint)OID.DaryaTheSeaMaid)
        {
            orderVFX.Add(vfxID);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Watersong or (uint)AID.Watersong1 or (uint)AID.Watersong2 or (uint)AID.Watersong3 or (uint)AID.Watersong4)
        {
            if ((WorldState.CurrentTime - lastVFX) < cooldown)
            {
                return;
            }

            if (indexVFX < orderVFX.Count)
            {
                ++indexVFX;
                lastVFX = WorldState.CurrentTime;
                ++NumCasts;
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (orderVFX.Count == 0)
        {
            hints.Add("Order: ????");
            return;
        }

        var remaining = orderVFX.Skip(indexVFX);
        hints.Add("Order: " + string.Join(", ", remaining.Select(v => vfxInfo[v].Name)));
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        aoes.Clear();

        var upcoming = orderVFX.Skip(indexVFX).Take(maxShow);
        var show = 0;
        foreach (var vfx in upcoming)
        {
            if (!vfxInfo.TryGetValue(vfx, out var info))
            {
                continue;
            }

            var colour = show == 0 ? Colors.Danger : Colors.AOE;
            foreach (var animal in Module.Enemies(info.Oid))
            {
                aoes.Add(new(info.Shape, animal.Position, animal.Rotation, default, colour, show == 0));
            }
            show++;
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

class EchoedSerenade2 : EchoedSerenade
{
    public EchoedSerenade2(BossModule module) : base(module)
    {
        maxShow = 1;
    }
}

class Hydrobullet(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Hydrobullet1, 15f);