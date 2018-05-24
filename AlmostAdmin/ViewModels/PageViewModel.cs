using AlmostAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.ViewModels
{
    public class SimilarQuestionsViewModel
    {
        public int QuestionId { get; set; }
        public IEnumerable<Question> SimilarQuestions { get; set; }
    }

    public class QuestionsViewModel
    {
        public IEnumerable<Question> Questions { get; set; }
        public PageViewModel PageViewModel { get; set; }
    }

    public class PageViewModel
    {
        public int PageNumber { get; private set; }
        public int TotalPages { get; private set; }

        public PageViewModel(int count, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }

        public bool HasPreviousPage
        {
            get
            {
                return (PageNumber > 1);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageNumber < TotalPages);
            }
        }
    }
}
