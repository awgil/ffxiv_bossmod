using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS3QueensGuard
{
    class OptimalPlaySword : Components.SelfTargetedAOEs
    {
        public OptimalPlaySword() : base(ActionID.MakeSpell(AID.OptimalPlaySword), new AOEShapeCircle(10)) { }
    }

    class OptimalPlayShield : Components.SelfTargetedAOEs
    {
        public OptimalPlayShield() : base(ActionID.MakeSpell(AID.OptimalPlayShield), new AOEShapeDonut(5, 60)) { }
    }

    class OptimalPlayCone : Components.SelfTargetedAOEs
    {
        public OptimalPlayCone() : base(ActionID.MakeSpell(AID.OptimalPlayCone), new AOEShapeCone(60, 135.Degrees())) { }
    }

    // note: apparently there is no 'front unseen' status
    class QueensShotUnseen : Components.CastWeakpoint
    {
        public QueensShotUnseen() : base(ActionID.MakeSpell(AID.QueensShotUnseen), new AOEShapeCircle(60), 0, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen) { }
    }

    class TurretsTourUnseen : Components.CastWeakpoint
    {
        public TurretsTourUnseen() : base(ActionID.MakeSpell(AID.TurretsTourUnseen), new AOEShapeRect(50, 2.5f), 0, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen) { }
    }

    class FieryPortent : Components.CastHint
    {
        public FieryPortent() : base(ActionID.MakeSpell(AID.FieryPortent), "Stand still!") { }
    }

    class IcyPortent : Components.CastHint
    {
        public IcyPortent() : base(ActionID.MakeSpell(AID.IcyPortent), "Move!") { }
    }

    // TODO: consider showing reflect hints
    class Fracture : Components.CastCounter
    {
        public Fracture() : base(ActionID.MakeSpell(AID.Fracture)) { }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.Knight)]
    public class DRS3 : BossModule
    {
        private List<Actor> _warrior;
        private List<Actor> _soldier;
        private List<Actor> _gunner;

        public Actor? Knight() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
        public Actor? Warrior() => _warrior.FirstOrDefault();
        public Actor? Soldier() => _soldier.FirstOrDefault();
        public Actor? Gunner() => _gunner.FirstOrDefault();
        public List<Actor> GunTurrets;
        public List<Actor> AuraSpheres;
        public List<Actor> SpiritualSpheres;

        public DRS3(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(244, -162), 25))
        {
            _warrior = Enemies(OID.Warrior);
            _soldier = Enemies(OID.Soldier);
            _gunner = Enemies(OID.Gunner);
            GunTurrets = Enemies(OID.GunTurret);
            AuraSpheres = Enemies(OID.AuraSphere);
            SpiritualSpheres = Enemies(OID.SpiritualSphere);
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(Knight(), ArenaColor.Enemy);
            Arena.Actor(Warrior(), ArenaColor.Enemy);
            Arena.Actor(Soldier(), ArenaColor.Enemy);
            Arena.Actor(Gunner(), ArenaColor.Enemy);
            Arena.Actors(GunTurrets, ArenaColor.Enemy);
            Arena.Actors(AuraSpheres, ArenaColor.Enemy);
            Arena.Actors(SpiritualSpheres, ArenaColor.Object);
        }
    }
}
