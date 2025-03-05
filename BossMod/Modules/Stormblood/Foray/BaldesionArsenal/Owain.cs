using BossMod.Modules.Stormblood.Foray;

namespace BossMod.Stormblood.Foray.BaldesionArsenal.Owain;

public enum OID : uint
{
    Boss = 0x265D, // R2.700, x1
    Munderg = 0x265E, // R2.700, x8
    Helper1 = 0x265F, // R0.500, x13, mixed types
    Helper2 = 0x2660, // R0.500, x1
    Helper3 = 0x2671, // R0.500, x12, Helper type
}

public enum AID : uint
{
    Thricecull = 14661, // Boss->player, 5.0s cast, single-target
    AcallamNaSenorach = 14662, // Boss->self, 5.0s cast, range 60 circle
    ElementalMagicksFire = 14650, // Boss->self, 5.0s cast, range 13 circle
    ElementalMagicksIce = 14651, // Boss->self, 5.0s cast, range 13 circle
    ElementalMagicksSpearFire = 14652, // Munderg->self, no cast, range 13 circle
    ElementalMagicksSpearIce = 14653, // Munderg->self, no cast, range 13 circle
    Spiritcull = 14654, // Boss->self, 3.0s cast, single-target
    PiercingLight = 14655, // Helper1->player, 5.0s cast, range 6 circle
    LegendaryImbas = 14656, // Helper1->self, 5.0s cast, ???, stack with targets
}

public enum SID : uint
{
    SoulOfFire = 1751, // none->Boss, extra=0x11F
    SoulOfIce = 1752, // none->Boss, extra=0x120
    SoulOfFireSpear = 1783, // none->Munderg, extra=0x121
    SoulOfIceSpear = 1784, // none->Munderg, extra=0x122
}

public enum IconID : uint
{
    LegendaryImbas = 55, // player->self
    PiercingLight = 139, // player->self
}

class Thricecull(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Thricecull));
class AcallamNaSenorach(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AcallamNaSenorach));
class PiercingLight(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.PiercingLight), 6);
class LegendaryImbas(BossModule module) : Components.StackTogether(module, (uint)IconID.LegendaryImbas, 5);
class ElementalMagicks(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? Caster;
    private uint ActiveStatus;

    private IEnumerable<Actor> Spears => Module.Enemies(OID.Munderg);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Caster == null)
            yield break;

        var activation = Module.CastFinishAt(Caster.CastInfo);

        yield return new AOEInstance(new AOEShapeCircle(13), Caster.Position, default, activation);

        foreach (var s in Spears)
            if (s.FindStatus(ActiveStatus) != null)
                yield return new AOEInstance(new AOEShapeCircle(13), s.Position, default, activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ElementalMagicksFire:
                Caster = caster;
                ActiveStatus = (uint)SID.SoulOfFireSpear;
                break;
            case AID.ElementalMagicksIce:
                Caster = caster;
                ActiveStatus = (uint)SID.SoulOfIceSpear;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ElementalMagicksSpearFire or AID.ElementalMagicksSpearIce)
        {
            Caster = null;
            ActiveStatus = 0;
        }
    }
}

class OwainStates : StateMachineBuilder
{
    public OwainStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Thricecull>()
            .ActivateOnEnter<AcallamNaSenorach>()
            .ActivateOnEnter<PiercingLight>()
            .ActivateOnEnter<ElementalMagicks>()
            .ActivateOnEnter<LegendaryImbas>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 639, NameID = 7970)]
public class Owain(WorldState ws, Actor primary) : BAModule(ws, primary, new(129, 748), new ArenaBoundsCircle(25));

