namespace BossMod.Endwalker.Extreme.Ex4Barbariccia
{
    class RagingStorm : Components.CastCounter
    {
        public RagingStorm() : base(ActionID.MakeSpell(AID.RagingStorm)) { }
    }

    class HairFlayUpbraid : Components.CastStackSpread
    {
        public HairFlayUpbraid() : base(ActionID.MakeSpell(AID.Upbraid), ActionID.MakeSpell(AID.HairFlay), 3, 10, maxStackSize: 2) { }
    }

    class CurlingIron : Components.CastCounter
    {
        public CurlingIron() : base(ActionID.MakeSpell(AID.CurlingIronAOE)) { }
    }

    class Catabasis : Components.CastCounter
    {
        public Catabasis() : base(ActionID.MakeSpell(AID.Catabasis)) { }
    }

    class VoidAeroTankbuster : Components.Cleave
    {
        public VoidAeroTankbuster() : base(ActionID.MakeSpell(AID.VoidAeroTankbuster), new AOEShapeCircle(5), originAtTarget: true) { }
    }

    class SecretBreezeCones : Components.SelfTargetedAOEs
    {
        public SecretBreezeCones() : base(ActionID.MakeSpell(AID.SecretBreezeAOE), new AOEShapeCone(40, 22.5f.Degrees())) { }
    }

    class SecretBreezeProteans : Components.SimpleProtean
    {
        public SecretBreezeProteans() : base(ActionID.MakeSpell(AID.SecretBreezeProtean), new AOEShapeCone(40, 22.5f.Degrees())) { }
    }

    class WarningGale : Components.SelfTargetedAOEs
    {
        public WarningGale() : base(ActionID.MakeSpell(AID.WarningGale), new AOEShapeCircle(6)) { }
    }

    class WindingGaleCharge : Components.ChargeAOEs
    {
        public WindingGaleCharge() : base(ActionID.MakeSpell(AID.WindingGaleCharge), 2) { }
    }

    class BoulderBreak : Components.SharedTankbuster
    {
        public BoulderBreak() : base(ActionID.MakeSpell(AID.BoulderBreak), 5) { }
    }

    class Boulder : Components.LocationTargetedAOEs
    {
        public Boulder() : base(ActionID.MakeSpell(AID.Boulder), 10) { }
    }

    class BrittleBoulder : Components.SpreadFromCastTargets
    {
        public BrittleBoulder() : base(ActionID.MakeSpell(AID.BrittleBoulder), 5) { }
    }

    class TornadoChainInner : Components.SelfTargetedAOEs
    {
        public TornadoChainInner() : base(ActionID.MakeSpell(AID.TornadoChainInner), new AOEShapeCircle(11)) { }
    }

    class TornadoChainOuter : Components.SelfTargetedAOEs
    {
        public TornadoChainOuter() : base(ActionID.MakeSpell(AID.TornadoChainOuter), new AOEShapeDonut(11, 20)) { }
    }

    class KnuckleDrum : Components.CastCounter
    {
        public KnuckleDrum() : base(ActionID.MakeSpell(AID.KnuckleDrum)) { }
    }

    class KnuckleDrumLast : Components.CastCounter
    {
        public KnuckleDrumLast() : base(ActionID.MakeSpell(AID.KnuckleDrumLast)) { }
    }

    class BlowAwayRaidwide : Components.CastCounter
    {
        public BlowAwayRaidwide() : base(ActionID.MakeSpell(AID.BlowAwayRaidwide)) { }
    }

    class BlowAwayPuddle : Components.SelfTargetedAOEs
    {
        public BlowAwayPuddle() : base(ActionID.MakeSpell(AID.BlowAwayPuddle), new AOEShapeCircle(6)) { }
    }

    class ImpactAOE : Components.SelfTargetedAOEs
    {
        public ImpactAOE() : base(ActionID.MakeSpell(AID.ImpactAOE), new AOEShapeCircle(6)) { }
    }

    class ImpactKnockback : Components.KnockbackFromCaster
    {
        public ImpactKnockback() : base(ActionID.MakeSpell(AID.ImpactKnockback), 6) { }
    }

    class BlusteryRuler : Components.SelfTargetedAOEs
    {
        public BlusteryRuler() : base(ActionID.MakeSpell(AID.BlusteryRuler), new AOEShapeCircle(6)) { }
    }

    class DryBlowsRaidwide : Components.CastCounter
    {
        public DryBlowsRaidwide() : base(ActionID.MakeSpell(AID.DryBlowsRaidwide)) { }
    }

    class DryBlowsPuddle : Components.LocationTargetedAOEs
    {
        public DryBlowsPuddle() : base(ActionID.MakeSpell(AID.DryBlowsPuddle), 3) { }
    }

    class IronOut : Components.CastCounter
    {
        public IronOut() : base(ActionID.MakeSpell(AID.IronOutAOE)) { }
    }

    public class Ex4Barbariccia : BossModule
    {
        public Ex4Barbariccia(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }
    }
}
