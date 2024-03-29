using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic 'chasing AOE' component - these are AOEs that follow the target for a set amount of casts
    public class ChasingAOEs : GenericAOEs
    {
        public class Chaser
        {
            public Actor Player;
            public WPos? Pos;
            public int NumCasts;
            public DateTime Activation;

            public Chaser(Actor player)
            {
                Player = player;
            }

            public WPos PredictedPosition()
            {
                if (Pos == null)
                    return default;
                if (NumCasts == 0)
                    return Pos.Value;
                var toPlayer = Player.Position - Pos.Value;
                var dist = toPlayer.Length();
                if (dist < MoveDist)
                    return Player.Position;
                return Pos.Value + toPlayer * MoveDist / dist;
            }
        }

        private List<Chaser> _chasers = new();

        public bool Active => _chasers.Count > 0;

        public AOEShape Shape;
        public static int MaxCasts;
        public static float MoveDist;
        public bool LocationTargeted; //if true chaser is location targeted instead of self targeted
        public Angle Rotation;
        public uint Icon;
        public ActionID ChasingAOEFirst;
        public ActionID ChasingAOERest;
        public float TimeBetweenCasts;

        public ChasingAOEs(uint icon, AOEShape shape, ActionID chasingAOEFirst, ActionID chasingAOERest, float movedist, int maxCasts, float timeBetweenCasts, bool locationtargeted = false, Angle rotation = default) : base(chasingAOEFirst, "GTFO from chasing AOE!")
        {
            Shape = shape;
            Icon = icon;
            ChasingAOEFirst = chasingAOEFirst;
            ChasingAOERest = chasingAOERest;
            MoveDist = movedist;
            MaxCasts = maxCasts;
            TimeBetweenCasts = timeBetweenCasts;
            LocationTargeted = locationtargeted;
            Rotation = rotation;
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _chasers.Select(c => new AOEInstance(Shape, c.PredictedPosition(), Rotation, c.Activation));

        public override void Update(BossModule module)
        {
            _chasers.RemoveAll(c => (c.Player.IsDestroyed || c.Player.IsDead) && (c.Pos == null || c.NumCasts > 0));
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var c in _chasers)
                if (c.Pos != null)
                {                
                    if (arena.Config.ShowOutlinesAndShadows)
                        arena.AddLine(c.Pos.Value, c.Player.Position, 0xFF000000, 2);
                    arena.AddLine(c.Pos.Value, c.Player.Position, ArenaColor.Danger);
                }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == Icon)
                _chasers.Add(new(actor));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == ChasingAOEFirst && _chasers.Where(c => c.Pos == null).MinBy(c => (c.Player.Position - caster.Position).LengthSq()) is var chaser && chaser != null)
            {
                if (LocationTargeted)
                    chaser.Pos = spell.LocXZ;
                else
                    chaser.Pos = caster.Position;
                chaser.Activation = spell.NPCFinishAt;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == ChasingAOEFirst)
            {
                if (LocationTargeted)
                    Advance(module, spell.LocXZ);
                else
                    Advance(module, caster.Position);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (spell.Action == ChasingAOERest)
            {
                if (LocationTargeted)
                    Advance(module, spell.TargetXZ);
                else
                    Advance(module, caster.Position);
            }
        }

        private void Advance(BossModule module, WPos pos)
        {
            ++NumCasts;
            var chaser = _chasers.MinBy(c => c.Pos != null ? (c.PredictedPosition() - pos).LengthSq() : float.MaxValue);
            if (chaser == null)
                return;

            if (++chaser.NumCasts < MaxCasts)
            {
                chaser.Pos = pos;
                chaser.Activation = module.WorldState.CurrentTime.AddSeconds(TimeBetweenCasts);
            }
            else
            {
                _chasers.Remove(chaser);
            }
        }
    }
}
