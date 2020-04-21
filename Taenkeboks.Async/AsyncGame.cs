using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Linq;
using PIM;
namespace Taenkeboks.Async
{
    public class AsyncGame<S, A, V>
    {
        Game<S, A, V> _game;
        string[] _playerNames;
        Channel<(int, A)> _action;
        CancellationTokenSource _cts;
        public AsyncPlayer<V,A>[] Players { get; private set; }
        public AsyncGame(Game<S, A, V> game,string[] playerNames)
        {
            _cts = new CancellationTokenSource();
            _game = game;
            _playerNames = playerNames;
            _action = Channel.CreateUnbounded<(int,A)> ();
            Players =playerNames.Select((n, i) => new AsyncPlayer<V,A>(n, i, _action.Writer, _cts.Token)).ToArray();
        }

        public async Task UpdatePlayer(AsyncPlayer<V, A> player, S state)
        {
            var visible = _game.visible.Invoke(state).Invoke(player.Side);
            await player.Update(visible);
        }

        public async Task UpdatePlayers(S state)
        {
            foreach (AsyncPlayer<V, A> p in Players)
                await UpdatePlayer(p, state);
        }

        public async Task ErrorPlayer(AsyncPlayer<V,A> p,string error)
        {
            await p.Error(error);
        }

        public async Task Start()
        {
            S state = _game.init.Invoke(_playerNames);
            await UpdatePlayers(state);
            try
            {
                while (!_game.gameOver.Invoke(state))
                {
                    var (side, action) = await _action.Reader.ReadAsync(_cts.Token);
                    var ar = _game.advance.Invoke(state).Invoke(side).Invoke(action);
                    if (ar.TryOk(ref state))
                        await UpdatePlayers(state);
                    else
                    {
                        await ErrorPlayer(Players[side], ar.Message);
                        await UpdatePlayer(Players[side], state);
                    }
                }
                foreach (var p in Players)
                    p.End();
            }
            catch (System.Threading.Channels.ChannelClosedException)
            {
                await Console.Out.WriteLineAsync($"Game channel closed");
            }
            catch (System.OperationCanceledException)
            {
                await Console.Out.WriteLineAsync($"Game channel cancelled");
            }
            catch(Exception)
            {
                await Console.Out.WriteLineAsync($"Game channel exception");
            }
        }

        public void Stop()
        {
            _cts.Cancel();// this should kill game and player tasks by System.OperationCanceledException
        }
    }
}
