using BossMod;
using System;
using System.Collections.Generic;

namespace UIDev
{
    class ConfigTest : ITest
    {
        public void Dispose()
        {
        }

        public void Draw()
        {
            Service.Config.Draw();
        }
    }
}
