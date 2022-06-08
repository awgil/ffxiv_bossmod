using System;
using System.Collections.Generic;

namespace BossMod
{
    public class CastEvent
    {
        public struct Target
        {
            public ulong ID;
            public ActionEffects Effects;
        }

        public ulong CasterID;
        public ulong MainTargetID; // note that actual affected targets could be completely different
        public ActionID Action;
        public float AnimationLockTime;
        public uint MaxTargets;
        public List<Target> Targets = new();
        public uint SourceSequence;

        public bool IsSpell() => Action.Type == ActionType.Spell;
        public bool IsSpell<AID>(AID aid) where AID : Enum => Action == ActionID.MakeSpell(aid);
    }

    // dispatcher for instant world events; part of the world state structure
    public class WorldEvents
    {
        public event EventHandler<(ulong actorID, uint iconID)>? Icon; // TODO: this should really be an actor field, but I have no idea what triggers icon clear...
        public void DispatchIcon((ulong actorID, uint iconID) args)
        {
            Icon?.Invoke(this, args);
        }

        public event EventHandler<CastEvent>? Cast;
        public void DispatchCast(CastEvent info)
        {
            Cast?.Invoke(this, info);
        }

        public event EventHandler<(uint directorID, uint updateID, uint p1, uint p2, uint p3, uint p4)>? DirectorUpdate;
        public void DispatchDirectorUpdate((uint directorID, uint updateID, uint p1, uint p2, uint p3, uint p4) args)
        {
            DirectorUpdate?.Invoke(this, args);
        }

        public event EventHandler<(uint featureID, byte index, uint state)>? EnvControl;
        public void DispatchEnvControl((uint featureID, byte index, uint state) args)
        {
            EnvControl?.Invoke(this, args);
        }
    }
}
