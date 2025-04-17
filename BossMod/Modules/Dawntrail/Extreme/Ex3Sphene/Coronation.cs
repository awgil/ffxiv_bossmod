namespace BossMod.Dawntrail.Extreme.Ex3Sphene;

class Coronation(BossModule module) : Components.GenericAOEs(module, AID.RuthlessRegalia)
{
    public struct Group
    {
        public required Actor Source;
        public Actor? LeftPartner;
        public Actor? RightPartner;

        public readonly bool Contains(Actor player) => LeftPartner == player || RightPartner == player;
    }

    public readonly List<Group> Groups = [];
    private DateTime _activation;

    private static readonly AOEShapeRect _shape = new(100, 6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Groups.Select(g => new AOEInstance(_shape, g.Source.Position, g.Source.Rotation, _activation));

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var index = Groups.FindIndex(g => g.Contains(pc));
        return index >= 0 && Groups[index].Contains(player) ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (ref var g in Groups.AsSpan())
        {
            Arena.Actor(g.Source, ArenaColor.Object, true);
            if (g.Contains(pc))
            {
                if (g.LeftPartner != null)
                    Arena.AddLine(g.LeftPartner.Position, g.Source.Position, ArenaColor.Danger);
                if (g.RightPartner != null)
                    Arena.AddLine(g.RightPartner.Position, g.Source.Position, ArenaColor.Danger);
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.CoronationL or TetherID.CoronationR)
        {
            _activation = WorldState.FutureTime(10.1f);
            var index = Groups.FindIndex(g => g.Source.InstanceID == tether.Target);
            if (index < 0 && WorldState.Actors.Find(tether.Target) is var target && target != null)
            {
                index = Groups.Count;
                Groups.Add(new() { Source = target });
            }
            if (index >= 0)
            {
                ref var group = ref Groups.Ref(index);
                ref var partner = ref (TetherID)tether.ID == TetherID.CoronationL ? ref group.LeftPartner : ref group.RightPartner;
                if (partner != null)
                    ReportError($"Both {source} and {partner} have identical tether");
                partner = source;
            }
        }
    }
}

class AtomicRay(BossModule module) : Components.SpreadFromCastTargets(module, AID.AtomicRayAOE, 16, false);
