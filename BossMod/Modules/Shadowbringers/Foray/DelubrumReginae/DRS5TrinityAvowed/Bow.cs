using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS5TrinityAvowed
{
    // aoe starts at cast and ends with envcontrol; it's not considered 'risky' when paired with quick march
    class FlamesOfBozja : Components.GenericAOEs
    {
        public AOEInstance? AOE { get; private set; }
        private bool _risky;

        public FlamesOfBozja(bool risky) : base(ActionID.MakeSpell(AID.FlamesOfBozjaAOE))
        {
            _risky = risky;
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (AOE != null)
                yield return AOE.Value;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                AOE = new(new AOEShapeRect(45, 25), caster.Position, spell.Rotation, spell.FinishAt, risky: _risky);
        }

        public override void OnEventEnvControl(BossModule module, uint directorID, byte index, uint state)
        {
            if (directorID == 0x8004001F && index is 0x12 or 0x13 && state == 0x00080004) // 12/13 for east/west
                AOE = null;
        }
    }

    // rows are 0 (Z=-102), 1 (Z=-92), 2 (Z=-82), 3 (Z=-72), 4 (Z=-62)
    // what we've seen so far:
    // ENVC 16.00020001; arrows spawn at X=-247 (E) aimed -90 (W); N to S we have Frost > None > Glacier > Flame > Spark, exploding N to S Spark > Glacier > Flame > Frost > None ==> 'inverted' pattern
    // ENVC 16.00200010; arrows spawn at X=-247 (E) aimed -90 (W); N to S we have Spark > None > Frost > Glacier > Flame; exploding N to S Glacier > Flame > None > Frost > Spark ==> 'normal' pattern
    // ENVC 17.00200010; arrows spawn at X=-297 (W) aimed +90 (E); N to S we have Glacier > Frost > Spark > Flame > None; exploding N to S None > Spark > Flame > Glacier > Frost ==> 'normal' pattern
    // so, assumption: 16 is W->E, 17 is E->W; 00020001 is inverted, 00200010 is normal
    class ShimmeringShot : TemperatureAOE
    {
        public enum Pattern { Unknown, EWNormal, EWInverted, WENormal, WEInverted }

        private int[] _slotTempAdjustments = new int[5];
        private BitMask _arrowsInited;
        private Pattern _pattern;
        private float _spawnToActivation;
        private DateTime _activation;

        private static AOEShapeRect _shapeCell = new(5, 5, 5);
        private static int[,] _remap = { { 0, 1, 2, 3, 4 }, { 4, 2, 3, 0, 1 }, { 3, 4, 1, 2, 0 }, { 3, 4, 1, 2, 0 }, { 4, 2, 3, 0, 1 } };

        public ShimmeringShot(float spawnToActivation)
        {
            _spawnToActivation = spawnToActivation;
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_arrowsInited.Raw != 0x1B)
                yield break;
            int temp = Temperature(actor);
            int cell = Array.IndexOf(_slotTempAdjustments, -temp);
            if (cell < 0)
                yield break;

            var xOffset = _pattern is Pattern.EWNormal or Pattern.EWInverted ? -20 : +20;
            var zOffset = 10 * (cell - 2);
            yield return new(_shapeCell, module.Bounds.Center + new WDir(xOffset, zOffset), new(), _activation, ArenaColor.SafeFromAOE, false);
        }

        public override void Update(BossModule module)
        {
            InitArrow(module, OID.SparkArrow, +1);
            InitArrow(module, OID.FlameArrow, +2);
            InitArrow(module, OID.FrostArrow, -1);
            InitArrow(module, OID.GlacierArrow, -2);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.ChillArrow1 or AID.FreezingArrow1 or AID.HeatedArrow1 or AID.SearingArrow1 or AID.ChillArrow2 or AID.FreezingArrow2 or AID.HeatedArrow2 or AID.SearingArrow2)
                ++NumCasts;
        }

        public override void OnEventEnvControl(BossModule module, uint directorID, byte index, uint state)
        {
            if (directorID != 0x8004001F)
                return;

            var pattern = (index, state) switch
            {
                (0x16, 0x00200010) => Pattern.EWNormal,
                (0x16, 0x00020001) => Pattern.EWInverted,
                (0x17, 0x00200010) => Pattern.WENormal,
                (0x17, 0x00020001) => Pattern.WEInverted,
                _ => Pattern.Unknown
            };
            if (pattern != Pattern.Unknown)
                _pattern = pattern;
        }

        public bool ActorUnsafeAt(BossModule module, Actor actor, WPos pos)
        {
            var offset = pos - module.Bounds.Center;
            bool posInFlames = _pattern switch
            {
                Pattern.EWNormal or Pattern.EWInverted => offset.X > -15,
                Pattern.WENormal or Pattern.WEInverted => offset.X < +15,
                _ => false
            };
            if (posInFlames)
                return true;

            if (_arrowsInited.Raw != 0x1B)
                return false; // no arrows yet, any position is safe
            int row = RowIndex(module, pos);
            return _slotTempAdjustments[row] != -Temperature(actor);
        }

        protected int RowIndex(BossModule module, WPos pos) => (pos.Z - module.Bounds.Center.Z) switch
        {
            < -15 => 0,
            < -5 => 1,
            < 5 => 2,
            < 15 => 3,
            _ => 4
        };

        private void InitArrow(BossModule module, OID oid, int temp)
        {
            if (_arrowsInited[temp + 2])
                return;
            var arrow = module.Enemies(oid).FirstOrDefault();
            if (arrow == null)
                return;

            if (arrow.Position.X < module.Bounds.Center.X != _pattern is Pattern.WENormal or Pattern.WEInverted)
                module.ReportError(this, "Unexpected arrow X coord");
            int srcRow = RowIndex(module, arrow.Position);
            int destRow = _remap[(int)_pattern, srcRow];
            _slotTempAdjustments[destRow] = temp;
            _arrowsInited.Set(temp + 2);
            _activation = module.WorldState.CurrentTime.AddSeconds(_spawnToActivation);
        }
    }

    class FlamesOfBozja1 : FlamesOfBozja
    {
        public FlamesOfBozja1() : base(false) { }
    }

    class QuickMarchBow1 : QuickMarch
    {
        private FlamesOfBozja1? _flames;

        public override void Init(BossModule module) => _flames = module.FindComponent<FlamesOfBozja1>();

        public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => !module.Bounds.Contains(pos) || (_flames?.AOE?.Shape.Check(pos, _flames.AOE.Value.Origin, _flames.AOE.Value.Rotation) ?? false);
    }

    class ShimmeringShot1 : ShimmeringShot
    {
        public ShimmeringShot1() : base(12.8f) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (ActorUnsafeAt(module, actor, actor.Position))
                hints.Add("Go to safe zone!");
        }
    }

    class FlamesOfBozja2 : FlamesOfBozja
    {
        public FlamesOfBozja2() : base(true) { }
    }

    class ShimmeringShot2 : ShimmeringShot
    {
        public ShimmeringShot2() : base(14.0f) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints) { } // no need for hints, quick march handles that
    }

    class QuickMarchBow2 : QuickMarch
    {
        private ShimmeringShot2? _shimmering;

        public override void Init(BossModule module) => _shimmering = module.FindComponent<ShimmeringShot2>();

        public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => !module.Bounds.Contains(pos) || (_shimmering?.ActorUnsafeAt(module, actor, pos) ?? false);
    }
}
