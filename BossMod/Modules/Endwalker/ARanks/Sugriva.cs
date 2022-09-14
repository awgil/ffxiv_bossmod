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

    public class Mechanics : BossComponent
    {
        private AOEShapeDonut _spark = new(14, 24);
        private AOEShapeCircle _scytheTail = new(17);
        private AOEShapeCircle _rockThrow = new(6);
        private AOEShapeCone _butcherRip = new(8, 45.Degrees()); // TODO: verify angle, too little data points so far...
        private Actor? _rockThrowTarget;
        private int _numSecondaryRockThrows;

        private static float _twisterRadius = 8;
        private static float _twisterKnockback = 20;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var (aoe, pos) = ActiveAOE(module);
            if (aoe?.Check(actor.Position, pos, module.PrimaryActor.Rotation) ?? false)
                hints.Add("GTFO from aoe!");
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
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

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            if ((module.PrimaryActor.CastInfo?.IsSpell(AID.Twister) ?? false) && module.PrimaryActor.CastInfo!.TargetID == player.InstanceID)
                return PlayerPriority.Interesting;
            if (player == _rockThrowTarget)
                return PlayerPriority.Interesting;
            return PlayerPriority.Irrelevant;
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
                    arena.AddCircle(target.Position, _twisterRadius, ArenaColor.Danger);
                    if (pc.Position.InCircle(target.Position, _twisterRadius))
                    {
                        var kbPos = Components.Knockback.AwayFromSource(pc.Position, target, _twisterKnockback);
                        if (kbPos != pc.Position)
                        {
                            arena.AddLine(pc.Position, kbPos, ArenaColor.Danger);
                            arena.Actor(kbPos, pc.Rotation, ArenaColor.Danger);
                        }
                    }
                }
            }

            if (_rockThrowTarget != null)
            {
                arena.AddCircle(_rockThrowTarget.Position, _rockThrow.Radius, ArenaColor.Danger);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (caster != module.PrimaryActor)
                return;

            if ((AID)spell.Action.ID == AID.ApplyPrey)
            {
                _rockThrowTarget = module.WorldState.Actors.Find(spell.MainTargetID);
            }
            else if ((AID)spell.Action.ID == AID.RockThrowRest && ++_numSecondaryRockThrows == 2)
            {
                _rockThrowTarget = null;
                _numSecondaryRockThrows = 0;
            }
        }

        private (AOEShape?, WPos) ActiveAOE(BossModule module)
        {
            if (!(module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return (null, new());

            return (AID)module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.Spark => (_spark, module.PrimaryActor.Position),
                AID.ScytheTail => (_scytheTail, module.PrimaryActor.Position),
                AID.RockThrowFirst or AID.RockThrowRest => (_rockThrow, module.PrimaryActor.CastInfo.LocXZ),
                AID.Butcher or AID.Rip => (_butcherRip, module.PrimaryActor.Position),
                _ => (null, new())
            };
        }
    }

    public class SugrivaStates : StateMachineBuilder
    {
        public SugrivaStates(BossModule module) : base(module)
        {
            TrivialPhase().ActivateOnEnter<Mechanics>();
        }
    }

    public class Sugriva : SimpleBossModule
    {
        public Sugriva(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
