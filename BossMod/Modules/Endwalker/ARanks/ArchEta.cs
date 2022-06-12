using System;

namespace BossMod.Endwalker.ARanks.ArchEta
{
    public enum OID : uint
    {
        Boss = 0x35C0,
    };

    public enum AID : uint
    {
        AutoAttack = 870,
        EnergyWave = 27269,
        TailSwipe = 27270,
        HeavyStomp = 27271,
        SonicHowl = 27272,
        SteelFang = 27273,
        FangedLunge = 27274,
    }

    public class Mechanics : BossModule.Component
    {
        private AOEShapeCircle _heavyStomp = new(17);
        private AOEShapeRect _energyWave = new(40, 7);
        private AOEShapeCone _tailSwipe = new(25, 45.Degrees(), 180.Degrees());

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
                AID.HeavyStomp or AID.EnergyWave or AID.TailSwipe => "Avoidable AOE",
                AID.SonicHowl => "Raidwide",
                AID.SteelFang => "Tankbuster",
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
                AID.HeavyStomp => _heavyStomp,
                AID.EnergyWave => _energyWave,
                AID.TailSwipe => _tailSwipe,
                _ => null
            };
        }
    }

    public class ArchEta : SimpleBossModule
    {
        public ArchEta(BossModuleManager manager, Actor primary)
            : base(manager, primary)
        {
            BuildStateMachine<Mechanics>();
        }
    }
}
