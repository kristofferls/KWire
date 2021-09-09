using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lawo.EmberPlusSharp.Model;

namespace KWire
{
    
        public sealed class RubyRoot : Root<RubyRoot> // FIRST Node in the tree : is usually the name of the mixer. 
        {
            internal Ruby Ruby { get; private set; }

        }
        public sealed class Ruby : FieldNode<Ruby> // The next node to jump into. 
        {
            internal GPIOs GPIOs { get; private set; }

        }
        public sealed class GPIOs : FieldNode<GPIOs>
        {
            
            internal EGPIO_AUTOCAM EGPIO_AUTOCAM { get; private set; }
        }

        public sealed class EGPIO_AUTOCAM: FieldNode<EGPIO_AUTOCAM>
        {
            [Element(Identifier = "Output Signals")] //A reference to something specific in the node.
            internal OutputSignals OutputSignals { get; private set; }
        }

        public sealed class OutputSignals : FieldNode<OutputSignals>

        {
            [Element(Identifier = "REDLIGHT")] //A reference to a specific member / child of the previous class, Output Signals in this case. 
            internal EGPO REDLIGHT { get; private set; }

            [Element(Identifier = "MASTERONOFF")]

            internal EGPO MASTERONOFF { get; private set; } 

        }

        public sealed class EGPO : FieldNode<EGPO>
        {
            [Element(Identifier = "State")] //A reference to a spesific member / child of the previous specified class member. 
            internal BooleanParameter State { get; private set; } //look for a boolean parameter stored in "State". Can be enums, IntegerParamter etc. See the SDK doc. 


        }

    }
