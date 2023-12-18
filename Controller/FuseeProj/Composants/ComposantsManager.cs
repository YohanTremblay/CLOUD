using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuseeProj.Composants
{
    internal class ComposantsManager
    {
        public Module Module { get ; set; }

        public ComposantsManager()
        {
            Module = new Module();
        }

    }
}
