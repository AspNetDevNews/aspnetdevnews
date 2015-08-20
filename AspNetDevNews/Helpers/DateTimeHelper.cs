using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class DateTimeHelper
    {
        public static string GetRowKey(this DateTime dtCurrentTime) {
            return dtCurrentTime.ToString("yyyy-MM-dd-HH-mm-ss-FFFFFFF");
        }
    }
}
