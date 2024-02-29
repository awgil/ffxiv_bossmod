using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic component for mechanics that require baiting some aoe (by proximity, by tether, etc) away from raid
    // some players can be marked as 'forbidden' - if any of them is baiting, they are warned
    // otherwise we show own bait as as outline (and warn if player is clipping someone) and other baits as filled (and warn if player is being clipped)
    public class GenericBaitAway : CastCounter
    {
        public struct Bait
        {
            public Actor Source;
            public Actor Target;
            public AOEShape Shape;

            public Angle Rotation => Angle.FromDirection(Target.Position - Source.Position);

            public Bait(Actor source, Actor target, AOEShape shape)
            {
                Source = source;
                Target = target;
                Shape = shape;
            }
        }

        public bool AlwaysDrawOtherBaits; // if false, other baits are drawn only if they are clipping a player
        public bool CenterAtTarget; // if true, aoe source is at target
        public bool AllowDeadTargets = true; // if false, baits with dead targets are ignored
        public bool EnableHints = true;
        public bool IgnoreOtherBaits = false; // if true, don't show hints/aoes for baits by others
        public PlayerPriority BaiterPriority = PlayerPriority.Interesting;
        public BitMask ForbiddenPlayers; // these players should avoid baiting
        public List<Bait> CurrentBaits = new();

        public IEnumerable<Bait> ActiveBaits => AllowDeadTargets ? CurrentBaits : CurrentBaits.Where(b => !b.Target.IsDead);
        public IEnumerable<Bait> ActiveBaitsOn(Actor target) => ActiveBaits.Where(b => b.Target == target);
        public IEnumerable<Bait> ActiveBaitsNotOn(Actor target) => ActiveBaits.Where(b => b.Target != target);
        public WPos BaitOrigin(Bait bait) => (CenterAtTarget ? bait.Target : bait.Source).Position;
        public bool IsClippedBy(Actor actor, Bait bait) => bait.Shape.Check(actor.Position, BaitOrigin(bait), bait.Rotation);
        public IEnumerable<Actor> PlayersClippedBy(BossModule module, Bait bait) => module.Raid.WithoutSlot().Exclude(bait.Target).InShape(bait.Shape, BaitOrigin(bait), bait.Rotation);

        public GenericBaitAway(ActionID aid = default, bool alwaysDrawOtherBaits = true, bool centerAtTarget = false) : base(aid)
        {
            AlwaysDrawOtherBaits = alwaysDrawOtherBaits;
            CenterAtTarget = centerAtTarget;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (!EnableHints)
                return;

            if (ForbiddenPlayers[slot])
            {
                if (ActiveBaitsOn(actor).Any())
                    hints.Add("Avoid baiting!");
            }
            else
            {
                if (ActiveBaitsOn(actor).Any(b => PlayersClippedBy(module, b).Any()))
                    hints.Add("Bait away from raid!");
            }

            if (!IgnoreOtherBaits && ActiveBaitsNotOn(actor).Any(b => IsClippedBy(actor, b)))
                hints.Add("GTFO from baited aoe!");
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return ActiveBaitsOn(player).Any() ? BaiterPriority : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (!IgnoreOtherBaits)
                foreach (var bait in ActiveBaitsNotOn(pc))
                    if (AlwaysDrawOtherBaits || IsClippedBy(pc, bait))
                        bait.Shape.Draw(arena, BaitOrigin(bait), bait.Rotation);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var bait in ActiveBaitsOn(pc))
            {
                bait.Shape.Outline(arena, BaitOrigin(bait), bait.Rotation);
            }
        }
    }

    // bait on all players, requiring everyone to spread out, by default originating from primary actor
    public class BaitAwayEveryone : GenericBaitAway
    {
        public AOEShape Shape;

        public BaitAwayEveryone(AOEShape shape, ActionID aid = default) : base(aid)
        {
            Shape = shape;
            AllowDeadTargets = false;
        }

        public override void Init(BossModule module) => SetSource(module, module.PrimaryActor);

        public void SetSource(BossModule module, Actor source)
        {
            CurrentBaits.Clear();
            CurrentBaits.AddRange(module.Raid.WithoutSlot(true).Select(p => new Bait(source, p, Shape)));
        }
    }

    // component for mechanics requiring tether targets to bait their aoe away from raid
    public class BaitAwayTethers : GenericBaitAway
    {
        public bool DrawTethers = true;
        public AOEShape Shape;
        public uint TID;

        public BaitAwayTethers(AOEShape shape, uint tetherID, ActionID aid = default) : base(aid)
        {
            Shape = shape;
            TID = tetherID;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);
            if (DrawTethers)
            {
                foreach (var b in ActiveBaits)
                {
                    if (arena.Config.ShowOutlinesAndShadows)
                        arena.AddLine(b.Source.Position, b.Target.Position, 0xFF000000, 2);
                    arena.AddLine(b.Source.Position, b.Target.Position, ArenaColor.Danger);
                }
            }
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            var (player, enemy) = DetermineTetherSides(module, source, tether);
            if (player != null && enemy != null)
            {
                CurrentBaits.Add(new(enemy, player, Shape));
            }
        }

        public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            var (player, enemy) = DetermineTetherSides(module, source, tether);
            if (player != null && enemy != null)
            {
                CurrentBaits.RemoveAll(b => b.Source == enemy && b.Target == player);
            }
        }

        // we support both player->enemy and enemy->player tethers
        private (Actor? player, Actor? enemy) DetermineTetherSides(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID != TID)
                return (null, null);

            var target = module.WorldState.Actors.Find(tether.Target);
            if (target == null)
                return (null, null);

            var (player, enemy) = source.Type == ActorType.Player ? (source, target) : (target, source);
            if (player.Type != ActorType.Player || enemy.Type == ActorType.Player)
            {
                module.ReportError(this, $"Unexpected tether pair: {source.InstanceID:X} -> {target.InstanceID:X}");
                return (null, null);
            }

            return (player, enemy);
        }
    }

    // component for mechanics requiring cast targets to gtfo from raid (aoe tankbusters etc)
    public class BaitAwayCast : GenericBaitAway
    {
        public AOEShape Shape;

        public BaitAwayCast(ActionID aid, AOEShape shape, bool centerAtTarget = false) : base(aid, centerAtTarget: centerAtTarget)
        {
            Shape = shape;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction && module.WorldState.Actors.Find(spell.TargetID) is var target && target != null)
                CurrentBaits.Add(new(caster, target, Shape));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                CurrentBaits.RemoveAll(b => b.Source == caster);
        }
    }

    // a variation of BaitAwayCast for charges that end at target
    public class BaitAwayChargeCast : GenericBaitAway
    {
        public float HalfWidth;

        public BaitAwayChargeCast(ActionID aid, float halfWidth) : base(aid)
        {
            HalfWidth = halfWidth;
        }

        public override void Update(BossModule module)
        {
            foreach (var b in CurrentBaits)
                ((AOEShapeRect)b.Shape).LengthFront = (b.Target.Position - b.Source.Position).Length();
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction && module.WorldState.Actors.Find(spell.TargetID) is var target && target != null)
                CurrentBaits.Add(new(caster, target, new AOEShapeRect(0, HalfWidth)));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                CurrentBaits.RemoveAll(b => b.Source == caster);
        }
    }
}
