using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using blazorWords.Data;
using Microsoft.AspNetCore.SignalR;

namespace BlazorSignalRApp.Server.Hubs
{
    public class WordHub : Hub
    {
        static ConcurrentDictionary<string, string> clientList = new ConcurrentDictionary<string, string>();
        private IWordService _service;
        static List<Words> words;
        public WordHub(IWordService service)
        {
            _service = service;
            //words = _service.GetWords();
        }
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("Client Connected:" + this.Context.ConnectionId);
            int otherMoney = 100;
            string otherName = null;
            /* if (clientList.Count > 0) */
            Console.WriteLine("Clients Count:" + clientList.Count);

            if (clientList.Count == 1)
            {
                //    otherName = clientList.FirstOrDefault().Key;
                /* await Clients.Caller.SendAsync("GetConnectionId", this.Context.ConnectionId, otherName, otherMoney); */
                //    Console.WriteLine("Client Name:" + otherName);
                //    await Clients.Caller.SendAsync("GetConnectionId", this.Context.ConnectionId, otherName, otherMoney);
                await Clients.Others.SendAsync("SendUserInformation", this.Context.ConnectionId);
            }
            else if (clientList.Count == 0)
            {
                await Clients.Caller.SendAsync("GetConnectionId", this.Context.ConnectionId, otherName, otherMoney);
            }
            else if (clientList.Count > 1)
            {
                await Clients.Caller.SendAsync("ComeLater");
            }

            /* foreach(var item in clientList)
             {
                 Console.WriteLine("clientlist Item:"+item.Key);
             } */
        }

        public async Task SendUserInformation(string otherUserName, int otherMoney, string senderConnectionID)
        {
            await Clients.Client(senderConnectionID).SendAsync("GetUserInformation",otherUserName,otherMoney,senderConnectionID);
        }

        public async Task Refresh()
        {
            if (words == null || (words != null && words.Count == 0))
            {
                words = _service.GetWords();
            }
            string wordText = "";
            var random = new Random();
            int index = random.Next(words.Count);
            wordText = words[index].word;

            words.Remove(words[index]);// Remove Used Words
            await Clients.All.SendAsync("RefreshWord", wordText);
        }

        public async Task AddList(string userName, string connectionId)
        {
            int money = 100;
            if (clientList.Count > 0 && clientList.Count < 2)
            {
                if (words == null || (words != null && words.Count == 0))
                {
                    words = _service.GetWords();
                }
                clientList.TryAdd(userName, connectionId);
                string wordText = "";
                var random = new Random();
                int index = random.Next(words.Count);
                wordText = words[index].word;

                words.Remove(words[index]);
                await Clients.All.SendAsync("ReceiveWord", wordText, userName, money);
            }
            else if (clientList.Count == 0)
            {
                clientList.TryAdd(userName, connectionId);
                await Clients.All.SendAsync("ReceiveUser", userName, connectionId, money);
            }
        }
        public async Task LoginUser(string userName, string connectionID)
        {
            await AddList(userName, connectionID);
        }

        public async Task OpenClient(int counter, string imgId, string lblId, int money)
        {
            await Clients.Others.SendAsync("ReceiveOpen", counter, imgId, lblId, money);
        }

        public async Task sendAnswer(string userName, string connectionID, int money)
        {
            await Clients.Others.SendAsync("ReceiveAnswer", userName, connectionID, money);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine("Client Disconnected:" + Context.ConnectionId);
            if (clientList.Count > 0)
            {
                string connectionId = Context.ConnectionId;
                Console.WriteLine("Client If Disconnected:" + connectionId);
                string userName = clientList.FirstOrDefault(entry => entry.Value == connectionId).Key;
                Console.WriteLine("Client If Disconnected UserName:" + userName);
                if (userName != null)
                {
                    clientList.TryRemove(userName, out _);
                }
                if (clientList.Count == 1 || clientList.Count == 0)
                {
                    await Clients.Others.SendAsync("RemoveUser", userName, connectionId);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}