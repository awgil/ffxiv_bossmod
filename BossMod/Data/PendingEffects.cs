using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    // game is somewhat weird in how it handles hp updates and status gains by cast events - these are typically delayed by 0.5-1s
    // when we receive ActionEffectN packet, it contains damage/heal effects and status gain effects - these are applied when later EffectResult[Basic] packet is received
    // we mostly ignore this, however it is important e.g. for autorotation - not taking these 'pending' effects can make us recast same spell again
    // this is most visible with long GCD casts - like BLM dots or GCD heals
    public class PendingEffects
    {
        public class Entry
        {
            public DateTime Timestamp;
            public ulong Source;
            public ActorCastEvent Event;
            public BitMask UnconfirmedTargets;
            public BitMask UnconfirmedSource;

            public Entry(DateTime ts, ulong source, ActorCastEvent ev)
            {
                Timestamp = ts;
                Source = source;
                Event = ev;
                for (int i = 0; i < ev.Targets.Count; ++i)
                {
                    foreach (var eff in ev.Targets[i].Effects)
                    {
                        if (eff.Type is ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage or ActionEffectType.Heal or ActionEffectType.ApplyStatusEffectTarget or ActionEffectType.ApplyStatusEffectSource or ActionEffectType.RecoveredFromStatusEffect)
                        {
                            if (ev.Targets[i].ID == source || (eff.Param4 & 0x80) != 0)
                                UnconfirmedSource.Set(i);
                            else
                                UnconfirmedTargets.Set(i);
                        }
                    }
                }
            }
        }

        private List<Entry> _entries = new(); // implicitly sorted by timestamp/global sequence?
        public IReadOnlyList<Entry> Entries => _entries;

        public void AddEntry(DateTime ts, ulong source, ActorCastEvent ev)
        {
            var entry = new Entry(ts, source, ev);
            if ((entry.UnconfirmedTargets | entry.UnconfirmedSource).Any())
                _entries.Add(entry);
        }

        public void Confirm(DateTime ts, uint seq, ulong target, int targetIndex)
        {
            var entryIndex = _entries.FindIndex(e => e.Event.GlobalSequence == seq);
            if (entryIndex < 0)
            {
                Service.Log($"[PendingEffects] Confirmation for missing event #{seq}/{targetIndex} @ {target:X}");
                return;
            }
            var entry = _entries[entryIndex];
            if (targetIndex >= entry.Event.Targets.Count)
            {
                Service.Log($"[PendingEffects] Confirmation for out-of-range target #{seq}/{targetIndex} @ {target:X}, event has {entry.Event.Targets.Count} targets");
                return;
            }

            if (entry.Source == target)
            {
                if (!entry.UnconfirmedSource[targetIndex])
                {
                    Service.Log($"[PendingEffects] Double confirmation for source #{seq}/{targetIndex} {entry.Event.Action} from {entry.Source:X} @ {target:X}");
                    return;
                }
                entry.UnconfirmedSource.Clear(targetIndex);
            }
            else if (entry.Event.Targets[targetIndex].ID == target)
            {
                if (!entry.UnconfirmedTargets[targetIndex])
                {
                    Service.Log($"[PendingEffects] Double confirmation for target #{seq}/{targetIndex} {entry.Event.Action} from {entry.Source:X} @ {target:X}");
                    return;
                }
                entry.UnconfirmedTargets.Clear(targetIndex);
            }
            else
            {
                Service.Log($"[PendingEffects] Confirmation for unexpected target #{seq}/{targetIndex} @ {target:X}; expected source={entry.Source:X}, expected target={entry.Event.Targets[targetIndex].ID:X}");
                return;
            }

            if ((entry.UnconfirmedTargets | entry.UnconfirmedSource).None())
            {
                _entries.RemoveAt(entryIndex);
            }
        }

        public void RemoveExpired(DateTime ts)
        {
            var minRemaining = ts.AddSeconds(-3);
            _entries.RemoveAll(e => {
                bool expired = e.Timestamp < minRemaining;
                if (expired)
                    Service.Log($"[PendingEffects] Expired #{e.Event.GlobalSequence} {e.Event.Action} from {e.Source:X} without confirmations");
                return expired;
            });
        }

        public int PendingHPDifference(ulong target)
        {
            int res = 0;
            foreach (var eff in PendingEffectsAtTarget(_entries, target))
            {
                if (eff.Type is ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage)
                {
                    res -= HealOrDamageValue(eff);
                }
                else if (eff.Type == ActionEffectType.Heal)
                {
                    res += HealOrDamageValue(eff);
                }
            }
            return res;
        }

        // returns low byte of extra if pending (stack count), or null if not
        public byte? PendingStatus(ulong target, uint statusID, ulong source)
        {
            foreach (var eff in PendingEffectsAtTarget(_entries.Where(e => e.Source == source), target))
            {
                if (eff.Type is ActionEffectType.ApplyStatusEffectTarget or ActionEffectType.ApplyStatusEffectSource && eff.Value == statusID)
                {
                    return eff.Param2;
                }
            }
            return null;
        }

        private static IEnumerable<ActionEffect> PendingEffectsAtTarget(IEnumerable<Entry> entries, ulong target)
        {
            foreach (var e in entries)
            {
                if (e.Source == target)
                {
                    foreach (var i in e.UnconfirmedSource.SetBits())
                    {
                        bool selfTargeted = e.Event.Targets[i].ID == e.Source;
                        foreach (var eff in e.Event.Targets[i].Effects)
                        {
                            if (selfTargeted || (eff.Param4 & 0x80) != 0)
                            {
                                yield return eff;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var i in e.UnconfirmedTargets.SetBits())
                    {
                        if (e.Event.Targets[i].ID == target)
                        {
                            foreach (var eff in e.Event.Targets[i].Effects)
                            {
                                if ((eff.Param4 & 0x80) == 0)
                                {
                                    yield return eff;
                                }
                            }
                        }
                    }
                }
            }
        }

        private int HealOrDamageValue(ActionEffect eff) => eff.Value + ((eff.Param4 & 0x40) != 0 ? eff.Param3 * 0x10000 : 0);
    }
}
