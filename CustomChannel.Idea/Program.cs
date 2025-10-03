namespace CustomChannel.Idea
{
    using System.Threading.Tasks;

    // Чтобы запустить, вызовите SimplifiedChannelDemo.Run() из Main
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await SimplifiedChannelDemo.Run();
            await ChannelDemo.Run();
        }
    }    
}