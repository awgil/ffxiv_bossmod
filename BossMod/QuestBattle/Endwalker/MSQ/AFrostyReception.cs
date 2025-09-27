namespace BossMod.QuestBattle.Endwalker.MSQ;

class AutoThancred(WorldState ws) : UnmanagedRotation(ws, 3)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (primaryTarget is not { IsAlly: false })
            return;

        if (Player.FindStatus(2957) != null)
        {
            UseAction(Roleplay.AID.SilentTakedown, primaryTarget);
            return;
        }

        switch (ComboAction)
        {
            case Roleplay.AID.KeenEdgeFR:
                UseAction(Roleplay.AID.BrutalShellFR, primaryTarget);
                break;
            case Roleplay.AID.BrutalShellFR:
                UseAction(Roleplay.AID.SolidBarrelFR, primaryTarget);
                break;
            default:
                UseAction(Roleplay.AID.KeenEdgeFR, primaryTarget);
                break;
        }
    }
}

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 812)]
internal class AFrostyReception(WorldState ws) : QuestBattle(ws)
{
    private readonly AutoThancred _ai = new(ws);

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        if (player.FindStatus(Roleplay.SID.RolePlaying) == null)
            return;

        foreach (var h in hints.PotentialTargets.Where(p => p.Actor.Position.InCircle(player.Position, 30)))
            if (!h.Actor.InCombat)
                if (player.FindStatus(Roleplay.SID.SwiftDeception) == null || h.Actor.OID == 0x362A)
                    hints.AddForbiddenZone(GetSightCone(h.Actor));

        _ai.Execute(player, hints);
    }

    private static Func<WPos, bool> GetSightCone(Actor p)
    {
        if (p.OID == 0x362A)
            return ShapeContains.Circle(p.Position, 8.5f + p.HitboxRadius);

        return ShapeContains.Cone(p.Position, 10 + p.HitboxRadius, p.Rotation, 45.Degrees());
    }

    private QuestObjective Takedown(Vector3 destination, uint enemyOID) => new QuestObjective(World)
        .Hints((player, hints) =>
        {
            var tar = hints.PotentialTargets.FirstOrDefault(x => x.Actor.Position.AlmostEqual(new WPos(destination.XZ()), 3) && x.Actor.OID == enemyOID);
            if (tar is AIHints.Enemy t)
            {
                if (!player.InCombat)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(Roleplay.AID.SwiftDeception), player, ActionQueue.Priority.High);
                t.Priority = 1;
            }
        })
        .With(obj =>
        {
            obj.OnEventCast += (act, spell) => obj.CompleteIf(act.OID == 0 && spell.Action.ID == (uint)Roleplay.AID.SilentTakedown);
        });

    private QuestObjective Dog(Vector3 destination) => new QuestObjective(World)
        .Hints((player, hints) =>
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(Roleplay.AID.BewildermentBomb), null, ActionQueue.Priority.High, targetPos: destination);
        })
        .With(obj =>
        {
            obj.OnStatusGain += (act, stat) => obj.CompleteIf(act.OID == 0x362A && stat.ID == 2824);
        });

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Named("Commence")
            .WithInteract(0x384C)
            .With(obj => {
                obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x80000000 && diru.Param1 == 0x883020 && diru.Param2 == 0);
            }),

        new QuestObjective(ws)
            .Named("Wall")
            .With(obj => {
                obj.AddAIHints += (player, hints) => {
                    if (World.Actors.Find(player.TargetID)?.OID == 0x384C)
                        hints.ForcedTarget = player;
                };
                obj.OnMapEffect += (env) => obj.CompleteIf(env.Index == 14 && env.State == 0x80002);
            }),

        Takedown(new(55, 0, -375), 0x3627).Named("Guard 1").MoveHint(new WPos(76.25f, -356.250f)),

        QuestObjective.StandardInteract(ws, 0x1EB310).Named("Gate 1"),
        QuestObjective.StandardInteract(ws, 0x1EB30F).Named("Sabotage 1.1").MoveHint(new WPos(61.73f, -381.46f)),
        QuestObjective.StandardInteract(ws, 0x1EB466).Named("Sabotage 1.2"),
        QuestObjective.StandardInteract(ws, 0x1EB467).Named("Sabotage 1.3"),

        Takedown(new(45.5f, 0, -425), 0x3628).Named("Guard 2"),

        QuestObjective.StandardInteract(ws, 0x1EB46A).Named("Gate 2"),

        Dog(new Vector3(26.094f, 0.3f, -392.915f))
            .Named("Dog 1")
            .MoveHint(new Vector3(28.44f, 0.30f, -409.67f)),

        QuestObjective.StandardInteract(ws, 0x1EB46B).Named("Gate 3"),

        QuestObjective.StandardInteract(ws, 0x1EB468).Named("Sabotage 2.1").MoveHint(new Vector3(26.63f, 0.37f, -383.65f)),
        QuestObjective.StandardInteract(ws, 0x1EB469).Named("Sabotage 2.2"),

        QuestObjective.StandardInteract(ws, 0x1EB31B).Named("Hide"),

        new QuestObjective(ws)
            .With(obj => {
                obj.OnActorEventStateChanged += (act) => obj.CompleteIf(act.OID == 0x1EB46C && act.EventState == 0);
            }),

        Takedown(new(10, 0, -342), 0x3628).Named("Guard 3"),

        QuestObjective.StandardInteract(ws, 0x1EB46C).Named("Gate 4"),

        new QuestObjective(ws)
            .With(obj => {
                obj.Update = () => {
                    if (World.Party.Player()?.InCombat ?? false)
                        return;

                    var cd = ActionDefinitions.Instance[ActionID.MakeSpell(Roleplay.AID.SwiftDeception)];
                    obj.CompleteIf(cd?.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions) < 0.5f);
                };
            }),

        Takedown(new(-27, 0, -387), 0x3627).Named("Guard 4")
            .MoveHint(new WPos(-9.75f, -359.75f), 0.5f)
            .MoveHint(new WPos(-32.25f, -378.75f), 0.75f),

        QuestObjective.StandardInteract(ws, 0x1EB46D).Named("Gate 5"),
        QuestObjective.StandardInteract(ws, 0x1EB46E)
            .Named("Controls")
            .MoveHint(new WPos(-38.25f, -396.75f)),

        new QuestObjective(ws)
            .Named("Exit")
            .MoveHint(new WPos(-59.25f, -426.25f))
            .With(obj => {
                obj.Update += () => obj.CompleteIf(World.Party.Player()?.Position.Z > 0);
            }),

        new QuestObjective(ws)
            .Named("Carriage 1")
            .With(obj => {
                obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x10000001 && diru.Param1 == 0x7B76);
            }),

        new QuestObjective(ws)
            .Named("Carriage 2")
            .MoveHint(new WPos(0, 235))
            .With(obj => {
                obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x10000001 && diru.Param1 == 0x7B77);
            }),

        new QuestObjective(ws)
            .Named("Carriage 3")
            .MoveHint(new WPos(0, 176))
            .CompleteOnKilled(0x3635),

        new QuestObjective(ws)
            .Named("Teleport")
            .Hints((player, hints) => {
                hints.ForcedMovement = new(0, 0, -1);
            })
            .With(obj => {
                obj.OnActorCombatChanged += (act) => obj.CompleteIf(act.OID == 0 && act.InCombat);
            })
    ];
}

