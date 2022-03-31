using System;
using System.Numerics;

namespace BossMod.Endwalker.ARanks.MoussePrincess
{
    public enum OID : uint
    {
        Boss = 0x360B,
    };

    public enum AID : uint
    {
        AutoAttack = 872,
        PrincessThrenodyPrepare = 27318,
        PrincessThrenodyResolve = 27319,
        WhimsyAlaMode = 27320,
        AmorphicFlail = 27321,
        PrincessCacophony = 27322,
        Banish = 27323,
    }

    public enum SID : uint
    {
        RightwardWhimsy = 2840,
        LeftwardWhimsy = 2841,
        BackwardWhimsy = 2842,
        ForwardWhimsy = 2958,
    }

    public class Mechanics : BossModule.Component
    {
        private float? _threnodyDirection = null;
        private AOEShapeCone _princessThrenody = new(40, MathF.PI / 3);
        private AOEShapeCircle _amorphicFlail = new(9);
        private AOEShapeCircle _princessCacophony = new(12);

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            bool inAOE = _threnodyDirection != null && _princessThrenody.Check(actor.Position, module.PrimaryActor.Position, _threnodyDirection.Value);
            if (!inAOE)
            {
                var (aoe, pos) = ActiveAOE(module, actor);
                inAOE = aoe?.Check(actor.Position, pos, module.PrimaryActor.Rotation) ?? false;
            }
            if (inAOE)
                hints.Add("GTFO from aoe!");
        }

        public override void AddGlobalHints(BossModule module, BossModule.GlobalHints hints)
        {
            if (!(module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return;

            string hint = (AID)module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.WhimsyAlaMode => "Select direction",
                AID.PrincessThrenodyPrepare or AID.PrincessThrenodyResolve or AID.AmorphicFlail or AID.PrincessCacophony => "Avoidable AOE",
                AID.Banish => "Tankbuster",
                _ => "",
            };
            if (hint.Length > 0)
                hints.Add(hint);
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_threnodyDirection != null)
                _princessThrenody.Draw(arena, module.PrimaryActor.Position, _threnodyDirection.Value);
            var (aoe, pos) = ActiveAOE(module, pc);
            aoe?.Draw(arena, pos, module.PrimaryActor.Rotation);
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor == module.PrimaryActor && actor.CastInfo!.IsSpell(AID.PrincessThrenodyPrepare))
                _threnodyDirection = module.PrimaryActor.Rotation + ThrenodyDirection(module);
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (actor == module.PrimaryActor && actor.CastInfo!.IsSpell(AID.PrincessThrenodyResolve))
                _threnodyDirection = null;
        }

        private (AOEShape?, Vector3) ActiveAOE(BossModule module, Actor pc)
        {
            if (!(module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return (null, new());

            return (AID)module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.AmorphicFlail => (_amorphicFlail, module.PrimaryActor.Position),
                AID.PrincessCacophony => (_princessCacophony, module.PrimaryActor.CastInfo.Location),
                _ => (null, new())
            };
        }

        private float ThrenodyDirection(BossModule module)
        {
            foreach (var s in module.PrimaryActor.Statuses)
            {
                switch ((SID)s.ID)
                {
                    case SID.RightwardWhimsy: return -MathF.PI / 2;
                    case SID.LeftwardWhimsy: return MathF.PI / 2;
                    case SID.BackwardWhimsy: return MathF.PI;
                    case SID.ForwardWhimsy: return 0;
                }
            }
            return 0;
        }
    }

    public class MoussePrincess : SimpleBossModule
    {
        public MoussePrincess(BossModuleManager manager, Actor primary)
            : base(manager, primary)
        {
            BuildStateMachine<Mechanics>();
        }
    }
}
