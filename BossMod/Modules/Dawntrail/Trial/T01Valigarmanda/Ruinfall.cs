namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

sealed class RuinfallAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RuinfallAOE, 6f);

sealed class RuinfallKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.RuinfallKB, 21f, stopAfterWall: true, kind: Kind.DirForward)
{
    private readonly RuinfallTower _tower = module.FindComponent<RuinfallTower>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
        {
            return;
        }
        ref readonly var c = ref Casters.Ref(0);
        var act = c.Activation;
        if (!IsImmune(slot, act))
        {
            if (actor.Role != Role.Tank)
            {
                hints.AddForbiddenZone(new SDKnockbackInAABBRectFixedDirection(Arena.Center, 21f * c.Direction.ToDirection(), 19f, 14f), act); // rect intentionally slightly smaller to prevent sus knockback
                return;
            }
            var towers = _tower.Towers;
            var count = towers.Count;
            if (count == 0)
            {
                return;
            }
            ref var t0 = ref towers.Ref(0);
            var isDelayDeltaLow = (act - WorldState.CurrentTime).TotalSeconds < 5d;

            if (isDelayDeltaLow && t0.IsInside(actor))
            {
                hints.ActionsToExecute.Push(ActionDefinitions.Armslength, actor, ActionQueue.Priority.High);
            }
        }
    }
}

sealed class RuinfallTower(BossModule module) : Components.CastTowers(module, (uint)AID.RuinfallTower, 6f, 2, 2)
{
    public override void Update()
    {
        var count = Towers.Count;
        if (count == 0)
        {
            return;
        }
        var party = Module.Raid.WithSlot(true, true, true);
        var len = party.Length;
        BitMask forbidden = default;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var p = ref party[i];
            if (p.Item2.Role != Role.Tank)
            {
                forbidden.Set(p.Item1);
            }
        }
        var towers = CollectionsMarshal.AsSpan(Towers);
        for (var i = 0; i < count; ++i)
        {
            ref var t = ref towers[i];
            t.ForbiddenSoakers = forbidden;
        }
    }
}
