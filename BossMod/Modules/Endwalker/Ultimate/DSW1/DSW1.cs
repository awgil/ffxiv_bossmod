using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW1
{
    class EmptyDimension : Components.SelfTargetedAOEs
    {
        public EmptyDimension() : base(ActionID.MakeSpell(AID.EmptyDimension), new AOEShapeDonut(6, 70)) {}
    }

    class FullDimension : Components.SelfTargetedAOEs
    {
        public FullDimension() : base(ActionID.MakeSpell(AID.FullDimension), new AOEShapeCircle(6)) { }
    }

    class HoliestHallowing : Components.CastHint
    {
        public HoliestHallowing() : base(ActionID.MakeSpell(AID.HoliestHallowing), "Interrupt!") { }
    }

    [ConfigDisplay(Order = 0x200, Parent = typeof(EndwalkerConfig))]
    public class DSW1Config : CooldownPlanningConfigNode { }

    [ModuleInfo(PrimaryActorOID = (uint)OID.SerAdelphel)]
    public class DSW1 : BossModule
    {
        private Actor? _grinnaux;
        private Actor? _charibert;
        public Actor? SerAdelphel() => PrimaryActor;
        public Actor? SerGrinnaux() => _grinnaux;
        public Actor? SerCharibert() => _charibert;

        public DSW1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 22)) { }

        protected override void UpdateModule()
        {
            // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
            // the problem is that on wipe, any actor can be deleted and recreated in the same frame
            _grinnaux ??= Enemies(OID.SerGrinnaux).FirstOrDefault();
            _charibert ??= Enemies(OID.SerCharibert).FirstOrDefault();
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(SerAdelphel(), ArenaColor.Enemy);
            Arena.Actor(SerGrinnaux(), ArenaColor.Enemy);
            Arena.Actor(SerCharibert(), ArenaColor.Enemy);
        }
    }
}
