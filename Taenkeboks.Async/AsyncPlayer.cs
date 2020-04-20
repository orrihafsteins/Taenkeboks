using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace Taenkeboks.AsyncConsole
{
    class AsyncPlayer<V, A>
    {

        Channel<V> _visible;
        Channel<string> _error;
        ChannelWriter<(int, A)> _actions;
        V _lastVisible = default(V);
        public AsyncPlayer(string name,int side, ChannelWriter<(int, A)> actions)
        {
            Name = name;
            Side = side;
            _actions = actions;
            _visible = Channel.CreateUnbounded<V>();
            _error = Channel.CreateUnbounded<string>();
        }
        public string Name { get; private set; }
        public int Side { get; private set; }
        public V Last() //Called by client
        {
            return _lastVisible;
        }
        public async Task<V> Next() //Called by client
        {
            var visible = await _visible.Reader.ReadAsync();
            _lastVisible = visible;
            return visible;
        }

        public V Current() //Called by client
        {
            if (_visible.Reader.TryRead(out V visible))
                return visible;
            else
                return _lastVisible;
        }
        public async Task<string> NextError() //Called by client
        {
            return await _error.Reader.ReadAsync(); ;
        }
        public async Task PerformAction(A action) //Called by client
        {
            await _actions.WriteAsync((Side, action));
        }
        internal async Task Update(V visible) //Called by game
        {
            await _visible.Writer.WriteAsync(visible);
        }
        internal async Task Error(string error) //Called by game
        {
            await _error.Writer.WriteAsync(error);
        }
    }
}
