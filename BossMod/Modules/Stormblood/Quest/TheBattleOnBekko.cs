namespace BossMod.Stormblood.Quest.TheBattleOnBekko;

public enum OID : uint
{
    Boss = 0x1BF8,
    Helper = 0x233C,
    UgetsuSlayerOfAThousandSouls = 0x1BF9, // R0.500, x20, Helper type
    Voidzone = 0x1E8EA9, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    HissatsuKyuten = 8433, // Boss->self, 3.0s cast, range 5+R circle
    TenkaGoken = 9145, // Boss->self, 3.0s cast, range 8+R 120-degree cone
    ShinGetsubaku = 8437, // 1BF9->location, 3.0s cast, range 6 circle
    MijinGiri = 8435, // 1BF9->self, 2.5s cast, range 80+R width 10 rect
    Ugetsuzan = 8439, // 1BF9->self, 2.5s cast, range -7 donut
    Ugetsuzan2 = 8440, // 1BF9->self, 2.5s cast, range -12 donut
    Ugetsuzan3 = 8441, // 1BF9->self, 2.5s cast, range -17 donut
    KuruiYukikaze = 8446, // UgetsuSlayerOfAThousandSouls->self, 2.5s cast, range 44+R width 4 rect
    KuruiGekko1 = 8447, // UgetsuSlayerOfAThousandSouls->self, 2.0s cast, range 30 circle
    KuruiKasha1 = 8448, // UgetsuSlayerOfAThousandSouls->self, 2.5s cast, range 8+R ?-degree cone
    Ugetsuzan4 = 8442, // UgetsuSlayerOfAThousandSouls->self, 2.5s cast, range -22 donut
}

class KuruiGekko(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.KuruiGekko1));
class KuruiKasha(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.KuruiKasha1), new AOEShapeDonutSector(4.5f, 8.5f, 45.Degrees()));
class KuruiYukikaze(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.KuruiYukikaze), new AOEShapeRect(44, 2), 8);
class HissatsuKyuten(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HissatsuKyuten), new AOEShapeCircle(5.5f));
class TenkaGoken(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TenkaGoken), new AOEShapeCone(8.5f, 60.Degrees()));
class ShinGetsubaku(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.ShinGetsubaku), 6);
class ShinGetsubakuVoidzone(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.Voidzone).Where(e => e.EventState != 7));
class MijinGiri(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MijinGiri), new AOEShapeRect(80, 5, 2));
class Ugetsuzan(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeDonutSector(2, 7, 90.Degrees()), new AOEShapeDonutSector(7, 12, 90.Degrees()), new AOEShapeDonutSector(12, 17, 90.Degrees())])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Ugetsuzan)
            AddSequence(caster.Position - caster.Rotation.ToDirection() * 4, Module.CastFinishAt(spell), caster.Rotation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var idx = (AID)spell.Action.ID switch
        {
            AID.Ugetsuzan => 0,
            AID.Ugetsuzan2 => 1,
            AID.Ugetsuzan3 => 2,
            AID.Ugetsuzan4 => 3,
            _ => -1
        };
        AdvanceSequence(idx, caster.Position - caster.Rotation.ToDirection() * 4, WorldState.FutureTime(2.5f), caster.Rotation);
    }
}

class UgetsuSlayerOfAThousandSoulsStates : StateMachineBuilder
{
    public UgetsuSlayerOfAThousandSoulsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HissatsuKyuten>()
            .ActivateOnEnter<TenkaGoken>()
            .ActivateOnEnter<ShinGetsubaku>()
            .ActivateOnEnter<ShinGetsubakuVoidzone>()
            .ActivateOnEnter<MijinGiri>()
            .ActivateOnEnter<Ugetsuzan>()
            .ActivateOnEnter<KuruiYukikaze>()
            .ActivateOnEnter<KuruiGekko>()
            .ActivateOnEnter<KuruiKasha>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68106, NameID = 6096)]
public class UgetsuSlayerOfAThousandSouls(WorldState ws, Actor primary) : BossModule(ws, primary, new(808.8f, 69.5f), new ArenaBoundsSquare(14));

