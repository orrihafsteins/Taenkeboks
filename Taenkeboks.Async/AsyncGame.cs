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
        CancellationToken _ct;
        public AsyncPlayer<V,A>[] Players { get; private set; }
        public AsyncGame(Game<S, A, V> game,string[] playerNames, CancellationToken ct)
        {
            _ct = ct;
            _game = game;
            _playerNames = playerNames;
            _action = Channel.CreateUnbounded<(int,A)> ();
            Players =playerNames.Select((n, i) => new AsyncPlayer<V,A>(n, i, _action.Writer,ct)).ToArray();
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
                    var (side, action) = await _action.Reader.ReadAsync(_ct);
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
            catch (System.Threading.Channels.ChannelClosedException e)
            {
                await Console.Out.WriteLineAsync($"Game channel closed");
            }
            catch (System.OperationCanceledException e)
            {
                await Console.Out.WriteLineAsync($"Game channel cancelled");
            }
            catch(Exception e)
            {
                await Console.Out.WriteLineAsync($"Game channel exception");
            }
        }
    }
}
