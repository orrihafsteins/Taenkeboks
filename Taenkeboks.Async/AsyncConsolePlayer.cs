using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Taenkeboks;

namespace Taenkeboks.Async
{
    class AsyncConsolePlayer
    {
        AsyncPlayer<TbVisible,TbAction> _p;
        public AsyncConsolePlayer(AsyncPlayer<TbVisible, TbAction> p)
        {
            _p = p;
        }
        public async Task RunPlayer()
        {
            try { 
                var v = await _p.Next();
                while (v.status.inPlay)
                {
                    TbConsole.updatePlayer(_p.Name, v);
                    if(v.nextPlayer == _p.Side) 
                    {
                        var a = TbConsole.getPlayerMove(_p.Name, v); // TODO: This blocks, implement as await
                        await _p.PerformAction(a);
                    }
                    v = await _p.Next();
                }
                TbConsole.updatePlayer(_p.Name, v);
            }
            catch (System.Threading.Channels.ChannelClosedException)
            {
                await Console.Out.WriteLineAsync($"{_p.Name} channel closed");
            }
            catch (System.OperationCanceledException)
            {
                await Console.Out.WriteLineAsync($"{_p.Name} channel cancelled");
            }
            catch (System.Exception)
            {
                await Console.Out.WriteLineAsync($"{_p.Name} channel exception");
            }
        }
    }
}
