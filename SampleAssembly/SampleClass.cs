

namespace SampleAssembly
{
    [Logger.Log]
    public class SampleClass
    {
        public void ProcessSimpleTypes(sbyte a, short b, int c, long d, byte e, ushort f, uint g, ulong h, float i, double j, bool k, char l )
        { }

        public void ProcessEnumTypes(System.Net.HttpStatusCode status_code)
        { }

        public void ProcessStructTypes( System.Guid guid )
        { }

        public void ProcessClassTypes( System.Version version )
        { }

        public void ProcessInterfaceTypes( System.Collections.Generic.IList<int> list )
        { }

        public void ProcessDelegateTypes( System.Func<int, bool> is_leap_year )
        { }

        public void ProcessArrayTypes( int[] int_array )
        { }

        public static void StaticMethod( int a )
        { }
    }
}
