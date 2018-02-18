using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.Data
{
    public interface IIntelligenceRequestsAdapter
    {
        IIntelligenceProcessedData ProcessData(string dataToProcess); // кинули запрос на обработку и получили какое то значение, которым общается ИИ
    }
}
