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
        private BossModule _module;
        private AOEShapeCircle _heavyStomp = new(17);
        private AOEShapeRect _energyWave = new(40, 7);
        private AOEShapeCone _tailSwipe = new(25, MathF.PI / 4, MathF.PI);

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
                AID.HeavyStomp or AID.EnergyWave or AID.TailSwipe => "Avoidable AOE",
                AID.SonicHowl => "Raidwide",
                AID.SteelFang => "Tankbuster",
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
            InitialState?.Enter.Add(() => ActivateComponent(new Mechanics(this)));
        }
    }
}
