using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    //simple rotation with upto 4 AIDs, this assumes all AIDs have the same RotationDirIncrement
    public class SimpleRotationAOE : GenericAOEs //TODO: this probably only works if a single actor is doing the rotation at the same time, needs to be further generalized
    {
        public AOEShape Shape { get; private init; }
        public ActionID Aid; //rotation start AID
        public ActionID Aid2; //rotation step AID
        public ActionID Aid3; //rotation step AID, set to default if not used
        public ActionID Aid4; //rotation step AID, set to default if not used
        public Angle RotationDir;
        public Angle RotationDirIncrement; // positive angles for ccw, negative angles for cw
        public Angle Offset = 0.Degrees(); //example set to 180° if the first hit starts at the back
        public bool FirstAIDisRotationStep; //false if first AID is just a telegraph, true if first AID is a rotation step
        public int MaxCasts; // amount of steps in the rotion
        public uint ImminentColor = ArenaColor.AOE; // can be customized if needed
        public uint FutureColor = ArenaColor.Danger; // can be customized if needed
        public bool Risky = true; // can be customized if needed
        private int _maxcasts;
        private Angle _RotationDir;
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;
        public IEnumerable<Actor> ActiveCasters => _casters.Take(MaxCasts);

        public SimpleRotationAOE(ActionID aid, ActionID aid2, ActionID aid3, ActionID aid4, AOEShape shape, int maxCasts, Angle rotationDirIncrement, Angle offset = default, bool firstAIDisRotationStep = false) : base(aid)
        {
            Shape = shape;
            Aid = aid;
            Aid2 = aid2;
            Aid3 = aid3;
            Aid4 = aid4;
            MaxCasts = maxCasts;
            RotationDirIncrement = rotationDirIncrement;
            Offset = offset;
            FirstAIDisRotationStep = firstAIDisRotationStep;
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var c in _casters)
            {
                if (_maxcasts > 1)
                yield return new AOEInstance(Shape, c.Position, _RotationDir + RotationDirIncrement, default, FutureColor, Risky);                
                if (_maxcasts > 0)
                yield return new AOEInstance(Shape, c.Position, _RotationDir, default, ImminentColor, Risky);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == Aid == WatchedAction)
            {
                _casters.Add(caster);                
                _RotationDir = spell.Rotation + Offset;
                _maxcasts = MaxCasts;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if(FirstAIDisRotationStep && spell.Action == Aid == WatchedAction && Aid != default)
                {
                    _RotationDir += RotationDirIncrement;
                    --_maxcasts;
                    if(_maxcasts == 0)
                        _casters.Remove(caster);
                }                
            if(spell.Action == Aid2 == WatchedAction && Aid2 != default)
                {
                    _RotationDir += RotationDirIncrement;
                    --_maxcasts;
                    if(_maxcasts == 0)
                        _casters.Remove(caster);
                }            
            if(spell.Action == Aid3 == WatchedAction && Aid3 != default)
                {
                    _RotationDir += RotationDirIncrement;
                    --_maxcasts;
                    if(_maxcasts == 0)
                        _casters.Remove(caster);
                }
            if(spell.Action == Aid4 == WatchedAction && Aid4 != default)
                {
                    _RotationDir += RotationDirIncrement;
                    --_maxcasts;
                    if(_maxcasts == 0)
                        _casters.Remove(caster);
            }
        }
    }
}