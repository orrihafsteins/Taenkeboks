using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Taenkeboks;

namespace Taenkeboks.Async
{
    public class AsyncCpuPlayer
    {
        AsyncPlayer<TbVisible, TbAction> _p;
        Func<TbVisible, TbAction> _policy;
        public AsyncCpuPlayer(AsyncPlayer<TbVisible, TbAction> p,TbGameSpec spec)
        {
            _policy = TbAiPlayerModule.createPolicy(spec,p.Name).Invoke;
            _p = p;
        }

        public async Task Start()
        {
            try
            {
                var v = await _p.Next();
                while (v.status.inPlay)
                {
                    if (v.nextPlayer == _p.Side)
                    {
                        var a = _policy(v);
                        await _p.PerformAction(a);
                    }
                    v = await _p.Next();
                }
            }
            catch (System.Threading.Channels.ChannelClosedException e)
            {
                await Console.Out.WriteLineAsync($"{_p.Name} channel closed");
            }
            catch (System.OperationCanceledException e)
            {
                await Console.Out.WriteLineAsync($"{_p.Name} channel cancelled");
            }
            catch (Exception e)
            {
                await Console.Out.WriteLineAsync($"{_p.Name} channel exception");
            }
        }
    }
}
