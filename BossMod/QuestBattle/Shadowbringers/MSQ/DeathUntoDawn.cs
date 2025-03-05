using RID = BossMod.Roleplay.AID;

namespace BossMod.QuestBattle.Shadowbringers.MSQ;

public class AutoAlisaie(WorldState ws) : UnmanagedRotation(ws, 25)
{
    public const ushort StatusParam = 157;

    protected override void Exec(Actor? primaryTarget)
    {
        if (primaryTarget == null || primaryTarget.OID == 0x1EB183)
            return;

        var melee = false;

        switch (ComboAction)
        {
            case RID.ShbVerfire:
                UseAction(RID.ShbVeraero, primaryTarget);
                break;
            case RID.ShbVeraero:
                UseAction(RID.ShbVerstone, primaryTarget);
                break;
            case RID.ShbVerstone:
                UseAction(RID.ShbVerflare, primaryTarget);
                break;
            case RID.ShbCorpsACorps:
                melee = true;
                UseAction(RID.ShbEnchantedRiposte, primaryTarget);
                break;
            case RID.ShbEnchantedRiposte:
                melee = true;
                UseAction(RID.ShbEnchantedZwerchhau, primaryTarget);
                break;
            case RID.ShbEnchantedZwerchhau:
                melee = true;
                UseAction(RID.ShbEnchantedRedoublement, primaryTarget);
                break;
            case RID.ShbEnchantedRedoublement:
                melee = true;
                UseAction(RID.ShbDisplacement, primaryTarget);
                break;
            case RID.ShbDisplacement:
                UseAction(RID.ShbVerholy, primaryTarget);
                break;
            case RID.ShbVerholy:
                UseAction(RID.ShbScorch, primaryTarget);
                break;
            default:
                UseAction(RID.ShbCorpsACorps, primaryTarget);
                UseAction(RID.ShbVerfire, primaryTarget);
                break;
        }

        if (melee)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(primaryTarget, 3));

        UseAction(RID.ShbFleche, primaryTarget);
        UseAction(RID.ShbContreSixte, primaryTarget);
    }
}

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 780)]
internal class DeathUntoDawn(WorldState ws) : QuestBattle(ws)
{
    private readonly AutoAlisaie _ai = new(ws);

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .With(obj => {
                obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.UpdateID == 0x10000002 && diru.Param1 == 0x68B1);
            })
            .PauseForCombat(false),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-45.56f, -22.71f, -101.05f))
            .PauseForCombat(false)
            .With(obj => {
                obj.OnStatusGain += (act, status) => obj.CompleteIf(status.ID == (uint)Roleplay.SID.RolePlaying && (status.Extra & 0xFF) == 157);
            }),

        new QuestObjective(ws)
            .With(obj => {
                obj.AddAIHints += (player, hints) => {
                    hints.PathfindMapCenter = new(0, -180);
                    hints.PathfindMapBounds = new ArenaBoundsCircle(20);
                    if (!player.InCombat)
                        hints.ForcedTarget = hints.PotentialTargets.MinBy(t => player.DistanceToPoint(t.Actor.Position))?.Actor;
                    _ai.Execute(player, hints);
                };
            })
            .CompleteOnKilled(0x3376),

        new QuestObjective(ws)
            .WithConnection(new Vector3(1.64f, -16.00f, -195.63f))
            .WithInteract(0x1EB183)
            .CompleteOnState7(0x1EB183)
    ];
}
