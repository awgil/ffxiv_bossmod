using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    // a set of existing actors in world; part of the world state structure
    public class ActorState : IEnumerable<Actor>
    {
        private Dictionary<uint, Actor> _actors = new();

        public IEnumerator<Actor> GetEnumerator() => _actors.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _actors.Values.GetEnumerator();

        public Actor? Find(uint instanceID) => instanceID != 0 ? _actors.GetValueOrDefault(instanceID) : null;

        public event EventHandler<Actor>? Added;
        public Actor Add(uint instanceID, uint oid, string name, ActorType type, Class classID, Vector4 posRot, float hitboxRadius = 1, uint hpCur = 0, uint hpMax = 0, bool targetable = true, uint ownerID = 0)
        {
            var act = _actors[instanceID] = new Actor(instanceID, oid, name, type, classID, posRot, hitboxRadius, hpCur, hpMax, targetable, ownerID);
            Added?.Invoke(this, act);
            return act;
        }

        public event EventHandler<Actor>? Removed;
        public void Remove(uint instanceID)
        {
            var actor = Find(instanceID);
            if (actor == null)
                return; // nothing to remove

            actor.IsDestroyed = true;
            ChangeInCombat(actor, false); // exit combat
            UpdateCastInfo(actor, null); // stop casting
            UpdateTether(actor, new()); // untether
            for (int i = 0; i < actor.Statuses.Length; ++i)
                UpdateStatus(actor, i, new()); // clear statuses
            Removed?.Invoke(this, actor);
            _actors.Remove(instanceID);
        }

        public event EventHandler<(Actor, string)>? Renamed; // actor already has new name, old is passed as extra arg
        public void Rename(Actor act, string newName)
        {
            if (act.Name != newName)
            {
                var prevName = act.Name;
                act.Name = newName;
                Renamed?.Invoke(this, (act, prevName));
            }
        }

        public event EventHandler<(Actor, Class)>? ClassChanged; // actor already has new class, old is passed as extra args
        public void ChangeClass(Actor act, Class newClass)
        {
            if (act.Class != newClass)
            {
                var prevClass = act.Class;
                act.Class = newClass;
                ClassChanged?.Invoke(this, (act, prevClass));
            }
        }

        public event EventHandler<(Actor, Vector4)>? Moved; // actor already contains new position/rotation, old is passed as extra args
        public void Move(Actor act, Vector4 newPosRot)
        {
            if (act.PosRot != newPosRot)
            {
                var prevPosRot = act.PosRot;
                act.PosRot = newPosRot;
                Moved?.Invoke(this, (act, prevPosRot));
            }
        }

        public event EventHandler<(Actor, uint, uint)>? HPChanged; // actor already contains new cur/max hp, old are passed as extra args
        public void UpdateHP(Actor act, uint cur, uint max)
        {
            if (act.HPCur != cur || act.HPMax != max)
            {
                var prevCur = act.HPCur;
                var prevMax = act.HPMax;
                act.HPCur = cur;
                act.HPMax = max;
                HPChanged?.Invoke(this, (act, prevCur, prevMax));
            }
        }

        public event EventHandler<Actor>? IsTargetableChanged; // actor contains new state, old is inverted
        public void ChangeIsTargetable(Actor act, bool newTargetable)
        {
            if (act.IsTargetable != newTargetable)
            {
                act.IsTargetable = newTargetable;
                IsTargetableChanged?.Invoke(this, act);
            }
        }

        public event EventHandler<Actor>? IsDeadChanged; // actor contains new state, old is inverted
        public void ChangeIsDead(Actor act, bool newDead)
        {
            if (act.IsDead != newDead)
            {
                act.IsDead = newDead;
                IsDeadChanged?.Invoke(this, act);
            }
        }

        public event EventHandler<Actor>? InCombatChanged; // actor contains new state, old is inverted
        public void ChangeInCombat(Actor act, bool newValue)
        {
            if (act.InCombat != newValue)
            {
                act.InCombat = newValue;
                InCombatChanged?.Invoke(this, act);
            }
        }

        public event EventHandler<(Actor, uint)>? TargetChanged; // actor already contains new target, old is passed as extra arg
        public void ChangeTarget(Actor act, uint newTarget)
        {
            if (act.TargetID != newTarget)
            {
                var prevTarget = act.TargetID;
                act.TargetID = newTarget;
                TargetChanged?.Invoke(this, (act, prevTarget));
            }
        }

        public event EventHandler<Actor>? CastStarted;
        public event EventHandler<Actor>? CastFinished; // note that actor structure still contains cast details when this is invoked; invoked if actor disappears without finishing cast
        public void UpdateCastInfo(Actor act, ActorCastInfo? cast)
        {
            if (cast == null && act.CastInfo == null)
                return; // was not casting and is not casting

            if (cast != null && act.CastInfo != null && cast.Action == act.CastInfo.Action && cast.TargetID == act.CastInfo.TargetID)
            {
                // continuing casting same spell
                act.CastInfo.TotalTime = cast.TotalTime;
                act.CastInfo.FinishAt = cast.FinishAt;
                return;
            }

            if (act.CastInfo != null)
            {
                // finish previous cast
                CastFinished?.Invoke(this, act);
            }
            act.CastInfo = cast;
            if (act.CastInfo != null)
            {
                // start new cast
                CastStarted?.Invoke(this, act);
            }
        }

        public event EventHandler<Actor>? Tethered;
        public event EventHandler<Actor>? Untethered; // note that actor structure still contains previous tether info when this is invoked; invoked if actor disappears without untethering
        public void UpdateTether(Actor act, ActorTetherInfo tether)
        {
            if (act.Tether.Target == tether.Target && act.Tether.ID == tether.ID)
                return; // nothing changes

            if (act.Tether.Target != 0)
            {
                Untethered?.Invoke(this, act);
            }
            act.Tether = tether;
            if (act.Tether.Target != 0)
            {
                Tethered?.Invoke(this, act);
            }
        }

        // argument = actor + status index
        public event EventHandler<(Actor, int)>? StatusGain;
        public event EventHandler<(Actor, int)>? StatusLose; // note that status structure still contains details when this is invoked; invoked if actor disappears
        public event EventHandler<(Actor, int, ushort, DateTime)>? StatusChange; // invoked when extra or expiration time is changed; status contains new values, old are passed as extra args
        public void UpdateStatus(Actor act, int index, ActorStatus value)
        {
            if (act.Statuses[index].ID == value.ID && act.Statuses[index].SourceID == value.SourceID)
            {
                // status was and still is active; just update details
                if (value.ID != 0 && (act.Statuses[index].Extra != value.Extra || act.Statuses[index].ExpireAt != value.ExpireAt))
                {
                    var prevExtra = act.Statuses[index].Extra;
                    var prevExpire = act.Statuses[index].ExpireAt;
                    act.Statuses[index].Extra = value.Extra;
                    act.Statuses[index].ExpireAt = value.ExpireAt;
                    StatusChange?.Invoke(this, (act, index, prevExtra, prevExpire));
                }
            }
            else
            {
                if (act.Statuses[index].ID != 0)
                {
                    // remove previous status
                    StatusLose?.Invoke(this, (act, index));
                }
                act.Statuses[index] = value;
                if (act.Statuses[index].ID != 0)
                {
                    // apply new status
                    StatusGain?.Invoke(this, (act, index));
                }
            }
        }
    }
}
