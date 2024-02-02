namespace HuntShowdownBot.Types
{
    public class HuntTeam
    {
        public int Id { get; set; }

        public List<HuntPlayer> Players { get; set; }

        public int MMR { get; set; }

        public HuntTeam(int id, List<HuntPlayer> players, int mMR)
        {
            Id = id;
            Players = players;
            MMR = mMR;
        }

    }
}
