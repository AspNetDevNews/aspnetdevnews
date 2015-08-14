using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetNews.Repos
{
    public class GhOrganization
    {
        public string Owner { get; set;  }
        public List<string> Repositories { get; set; }

    }
}
