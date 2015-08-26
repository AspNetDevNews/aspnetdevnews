using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.Interfaces
{
    public enum MessageType { Fatal, Error, Warning, Info };

    public interface ISessionLogger
    {

        void StartSession();
        void AddMessage(string operation, string message, string data, MessageType type);
        Task EndSession();
    }
}
