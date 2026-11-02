namespace BossMod.QuestBattle.Endwalker.MSQ;

class ImperialAI(WorldState ws) : UnmanagedRotation(ws, 3)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (Player.FindStatus(2736) != null)
        {
            var someTarget = Hints.PotentialTargets.FirstOrDefault(x => x.Actor.Position.InRect(new WPos(68, -222), new WDir(52, 0), 16) && (x.Actor.Position - Player.Position).Length() <= 30);
            if (someTarget != null)
            {
                UseAction(Roleplay.AID.DiffractiveMagitekCannonIFTC, someTarget.Actor, 0, someTarget.Actor.PosRot.XYZ());
                UseAction(Roleplay.AID.MagitekCannonIFTC, someTarget.Actor, 0, someTarget.Actor.PosRot.XYZ());
            }
            // mount rotation
            return;
        }

        if (Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.75f && World.Client.DutyActions[0].CurCharges > 0)
            UseAction(Roleplay.AID.MedicalKit, Player, -50);

        if (primaryTarget is not { IsAlly: false })
            return;

        if (Player.InCombat)
            UseAction(Roleplay.AID.RampartIFTC, Player, -50);

        if (ComboAction is Roleplay.AID.RiotBladeIFTC or Roleplay.AID.FastBladeIFTC)
        {
            UseAction(Roleplay.AID.FightOrFlightIFTC, Player, -50);
            UseAction(ComboAction == Roleplay.AID.RiotBladeIFTC ? Roleplay.AID.RageOfHaloneIFTC : Roleplay.AID.RiotBladeIFTC, primaryTarget);
        }

        UseAction(Roleplay.AID.FastBladeIFTC, primaryTarget);
    }
}

[ZoneModuleInfo(793)]
internal class InFromTheCold(WorldState ws) : QuestBattle(ws)
{
    private readonly ImperialAI _ai = new(ws);
    private readonly Pathfinding.NavigationDecision.Context _sneakContext = new();
    private readonly Dictionary<ulong, (WPos Position, DateTime Time, WDir Velocity)> _motion = [];
    private bool _sneak;
    private bool _visitedFarWreckage;

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets.Where(p => p.Actor.Position.InCircle(player.Position, 40)))
            if (!h.Actor.InCombat && !h.Actor.Position.AlmostEqual(new(111, -317), 10))
            {
                // 0x3515 spots the player from ~10.1y away
                var sight = (h.Actor.OID == 0x3515 ? 11 : 8.5f) + h.Actor.HitboxRadius;
                hints.AddForbiddenZone(ShapeDistance.Cone(h.Actor.Position, sight, h.Actor.Rotation, 45.Degrees()));
                hints.AddForbiddenZone(ShapeDistance.Circle(h.Actor.Position, h.Actor.HitboxRadius + player.HitboxRadius + 1.5f));

                // patrols walk into paths that only clear their current cone
                var velocity = Velocity(h.Actor);
                if (velocity.LengthSq() > 0.25f)
                    hints.AddForbiddenZone(ShapeDistance.Cone(h.Actor.Position + velocity * 1.5f, sight, h.Actor.Rotation, 45.Degrees()));
            }

        if (_sneak)
            SneakAlongRoute(player, hints);
        _sneak = false;

        _ai.Execute(player, hints);
    }

    private WDir Velocity(Actor actor)
    {
        if (!_motion.TryGetValue(actor.InstanceID, out var m))
        {
            _motion[actor.InstanceID] = (actor.Position, World.CurrentTime, default);
            return default;
        }
        var dt = (float)(World.CurrentTime - m.Time).TotalSeconds;
        if (dt >= 0.5f)
        {
            m = (actor.Position, World.CurrentTime, (actor.Position - m.Position) / dt);
            _motion[actor.InstanceID] = m;
        }
        return m.Velocity;
    }

    private void SneakAlongRoute(Actor player, AIHints hints)
    {
        if (player.InCombat)
        {
            hints.ForcedMovement = null;
            return;
        }
        if (hints.ForcedMovement is not { } route)
            return;

        if (hints.InteractWithTarget is { } target && target.OID != 0x1EB452 && player.DistanceToHitbox(target) < 20)
        {
            hints.ForcedMovement = null;
            return;
        }

        var dir = new WDir(route.X, route.Z);
        var len = dir.Length();
        if (len <= 0)
            return;
        var next = player.Position + dir;
        var step = dir / len;
        var distance = Math.Clamp(len, 0.5f, 8);
        var samples = (int)Math.Ceiling(distance / 0.25f);
        for (var i = 0; i <= samples; ++i)
        {
            var p = player.Position + step * (distance * i / samples);
            if (hints.ForbiddenZones.Any(z => z.shape.Check(p)))
            {
                hints.ForcedMovement = Vector3.Zero;
                if (hints.InteractWithTarget?.OID == 0x1EB452)
                    hints.InteractWithTarget = null;
                hints.GoalZones.Add(AIHints.GoalProximity(next, Math.Max(40, len + 10), 1));
                if (World.Client.MoveSpeed > 0)
                {
                    hints.ForbiddenZones.SortBy(z => z.activation);
                    var decision = Pathfinding.NavigationDecision.Build(_sneakContext, World.CurrentTime, hints, player.Position, World.Client.MoveSpeed, World.Network.LastRTT, 0.5f);
                    if (decision.Destination is { } destination && !player.Position.AlmostEqual(destination, 0.1f) && SafeStep(player.Position, destination, hints))
                        hints.ForcedMovement = (destination - player.Position).ToVec3();
                }
                return;
            }
        }
    }

    private bool SafeStep(WPos from, WPos to, AIHints hints)
    {
        var samples = Math.Max(1, (int)Math.Ceiling((to - from).Length() / 0.25f));
        foreach (var zone in hints.ForbiddenZones.Where(z => z.activation <= World.CurrentTime))
        {
            var outside = !zone.shape.Check(from);
            for (var i = 1; i <= samples; ++i)
            {
                var inside = zone.shape.Check(from + (to - from) * ((float)i / samples));
                if (outside && inside)
                    return false;
                outside |= !inside;
            }
        }
        return true;
    }

    private bool HasFuel => World.Party.Player()?.FindStatus(404) != null;
    private bool FuelInNearWreckage => World.Actors.Any(a => a.OID == 0x1EB69E && a.IsTargetable);

    private void RefuelHints(Actor player, AIHints hints)
    {
        _sneak = true;
        InteractWithFirst(player, hints, HasFuel ? 0x1EB4F1u : 0x1EB56Fu);
    }

    private void InteractWithFirst(Actor player, AIHints hints, params uint[] oids)
    {
        if (player.InCombat)
            return;
        foreach (var oid in oids)
        {
            hints.InteractWithOID(World, oid);
            if (hints.InteractWithTarget != null)
                return;
        }
    }

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Named("Combat")
            .Hints((player, hints) => hints.ForcedMovement = new(0, 0, 1))
            .With(obj => obj.Update += () => obj.CompleteIf(World.Party.Player()?.Position.Z > -320)),

        new QuestObjective(ws)
            .Named("Medkit")
            .Hints((player, hints) => {
                hints.PrioritizeTargetsByOID([0x3506, 0x3507]);
                InteractWithFirst(player, hints, 0x3507, 0x3506);
            })
            .With(obj => obj.OnDutyActionsChange += (op) => obj.CompleteIf(op.Slots[0].Action.ID == 27315)),

        new QuestObjective(ws)
            .Named("F")
            .Hints((player, hints) => InteractWithFirst(player, hints, 0x3507, 0x3506))
            .With(obj => obj.OnEventObjectAnimation += (act, p1, p2) => obj.CompleteIf(act.OID == 0x1EA1A1 && p1 == 4 && p2 == 8)),

        new QuestObjective(ws)
            .Named("Reaper 1")
            .MoveHint(new WPos(134, -290), 0.1f)
            .MoveHint(new WPos(126, -264), 0.2f)
            .MoveHint(new WPos(123, -238), 0.3f)
            .WithInteract(0x1EB456)
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x10000002 && diru.Param1 == 0x76DF)),

        new QuestObjective(ws)
            .Named("Wounded Imperial")
            .Hints((player, hints) => {
                hints.GoalZones.Add(AIHints.GoalSingleTarget(new WPos(105, -259), 3));
                if (player.Position.AlmostEqual(new WPos(111.218f, -257.802f), 2))
                    hints.WantJump = true;
            })
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x10000002 && diru.Param1 == 0x76E0)),

        new QuestObjective(ws)
            .Named("Key")
            .WithInteract(0xFE20B)
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x10000002 && diru.Param1 == 0x76E2)),

        new QuestObjective(ws)
            .Named("Leave church")
            .WithConnections(new Waypoint(new Vector3(108.5f, 10.8f, -258.2f), false), new Waypoint(new Vector3(116, 10.8f, -259.3f), false))
            .Hints((player, hints) => {
                if (player.Position.AlmostEqual(new WPos(109, -257.263f), 2))
                    hints.WantJump = true;
            })
            .CompleteAtDestination(),

        new QuestObjective(ws)
            .Named("Fuel (near wreckage)")
            .WithConnections(new Vector3(124.5f, 10.8f, -232.9f), new Vector3(175.5f, 10.8f, -227.7f))
            .Hints((player, hints) => {
                _sneak = true;
                InteractWithFirst(player, hints, 0x1EB69E);
            })
            .With(obj => obj.Update += () => obj.CompleteIf(HasFuel || !FuelInNearWreckage)),

        new QuestObjective(ws)
            .Named("Approach recovery pouch")
            .WithConnections(new Vector3(124.5f, 10.8f, -232.9f), new Vector3(175.5f, 10.8f, -227.7f), new Vector3(188.4f, 10.8f, -256.8f), new Vector3(225.1f, 10.8f, -286.9f), new Vector3(268.8f, 10.8f, -325.2f))
            .Hints((player, hints) => _sneak = true)
            .CompleteAtDestination()
            .With(obj => obj.Update += () => obj.CompleteIf(HasFuel || FuelInNearWreckage)),

        new QuestObjective(ws)
            .Named("Recovery pouch")
            .WithInteract(0x1EB56E)
            .With(obj => obj.Update += () => obj.CompleteIf(HasFuel || FuelInNearWreckage
                || World.Party.Player()?.FindStatus(1014) != null
                || !World.Actors.Any(a => a.OID == 0x1EB56E && a.IsTargetable))),

        new QuestObjective(ws)
            .Named("Fuel (far wreckage)")
            .WithConnections(new Vector3(295.5f, 10.8f, -366.3f), new Vector3(308.8f, 10.8f, -364.1f), new Vector3(315.2f, 10.8f, -379.9f), new Vector3(302.4f, 10.8f, -392.6f))
            .Hints((player, hints) => {
                _sneak = true;
                _visitedFarWreckage |= !HasFuel && !FuelInNearWreckage;
                InteractWithFirst(player, hints, 0x1EB452);
            })
            .With(obj => obj.Update += () => obj.CompleteIf(HasFuel || FuelInNearWreckage)),

        new QuestObjective(ws)
            .Named("Back from far wreckage")
            .WithConnections(new Vector3(315.2f, 10.8f, -379.9f), new Vector3(308.8f, 10.8f, -364.1f), new Vector3(295.5f, 10.8f, -366.3f), new Vector3(268.8f, 10.8f, -325.2f), new Vector3(225.1f, 10.8f, -286.9f), new Vector3(188.4f, 10.8f, -256.8f))
            .Hints(RefuelHints)
            .CompleteAtDestination()
            .With(obj => obj.Update += () => obj.CompleteIf(!_visitedFarWreckage || FuelInNearWreckage)),

        new QuestObjective(ws)
            .Named("Refuel")
            .WithConnections(new Vector3(175.5f, 10.8f, -227.7f), new Vector3(130.3f, 10.7f, -230.5f))
            .Hints(RefuelHints)
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x10000002 && diru.Param1 == 0x76C1)),

        new QuestObjective(ws)
            .Named("Mount")
            .WithInteract(0x1EB45B)
            .With(obj => obj.OnStatusGain += (act, st) => obj.CompleteIf(act.OID == 0 && st.ID == 2736)),

        new QuestObjective(ws)
            .Named("Combat")
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x10000002 && diru.Param1 == 0x76C2)),

        new QuestObjective(ws)
            .Named("Help the townspeople")
            .WithConnection(new Vector3(96, 10.8f, -215.7f))
            .WithConnection(new Vector3(17, 10.8f, -154.9f))
            .Hints((player, hints) => hints.WantDismount = true)
            .CompleteOnKilled(0x351C), // almasty

        new QuestObjective(ws)
            .Named("Miniboss")
            .WithConnection(new Vector3(-80, 10.8f, -181.5f))
            .With(obj => obj.OnStatusGain += (act, st) => obj.CompleteIf(act.OID == 0 && st.ID == 2737)),

        new QuestObjective(ws)
            .Named("Leave")
            .Hints((player, hints) => hints.ForcedMovement = new(-0.33f, 0, 0.67f))
    ];
}
