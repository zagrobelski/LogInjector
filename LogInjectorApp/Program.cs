using System.Linq;

namespace LogInjectorApp
{
    class Program
    {
        static void Main( string[] args )
        {
            string input_assembly_path = args[ 0 ];
            string output_assembly_path = args[ 1 ];
            string logger_assembly_path = args[ 2 ];
            string logger_type_name = args[ 3 ];
            string before_call_log_method_name = args[ 4 ];
            string log_attribute_name = args[ 5 ];

            LogInjector.LogInjector.Inject( input_assembly_path, output_assembly_path, logger_assembly_path, logger_type_name, before_call_log_method_name, log_attribute_name );
        }
    }
}
