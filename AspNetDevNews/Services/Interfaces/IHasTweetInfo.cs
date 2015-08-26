using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.Interfaces
{
    public interface IHasTweetInfo
    {
        ulong StatusID { get; set; }
    }
}
