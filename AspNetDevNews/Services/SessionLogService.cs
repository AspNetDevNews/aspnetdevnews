using AspNetDevNews.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services
{
    public class SessionLogService : ISessionLogger
    {
        private StringBuilder Content { get; set; }

        private IStorageService Storage { get; set; }

        public SessionLogService(IStorageService storage) {
            Storage = storage;
        }
        public void AddMessage(string operation, string message, string data, MessageType type)
        {
            if (Content == null)
                Content = new StringBuilder();

            string row = DateTime.Now.ToLongDateString() + $" {DateTime.Now.ToLongTimeString()} - {type} - {operation} -> {message} : " + data;
            Content.AppendLine(row);
        }

        public void EndSession()
        {
            this.Storage.StoreSessionLog(Content.ToString());
            Content = new StringBuilder();
        }

        public void StartSession()
        {
            Content = new StringBuilder();
        }
    }
}
