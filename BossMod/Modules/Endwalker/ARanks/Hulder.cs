using System;

namespace BossMod.Endwalker.ARanks.Hulder
{
    public enum OID : uint
    {
        Boss = 0x35DD,
    };

    public enum AID : uint
    {
        AutoAttack = 872,
        LayOfMislaidMemory = 27073,
        TempestuousWrath = 27075,
        RottingElegy = 27076,
        OdeToLostLove = 27077,
        StormOfColor = 27078,
    }

    public class Mechanics : BossModule.Component
    {
        private BossModule _module;
        private AOEShapeCone _layOfMislaidMemory = new(30, MathF.PI / 4); // TODO: verify angle
        private AOEShapeRect _tempestuousWrath = new(0, 4);
        private AOEShapeDonut _rottingElegy = new(5.4f, 50); // TODO: verify inner radius

        public Mechanics(BossModule module)
        {
            _module = module;
        }

        public override void Update()
        {
            if (_module.PrimaryActor.CastInfo?.IsSpell(AID.TempestuousWrath) ?? false)
            {
                _tempestuousWrath.SetEndPointFromCastLocation(_module.PrimaryActor);
            }
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
                AID.LayOfMislaidMemory or AID.TempestuousWrath or AID.RottingElegy => "Avoidable AOE",
                AID.StormOfColor => "Tankbuster",
                AID.OdeToLostLove => "Raidwide",
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
                AID.LayOfMislaidMemory => _layOfMislaidMemory,
                AID.TempestuousWrath => _tempestuousWrath,
                AID.RottingElegy => _rottingElegy,
                _ => null
            };
        }
    }

    public class Hulder : SimpleBossModule
    {
        public Hulder(BossModuleManager manager, Actor primary)
            : base(manager, primary)
        {
            InitialState?.Enter.Add(() => ActivateComponent(new Mechanics(this)));
        }
    }
}
