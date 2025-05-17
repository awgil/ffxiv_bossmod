namespace BossMod.Dawntrail.Savage.RM06SSugarRiot;

class MousseDripStack(BossModule module) : Components.GenericStackSpread(module)
{
    private readonly int[] Counters = new int[PartyState.MaxPartySize];
    public int NumCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MousseDrip)
        {
            var isle = RM06SSugarRiot.GetIsland(actor.Position);
            Stacks.Add(new(actor, 5, minSize: 2, maxSize: 2, forbiddenPlayers: Raid.WithSlot().WhereActor(a => RM06SSugarRiot.GetIsland(a.Position) != isle).Mask()));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MousseDrip)
        {
            NumCasts++;
            if (Raid.TryFindSlot(spell.MainTargetID, out var slot))
            {
                Counters[slot]++;
                if (Counters[slot] >= 4)
                    Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var shouldSpread = IsSpreadTarget(player);
        var shouldStack = IsStackTarget(player);
        return shouldSpread || shouldSpread ? PlayerPriority.Danger
            : shouldStack ? PlayerPriority.Interesting
            : Active ? PlayerPriority.Normal : PlayerPriority.Irrelevant;
    }
}

class MousseDrip(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID.MousseDrip, m => m.Enemies(0x1EBD92).Where(e => e.EventState != 7), 2);

class Moussacre(BossModule module) : Components.GenericBaitAway(module)
{
    private DateTime Activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MoussacreVisual)
            Activation = Module.CastFinishAt(spell, 0.9f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Moussacre)
        {
            NumCasts++;
            Activation = default;
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();
        if (Activation != default)
        {
            foreach (var player in Raid.WithoutSlot().SortedByRange(Module.PrimaryActor.Position).Take(4))
                CurrentBaits.Add(new(Module.PrimaryActor, player, new AOEShapeCone(60, 15.Degrees()), WorldState.FutureTime(10)));
        }
    }
}
