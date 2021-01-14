using System;

namespace pactheman_server
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new PacTheManClient())
                game.Run();
        }
    }
}
