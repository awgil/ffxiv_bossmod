using BossMod.Autorotation.xan;
using RID = BossMod.Roleplay.AID;

namespace BossMod.QuestBattle.Endwalker.MSQ;

// single target shield is status 2844

class AlphinaudAI(WorldState ws) : UnmanagedRotation(ws, 25)
{
    private readonly TrackPartyHealth PartyHealth = new(ws);

    protected override void Exec(Actor? primaryTarget)
    {
        PartyHealth.Update(Hints);

        Hints.InteractWithTarget = World.Actors.FirstOrDefault(x => x.OID is 0x1EB44F or 0x1EB2FB && x.IsTargetable);

        var refugee = World.Party.WithoutSlot().FirstOrDefault(x => x.OID == 0x35F1 && x.HPMP.CurHP < x.HPMP.MaxHP && x.IsTargetable);
        if (refugee is Actor r)
            UseAction(RID.Diagnosis, r);

        AutoHeal();

        if (primaryTarget is not { IsAlly: false })
            return;

        UseAction(RID.DosisIII, primaryTarget);
        UseAction(RID.LeveilleurToxikon, primaryTarget, -50);
    }

    private void AutoHeal()
    {
        foreach (var h in Hints.PredictedDamage.Where(pd => pd.Players.NumSetBits() == 1))
            foreach (var (_, player) in World.Party.WithSlot().IncludedInMask(h.Players))
                if (StatusDetails(player, 2844, Player.InstanceID).Left == 0)
                    UseAction(RID.LeveilleurDiagnosis, player);

        if (PartyHealth.PredictShouldHealInArea(Player.Position, 15, 0.6f))
            UseAction(RID.Prognosis, Player);

        if (PartyHealth.BestSTHealTarget is (Actor a, var st))
        {
            UseAction(RID.LeveilleurDruochole, a);
            if (st.PredictedHPRatio <= 0.5f)
                UseAction(RID.Diagnosis, a);
        }
    }
}

class AlisaieAI(WorldState ws) : UnmanagedRotation(ws, 25)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (primaryTarget == null)
            return;

        if (World.Party.LimitBreakCur == 10000)
            UseAction(RID.VermilionPledge, primaryTarget, 100);

        bool melee = false;

        switch (ComboAction)
        {
            case RID.EWVerfire:
                UseAction(RID.EWVeraero, primaryTarget);
                break;
            case RID.EWVeraero:
                UseAction(RID.EWVerstone, primaryTarget);
                break;
            case RID.EWVerstone:
                UseAction(RID.EWVerthunder, primaryTarget);
                break;
            case RID.EWVerthunder:
                UseAction(RID.EWVerflare, primaryTarget);
                break;
            case RID.EWCorpsACorps:
                melee = true;
                UseAction(RID.EWEnchantedRiposte, primaryTarget);
                break;
            case RID.EWEnchantedRiposte:
                melee = true;
                UseAction(RID.EWEnchantedZwerchhau, primaryTarget);
                break;
            case RID.EWEnchantedZwerchhau:
                melee = true;
                UseAction(RID.EWEnchantedRedoublement, primaryTarget);
                break;
            case RID.EWEnchantedRedoublement:
                melee = true;
                UseAction(RID.EWEngagement, primaryTarget);
                break;
            case RID.EWEngagement:
                UseAction(RID.EWVerholy, primaryTarget);
                break;
            case RID.EWVerholy:
                UseAction(RID.EWScorch, primaryTarget);
                break;
            default:
                UseAction(RID.EWCorpsACorps, primaryTarget);
                UseAction(RID.EWVerfire, primaryTarget);
                break;
        }

        if (melee)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(primaryTarget, 3));

        UseAction(RID.EWEmbolden, Player, -50);
        UseAction(RID.EWContreSixte, primaryTarget, -50);

    }
}

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 804)]
internal class AsTheHeavensBurn(WorldState ws) : QuestBattle(ws)
{
    private readonly AlphinaudAI _alphi = new(ws);
    private readonly AlisaieAI _alisaie = new(ws);

    public static WPos P2Center = new(-260.28f, 80.75f);

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Hints(_alphi.Execute)
            .With(obj => obj.Update += () => obj.CompleteIf(ws.Party.Player()?.Position.InCircle(P2Center, 30) ?? false)),

        new QuestObjective(ws)
            .Hints((player, hints) => {
                if (!player.InCombat)
                    hints.PrioritizeAll();

                _alisaie.Execute(player, hints);
            })
            .CompleteOnCreated(0x35EE)
    ];
}

