using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBF;

namespace Blip
{
    /// <summary>
    /// Represents a query sequence.
    /// </summary>
    public class QueueSequence
    {
        public ISequence Sequence { get; set; }
        public int Position { get; set; } // Original position in the file or other initial order.
    }
}
