namespace HuntShowdownBot.Types
{
    public class HuntPlayer
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int MMR { get; set; }

        public HuntPlayer(long id, string name, int mMR)
        {
            Id = id;
            Name = name;
            MMR = mMR;
        }
    }
}