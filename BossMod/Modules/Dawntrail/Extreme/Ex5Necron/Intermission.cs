namespace BossMod.Dawntrail.Extreme.Ex5Necron;

sealed class FearOfDeathAOE2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FearOfDeathAOE2, 3f);
sealed class MutedStruggle(BossModule module) : Components.BaitAwayCast(module, (uint)AID.MutedStruggle, ChokingGrasp.Rect, endsOnCastEvent: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class DarknessOfEternity(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.DarknessOfEternityVisual, (uint)AID.DarknessOfEternity, 6.4d);

sealed class SpreadingFearEnrage(BossModule module) : Components.CastHint(module, (uint)AID.SpreadingFear1, "Enrage!", true)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class SpreadingFearInterrupt(BossModule module) : Components.CastInterruptHint(module, (uint)AID.SpreadingFear2)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class ChokingGraspTB(BossModule module) : Components.BaitAwayCast(module, (uint)AID.ChokingGraspTB, ChokingGrasp.Rect, endsOnCastEvent: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class ChokingGraspHealer(BossModule module) : Components.BaitAwayCast(module, (uint)AID.ChokingGraspHealer, ChokingGrasp.Rect, endsOnCastEvent: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class ChillingFingers(BossModule module) : Components.BaitAwayCast(module, (uint)AID.ChillingFingers, ChokingGrasp.Rect, endsOnCastEvent: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class ChokingGrasp3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChokingGraspAOE3, ChokingGrasp.Rect)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class Slow(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Slow, "Slow", "slowed")
{
    public override bool KeepOnPhaseChange => true;
}

sealed class Prisons(BossModule module) : BossComponent(module)
{
    public BitMask Doomed;
    public int NumDooms;
    private BitMask activeTeleporters;
    public override bool KeepOnPhaseChange => true;

    private readonly WPos[] prisonPositions = [new(100f, -100f), new(300f, -100f), new(300f, 100f), new(300f, 300f),
    new(100f, 300f), new(-100f, 300f), new(-100f, 100f), new(-100f, -100f)];

    public override void OnMapEffect(byte index, uint state)
    {
        if (index <= 0x07)
        {
            switch (state)
            {
                case 0x00020001u:
                    activeTeleporters.Set(index);
                    break;
                case 0x00080004u:
                    activeTeleporters.Clear(index);
                    break;
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
        {
            Doomed.Set(Raid.FindSlot(actor.InstanceID));
            ++NumDooms;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
        {
            Doomed.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (pc.PosRot.Y < -100f)
        {
            var playerPos = pc.Position;
            for (var i = 0; i < 8; ++i)
            {
                var pos = prisonPositions[i];
                if (playerPos.InSquare(pos, 50f))
                {
                    if (activeTeleporters[i])
                    {
                        var color = Colors.SafeFromAOE;
                        Arena.ZoneCircle(pos + new WDir(default, -7.4f), 2f, color);
                        Arena.ZoneCircle(pos + new WDir(-2.5f, -20f), 2f, color);
                        Arena.ZoneCircle(pos + new WDir(15f, -11.5f), 2f, color);
                        Arena.ZoneCircle(pos + new WDir(20f, default), 1.5f, color);
                    }
                    return;
                }
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (pc.PosRot.Y < -100f)
        {
            var playerPos = pc.Position;
            if (playerPos.InSquare(Arena.Center, 50f))
            {
                return;
            }
            for (var i = 0; i < 8; ++i)
            {
                var pos = prisonPositions[i];
                if (playerPos.InSquare(pos, 50f))
                {
                    var arena = new ArenaBoundsCustom([new Polygon(pos, 9.5f, 32), new Polygon(pos + new WDir(-5f, -21f), 4.5f, 32),
                        new Polygon(pos + new WDir(14f, -14f), 4.5f, 32), new Polygon(pos + new WDir(20f, default), 3.25f, 32)]);
                    Arena.Bounds = arena;
                    Arena.Center = arena.Center;
                    return;
                }
            }
        }
        else if (Arena.Bounds.Radius == 17.5f)
        {
            Arena.Bounds = new ArenaBoundsRect(18f, 15f);
            Arena.Center = Trial.T05Necron.Necron.ArenaCenter;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.PosRot.Y < -100f)
        {
            var playerPos = actor.Position;
            for (var i = 0; i < 8; ++i)
            {
                var pos = prisonPositions[i];
                if (playerPos.InSquare(pos, 50f))
                {
                    if (activeTeleporters[i])
                    {
                        hints.Teleporters.Add(new(pos + new WDir(default, -7.4f), pos + new WDir(-6f, -18f), 2f, false));
                        hints.Teleporters.Add(new(pos + new WDir(-2.5f, -20f), pos + new WDir(10f, -15f), 2f, false));
                        hints.Teleporters.Add(new(pos + new WDir(15f, -11.5f), pos + new WDir(19f, -2f), 2f, false));
                        hints.GoalZones.Add(AIHints.GoalSingleTarget(pos + new WDir(20f, default), 1f, 9f));
                    }
                    return;
                }
            }
        }
    }
}

sealed class LimitBreakAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.IcyHands2, (uint)OID.BeckoningHands])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.Role != Role.Tank)
        {
            return;
        }
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.BeckoningHands => 1,
                _ => 0
            };
        }
    }
}

sealed class PrisonAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.IcyHands3, (uint)OID.IcyHands5, (uint)OID.IcyHands6])
{
    public override bool KeepOnPhaseChange => true;
}
