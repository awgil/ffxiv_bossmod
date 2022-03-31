using System;
using System.Numerics;

namespace BossMod.Endwalker.ARanks.Sugriva
{
    public enum OID : uint
    {
        Boss = 0x35FC,
    };

    public enum AID : uint
    {
        AutoAttack = 872,
        Twister = 27219,
        BarrelingSmash = 27220, // instant cast, charges to random player - starts casting Scythe Tail immediately afterwards
        Spark = 27221,
        ScytheTail = 27222,
        Butcher = 27223,
        Rip = 27224,
        RockThrowFirst = 27225,
        RockThrowRest = 27226,
        Crosswind = 27227,
        ApplyPrey = 27229,
    }

    public class Mechanics : BossModule.Component
    {
        private AOEShapeDonut _spark = new(14, 24);
        private AOEShapeCircle _scytheTail = new(17); // TODO: is this actually a cone? wiki seems to imply it is, but in lumina it looks like a circle...
        private AOEShapeCircle _rockThrow = new(6);
        private AOEShapeCone _butcherRip = new(8, MathF.PI / 4); // TODO: verify angle
        private Actor? _rockThrowTarget;
        private int _numSecondaryRockThrows;

        private static float _twisterRadius = 8;
        private static float _twisterKnockback = 20;

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            var (aoe, pos) = ActiveAOE(module);
            if (aoe?.Check(actor.Position, pos, module.PrimaryActor.Rotation) ?? false)
                hints.Add("GTFO from aoe!");
        }

        public override void AddGlobalHints(BossModule module, BossModule.GlobalHints hints)
        {
            if (!(module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return;

            string hint = (AID)module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.Twister => "Stack and knockback",
                AID.Spark or AID.ScytheTail or AID.Butcher or AID.Rip or AID.RockThrowFirst or AID.RockThrowRest => "Avoidable AOE",
                AID.Crosswind => "Raidwide",
                _ => "",
            };
            if (hint.Length > 0)
                hints.Add(hint);
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var (aoe, pos) = ActiveAOE(module);
            aoe?.Draw(arena, pos, module.PrimaryActor.Rotation);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (module.PrimaryActor.CastInfo?.IsSpell(AID.Twister) ?? false)
            {
                var target = module.WorldState.Actors.Find(module.PrimaryActor.CastInfo!.TargetID);
                if (target != null)
                {
                    arena.Actor(target, arena.ColorDanger);
                    arena.AddCircle(target.Position, _twisterRadius, arena.ColorDanger);
                    if (GeometryUtils.PointInCircle(pc.Position - target.Position, _twisterRadius))
                    {
                        var kbPos = BossModule.AdjustPositionForKnockback(pc.Position, target, _twisterKnockback);
                        if (kbPos != pc.Position)
                        {
                            arena.AddLine(pc.Position, kbPos, arena.ColorDanger);
                            arena.Actor(kbPos, pc.Rotation, arena.ColorDanger);
                        }
                    }
                }
            }

            if (_rockThrowTarget != null)
            {
                arena.Actor(_rockThrowTarget, arena.ColorDanger);
                arena.AddCircle(_rockThrowTarget.Position, _rockThrow.Radius, arena.ColorDanger);
            }
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (info.CasterID != module.PrimaryActor.InstanceID)
                return;

            if (info.IsSpell(AID.ApplyPrey))
            {
                _rockThrowTarget = module.WorldState.Actors.Find(info.MainTargetID);
            }
            else if (info.IsSpell(AID.RockThrowRest) && ++_numSecondaryRockThrows == 2)
            {
                _rockThrowTarget = null;
                _numSecondaryRockThrows = 0;
            }
        }

        private (AOEShape?, Vector3) ActiveAOE(BossModule module)
        {
            if (!(module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return (null, new());

            return (AID)module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.Spark => (_spark, module.PrimaryActor.Position),
                AID.ScytheTail => (_scytheTail, module.PrimaryActor.Position),
                AID.RockThrowFirst or AID.RockThrowRest => (_rockThrow, module.PrimaryActor.CastInfo.Location),
                AID.Butcher or AID.Rip => (_butcherRip, module.PrimaryActor.Position),
                _ => (null, new())
            };
        }
    }

    public class Sugriva : SimpleBossModule
    {
        public Sugriva(BossModuleManager manager, Actor primary)
            : base(manager, primary)
        {
            BuildStateMachine<Mechanics>();
        }
    }
}
