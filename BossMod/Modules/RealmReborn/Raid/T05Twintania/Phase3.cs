using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.RealmReborn.Raid.T05Twintania
{
    // P3 mechanics
    // TODO: preposition for divebombs? it seems that boss spawns in one of the fixed spots that is closest to target...
    class P3Divebomb : Components.GenericAOEs
    {
        public WPos? Target { get; private set; }
        public DateTime HitAt { get; private set; }

        private static AOEShapeRect _shape = new(35, 6);

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (Target != null)
            {
                if (module.PrimaryActor.CastInfo == null)
                    yield return (_shape, module.PrimaryActor.Position, Angle.FromDirection(Target.Value - module.PrimaryActor.Position), HitAt);
                else
                    yield return (_shape, module.PrimaryActor.Position, module.PrimaryActor.CastInfo.Rotation, module.PrimaryActor.CastInfo.FinishAt);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            switch ((AID)spell.Action.ID)
            {
                case AID.DivebombMarker:
                    Target = module.WorldState.Actors.Find(spell.MainTargetID)?.Position;
                    HitAt = module.WorldState.CurrentTime.AddSeconds(1.7f);
                    break;
                case AID.DivebombAOE:
                    Target = null;
                    break;
            }
        }
    }

    class P3Adds : BossComponent
    {
        private List<Actor> _hygieia = new();
        public List<Actor> Asclepius { get; private set; } = new();
        public IEnumerable<Actor> ActiveHygieia => _hygieia.Where(a => !a.IsDead);

        private const float _explosionRadius = 8;

        public override void Init(BossModule module)
        {
            _hygieia = module.Enemies(OID.Hygieia);
            Asclepius = module.Enemies(OID.Asclepius);
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            var nextHygieia = ActiveHygieia.MinBy(a => a.InstanceID); // select next add to kill by lowest hp
            var asclepiusVuln = Asclepius.FirstOrDefault()?.FindStatus(SID.Disseminate);
            bool killHygieia = asclepiusVuln == null || (asclepiusVuln.Value.ExpireAt - module.WorldState.CurrentTime).TotalSeconds < 10;
            foreach (var e in hints.PotentialTargets)
            {
                switch ((OID)e.Actor.OID)
                {
                    case OID.Hygieia:
                        var predictedHP = e.Actor.HP.Cur + module.WorldState.PendingEffects.PendingHPDifference(e.Actor.InstanceID);
                        e.Priority = e.Actor.HP.Cur == 1 ? 0
                            : killHygieia && e.Actor == nextHygieia ? 2
                            : predictedHP < 0.3f * e.Actor.HP.Max ? -1
                            : 1;
                        e.ShouldBeTanked = assignment == PartyRolesConfig.Assignment.OT;
                        bool gtfo = predictedHP <= (e.ShouldBeTanked ? 1 : 0.1f * e.Actor.HP.Max);
                        if (gtfo)
                            hints.AddForbiddenZone(ShapeDistance.Circle(e.Actor.Position, 9));
                        break;
                    case OID.Asclepius:
                        e.Priority = 1;
                        e.AttackStrength = 0.15f;
                        e.ShouldBeTanked = assignment == PartyRolesConfig.Assignment.MT;
                        break;
                }
            }

            if (!module.PrimaryActor.IsTargetable && !ActiveHygieia.Any() && !Asclepius.Any(a => !a.IsDead))
            {
                // once all adds are dead, gather where boss will return
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(-6.67f, 5), 5), DateTime.MaxValue);
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var a in ActiveHygieia)
            {
                arena.Actor(a, ArenaColor.Enemy);
                arena.AddCircle(a.Position, _explosionRadius, ArenaColor.Danger);
            }
            foreach (var a in Asclepius)
                arena.Actor(a, ArenaColor.Enemy);
        }
    }

    class P3AethericProfusion : Components.CastCounter
    {
        private DateTime _activation;

        public P3AethericProfusion() : base(ActionID.MakeSpell(AID.AethericProfusion)) { }

        public override void Init(BossModule module)
        {
            _activation = module.WorldState.CurrentTime.AddSeconds(6.7f);
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            // select neurolinks to stand at; let everyone except MT stay in one closer to boss
            var neurolinks = module.Enemies(OID.Neurolink);
            var closerNeurolink = neurolinks.Closest(module.PrimaryActor.Position);
            foreach (var neurolink in neurolinks)
            {
                bool isClosest = neurolink == closerNeurolink;
                bool stayAtClosest = assignment != PartyRolesConfig.Assignment.MT;
                if (isClosest == stayAtClosest)
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(neurolink.Position, T05Twintania.NeurolinkRadius), _activation);
            }

            // let MT taunt boss if needed
            var boss = hints.PotentialTargets.Find(e => e.Actor == module.PrimaryActor);
            if (boss != null)
                boss.PreferProvoking = true;

            // mitigate heavy raidwide
            hints.PredictedDamage.Add((module.Raid.WithSlot().Mask(), _activation));
            if (actor.Role == Role.Ranged)
                hints.PlannedActions.Add((ActionID.MakeSpell(BLM.AID.Addle), module.PrimaryActor, (float)(_activation - module.WorldState.CurrentTime).TotalSeconds, false));
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var neurolink in module.Enemies(OID.Neurolink))
                arena.AddCircle(neurolink.Position, T05Twintania.NeurolinkRadius, ArenaColor.Safe);
        }
    }
}
