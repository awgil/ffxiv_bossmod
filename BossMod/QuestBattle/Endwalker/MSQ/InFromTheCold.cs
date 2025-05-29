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

        switch (ComboAction)
        {
            case Roleplay.AID.RiotBladeIFTC:
                UseAction(Roleplay.AID.FightOrFlightIFTC, Player, -50);
                UseAction(Roleplay.AID.RageOfHaloneIFTC, primaryTarget);
                break;
            case Roleplay.AID.FastBladeIFTC:
                UseAction(Roleplay.AID.FightOrFlightIFTC, Player, -50);
                UseAction(Roleplay.AID.RiotBladeIFTC, primaryTarget);
                break;
        }

        UseAction(Roleplay.AID.FastBladeIFTC, primaryTarget);
    }
}

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 793)]
internal class InFromTheCold(WorldState ws) : QuestBattle(ws)
{
    private readonly ImperialAI _ai = new(ws);

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets.Where(p => p.Actor.Position.InCircle(player.Position, 40)))
            if (!h.Actor.InCombat && !h.Actor.Position.AlmostEqual(new(111, -317), 10))
                hints.AddForbiddenZone(ShapeContains.Cone(h.Actor.Position, 8.5f + h.Actor.HitboxRadius, h.Actor.Rotation, 45.Degrees()));

        _ai.Execute(player, hints);
    }

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Named("Combat")
            .Hints((player, hints) => hints.ForcedMovement = new(0, 0, 1))
            .With(obj => {
                obj.Update += () => obj.CompleteIf(World.Party.Player()?.Position.Z > -320);
            }),

        new QuestObjective(ws)
            .Named("Medkit")
            .Hints((player, hints) => hints.PrioritizeTargetsByOID(0x3507))
            .WithInteract(0x3507)
            .With(obj => obj.OnDutyActionsChange += (op) => obj.CompleteIf(op.Slots[0].Action.ID == 27315)),

        new QuestObjective(ws)
            .Named("F")
            .WithInteract(0x3507)
            .With(obj => obj.OnEventObjectAnimation += (act, p1, p2) => obj.CompleteIf(act.OID == 0x1EA1A1 && p1 == 4 && p2 == 8)),

        new QuestObjective(ws)
            .Named("Reaper 1")
            .WithInteract(0x1EB456)
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x10000002 && diru.Param1 == 0x76DF)),

        new QuestObjective(ws)
            .Named("Wounded Imperial")
            .Hints((player, hints) => {
                hints.GoalZones.Add(hints.GoalSingleTarget(new WPos(105, -259), 3));
                if (player.Position.AlmostEqual(new WPos(111.218f, -257.802f), 2))
                    hints.WantJump = true;
            })
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x10000002 && diru.Param1 == 0x76E0)),

        new QuestObjective(ws)
            .Named("Key")
            .WithInteract(0xFE20B)
            .With(obj => obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x10000002 && diru.Param1 == 0x76E2)),

        new QuestObjective(ws)
            .Named("Fuel")
            .WithInteract(0x1EB69E)
            .Hints((player, hints) => {
                if (player.Position.AlmostEqual(new WPos(109, -257.263f), 2))
                    hints.WantJump = true;
            })
            .With(obj => obj.OnStatusGain += (act, st) => obj.CompleteIf(act.OID == 0 && st.ID == 404)),

        new QuestObjective(ws)
            .Named("Refuel")
            .Hints((player, hints) => {
                hints.InteractWithOID(World, 0x1EB56F);
                if (hints.InteractWithTarget == null)
                    hints.InteractWithOID(World, 0x1EB4F1);
            })
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
            .Hints((player, hints) => {
                hints.WantDismount = true;
                hints.GoalZones.Add(hints.GoalSingleTarget(new WPos(12, -148), 5, 0.5f));
                hints.GoalZones.Add(hints.GoalSingleTarget(new WPos(-81, -180), 5, 0.75f));
            })
            .With(obj => {
                obj.OnStatusGain += (act, st) => obj.CompleteIf(act.OID == 0 && st.ID == 2737);
            }),

        new QuestObjective(ws)
            .Named("Leave")
            .Hints((player, hints) => hints.ForcedMovement = new(-0.33f, 0, 0.67f))
    ];
}
