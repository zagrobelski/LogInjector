using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace LogInjectorTest
{
    [TestClass]
    public class LogInjectorTest
    {
        private static readonly string sample_assembly_name = "SampleAssembly";
        private static readonly string sample_class_name = "SampleAssembly.SampleClass";
        private static System.Type sample_class_type = null;
        private static System.Object sample_class_instance = null;

        [ClassInitialize]
        public static void InjectAndLoadSampleAssembly( TestContext context )
        {
            string solution_dir = "../../../";
            string assembly_path = solution_dir + "SampleAssembly/bin/Debug/SampleAssembly.dll";
            string injected_assembly_path = solution_dir + "SampleAssembly/bin/Debug/SampleAssemblyInjected.dll";
            if ( System.IO.File.Exists( injected_assembly_path ) )
                System.IO.File.Delete( injected_assembly_path );
            string logger_assembly_path = solution_dir + "SampleLogger/bin/Debug/Logger.dll";
            string logger_type_name = "Logger.CallLogger";
            string before_call_log_method_name = "before";
            string log_attribute_name = "Logger.LogAttribute";
            LogInjector.LogInjector.Inject( assembly_path, injected_assembly_path, logger_assembly_path, logger_type_name, before_call_log_method_name, log_attribute_name );
            System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFrom( injected_assembly_path );
            sample_class_type = assembly.GetType( sample_class_name );
            Assert.IsNotNull( sample_class_type );
            sample_class_instance = System.Activator.CreateInstance( sample_class_type );
            Assert.IsNotNull( sample_class_instance );
        }

        private void TestSimpleTypes()
        {
            string method_name = "ProcessSimpleTypes";
            System.Reflection.MethodInfo method_info = sample_class_type.GetMethod( method_name );
            sbyte a = 11;
            short b = 22;
            int c = 33;
            long d = 44;
            byte e = 55;
            ushort f = 66;
            uint g = 77;
            ulong h = 88;
            float i = 99;
            double j = 111;
            bool k = true;
            char l = 'a';
            method_info.Invoke( sample_class_instance, new System.Object [] { a, b, c, d, e, f, g, h, i, j, k, l } );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( sample_class_name + "::" + method_name ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( a.ToString() ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( b.ToString() ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( c.ToString() ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( d.ToString() ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( e.ToString() ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( f.ToString() ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( g.ToString() ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( h.ToString() ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( i.ToString() ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( j.ToString() ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( k.ToString() ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( l.ToString() ) );
        }

        private void TestEnumTypes()
        {
            string method_name = "ProcessEnumTypes";
            System.Reflection.MethodInfo method_info = sample_class_type.GetMethod( method_name );
            System.Net.HttpStatusCode status_code = System.Net.HttpStatusCode.BadGateway;
            method_info.Invoke( sample_class_instance, new System.Object[] { status_code } );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( sample_class_name + "::" + method_name ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( status_code.ToString() ) );
        }

        private void TestStructTypes()
        {
            string method_name = "ProcessStructTypes";
            System.Reflection.MethodInfo method_info = sample_class_type.GetMethod( method_name );
            System.Guid guid = System.Guid.NewGuid();
            method_info.Invoke( sample_class_instance, new System.Object[] { guid } );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( sample_class_name + "::" + method_name ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( guid.ToString() ) );
        }

        private void TestValueTypes()
        {
            TestSimpleTypes();
            TestEnumTypes();
            TestStructTypes();
        }

        private void TestClassTypes()
        {
            string method_name = "ProcessClassTypes";
            System.Reflection.MethodInfo method_info = sample_class_type.GetMethod( method_name );
            System.Version version = new System.Version( "1.2.3.4" );
            method_info.Invoke( sample_class_instance, new System.Object[] { version } );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( sample_class_name + "::" + method_name ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( version.ToString() ) );
        }

        private void TestInterfaceTypes()
        {
            string method_name = "ProcessInterfaceTypes";
            System.Reflection.MethodInfo method_info = sample_class_type.GetMethod( method_name );
            System.Collections.Generic.IList<int> list = new System.Collections.Generic.List<int>();
            method_info.Invoke( sample_class_instance, new System.Object[] { list } );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( sample_class_name + "::" + method_name ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( list.ToString() ) );
        }

        private void TestDelegateTypes()
        {
            string method_name = "ProcessDelegateTypes";
            System.Reflection.MethodInfo method_info = sample_class_type.GetMethod( method_name );
            System.Func<int, bool> is_leap_year = System.DateTime.IsLeapYear;
            method_info.Invoke( sample_class_instance, new System.Object[] { is_leap_year } );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( sample_class_name + "::" + method_name ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( is_leap_year.ToString() ) );
        }

        private void TestArrayTypes()
        {
            string method_name = "ProcessArrayTypes";
            System.Reflection.MethodInfo method_info = sample_class_type.GetMethod( method_name );
            int[] int_array = new[] { 1, 2, 3, 4, 5 };
            method_info.Invoke( sample_class_instance, new System.Object[] { int_array } );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( sample_class_name + "::" + method_name ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( int_array.ToString() ) );
        }

        private void TestReferenceTypes()
        {
            TestClassTypes();
            TestInterfaceTypes();
            TestDelegateTypes();
            TestArrayTypes();
        }

        [TestMethod]
        public void TestTypes()
        {
            TestValueTypes();
            TestReferenceTypes();
        }

        [TestMethod]
        public void TestStaticMethod()
        {
            string method_name = "StaticMethod";
            System.Reflection.MethodInfo method_info = sample_class_type.GetMethod( method_name );
            int a = 111;
            method_info.Invoke( null, new System.Object[] { a } );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( sample_class_name + "::" + method_name ) );
            Assert.IsTrue( Logger.CallLogger.LastLog.Contains( a.ToString() ) );
        }
    }
}
