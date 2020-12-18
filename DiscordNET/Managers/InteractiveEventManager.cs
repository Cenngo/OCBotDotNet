using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordNET.Managers
{
    public class InteractiveEventManager : IDisposable
    {
        private readonly DiscordSocketClient _client;
        private SocketMessage _message = null;
        private EventWaitHandle _handle;

        private readonly ICommandContext _context;
        private readonly bool _fromSourceUser;
        private readonly bool _fromSourceChannel;
        public InteractiveEventManager ( ICommandContext context, bool fromSourceUser, bool fromSourceChannel )
        {
            _client = context.Client.GetType() == typeof(DiscordSocketClient) ? context.Client as DiscordSocketClient
                : ( context.Client as DiscordShardedClient ).GetShardFor(context.Guild);
            this._context = context;
            this._fromSourceChannel = fromSourceChannel;
            this._fromSourceUser = fromSourceUser;
        }

        public void Dispose ( )
        {
            _handle.Dispose();
        }

        public async Task<SocketMessage> NextMessage ( TimeSpan duration )
        {
            _handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            _client.MessageReceived += OnMessage;
            await Task.CompletedTask;

            _handle.WaitOne(duration);
            _handle.Close();
            return _message;
        }

        private Task OnMessage ( SocketMessage msg )
        {
            if (( !_fromSourceChannel || ( msg.Channel == _context.Channel ) ) && ( !_fromSourceUser || ( msg.Author == _context.User ) ))
            {
                _message = msg;
                _handle.Set();
                _client.MessageReceived -= OnMessage;
            }
            return Task.CompletedTask;
        }
    }
}
