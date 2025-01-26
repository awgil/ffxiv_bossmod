using BossMod.QuestBattle;

namespace BossMod.Stormblood.Quest.ARequiemForHeroes;

class AutoHien(WorldState ws) : UnmanagedRotation(ws, 3)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (primaryTarget == null)
            return;

        Hints.GoalZones.Add(Hints.GoalSingleTarget(primaryTarget, 3));

        var ajisai = StatusDetails(primaryTarget, Roleplay.SID.Ajisai, Player.InstanceID);

        switch (ComboAction)
        {
            case Roleplay.AID.Gofu:
                UseAction(Roleplay.AID.Yagetsu, primaryTarget);
                break;

            case Roleplay.AID.Kyokufu:
                UseAction(Roleplay.AID.Gofu, primaryTarget);
                break;

            default:
                if (ajisai.Left < 5)
                    UseAction(Roleplay.AID.Ajisai, primaryTarget);
                UseAction(Roleplay.AID.Kyokufu, primaryTarget);
                break;
        }

        if (Player.HPMP.CurHP < 5000)
            UseAction(Roleplay.AID.SecondWind, Player, -10);

        UseAction(Roleplay.AID.HissatsuGyoten, primaryTarget, -10);
    }
}

class HienAI(BossModule module) : RotationModule<AutoHien>(module);

public class ZenosP1States : StateMachineBuilder
{
    public ZenosP1States(BossModule module) : base(module)
    {
        TrivialPhase().ActivateOnEnter<HienAI>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68721, NameID = 6039, PrimaryActorOID = (uint)OID.BossP1)]
public class ZenosP1(WorldState ws, Actor primary) : BossModule(ws, primary, new(233, -93.25f), new ArenaBoundsCircle(20));
