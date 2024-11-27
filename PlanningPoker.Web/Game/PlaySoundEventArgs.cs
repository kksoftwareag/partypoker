namespace PlanningPoker.Web.Game
{
    public class PlaySoundEventArgs
    {
        public string SoundName { get; }

        public PlaySoundEventArgs(string soundName)
        {
            this.SoundName = soundName;
        }
    }
}
