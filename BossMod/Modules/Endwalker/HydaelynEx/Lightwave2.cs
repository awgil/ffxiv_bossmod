using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.HydaelynEx
{
    using static BossModule;

    // component for second lightwave (3 waves, 5 crystals) + hero's glory mechanics
    class Lightwave2 : LightwaveCommon
    {
        private WPos _safeCrystal;
        private Vector4? _safeCrystalOrigin;

        private static WPos _crystalCenter = new(100, 101);
        private static WPos _crystalTL = new( 90,  92);
        private static WPos _crystalTR = new(110,  92);
        private static WPos _crystalBL = new( 90, 110);
        private static WPos _crystalBR = new(110, 110);
        private static AOEShapeCone _gloryAOE = new(40, 90.Degrees());

        public override void Update(BossModule module)
        {
            if (NumCasts == 4 && (module.PrimaryActor.CastInfo?.IsSpell(AID.HerosGlory) ?? false) && module.PrimaryActor.PosRot != _safeCrystalOrigin)
            {
                _safeCrystalOrigin = module.PrimaryActor.PosRot;
                _safeCrystal = new[] { _crystalTL, _crystalTR, _crystalBL, _crystalBR }.FirstOrDefault(c => !_gloryAOE.Check(c, module.PrimaryActor));
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if ((module.PrimaryActor.CastInfo?.IsSpell(AID.HerosGlory) ?? false) && _gloryAOE.Check(actor.Position, module.PrimaryActor))
                hints.Add("GTFO from glory aoe!");

            (bool inWave, bool inSafeCone) = NumCasts < 4
                ? (WaveAOE.Check(actor.Position, Wave1Pos(), new()) || WaveAOE.Check(actor.Position, Wave2Pos(), new()), InSafeCone(NextSideCrystal(), _crystalCenter, actor.Position))
                : (WaveAOE.Check(actor.Position, Wave3Pos(), new()), _safeCrystal != new WPos() ? InSafeCone(_crystalCenter, _safeCrystal, actor.Position) : true);

            if (inWave)
                hints.Add("GTFO from wave!");
            if (!inSafeCone)
                hints.Add("Hide behind crystal!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (module.PrimaryActor.CastInfo?.IsSpell(AID.HerosGlory) ?? false)
                _gloryAOE.Draw(arena, module.PrimaryActor);

            if (NumCasts < 4)
            {
                WaveAOE.Draw(arena, Wave1Pos(), new());
                WaveAOE.Draw(arena, Wave2Pos(), new());
                DrawSafeCone(arena, NextSideCrystal(), _crystalCenter);
            }
            else
            {
                WaveAOE.Draw(arena, Wave3Pos(), new());
                if (_safeCrystal != new WPos())
                {
                    DrawSafeCone(arena, _crystalCenter, _safeCrystal);
                }
            }
        }

        private WPos Wave1Pos() => Waves.Count > 0 ? Waves[0].Position : new(86, 70);
        private WPos Wave2Pos() => Waves.Count switch
        {
            0 => new(114, 70),
            1 => new(Waves[0].Position.X < 100 ? 114 : 86, 70),
            _ => Waves[1].Position
        };
        private WPos Wave3Pos() => Waves.Count > 2 ? Waves[2].Position : new(100, 70);

        private WPos NextSideCrystal()
        {
            bool w1Next = (NumCasts & 1) == 0;
            bool w1Left = Wave1Pos().X < 100;
            float nextX = w1Next == w1Left ? _crystalTL.X : _crystalBR.X;
            float nextZ = (NumCasts & 2) == 0 ? _crystalTL.Z : _crystalBR.Z;
            return new(nextX, nextZ);
        }
    }
}
