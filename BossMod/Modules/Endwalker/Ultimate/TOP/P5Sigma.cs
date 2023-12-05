using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.TOP
{
    // note: this is all very tied to LPDU strat
    class P5Sigma : BossComponent
    {
        public enum Glitch { Unknown, Mid, Remote }

        public struct PlayerState
        {
            public int Order;
            public int PartnerSlot;
            public bool WaveCannonTarget;
            public Angle SpreadAngle;
        }

        public Glitch ActiveGlitch;
        public PlayerState[] Players = Utils.MakeArray(PartyState.MaxPartySize, new PlayerState() { PartnerSlot = -1 });
        private WDir _waveCannonNorthDir;
        private int _numWaveCannonTargets;
        private bool _waveCannonsDone;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var ps = Players[slot];
            if (ps.Order > 0)
                hints.Add($"Order: {ps.Order}", false);
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (ActiveGlitch != Glitch.Unknown)
                hints.Add($"Glitch: {ActiveGlitch}");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var partner = module.Raid[Players[pcSlot].PartnerSlot];
            if (partner != null)
            {
                var distSq = (partner.Position - pc.Position).LengthSq();
                var range = DistanceRange;
                arena.AddLine(pc.Position, partner.Position, distSq < range.min * range.min || distSq > range.max * range.max ? ArenaColor.Danger : ArenaColor.Safe);
            }

            foreach (var safeSpot in SafeSpotOffsets(module, pcSlot))
                arena.AddCircle(module.Bounds.Center + safeSpot, 1, ArenaColor.Safe);
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.PartySynergy)
            {
                var s1 = module.Raid.FindSlot(source.InstanceID);
                var s2 = module.Raid.FindSlot(tether.Target);
                if (s1 >= 0 && s2 >= 0)
                {
                    Players[s1].PartnerSlot = s2;
                    Players[s2].PartnerSlot = s1;
                }
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.MidGlitch:
                    ActiveGlitch = Glitch.Mid;
                    break;
                case SID.RemoteGlitch:
                    ActiveGlitch = Glitch.Remote;
                    break;
                //case SID.HelloNearWorld:
                //    _nearWorld = actor;
                //    _firstActivation = status.ExpireAt;
                //    break;
                //case SID.HelloDistantWorld:
                //    _distantWorld = actor;
                //    _firstActivation = status.ExpireAt;
                //    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.SigmaWaveCannonAOE)
                _waveCannonsDone = true;
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            var slot = module.Raid.FindSlot(actor.InstanceID);
            if (slot < 0)
                return;

            // assuming standard 'blue-purple-orange-green' order
            var order = (IconID)iconID switch
            {
                IconID.PartySynergyCross => 1,
                IconID.PartySynergySquare => 2,
                IconID.PartySynergyCircle => 3,
                IconID.PartySynergyTriangle => 4,
                _ => 0
            };
            if (order > 0)
            {
                Players[slot].Order = order;
            }
            else if (iconID == (uint)IconID.SigmaWaveCannon)
            {
                Players[slot].WaveCannonTarget = true;
                if (++_numWaveCannonTargets == 6)
                    InitSpreadPositions(module);
            }
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            switch ((OID)actor.OID)
            {
                case OID.RightArmUnit: // TODO: can it be left unit instead?..
                    if (id == 0x1E43)
                        _waveCannonNorthDir -= actor.Position - module.Bounds.Center;
                    break;
                case OID.OmegaMP5:
                    if (id == 0x1E43)
                        _waveCannonNorthDir = actor.Position - module.Bounds.Center; // just in case...
                    break;
            }
        }

        private (float min, float max) DistanceRange => ActiveGlitch switch
        {
            Glitch.Mid => (20, 26),
            Glitch.Remote => (34, 50),
            _ => (0, 50)
        };

        private void InitSpreadPositions(BossModule module)
        {
            var northAngle = Angle.FromDirection(_waveCannonNorthDir);
            var waveCannonsPerPair = new BitMask[4];
            for (int i = 0; i < Players.Length; ++i)
            {
                var ps = Players[i];
                if (ps.WaveCannonTarget && ps.Order > 0)
                    waveCannonsPerPair[ps.Order - 1].Set(i);
            }
            int nextSingle = 0;
            int nextDouble = 0;
            foreach (var mask in waveCannonsPerPair)
            {
                if (mask.NumSetBits() == 2)
                {
                    var s1 = mask.LowestSetBit();
                    var s2 = mask.HighestSetBit();
                    var dir = (module.Raid[s2]?.Position ?? default) - (module.Raid[s1]?.Position ?? default); // s1 to s2
                    if (_waveCannonNorthDir.OrthoL().Dot(dir) > 0)
                        Utils.Swap(ref s1, ref s2); // s1 is now N/W, s2 is S/E
                    if (nextSingle == 0)
                    {
                        Players[s1].SpreadAngle = northAngle;
                        Players[s2].SpreadAngle = northAngle + 180.Degrees();
                    }
                    else
                    {
                        Players[s1].SpreadAngle = northAngle + 90.Degrees();
                        Players[s2].SpreadAngle = northAngle - 90.Degrees();
                    }
                    ++nextSingle;
                }
                else
                {
                    var s1 = mask.LowestSetBit();
                    var s2 = Players[s1].PartnerSlot;
                    if (nextDouble == 0)
                    {
                        Players[s1].SpreadAngle = northAngle - 135.Degrees();
                        Players[s2].SpreadAngle = northAngle + 45.Degrees();
                    }
                    else
                    {
                        Players[s1].SpreadAngle = northAngle + 135.Degrees();
                        Players[s2].SpreadAngle = northAngle - 45.Degrees();
                    }
                    ++nextDouble;
                }
            }

            foreach (ref var p in Players.AsSpan())
                p.SpreadAngle = p.SpreadAngle.Normalized();
        }

        private IEnumerable<WDir> SafeSpotOffsets(BossModule module, int slot)
        {
            var p = Players[slot];
            if (_waveCannonNorthDir == default)
                yield break; // no safe spots yet

            if (_numWaveCannonTargets < 6)
            {
                var dir = _waveCannonNorthDir.Normalized();
                yield return (20 - 2 * p.Order) * dir + 2 * dir.OrthoL();
                yield return (20 - 2 * p.Order) * dir + 2 * dir.OrthoR();
                yield break;
            }

            if (!_waveCannonsDone)
            {
                yield return (ActiveGlitch == Glitch.Mid ? 11 : 19) * p.SpreadAngle.ToDirection();
                yield break;
            }
        }
    }

    class P5SigmaHyperPulse : Components.BaitAwayTethers
    {
        public P5SigmaHyperPulse() : base(new AOEShapeRect(100, 3), (uint)TetherID.SigmaHyperPulse, ActionID.MakeSpell(AID.SigmaHyperPulse)) { }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var b in CurrentBaits)
                arena.Actor(b.Source, ArenaColor.Object, true);
            base.DrawArenaForeground(module, pcSlot, pc, arena);
        }
    }

    class P5SigmaWaveCannon : Components.GenericBaitAway
    {
        private BitMask _waveCannonTargets;

        private static AOEShapeCone _shapeWaveCannon = new(100, 22.5f.Degrees()); // TODO: verify angle

        public P5SigmaWaveCannon() : base(ActionID.MakeSpell(AID.SigmaWaveCannonAOE)) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.SigmaWaveCannon)
                foreach (var p in module.Raid.WithSlot(true).IncludedInMask(_waveCannonTargets).Actors())
                    CurrentBaits.Add(new(caster, p, _shapeWaveCannon));
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.SigmaWaveCannon)
                _waveCannonTargets.Set(module.Raid.FindSlot(actor.InstanceID));
        }
    }

    class P5SigmaTowers : Components.GenericTowers
    {
        private int _soakerSum;

        public override void OnActorCreated(BossModule module, Actor actor)
        {
            var numSoakers = (OID)actor.OID switch
            {
                OID.Tower2 => 1,
                OID.Tower3 => 2,
                _ => 0,
            };
            if (numSoakers == 0)
                return;

            Towers.Add(new(actor.Position, 3, numSoakers, numSoakers));
            _soakerSum += numSoakers;
            if (_soakerSum == PartyState.MaxPartySize)
                InitAssignments(module);
        }

        private void InitAssignments(BossModule module)
        {
            var sigma = module.FindComponent<P5Sigma>();
            if (sigma == null)
                return;

            WDir relNorth = default;
            foreach (var t in Towers)
                relNorth -= t.Position - module.Bounds.Center;

            foreach (ref var tower in Towers.AsSpan())
            {
                var offset = tower.Position - module.Bounds.Center;
                var left = relNorth.OrthoL().Dot(offset) > 0;
                if (Towers.Count == 5)
                {
                    // 1's are rel W (A) and E (D), 2's are N (12), SW (B3), SE (C4)
                    if (tower.MinSoakers == 1)
                        AssignPlayers(sigma, ref tower, (left ? 180 : -90).Degrees()); // A/D
                    else if (relNorth.Dot(offset) > 0)
                        AssignPlayers(sigma, ref tower, 135.Degrees(), 45.Degrees()); // 1/2
                    else if (left)
                        AssignPlayers(sigma, ref tower, 90.Degrees(), -45.Degrees()); // B/3
                    else
                        AssignPlayers(sigma, ref tower, 0.Degrees(), -135.Degrees()); // C/4
                }
                else
                {
                    // 1's are NW (1), NE (2), SW (B), SE (C), 2's are W (A3) and E (D4)
                    if (tower.MinSoakers == 2)
                        AssignPlayers(sigma, ref tower, (left ? 180 : -90).Degrees(), (left ? -45 : -135).Degrees()); // A3/D4
                    else if (relNorth.Dot(offset) > 0)
                        AssignPlayers(sigma, ref tower, (left ? 135 : 45).Degrees()); // 1/2
                    else
                        AssignPlayers(sigma, ref tower, (left ? 90 : 0).Degrees()); // B/C
                }
            }
        }

        private void AssignPlayers(P5Sigma sigma, ref Tower tower, params Angle[] angles)
        {
            for (int i = 0; i < sigma.Players.Length; ++i)
                if (!angles.Any(a => a.AlmostEqual(sigma.Players[i].SpreadAngle, 0.1f)))
                    tower.ForbiddenSoakers.Set(i);
        }
    }
}
