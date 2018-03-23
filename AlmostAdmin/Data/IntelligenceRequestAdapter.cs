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
            // TODO: создаём instance обработчика данных, в нашем случае SmartDataAnalizerByDenisShvetsov
            _processor = new object();
        }

        /*
        public void Educate(int key, string answer)
        {
            throw new NotImplementedException();
        }
        */

        public async Task<IIntelligenceProcessedData> ProcessDataAsync(string dataToProcess)
        {
            throw new NotImplementedException();
        }
    }
}
