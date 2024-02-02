namespace HuntShowdownBot.Types
{
    public class HuntMatch
    {
        public List<HuntTeam> Teams { get; set; }

        public HuntMatch(List<HuntTeam> teams)
        {
            Teams = teams;
        }
    }
}
