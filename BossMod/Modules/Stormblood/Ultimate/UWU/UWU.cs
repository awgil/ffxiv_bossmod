using System.Collections.Generic;
using System.Linq;

namespace BossMod.Stormblood.Ultimate.UWU
{
    class P1Slipstream : Components.SelfTargetedAOEs
    {
        public P1Slipstream() : base(ActionID.MakeSpell(AID.Slipstream), new AOEShapeCone(11.7f, 45.Degrees())) { }
    }

    class P1Downburst : Components.Cleave
    {
        public P1Downburst() : base(ActionID.MakeSpell(AID.Downburst), new AOEShapeCone(11.7f, 45.Degrees())) { }
    }

    // TODO: hints... (use wild charge component? need 'avoid or share-non-first' role?)
    class P1MistralSongBoss : Components.CastCounter
    {
        public P1MistralSongBoss() : base(ActionID.MakeSpell(AID.MistralSongBoss)) { }
    }

    // TODO: same as boss variant + multi targets
    class P1MistralSongAdds : Components.CastCounter
    {
        public P1MistralSongAdds() : base(ActionID.MakeSpell(AID.MistralSongAdds)) { }
    }

    class P1GreatWhirlwind : Components.LocationTargetedAOEs
    {
        public P1GreatWhirlwind() : base(ActionID.MakeSpell(AID.GreatWhirlwind), 8) { }
    }

    class P1EyeOfTheStorm : Components.SelfTargetedAOEs
    {
        public P1EyeOfTheStorm() : base(ActionID.MakeSpell(AID.EyeOfTheStorm), new AOEShapeDonut(12, 25)) { }
    }

    class P1Gigastorm : Components.SelfTargetedAOEs
    {
        public P1Gigastorm() : base(ActionID.MakeSpell(AID.Gigastorm), new AOEShapeCircle(6.5f)) { }
    }

    class P2RadiantPlume : Components.LocationTargetedAOEs
    {
        public P2RadiantPlume() : base(ActionID.MakeSpell(AID.RadiantPlumeAOE), 8) { }
    }

    class P2Incinerate : Components.Cleave
    {
        public P2Incinerate() : base(ActionID.MakeSpell(AID.Incinerate), new AOEShapeCone(15, 60.Degrees()), (uint)OID.Ifrit) { }
    }

    class P3RockBuster : Components.Cleave
    {
        public P3RockBuster() : base(ActionID.MakeSpell(AID.RockBuster), new AOEShapeCone(10.55f, 60.Degrees()), (uint)OID.Titan) { } // TODO: verify angle
    }

    class P3MountainBuster : Components.Cleave
    {
        public P3MountainBuster() : base(ActionID.MakeSpell(AID.MountainBuster), new AOEShapeCone(15.55f, 45.Degrees()), (uint)OID.Titan) { } // TODO: verify angle
    }

    class P3WeightOfTheLand : Components.LocationTargetedAOEs
    {
        public P3WeightOfTheLand() : base(ActionID.MakeSpell(AID.WeightOfTheLandAOE), 6) { }
    }

    class P3Upheaval : Components.KnockbackFromCastTarget
    {
        public P3Upheaval() : base(ActionID.MakeSpell(AID.Upheaval), 24, true) { }
    }

    class P3Tumult : Components.CastCounter
    {
        public P3Tumult() : base(ActionID.MakeSpell(AID.Tumult)) { }
    }

    class P4Blight : Components.CastCounter
    {
        public P4Blight() : base(ActionID.MakeSpell(AID.Blight)) { }
    }

    class P4HomingLasers : Components.SpreadFromCastTargets
    {
        public P4HomingLasers() : base(ActionID.MakeSpell(AID.HomingLasers), 4) { }
    }

    class P4DiffractiveLaser : Components.Cleave
    {
        public P4DiffractiveLaser() : base(ActionID.MakeSpell(AID.DiffractiveLaser), new AOEShapeCone(18, 45.Degrees()), (uint)OID.UltimaWeapon) { } // TODO: verify angle
    }

    class P5MistralSongCone : Components.SelfTargetedAOEs
    {
        public P5MistralSongCone() : base(ActionID.MakeSpell(AID.MistralSongCone), new AOEShapeCone(21.7f, 45.Degrees())) { } // TODO: verify angle
    }

    class P5AetherochemicalLaserCenter : Components.SelfTargetedAOEs
    {
        public P5AetherochemicalLaserCenter() : base(ActionID.MakeSpell(AID.AetherochemicalLaserCenter), new AOEShapeRect(46, 4, 6)) { }
    }

    class P5AetherochemicalLaserRight : Components.SelfTargetedAOEs
    {
        public P5AetherochemicalLaserRight() : base(ActionID.MakeSpell(AID.AetherochemicalLaserRight), new AOEShapeRect(46, 4, 6)) { }
    }

    class P5AetherochemicalLaserLeft : Components.SelfTargetedAOEs
    {
        public P5AetherochemicalLaserLeft() : base(ActionID.MakeSpell(AID.AetherochemicalLaserLeft), new AOEShapeRect(46, 4, 6)) { }
    }

    // TODO: consider showing circle around baiter
    class P5LightPillar : Components.LocationTargetedAOEs
    {
        public P5LightPillar() : base(ActionID.MakeSpell(AID.LightPillarAOE), 3) { }
    }

    class P5AethericBoom : Components.KnockbackFromCastTarget
    {
        public P5AethericBoom() : base(ActionID.MakeSpell(AID.AethericBoom), 10) { }
    }


    [ModuleInfo(PrimaryActorOID = (uint)OID.Garuda)]
    public class UWU : BossModule
    {
        private List<Actor> _ifrits;
        private List<Actor> _titan;
        private List<Actor> _lahabrea;
        private List<Actor> _ultima;
        private Actor? _mainIfrit;

        public IReadOnlyList<Actor> Ifrits => _ifrits;

        public Actor? Garuda() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
        public Actor? Ifrit() => _mainIfrit;
        public Actor? Titan() => _titan.FirstOrDefault();
        public Actor? Lahabrea() => _lahabrea.FirstOrDefault();
        public Actor? Ultima() => _ultima.FirstOrDefault();

        public UWU(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20))
        {
            _ifrits = Enemies(OID.Ifrit);
            _titan = Enemies(OID.Titan);
            _lahabrea = Enemies(OID.Lahabrea);
            _ultima = Enemies(OID.UltimaWeapon);
        }

        protected override void UpdateModule()
        {
            if (StateMachine.ActivePhaseIndex == 1)
                _mainIfrit ??= _ifrits.FirstOrDefault(a => a.IsTargetable);
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(Garuda(), ArenaColor.Enemy);
            Arena.Actor(Ifrit(), ArenaColor.Enemy);
            Arena.Actor(Titan(), ArenaColor.Enemy);
            Arena.Actor(Lahabrea(), ArenaColor.Enemy);
            Arena.Actor(Ultima(), ArenaColor.Enemy);
        }
    }
}
