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

    class P1WickedWheel : Components.SelfTargetedAOEs
    {
        public P1WickedWheel() : base(ActionID.MakeSpell(AID.WickedWheel), new AOEShapeCircle(8.7f)) { }
    }

    class P1Cyclone : Components.CastCounter
    {
        public P1Cyclone() : base(ActionID.MakeSpell(AID.Cyclone)) { }
    }

    class P1Gigastorm : Components.SelfTargetedAOEs
    {
        public P1Gigastorm() : base(ActionID.MakeSpell(AID.Gigastorm), new AOEShapeCircle(6.5f)) { }
    }


    [ModuleInfo(PrimaryActorOID = (uint)OID.Garuda)]
    public class UWU : BossModule
    {
        private List<Actor> _ifrit;
        public Actor? Garuda() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
        public Actor? Ifrit() => _ifrit.FirstOrDefault();

        public UWU(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20))
        {
            _ifrit = Enemies(OID.Ifrit);
        }
    }
}
