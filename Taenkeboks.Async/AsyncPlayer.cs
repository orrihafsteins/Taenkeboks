using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace Taenkeboks.Async
{
    public class AsyncPlayer<V, A>
    {

        Channel<V> _visible;
        Channel<string> _error;
        ChannelWriter<(int, A)> _actions;
        CancellationToken _ct;
        V _lastVisible = default(V);
        public AsyncPlayer(string name,int side, ChannelWriter<(int, A)> actions,CancellationToken ct)
        {
            Name = name;
            Side = side;
            _ct = ct;
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
        public V Current() //Called by client
        {
            if (_visible.Reader.TryRead(out V visible))
                return visible;
            else
                return _lastVisible;
        }
        public async Task<V> Next() //Called by client
        {
            var visible = await _visible.Reader.ReadAsync(_ct);
            _lastVisible = visible;
            return visible;
        }
        public async Task<string> NextError() //Called by client
        {
            return await _error.Reader.ReadAsync(_ct);
        }
        public async Task PerformAction(A action) //Called by client
        {
            await _actions.WriteAsync((Side, action), _ct);
        }
        internal async Task Update(V visible) //Called by game
        {
            await _visible.Writer.WriteAsync(visible, _ct);
        }
        internal async Task Error(string error) //Called by game
        {
            await _error.Writer.WriteAsync(error, _ct);
        }

        internal void End() //Called by game
        {
            _error.Writer.Complete();
            _visible.Writer.Complete();
        }
    }
}
