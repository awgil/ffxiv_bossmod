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
        private AOEShapeCone _layOfMislaidMemory = new(30, MathF.PI / 4); // TODO: verify angle
        private AOEShapeRect _tempestuousWrath = new(0, 4);
        private AOEShapeDonut _rottingElegy = new(5.4f, 50); // TODO: verify inner radius

        public override void Update(BossModule module)
        {
            if (module.PrimaryActor.CastInfo?.IsSpell(AID.TempestuousWrath) ?? false)
            {
                _tempestuousWrath.SetEndPointFromCastLocation(module.PrimaryActor);
            }
        }

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
                AID.LayOfMislaidMemory or AID.TempestuousWrath or AID.RottingElegy => "Avoidable AOE",
                AID.StormOfColor => "Tankbuster",
                AID.OdeToLostLove => "Raidwide",
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
            BuildStateMachine<Mechanics>();
        }
    }
}
