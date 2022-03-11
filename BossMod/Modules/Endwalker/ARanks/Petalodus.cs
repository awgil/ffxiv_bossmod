using System;

namespace BossMod.Endwalker.ARanks.Petalodus
{
    public enum OID : uint
    {
        Boss = 0x35FB,
    };

    public enum AID : uint
    {
        AutoAttack = 872,
        MarineMayhem = 27063,
        Waterga = 27067,
        TidalGuillotine = 27068,
        AncientBlizzard = 27069,
    }

    public class Mechanics : BossModule.Component
    {
        private BossModule _module;
        private AOEShapeCircle _tidalGuillotine = new(13);
        private AOEShapeCone _ancientBlizzard = new(40, MathF.PI / 4); // TODO: verify angle

        public Mechanics(BossModule module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (ActiveAOE()?.Check(actor.Position, _module.PrimaryActor) ?? false)
                hints.Add("GTFO from aoe!");
        }

        public override void AddGlobalHints(BossModule.GlobalHints hints)
        {
            if (!(_module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return;

            string hint = (AID)_module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.MarineMayhem => "Interruptible Raidwide",
                AID.TidalGuillotine or AID.AncientBlizzard => "Avoidable AOE",
                AID.Waterga => "AOE marker",
                _ => "",
            };
            if (hint.Length > 0)
                hints.Add(hint);
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            ActiveAOE()?.Draw(arena, _module.PrimaryActor);
        }

        private AOEShape? ActiveAOE()
        {
            if (!(_module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return null;

            return (AID)_module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.TidalGuillotine => _tidalGuillotine,
                AID.AncientBlizzard => _ancientBlizzard,
                _ => null
            };
        }
    }

    public class Petalodus : SimpleBossModule
    {
        public Petalodus(BossModuleManager manager, Actor primary)
            : base(manager, primary)
        {
            InitialState?.Enter.Add(() => ActivateComponent(new Mechanics(this)));
        }
    }
}
