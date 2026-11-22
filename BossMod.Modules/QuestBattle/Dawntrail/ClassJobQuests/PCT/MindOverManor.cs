using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.QuestBattle.Dawntrail.ClassJobQuests.PCT;

public enum OID : uint
{
    Kupopo = 0x42BF,
    TimewornStrikingDummy = 0x428F,
    SprigganClatterer = 0x4292,
}

[ZoneModuleInfo(1000, territoryID: 1233)]
internal class MindOverManor(WorldState ws) : QuestBattle(ws)
{
    private uint _step;

    private static bool StepReached(WorldState.OpDirectorUpdate diru, uint step) => diru.UpdateID == 0x80000015 && diru.Param1 >= step;

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Named("Motifs and muses")
            .With(obj => obj.OnDirectorUpdate += diru =>
            {
                if (diru.UpdateID == 0x80000015)
                    _step = diru.Param1;
                obj.CompleteIf(StepReached(diru, 7));
            })
            .Hints((player, hints) =>
            {
                var dummy = World.Actors.FirstOrDefault(a => a.OID == (uint)OID.TimewornStrikingDummy && a.IsTargetable);
                var canvas = World.Client.GetGauge<PictomancerGauge>().CanvasFlags;
                if (_step is 1 or 4 && (canvas & (CanvasFlags.Pom | CanvasFlags.Wing)) == 0)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.PCT.AID.CreatureMotif), player, ActionQueue.Priority.VeryHigh);
                else if (_step is 3 or 5 && dummy != null)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.PCT.AID.LivingMuse), dummy, ActionQueue.Priority.VeryHigh);
                else if (_step == 6 && dummy != null)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.PCT.AID.MogOfTheAges), dummy, ActionQueue.Priority.VeryHigh);
            }),

        new QuestObjective(ws)
            .Named("Speak with Kupopo")
            .WithInteract((uint)OID.Kupopo, true)
            .With(obj => obj.OnDirectorUpdate += diru => obj.CompleteIf(StepReached(diru, 8))),

        new QuestObjective(ws)
            .Named("Speak with Kupopo again")
            .WithConnection(new Vector3(-81.1f, -11f, 3.4f))
            .WithInteract((uint)OID.Kupopo, true)
            .With(obj => obj.OnDirectorUpdate += diru => obj.CompleteIf(StepReached(diru, 9))),

        new QuestObjective(ws)
            .Named("Subtractive spells")
            .WithConnection(new Vector3(-73.1f, -16.8f, 7.8f))
            .Hints((player, hints) => hints.PrioritizeAll())
            .With(obj => obj.OnDirectorUpdate += diru => obj.CompleteIf(StepReached(diru, 10))),

        new QuestObjective(ws)
            .Named("Spriggan clatterer")
            .WithConnection(new Vector3(-7.5f, -18.8f, 1.5f))
            .Hints((player, hints) => hints.PrioritizeAll())
            .CompleteOnKilled((uint)OID.SprigganClatterer)
    ];
}
