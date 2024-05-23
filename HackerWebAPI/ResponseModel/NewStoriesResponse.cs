using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerWebAPI
{
    public class NewStoriesResponse
    {
        public int Total { get; set; }
        public IEnumerable<object> NewStories { get; set; }
    }

}
