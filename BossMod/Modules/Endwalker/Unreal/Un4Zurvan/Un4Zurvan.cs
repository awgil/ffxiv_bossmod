using System.Linq;

namespace BossMod.Endwalker.Unreal.Un4Zurvan
{
    class P1MetalCutter : Components.Cleave
    {
        public P1MetalCutter() : base(ActionID.MakeSpell(AID.MetalCutterP1), new AOEShapeCone(37.44f, 45.Degrees()), (uint)OID.BossP1) { }
    }

    class P1FlareStar : Components.LocationTargetedAOEs
    {
        public P1FlareStar() : base(ActionID.MakeSpell(AID.FlareStarAOE), 6) { }
    }

    class P1Purge : Components.CastCounter
    {
        public P1Purge() : base(ActionID.MakeSpell(AID.Purge)) { }
    }

    class P2MetalCutter : Components.Cleave
    {
        public P2MetalCutter() : base(ActionID.MakeSpell(AID.MetalCutterP2), new AOEShapeCone(37.44f, 45.Degrees()), (uint)OID.BossP2) { }
    }

    class P2IcyVoidzone : Components.PersistentVoidzone
    {
        public P2IcyVoidzone() : base(5, m => m.Enemies(OID.IcyVoidzone).Where(z => z.EventState != 7)) { }
    }

    class P2BitingHalberd : Components.SelfTargetedAOEs
    {
        public P2BitingHalberd() : base(ActionID.MakeSpell(AID.BitingHalberd), new AOEShapeCone(55.27f, 135.Degrees())) { }
    }

    class P2TailEnd : Components.SelfTargetedAOEs
    {
        public P2TailEnd() : base(ActionID.MakeSpell(AID.TailEnd), new AOEShapeCircle(15)) { }
    }

    class P2Ciclicle : Components.SelfTargetedAOEs
    {
        public P2Ciclicle() : base(ActionID.MakeSpell(AID.Ciclicle), new AOEShapeDonut(10, 20)) { } // TODO: verify inner radius
    }

    class P2SouthernCross : Components.LocationTargetedAOEs
    {
        public P2SouthernCross() : base(ActionID.MakeSpell(AID.SouthernCrossAOE), 6) { }
    }

    class P2SouthernCrossVoidzone : Components.PersistentVoidzone
    {
        public P2SouthernCrossVoidzone() : base(6, m => m.Enemies(OID.SouthernCrossVoidzone).Where(z => z.EventState != 7)) { }
    }

    class P2WaveCannon : Components.BaitAwayCast
    {
        public P2WaveCannon() : base(ActionID.MakeSpell(AID.WaveCannonSolo), new AOEShapeRect(55.27f, 5)) { }
    }

    class P2TyrfingFire : Components.Cleave
    {
        public P2TyrfingFire() : base(ActionID.MakeSpell(AID.TyrfingFire), new AOEShapeCircle(5), (uint)OID.BossP2, originAtTarget: true) { }
    }

    [ConfigDisplay(Order = 0x340, Parent = typeof(EndwalkerConfig))]
    public class Un4ZurvanConfig : CooldownPlanningConfigNode
    {
        public Un4ZurvanConfig() : base(90) { }
    }

    [ModuleInfo(PrimaryActorOID = (uint)OID.BossP1, CFCID = 951, NameID = 5567)]
    public class Un4Zurvan : BossModule
    {
        private Actor? _bossP2;

        public Actor? BossP1() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
        public Actor? BossP2() => _bossP2;

        public Un4Zurvan(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 0), 20)) { }

        protected override void UpdateModule()
        {
            // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
            // the problem is that on wipe, any actor can be deleted and recreated in the same frame
            _bossP2 ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.BossP2).FirstOrDefault() : null;
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            Arena.Actor(_bossP2, ArenaColor.Enemy);
        }
    }
}
