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
        private AOEShapeCircle _tidalGuillotine = new(13);
        private AOEShapeCone _ancientBlizzard = new(40, MathF.PI / 8);

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (ActiveAOE(module)?.Check(actor.Position, module.PrimaryActor) ?? false)
                hints.Add("GTFO from aoe!");
        }

        public override void AddGlobalHints(BossModule module, BossModule.GlobalHints hints)
        {
            if (!(module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return;

            string hint = (AID)module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.MarineMayhem => "Interruptible Raidwide",
                AID.TidalGuillotine or AID.AncientBlizzard => "Avoidable AOE",
                AID.Waterga => "AOE marker",
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
            BuildStateMachine<Mechanics>();
        }
    }
}
