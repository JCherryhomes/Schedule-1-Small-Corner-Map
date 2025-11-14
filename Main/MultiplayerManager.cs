using S1API.Entities;

namespace Small_Corner_Map.Main
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
