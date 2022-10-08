using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.RealmReborn.Raid.T01Caduceus
{
    public enum OID : uint
    {
        Boss = 0x7D7, // x1, and more spawn during fight
        Helper = 0x1B2, // x1
        DarkMatterSlime = 0x7D8, // spawn during fight
        Platform = 0x1E8729, // x13
        Regorge = 0x1E8B20, // EventObj type, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttackBoss = 1207, // Boss->self, no cast, range 6+R ?-degree cone
        HoodSwing = 1208, // Boss->self, no cast, range 8+R ?-degree cone cleave
        WhipBack = 1209, // Boss->self, 2.0s cast, range 6+R 120-degree cone (baited backward cleave)
        Regorge = 1210, // Boss->location, no cast, range 4 circle aoe that leaves voidzone
        SteelScales = 1211, // Boss->self, no cast, single-target, damage up buff stack

        PlatformExplosion = 674, // Helper->self, no cast, hits players on glowing platforms and spawns dark matter slime on them
        AutoAttackSlime = 872, // DarkMatterSlime->player, no cast, single-target
        Rupture = 1213, // DarkMatterSlime->self, no cast, range 16+R circle aoe suicide (damage depends on cur hp?)
        Devour = 1454, // Boss->DarkMatterSlime, no cast, single-target visual
    };

    public enum SID : uint
    {
        SteelScales = 349, // Boss->Boss, extra=1-8 (num stacks)
    };

    class HoodSwing : Components.Cleave
    {
        public HoodSwing() : base(ActionID.MakeSpell(AID.HoodSwing), new AOEShapeCone(11, 60.Degrees()), (uint)OID.Boss) { } // TODO: verify angle
    }

    class WhipBack : Components.SelfTargetedAOEs
    {
        public WhipBack() : base(ActionID.MakeSpell(AID.WhipBack), new AOEShapeCone(9, 60.Degrees())) { }
    }

    class Regorge : Components.GenericAOEs
    {
        private List<(WPos pos, DateTime time)> _predictedSpawns = new();
        private List<Actor> _voidzones = new();

        private static AOEShapeCircle _shape = new(4);

        public Regorge() : base(ActionID.MakeSpell(AID.Regorge), "GTFO from voidzone!") { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var p in _predictedSpawns)
                yield return (_shape, p.pos, new(), p.time);
            foreach (var z in _voidzones.Where(z => z.EventState != 7))
                yield return (_shape, z.Position, new(), new());
        }

        public override void Init(BossModule module)
        {
            _voidzones = module.Enemies(OID.Regorge);
        }

        public override void Update(BossModule module)
        {
            _predictedSpawns.RemoveAll(p => _voidzones.InRadius(p.pos, 2).Any());
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if (spell.Action == WatchedAction)
                _predictedSpawns.Add((spell.TargetXZ, module.WorldState.CurrentTime.AddSeconds(2.1f)));
        }
    }

    // TODO: merge: happens if bosses are 'close enough' and more than 20s passed since split

    class T01CaduceusStates : StateMachineBuilder
    {
        public T01CaduceusStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<HoodSwing>()
                .ActivateOnEnter<WhipBack>()
                .ActivateOnEnter<Regorge>();
        }
    }

    public class T01Caduceus : BossModule
    {
        public Actor? Clone { get; private set; }
        public List<Actor> Slimes { get; private set; }

        public T01Caduceus(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(-26, -407), 35, 43))
        {
            Slimes = Enemies(OID.DarkMatterSlime);
            ActivateComponent<Platforms>();
        }

        public override void CalculateAIHints(int slot, Actor actor, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, hints);
            // attack priorities:
            // - just after split (hp > 30%) - MT/H1/M1/R1 on boss, rest on clone
            // - after that - equal priorities unless hp diff is larger than 5%
            var hpDiff = Clone != null ? (float)(Clone.HP.Cur - PrimaryActor.HP.Cur) / PrimaryActor.HP.Max : 0;
            var assignment = Service.Config.Get<PartyRolesConfig>()[WorldState.Party.ContentIDs[slot]];
            hints.UpdatePotentialTargets(e =>
            {
                if (e.Actor == PrimaryActor)
                {
                    e.Priority = hpDiff < -0.05f || e.Actor.HP.Cur > 0.3f * e.Actor.HP.Max && assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.R1 ? 1 : 0;
                    if (Clone == null && e.Actor.HP.Cur < 0.7f * e.Actor.HP.Max && e.Actor.FindStatus(SID.SteelScales) != null)
                        e.Priority = -1; // stop dps until stack can be dropped
                    e.AttackStrength = Clone == null || Clone.IsDead ? 0.3f : 0.15f;
                }
                else if (e.Actor == Clone)
                {
                    e.Priority = hpDiff > 0.05f || e.Actor.HP.Cur > 0.3f * e.Actor.HP.Max && assignment is PartyRolesConfig.Assignment.OT or PartyRolesConfig.Assignment.H2 or PartyRolesConfig.Assignment.M2 or PartyRolesConfig.Assignment.R2 ? 1 : 0;
                    e.TankAffinity = AIHints.TankAffinity.OT;
                    if ((e.Actor.Position - PrimaryActor.Position).LengthSq() < 100)
                        e.DesiredPosition = Platforms.HexaPlatformCenters[6];
                    e.DesiredRotation = -90.Degrees();
                    e.AttackStrength = PrimaryActor.IsDead ? 0.3f : 0.15f;
                }
                else if ((OID)e.Actor.OID == OID.DarkMatterSlime)
                {
                    // for now, let kiter damage it until 20%
                    e.Priority = e.Actor.HP.Cur < 0.2f * e.Actor.HP.Max ? -1 : e.Actor.TargetID == actor.InstanceID ? 2 : 0;
                    e.TankAffinity = AIHints.TankAffinity.None;
                }
            });
        }

        public override bool NeedToJump(WPos from, WDir dir) => Platforms.IntersectJumpEdge(from, dir, 2);

        // don't activate module created for clone (this is a hack...)
        protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat && PrimaryActor.HP.Cur > PrimaryActor.HP.Max / 2; }

        protected override void UpdateModule()
        {
            Clone ??= Enemies(OID.Boss).FirstOrDefault(a => a != PrimaryActor);
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            Arena.Actor(Clone, ArenaColor.Enemy);
        }
    }
}
