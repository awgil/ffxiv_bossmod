using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic component for mechanics that require baiting some aoe (by proximity, by tether, etc) away from raid
    // some players can be marked as 'forbidden' - if any of them is baiting, they are warned
    // otherwise we show own bait as as outline (and warn if player is clipping someone) and other baits as filled (and warn if player is being clipped)
    public class GenericBaitAway : CastCounter
    {
        public bool AlwaysDrawOtherBaits; // if false, other baits are drawn only if they are clipping a player
        public bool CenterAtTarget; // if true, aoe source is at target
        public bool AllowDeadTargets = true; // if false, baits with dead targets are ignored
        public BitMask ForbiddenPlayers;
        public List<(Actor source, Actor target, AOEShape shape)> CurrentBaits = new();

        public IEnumerable<(Actor source, Actor target, AOEShape shape)> ActiveBaits => AllowDeadTargets ? CurrentBaits : CurrentBaits.Where(b => !b.target.IsDead);

        public GenericBaitAway(ActionID aid = default, bool alwaysDrawOtherBaits = true, bool centerAtTarget = false) : base(aid)
        {
            AlwaysDrawOtherBaits = alwaysDrawOtherBaits;
            CenterAtTarget = centerAtTarget;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            foreach (var bait in ActiveBaits)
            {
                if (bait.target == actor)
                {
                    if (ForbiddenPlayers[slot])
                        hints.Add("Avoid baiting!");
                    else if (module.Raid.WithoutSlot().Exclude(actor).InShape(bait.shape, (CenterAtTarget ? bait.target : bait.source).Position, Angle.FromDirection(bait.target.Position - bait.source.Position)).Any())
                        hints.Add("Bait away from raid!");
                }
                else if (bait.shape.Check(actor.Position, (CenterAtTarget ? bait.target : bait.source).Position, Angle.FromDirection(bait.target.Position - bait.source.Position)))
                {
                    hints.Add("GTFO from baited aoe!");
                }
            }
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return ActiveBaits.Any(b => b.target == player) ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var bait in ActiveBaits.Where(bait => bait.target != pc))
            {
                var dir = Angle.FromDirection(bait.target.Position - bait.source.Position);
                if (AlwaysDrawOtherBaits || bait.shape.Check(pc.Position, (CenterAtTarget ? bait.target : bait.source).Position, dir))
                    bait.shape.Draw(arena, (CenterAtTarget ? bait.target : bait.source).Position, dir);
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var bait in ActiveBaits.Where(bait => bait.target == pc))
            {
                bait.shape.Outline(arena, (CenterAtTarget ? bait.target : bait.source).Position, Angle.FromDirection(bait.target.Position - bait.source.Position));
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
            CurrentBaits.AddRange(module.Raid.WithoutSlot(true).Select(p => (source, p, Shape)));
        }
    }

    // component for mechanics requiring tether targets to bait their aoe away from raid
    public class BaitAwayTethers : GenericBaitAway
    {
        public AOEShape Shape;
        public uint TID;

        public BaitAwayTethers(AOEShape shape, uint tetherID, ActionID aid = default) : base(aid)
        {
            Shape = shape;
            TID = tetherID;
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            var (player, enemy) = DetermineTetherSides(module, source, tether);
            if (player != null && enemy != null)
            {
                CurrentBaits.Add((enemy, player, Shape));
            }
        }

        public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            var (player, enemy) = DetermineTetherSides(module, source, tether);
            if (player != null && enemy != null)
            {
                CurrentBaits.RemoveAll(b => b.source == enemy && b.target == player);
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
                CurrentBaits.Add((caster, target, Shape));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                CurrentBaits.RemoveAll(b => b.source == caster);
        }
    }
}
