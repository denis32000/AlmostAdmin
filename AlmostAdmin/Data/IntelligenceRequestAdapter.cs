using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.Data
{
    public class IntelligenceRequestAdapter : IIntelligenceRequestsAdapter
    {
        private object _processor;

        public IntelligenceRequestAdapter()
        {
            // TODO: создаём instance обработчика данных
            _processor = new object();
        }

        public IIntelligenceProcessedData ProcessData(string dataToProcess)
        {
            throw new NotImplementedException();
        }
    }
}
