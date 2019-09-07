using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using System.Collections.Generic;

namespace Ccf.Ck.Models.ContextBasket
{
    public class ProcessingContextCollection : IProcessingContextCollection
    {
        List<IProcessingContext> _ProcessingContexts;

        public ProcessingContextCollection(List<IProcessingContext> processingContexts) : this(processingContexts, false)
        {
        }

        public ProcessingContextCollection(List<IProcessingContext> processingContexts, bool isMaintenance)
        {
            _ProcessingContexts = processingContexts;
            IsMaintenance = isMaintenance;
        }

        public IEnumerable<IProcessingContext> ProcessingContexts
        {
            get
            {
                return _ProcessingContexts;
            }
        }

        public bool IsMaintenance { get; }

        public IProcessingContext this[int index]    // Indexer declaration  
        {
            get
            {
                return _ProcessingContexts[index];
            }
        }

        public int Length
        {
            get
            {
                return _ProcessingContexts.Count;
            }
        }
    }
}
