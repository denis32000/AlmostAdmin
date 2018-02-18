using AlmostAdmin.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.Services
{
    public class MainService
    {
        private MainRepository _mainRepository;

        public MainService(MainRepository mainRepository)
        {
            _mainRepository = mainRepository;
        }

        internal object GetListOfQuestions()
        {
            throw new NotImplementedException();
        }

        internal object GetListOfAnswers()
        {
            throw new NotImplementedException();
        }
    }
}
