namespace LogAnalyser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch (arg)
                {
                    case "--help":
                        HelpMenu();
                        break;
                    //TODO Usage Paramaters
                }
            }
        }

        #region Menu Prints

        private static void HelpMenu()
        {
            Console.WriteLine($"""
            This is the help menu for SSDHash. Below are the command-line arguments available:
            -h|--help : Help Manu,


            """);
        }

        #endregion
    }
}