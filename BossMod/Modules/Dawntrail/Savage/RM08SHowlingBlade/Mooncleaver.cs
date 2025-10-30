namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class Mooncleaver(BossModule module) : Components.StandardAOEs(module, AID.Mooncleaver, new AOEShapeCircle(8));

class TemporalBlast(BossModule module) : Components.GenericStackSpread(module, alwaysShowSpreads: true)
{
    public int NumFinishedStacks;
    public int NumFinishedSpreads;

    // 10.1s delay between icon and cast
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Target)
            if (actor.Role == Role.Tank)
                Spreads.Add(new(actor, 16, WorldState.FutureTime(10.1f)));
            else
                Stacks.Add(new(actor, 6, activation: WorldState.FutureTime(10.1f), forbiddenPlayers: Raid.WithSlot().Where(r => r.Item2.Role == Role.Tank).Mask()));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.GeotemporalBlast:
                NumFinishedStacks++;
                Stacks.Clear();
                break;
            case AID.AerotemporalBlast:
                NumFinishedSpreads++;
                Spreads.Clear();
                break;
        }
    }
}

class ElementalPurgeBind(BossModule module) : BossComponent(module)
{
    public bool Bound;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Bind)
            Bound = true;
    }
}

class HuntersHarvest(BossModule module) : Components.GenericBaitAway(module, AID.HuntersHarvest)
{
    public bool Active { get; private set; }
    private readonly TemporalBlast? _ab = module.FindComponent<TemporalBlast>();

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Target)
            Active = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (spell.Action == WatchedAction)
            Active = false;
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        if (Active)
        {
            var boss = Module.Enemies(OID.BossP2).FirstOrDefault();
            if (boss != null && Raid.TryFindSlot(boss.TargetID, out var tar))
                CurrentBaits.Add(new(boss, Raid[tar]!, new AOEShapeCone(40, 100.Degrees())));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        var abTank = _ab?.ActiveSpreads.Select(s => s.Target).FirstOrDefault();

        if (actor.Role == Role.Tank && CurrentBaits.Any(b => b.Target == abTank))
        {
            if (abTank == actor)
                hints.Add("Shirk!");
            else
                hints.Add("Provoke!");
        }
    }
}
