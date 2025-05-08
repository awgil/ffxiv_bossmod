namespace BossMod.Dawntrail.Savage.RM03SBruteBomber;

class BombarianSpecial(BossModule module) : Components.UniformStackSpread(module, 5, 5, alwaysShowSpreads: true)
{
    public enum Mechanic { None, Spread, Pairs }

    public Mechanic CurMechanic;

    public void Show(float delay)
    {
        switch (CurMechanic)
        {
            case Mechanic.Spread:
                AddSpreads(Raid.WithoutSlot(true), WorldState.FutureTime(delay));
                break;
            case Mechanic.Pairs:
                // TODO: can target any role
                AddStacks(Raid.WithoutSlot(true).Where(p => p.Class.IsSupport()), WorldState.FutureTime(delay));
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurMechanic != Mechanic.None)
            hints.Add(CurMechanic.ToString());
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var mechanic = (AID)spell.Action.ID switch
        {
            AID.OctoboomBombarianSpecial => Mechanic.Spread,
            AID.QuadroboomBombarianSpecial => Mechanic.Pairs,
            _ => Mechanic.None
        };
        if (mechanic != Mechanic.None)
            CurMechanic = mechanic;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BombariboomSpread or AID.BombariboomPair)
        {
            Spreads.Clear();
            Stacks.Clear();
            CurMechanic = Mechanic.None;
        }
    }
}

class BombarianSpecialRaidwide(BossModule module) : Components.CastCounter(module, default)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BombarianSpecialRaidwide1 or AID.BombarianSpecialRaidwide2 or AID.BombarianSpecialRaidwide3 or AID.BombarianSpecialRaidwide4 or AID.BombarianSpecialRaidwide5 or AID.BombarianSpecialRaidwide6
            or AID.SpecialBombarianSpecialRaidwide1 or AID.SpecialBombarianSpecialRaidwide2 or AID.SpecialBombarianSpecialRaidwide3 or AID.SpecialBombarianSpecialRaidwide4 or AID.SpecialBombarianSpecialRaidwide5 or AID.SpecialBombarianSpecialRaidwide6)
        {
            ++NumCasts;
        }
    }
}

class BombarianSpecialOut(BossModule module) : Components.StandardAOEs(module, AID.BombarianSpecialOut, new AOEShapeCircle(10));
class BombarianSpecialIn(BossModule module) : Components.StandardAOEs(module, AID.BombarianSpecialIn, new AOEShapeDonut(6, 40));
class BombarianSpecialAOE(BossModule module) : Components.StandardAOEs(module, AID.BombarianSpecialAOE, new AOEShapeCircle(8));
class BombarianSpecialKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.BombarianSpecialKnockback, 10);
class SpecialBombarianSpecialOut(BossModule module) : Components.StandardAOEs(module, AID.SpecialBombarianSpecialOut, new AOEShapeCircle(10));
class SpecialBombarianSpecialIn(BossModule module) : Components.StandardAOEs(module, AID.SpecialBombarianSpecialIn, new AOEShapeDonut(6, 40));
