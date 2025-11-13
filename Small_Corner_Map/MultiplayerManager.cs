using S1API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Small_Corner_Map
{
    internal class MultiplayerManager
    {
        private List<Player> players;
        private MinimapContent minimapContent;

        public MultiplayerManager(MinimapContent minimapContent)
        {
            // Initialization code here
            this.minimapContent = minimapContent;

            players = Player.All.Where(p => !p.IsLocal).ToList();
        }
    }
}
