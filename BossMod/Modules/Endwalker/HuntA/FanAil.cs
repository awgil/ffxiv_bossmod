namespace BossMod.Endwalker.HuntA.FanAil
{
    public enum OID : uint
    {
        Boss = 0x35C1,
    };

    public enum AID : uint
    {
        Divebomb = 27373,
        DivebombDisappear = 27374,
        DivebombReappear = 27375,
        LiquidHell = 27376,
        Plummet = 27378,
        DeathSentence = 27379,
        CycloneWing = 27380,
        AutoAttack = 27381,
    }

    public class Mechanics : BossComponent
    {
        private AOEShapeCone _plummet = new(8, 45.Degrees());
        private AOEShapeRect _divebomb = new(30, 5.5f);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (ActiveAOE(module)?.Check(actor.Position, module.PrimaryActor) ?? false)
                hints.Add("GTFO from aoe!");
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (!(module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return;

            string hint = (AID)module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.CycloneWing => "Raidwide",
                AID.Plummet or AID.Divebomb or AID.LiquidHell => "Avoidable AOE",
                AID.DeathSentence => "Tankbuster",
                _ => "",
            };
            if (hint.Length > 0)
                hints.Add(hint);
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            ActiveAOE(module)?.Draw(arena, module.PrimaryActor);
        }

        private AOEShape? ActiveAOE(BossModule module)
        {
            if (!(module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return null;

            return (AID)module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.Plummet => _plummet,
                AID.Divebomb => _divebomb,
                _ => null
            };
        }
    }

    public class FanAilStates : StateMachineBuilder
    {
        public FanAilStates(BossModule module) : base(module)
        {
            TrivialPhase().ActivateOnEnter<Mechanics>();
        }
    }

    public class FanAil : SimpleBossModule
    {
        public FanAil(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
