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
        private BossModule _module;
        private float? _threnodyDirection = null;
        private AOEShapeCone _princessThrenody = new(40, MathF.PI / 3); // TODO: verify angle
        private AOEShapeCircle _amorphicFlail = new(9);
        private AOEShapeCircle _princessCacophony = new(12);

        public Mechanics(BossModule module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            bool inAOE = _threnodyDirection != null && _princessThrenody.Check(actor.Position, _module.PrimaryActor.Position, _threnodyDirection.Value);
            if (!inAOE)
            {
                var (aoe, pos) = ActiveAOE(actor);
                inAOE = aoe?.Check(actor.Position, pos, _module.PrimaryActor.Rotation) ?? false;
            }
            if (inAOE)
                hints.Add("GTFO from aoe!");
        }

        public override void AddGlobalHints(BossModule.GlobalHints hints)
        {
            if (!(_module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return;

            string hint = (AID)_module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.WhimsyAlaMode => "Select direction",
                AID.PrincessThrenodyPrepare or AID.PrincessThrenodyResolve or AID.AmorphicFlail or AID.PrincessCacophony => "Avoidable AOE",
                AID.Banish => "Tankbuster",
                _ => "",
            };
            if (hint.Length > 0)
                hints.Add(hint);
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (_threnodyDirection != null)
                _princessThrenody.Draw(arena, _module.PrimaryActor.Position, _threnodyDirection.Value);
            var (aoe, pos) = ActiveAOE(pc);
            aoe?.Draw(arena, pos, _module.PrimaryActor.Rotation);
        }

        public override void OnCastStarted(Actor actor)
        {
            if (actor == _module.PrimaryActor && actor.CastInfo!.IsSpell(AID.PrincessThrenodyPrepare))
                _threnodyDirection = _module.PrimaryActor.Rotation + ThrenodyDirection();
        }

        public override void OnCastFinished(Actor actor)
        {
            if (actor == _module.PrimaryActor && actor.CastInfo!.IsSpell(AID.PrincessThrenodyResolve))
                _threnodyDirection = null;
        }

        private (AOEShape?, Vector3) ActiveAOE(Actor pc)
        {
            if (!(_module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return (null, new());

            return (AID)_module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.AmorphicFlail => (_amorphicFlail, _module.PrimaryActor.Position),
                AID.PrincessCacophony => (_princessCacophony, _module.PrimaryActor.CastInfo.Location),
                _ => (null, new())
            };
        }

        private float ThrenodyDirection()
        {
            foreach (var s in _module.PrimaryActor.Statuses)
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
            InitialState?.Enter.Add(() => ActivateComponent(new Mechanics(this)));
        }
    }
}
