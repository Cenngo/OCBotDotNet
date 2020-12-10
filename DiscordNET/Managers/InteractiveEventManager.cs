using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.Threading;

namespace DiscordNET.Managers
{
    public class InteractiveEventManager : IDisposable
    {
        private readonly DiscordSocketClient _client;
        private SocketMessage _message = null;
        private EventWaitHandle _handle;

        private ICommandContext context;
        private bool fromSourceUser;
        private bool fromSourceChannel;
        public InteractiveEventManager (ICommandContext context, bool fromSourceUser, bool fromSourceChannel)
        {
            _client = context.Client.GetType() == typeof(DiscordSocketClient) ? context.Client as DiscordSocketClient 
                : ( context.Client as DiscordShardedClient ).GetShardFor(context.Guild);
            this.context = context;
            this.fromSourceChannel = fromSourceChannel;
            this.fromSourceUser = fromSourceUser;
        }

        public void Dispose ( )
        {
            _handle.Dispose();
        }

        public async Task<SocketMessage> NextMessage (TimeSpan duration )
        {
            _handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            _client.MessageReceived += OnMessage;

            _handle.WaitOne(duration);
            return _message;
        }

        private async Task OnMessage ( SocketMessage msg )
        {
            if((!fromSourceChannel || (msg.Channel == context.Channel)) && (!fromSourceUser || (msg.Author == context.User ) ))
            {
                _message = msg;
                _handle.Set();
                _client.MessageReceived -= OnMessage;
                _handle.Close();
            }
        }
    }
}
