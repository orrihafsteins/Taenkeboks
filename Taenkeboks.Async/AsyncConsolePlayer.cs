using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Taenkeboks;

namespace Taenkeboks.AsyncConsole
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
        }
    }
}
