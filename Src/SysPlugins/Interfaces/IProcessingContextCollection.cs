using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IProcessingContextCollection
    {
        IEnumerable<IProcessingContext> ProcessingContexts { get; }

        bool IsMaintenance { get; }

        IProcessingContext this[int index] { get; }

        int Length { get; }
    }
}
