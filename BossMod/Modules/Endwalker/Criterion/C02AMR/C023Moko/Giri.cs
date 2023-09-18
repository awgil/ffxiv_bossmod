using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Criterion.C02AMR.C023Moko
{
    // the main complexity is that first status and cast-start happen at the same time, so we can receive them in arbitrary order
    // we need cast to know proper rotation (we can't use actor's rotation, since it's interpolated)
    class TripleKasumiGiri : Components.GenericAOEs
    {
        private List<string> _hints = new();
        private List<Angle> _directionOffsets = new();
        private BitMask _ins; // [i] == true if i'th aoe is in
        private List<AOEInstance> _aoes = new();

        private static AOEShapeCone _shapeCone = new(60, 135.Degrees());
        private static AOEShapeCircle _shapeOut = new(6);
        private static AOEShapeDonut _shapeIn = new(6, 40);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(2);

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (_hints.Count > 0)
                hints.Add($"Safespots: {string.Join(" > ", _hints)}");
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Giri)
            {
                var (dir, donut, hint) = status.Extra switch
                {
                    0x24C => (0.Degrees(), false, "out/back"),
                    0x24D => (-90.Degrees(), false, "out/left"),
                    0x24E => (180.Degrees(), false, "out/front"),
                    0x24F => (90.Degrees(), false, "out/right"),
                    0x250 => (0.Degrees(), true, "in/back"),
                    0x251 => (-90.Degrees(), true, "in/left"),
                    0x252 => (180.Degrees(), true, "in/front"),
                    0x253 => (90.Degrees(), true, "in/right"),
                    _ => (0.Degrees(), false, "")
                };
                if (hint.Length == 0)
                {
                    module.ReportError(this, $"Unexpected extra {status.Extra:X}");
                    return;
                }

                _ins[_directionOffsets.Count] = donut;
                _directionOffsets.Add(dir);
                _hints.Add(hint);

                //var activation = _aoes.Count > 0 ? _aoes.Last().Activation.AddSeconds(3.1f) : actor.CastInfo?.FinishAt ?? module.WorldState.CurrentTime.AddSeconds(12);
                //var rotation = (_aoes.Count > 0 ? _aoes.Last().Rotation : actor.Rotation) + dir;
                //_aoes.Add(new(donut ? _shapeIn : _shapeOut, actor.Position, rotation, activation));
                //_aoes.Add(new(_shapeCone, actor.Position, rotation, activation));
                //_hints.Add(hint);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var (dir, donut, order) = ClassifyAction(spell.Action);
            if (order < 0)
                return; // irrelevant spell

            if (order == 0)
            {
                // first cast: create first aoe pair, first status may or may not be known yet, so verify consistency on cast-finish
                if (_aoes.Count > 0 || NumCasts > 0)
                    module.ReportError(this, "Unexpected state on first cast start");
            }
            else if (order > 0)
            {
                // subsequent casts: ensure predicted state is correct, then update aoes in case it was not
                if (NumCasts == 0 || NumCasts >= 3 || _aoes.Count != 2)
                    module.ReportError(this, "Unexpected state on subsequent cast");
                var mismatch = _aoes.FindIndex(aoe => !aoe.Rotation.AlmostEqual(spell.Rotation, 0.1f));
                if (mismatch >= 0)
                    module.ReportError(this, $"Mispredicted rotation: {spell.Rotation} vs predicted {_aoes[mismatch].Rotation}");
                _aoes.Clear();
            }
            _aoes.Add(new(donut ? _shapeIn : _shapeOut, caster.Position, spell.Rotation, spell.FinishAt));
            _aoes.Add(new(_shapeCone, caster.Position, spell.Rotation, spell.FinishAt));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var (dir, donut, order) = ClassifyAction(spell.Action);
            if (order < 0)
                return; // irrelevant spell

            if ((order == 0) != (NumCasts == 0))
                module.ReportError(this, $"Unexpected cast order: spell {order} at num-casts {NumCasts}");
            if (_aoes.Count != 2)
                module.ReportError(this, "No predicted casts");
            if (NumCasts >= _directionOffsets.Count)
                module.ReportError(this, $"Unexpected cast #{NumCasts}");
            if (order == 0 && NumCasts == 0 && _directionOffsets.Count > 0 && !spell.Rotation.AlmostEqual(caster.Rotation + _directionOffsets[0], 0.1f))
                module.ReportError(this, $"Mispredicted first rotation: expected {caster.Rotation}+{_directionOffsets[0]}, got {spell.Rotation}");

            // complete part of mechanic
            if (_hints.Count > 0)
                _hints.RemoveAt(0);
            ++NumCasts;
            _aoes.Clear();

            // predict next set of aoes
            if (NumCasts < _directionOffsets.Count)
            {
                var activation = module.WorldState.CurrentTime.AddSeconds(3.1f);
                var rotation = spell.Rotation + _directionOffsets[NumCasts];
                _aoes.Add(new(_ins[NumCasts] ? _shapeIn : _shapeOut, caster.Position, rotation, activation));
                _aoes.Add(new(_shapeCone, caster.Position, rotation, activation));
            }
        }

        private (Angle, bool, int) ClassifyAction(ActionID action) => (AID)action.ID switch
        {
            AID.NTripleKasumiGiriOutFrontFirst or AID.STripleKasumiGiriOutFrontFirst => (0.Degrees(), false, 0),
            AID.NTripleKasumiGiriOutRightFirst or AID.STripleKasumiGiriOutRightFirst => (-90.Degrees(), false, 0),
            AID.NTripleKasumiGiriOutBackFirst or AID.STripleKasumiGiriOutBackFirst => (180.Degrees(), false, 0),
            AID.NTripleKasumiGiriOutLeftFirst or AID.STripleKasumiGiriOutLeftFirst => (90.Degrees(), false, 0),
            AID.NTripleKasumiGiriInFrontFirst or AID.STripleKasumiGiriInFrontFirst => (0.Degrees(), true, 0),
            AID.NTripleKasumiGiriInRightFirst or AID.STripleKasumiGiriInRightFirst => (-90.Degrees(), true, 0),
            AID.NTripleKasumiGiriInBackFirst or AID.STripleKasumiGiriInBackFirst => (180.Degrees(), true, 0),
            AID.NTripleKasumiGiriInLeftFirst or AID.STripleKasumiGiriInLeftFirst => (90.Degrees(), true, 0),
            AID.NTripleKasumiGiriOutFrontRest or AID.STripleKasumiGiriOutFrontRest => (0.Degrees(), false, 1),
            AID.NTripleKasumiGiriOutRightRest or AID.STripleKasumiGiriOutRightRest => (-90.Degrees(), false, 1),
            AID.NTripleKasumiGiriOutBackRest or AID.STripleKasumiGiriOutBackRest => (180.Degrees(), false, 1),
            AID.NTripleKasumiGiriOutLeftRest or AID.STripleKasumiGiriOutLeftRest => (90.Degrees(), false, 1),
            AID.NTripleKasumiGiriInFrontRest or AID.STripleKasumiGiriInFrontRest => (0.Degrees(), true, 1),
            AID.NTripleKasumiGiriInRightRest or AID.STripleKasumiGiriInRightRest => (-90.Degrees(), true, 1),
            AID.NTripleKasumiGiriInBackRest or AID.STripleKasumiGiriInBackRest => (180.Degrees(), true, 1),
            AID.NTripleKasumiGiriInLeftRest or AID.STripleKasumiGiriInLeftRest => (90.Degrees(), true, 1),
            _ => (0.Degrees(), false, -1)
        };
    }

    class IaiGiriBait : Components.GenericBaitAway
    {
        public class Instance
        {
            public Actor Source;
            public Actor FakeSource;
            public Actor? Target;
            public List<Angle> DirOffsets = new();
            public List<string> Hints = new();

            public Instance(Actor source)
            {
                Source = source;
                FakeSource = new(0, 0, -1, "", ActorType.None, Class.None, new());
            }
        }

        public float Distance;
        public List<Instance> Instances = new();
        private float _jumpOffset;
        private bool _baitsDirty;

        public IaiGiriBait(float jumpOffset, float distance)
        {
            Distance = distance;
            IgnoreOtherBaits = true; // this really makes things only worse...
            _jumpOffset = jumpOffset;
        }

        public override void Update(BossModule module)
        {
            foreach (var inst in Instances)
                if (inst.Target != null)
                    inst.FakeSource.PosRot = inst.Target.PosRot - _jumpOffset * inst.Target.Rotation.ToDirection().ToVec4();

            // these typically are assigned over a single frame
            if (_baitsDirty)
            {
                _baitsDirty = false;
                CurrentBaits.Clear();
                foreach (var i in Instances)
                    if (i.Target != null && i.DirOffsets.Count > 0)
                        CurrentBaits.Add(new(i.FakeSource, i.Target, new AOEShapeCone(Distance - 3, 135.Degrees(), i.DirOffsets[0]))); // distance-3 is a hack for better double kasumi-giri bait indicator
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Giri)
            {
                var (dir, hint) = status.Extra switch
                {
                    0x248 => (0.Degrees(), "back"),
                    0x249 => (-90.Degrees(), "left"),
                    0x24A => (180.Degrees(), "front"),
                    0x24B => (90.Degrees(), "right"),
                    _ => (0.Degrees(), "")
                };
                if (hint.Length == 0)
                {
                    module.ReportError(this, $"Unexpected extra {status.Extra:X}");
                    return;
                }

                var i = InstanceFor(actor);
                _baitsDirty |= i.DirOffsets.Count == 0;
                i.DirOffsets.Add(dir);
                i.Hints.Add(hint);
            }
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID is (uint)TetherID.RatAndMouse)
            {
                InstanceFor(source).Target = module.WorldState.Actors.Find(tether.Target);
                _baitsDirty = true;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.FleetingIaiGiri or AID.DoubleIaiGiri)
                CurrentBaits.Clear(); // TODO: this is a hack, if we ever mark baits as dirty again, they will be recreated - but we need instances for resolve
        }

        private Instance InstanceFor(Actor source)
        {
            var i = Instances.Find(i => i.Source == source);
            if (i == null)
            {
                i = new(source);
                Instances.Add(i);
            }
            return i;
        }
    }

    class IaiGiriResolve : Components.GenericAOEs
    {
        public class Instance
        {
            public Actor Source;
            public List<AOEInstance> AOEs = new();

            public Instance(Actor source)
            {
                Source = source;
            }
        }

        private List<Instance> _instances = new();

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var i in _instances)
                if (i.AOEs.Count > 0)
                    yield return i.AOEs.First();
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID
                is AID.NFleetingIaiGiriFront or AID.NFleetingIaiGiriRight or AID.NFleetingIaiGiriLeft or AID.SFleetingIaiGiriFront or AID.SFleetingIaiGiriRight or AID.SFleetingIaiGiriLeft
                or AID.NShadowKasumiGiriFrontFirst or AID.NShadowKasumiGiriRightFirst or AID.NShadowKasumiGiriBackFirst or AID.NShadowKasumiGiriLeftFirst
                or AID.SShadowKasumiGiriFrontFirst or AID.SShadowKasumiGiriRightFirst or AID.SShadowKasumiGiriBackFirst or AID.SShadowKasumiGiriLeftFirst
                or AID.NShadowKasumiGiriFrontSecond or AID.NShadowKasumiGiriRightSecond or AID.NShadowKasumiGiriBackSecond or AID.NShadowKasumiGiriLeftSecond
                or AID.SShadowKasumiGiriFrontSecond or AID.SShadowKasumiGiriRightSecond or AID.SShadowKasumiGiriBackSecond or AID.SShadowKasumiGiriLeftSecond)
            {
                var inst = _instances.Find(i => i.Source == caster);
                if (inst == null)
                {
                    module.ReportError(this, $"Did not predict cast for {caster.InstanceID:X}");
                    return;
                }

                // update all aoe positions, first rotation/activation
                bool first = true;
                foreach (ref var aoe in inst.AOEs.AsSpan())
                {
                    aoe.Origin = caster.Position;
                    if (first)
                    {
                        first = false;
                        aoe.Rotation = spell.Rotation;
                        aoe.Activation = spell.FinishAt;
                    }
                }
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.FleetingIaiGiri:
                case AID.DoubleIaiGiri:
                    var comp = module.FindComponent<IaiGiriBait>();
                    var bait = comp?.Instances.Find(i => i.Source == caster);
                    if (bait?.Target == null)
                    {
                        module.ReportError(this, $"Failed to find bait for {caster.InstanceID:X}");
                        return;
                    }

                    var inst = new Instance(caster);
                    var curRot = bait.Target.Rotation;
                    var nextActivation = module.WorldState.CurrentTime.AddSeconds(2.7f);
                    foreach (var off in bait.DirOffsets)
                    {
                        curRot += off;
                        inst.AOEs.Add(new(new AOEShapeCone(comp!.Distance, 135.Degrees()), bait.FakeSource.Position, curRot, nextActivation));
                        nextActivation = nextActivation.AddSeconds(3.1f);
                    }
                    _instances.Add(inst);
                    break;
                case AID.NFleetingIaiGiriFront:
                case AID.NFleetingIaiGiriRight:
                case AID.NFleetingIaiGiriLeft:
                case AID.SFleetingIaiGiriFront:
                case AID.SFleetingIaiGiriRight:
                case AID.SFleetingIaiGiriLeft:
                case AID.NShadowKasumiGiriFrontFirst:
                case AID.NShadowKasumiGiriRightFirst:
                case AID.NShadowKasumiGiriBackFirst:
                case AID.NShadowKasumiGiriLeftFirst:
                case AID.SShadowKasumiGiriFrontFirst:
                case AID.SShadowKasumiGiriRightFirst:
                case AID.SShadowKasumiGiriBackFirst:
                case AID.SShadowKasumiGiriLeftFirst:
                case AID.NShadowKasumiGiriFrontSecond:
                case AID.NShadowKasumiGiriRightSecond:
                case AID.NShadowKasumiGiriBackSecond:
                case AID.NShadowKasumiGiriLeftSecond:
                case AID.SShadowKasumiGiriFrontSecond:
                case AID.SShadowKasumiGiriRightSecond:
                case AID.SShadowKasumiGiriBackSecond:
                case AID.SShadowKasumiGiriLeftSecond:
                    ++NumCasts;
                    var instance = _instances.Find(i => i.Source == caster);
                    if (instance?.AOEs.Count > 0)
                        instance.AOEs.RemoveAt(0);
                    break;
            }
        }
    }

    class FleetingIaiGiriBait : IaiGiriBait
    {
        public FleetingIaiGiriBait() : base(3, 60) { }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (Instances.Count == 1 && Instances[0].Hints.Count == 1)
                hints.Add($"Safespot: {Instances[0].Hints[0]}");
        }
    }

    class DoubleIaiGiriBait : IaiGiriBait
    {
        public DoubleIaiGiriBait() : base(1, 23) { }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            var safespots = string.Join(", ", Instances.Where(i => i.Hints.Count == 2).Select(i => i.Hints[1]));
            if (safespots.Length > 0)
                hints.Add($"Second safespots: {safespots}");
        }
    }
}
