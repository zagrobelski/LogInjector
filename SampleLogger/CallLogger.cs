
namespace Logger
{
    public static class CallLogger
    {
        public static string LastLog { get; private set; }

        public static void before( string method_name, string[] arguments )
        {
            System.Text.StringBuilder log_entry = new System.Text.StringBuilder();
            log_entry.Append( string.Format( "{0}( ", method_name ) );
            foreach ( var arg in arguments )
            {
                string arg_string = ( arg != null ) ? arg : "null";
                log_entry.Append( string.Format( "{0}, ", arg_string ) );
            }
            log_entry.Remove( log_entry.Length - 2, 2 ); 
            log_entry.Append( " )" );
            string log = log_entry.ToString();
            System.Console.WriteLine( log );
            LastLog = log;
        }

        public static void after()
        {
        }
    };
}
