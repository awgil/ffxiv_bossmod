using System;
using System.Collections.Generic;
using System.Linq;

namespace UIDev.Analysis
{
    class CommonEnumInfo
    {
        protected Type? _oidType;

        protected string JoinStrings(IEnumerable<string> strings)
        {
            var s = string.Join('/', strings);
            return s.Length > 0 ? s : "none";
        }

        protected string OIDString(uint oid) => oid == 0 ? "player" : _oidType?.GetEnumName(oid) ?? $"{oid:X}";

        protected string OIDListString(IEnumerable<uint> oids) => JoinStrings(oids.Select(OIDString));
    }
}
