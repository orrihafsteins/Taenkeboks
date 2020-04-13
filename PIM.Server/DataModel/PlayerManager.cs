using PIM.Server.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PIM.Server.DataModel
{
    public class PlayerManager
    {
        public PlayerManager()
        {
            
        }
        public IdentityUser GetPlayer(UserManager<IdentityUser> userManager,string playerID)
        {
            return userManager.FindByIdAsync(playerID).Result;
        }
    }
}
