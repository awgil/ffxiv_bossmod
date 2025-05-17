namespace BossMod.Stormblood.Quest.TheFaceOfTrueEvil;

public enum OID : uint
{
    Boss = 0x1BEE,
    Helper = 0x233C,
    Musosai = 0x1BEF, // R0.500, x12, Helper type
    Musosai1 = 0x1BF0, // R1.000, x0 (spawn during fight)
    ViolentWind = 0x1BF1, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    HissatsuTo1 = 8415, // 1BEF->self, 3.0s cast, range 44+R width 4 rect
    HissatsuKyuten = 8412, // Boss->self, 3.0s cast, range 5+R circle
    Arashi = 8418, // Boss->self, 4.0s cast, single-target
    Arashi1 = 8419, // 1BF0->self, no cast, range 4 circle
    HissatsuKiku1 = 8417, // Musosai->self, 4.0s cast, range 44+R width 4 rect
    Maiogi1 = 8421, // Musosai->self, 4.0s cast, range 80+R ?-degree cone
    Musojin = 8422, // Boss->self, 25.0s cast, single-target
    ArashiNoKiku = 8643, // Boss->self, 3.0s cast, single-target
    ArashiNoMaiogi = 8642, // Boss->self, 3.0s cast, single-target
}

class Musojin(BossModule module) : Components.RaidwideCast(module, AID.Musojin);
class HissatsuKiku(BossModule module) : Components.StandardAOEs(module, AID.HissatsuKiku1, new AOEShapeRect(44.5f, 2));
class Maiogi(BossModule module) : Components.StandardAOEs(module, AID.Maiogi1, new AOEShapeCone(80, 25.Degrees()));
class HissatsuTo(BossModule module) : Components.StandardAOEs(module, AID.HissatsuTo1, new AOEShapeRect(44.5f, 2));
class HissatsuKyuten(BossModule module) : Components.StandardAOEs(module, AID.HissatsuKyuten, new AOEShapeCircle(5.5f));
class Arashi(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime? Activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Activation == null)
            yield break;

        foreach (var e in Module.Enemies(OID.Musosai1))
            yield return new AOEInstance(new AOEShapeCircle(4), e.Position, default, Activation.Value);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Arashi or AID.ArashiNoKiku or AID.ArashiNoMaiogi)
            Activation = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Arashi1)
            Activation = null;
    }
}
class ViolentWind(BossModule module) : Components.Adds(module, (uint)OID.ViolentWind);

class MusosaiStates : StateMachineBuilder
{
    public MusosaiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HissatsuTo>()
            .ActivateOnEnter<HissatsuKyuten>()
            .ActivateOnEnter<Arashi>()
            .ActivateOnEnter<HissatsuKiku>()
            .ActivateOnEnter<Maiogi>()
            .ActivateOnEnter<Musojin>()
            .ActivateOnEnter<ViolentWind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68101, NameID = 6111)]
public class Musosai(WorldState ws, Actor primary) : BossModule(ws, primary, new(-217.27f, -158.31f), new ArenaBoundsSquare(15));
