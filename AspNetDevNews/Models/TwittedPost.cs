using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Models
{
    public class TwittedPost
    {
        public string Id { get; set; }
        public DateTime PublishDate { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }

        public ulong StatusID { get; set; }
        public string Feed { get; set; }
    }
}
