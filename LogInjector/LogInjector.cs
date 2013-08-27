using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogInjector
{
    public class LogInjector
    {
        public static void Inject( string input_assembly_path, string output_assembly_path, string logger_assembly_path, string logger_type_name, string before_call_log_method_name, string log_attribute_name )
        {
            System.Diagnostics.Debug.Assert( System.IO.File.Exists( input_assembly_path ) );

            if ( input_assembly_path != output_assembly_path )
            {
                if ( System.IO.File.Exists( output_assembly_path ) )
                    System.IO.File.Delete( output_assembly_path );
                System.IO.File.Copy( input_assembly_path, output_assembly_path );
                System.Diagnostics.Debug.Assert( System.IO.File.Exists( output_assembly_path ) );
            }

            var assembly_resolver = new Mono.Cecil.DefaultAssemblyResolver();
            string input_assembly_directory = System.IO.Path.GetDirectoryName( input_assembly_path );
            assembly_resolver.AddSearchDirectory( input_assembly_directory );
            #if SILVERLIGHT
            Microsoft.Silverlight.Build.Tasks.GetSilverlightFrameworkPath path_task = new Microsoft.Silverlight.Build.Tasks.GetSilverlightFrameworkPath();
            path_task.RegistryBase = "Software\\Microsoft\\Microsoft SDKs\\Silverlight";
            path_task.Execute();
            assembly_resolver.AddSearchDirectory( path_task.SilverlightPath );
            foreach ( string path in path_task.SilverlightSDKPaths )
                assembly_resolver.AddSearchDirectory( path );
            #endif
            Mono.Cecil.AssemblyDefinition injectible_assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly( output_assembly_path, new Mono.Cecil.ReaderParameters { AssemblyResolver = assembly_resolver } );

            System.Diagnostics.Debug.Assert( System.IO.File.Exists( logger_assembly_path ) );
            Mono.Cecil.AssemblyDefinition logger_assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly( logger_assembly_path );
            Mono.Cecil.TypeDefinition logger_type = logger_assembly.MainModule.GetType( logger_type_name );
            System.Diagnostics.Debug.Assert( logger_type != null );
            Mono.Cecil.MethodDefinition before_callback_method_info = logger_type.Methods.First( m => m.Name == before_call_log_method_name );
            Mono.Cecil.MethodReference before_callback_reference = injectible_assembly.MainModule.Import( before_callback_method_info );

            //make sure to get System.Object.ToString method from assembly compatible with the injected assembly
            #if SILVERLIGHT
            string core_assembly_path = path_task.SilverlightPath + "mscorlib.dll";
            Mono.Cecil.AssemblyDefinition core_assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly( core_assembly_path );
            Mono.Cecil.TypeDefinition object_type = core_assembly.MainModule.GetType( "System.Object" );
            Mono.Cecil.MethodDefinition to_string_method_info = object_type.Methods.First( m => m.Name == "ToString" );
            #else
            System.Reflection.MethodInfo to_string_method_info = typeof( System.Object ).GetMethod( "ToString" );
            #endif
            Mono.Cecil.MethodReference to_string_reference = injectible_assembly.MainModule.Import( to_string_method_info );

            foreach ( Mono.Cecil.TypeDefinition type_definition in injectible_assembly.MainModule.Types )
            {
                bool is_type_logable = type_definition.CustomAttributes.Any( a => a.AttributeType.FullName == log_attribute_name );
                foreach ( Mono.Cecil.MethodDefinition method_definition in type_definition.Methods )
                {
                    bool is_method_logable = is_type_logable || method_definition.CustomAttributes.Any( a => a.AttributeType.FullName == log_attribute_name );

                    if ( is_method_logable )
                    {
                        Mono.Cecil.Cil.ILProcessor processor = method_definition.Body.GetILProcessor();

                        System.Collections.Generic.List<Mono.Cecil.Cil.Instruction> original_instructions = new System.Collections.Generic.List<Mono.Cecil.Cil.Instruction>();
                        original_instructions.AddRange( method_definition.Body.Instructions );
                        method_definition.Body.Instructions.Clear();

                        #region parameters

                        int method_parameters_count = method_definition.Parameters.Count;
                        int local_variables_count = method_definition.Body.Variables.Count;
                        int arguments_array_index = local_variables_count;

                        // Create an array of type System.String with the same number of elements as count of method parameters
                        // Add metadata for a new variable of type System.String[] to method body
                        // .locals init (System.String[] V_0)
                        Mono.Cecil.ArrayType arguments = new Mono.Cecil.ArrayType( injectible_assembly.MainModule.TypeSystem.String );
                        method_definition.Body.Variables.Add( new Mono.Cecil.Cil.VariableDefinition( ( Mono.Cecil.TypeReference ) arguments ) );
                        method_definition.Body.InitLocals = true;
                        method_definition.Body.Instructions.Add( processor.Create( Mono.Cecil.Cil.OpCodes.Ldc_I4, method_parameters_count ) );
                        method_definition.Body.Instructions.Add( processor.Create( Mono.Cecil.Cil.OpCodes.Newarr, injectible_assembly.MainModule.TypeSystem.String ) );
                        // This instruction will store the address of the newly created array in the newly added local variable, which is at index = local_variables_count
                        method_definition.Body.Instructions.Add( processor.Create( Mono.Cecil.Cil.OpCodes.Stloc, arguments_array_index ) );

                        #region parameters_to_string

			// Instance methods have an an implicit argument called "this"
                        // so in that case we need to refer to actual arguments with +1 position
			int parameter_offset = method_definition.IsStatic ? 0 : 1;
                        for ( int i = 0; i < method_parameters_count; ++i )
                        {
                            // load argument array and current index
                            method_definition.Body.Instructions.Add( processor.Create( Mono.Cecil.Cil.OpCodes.Ldloc, arguments_array_index ) );
                            method_definition.Body.Instructions.Add( processor.Create( Mono.Cecil.Cil.OpCodes.Ldc_I4, i ) );

                            // load argument
                            method_definition.Body.Instructions.Add( processor.Create( Mono.Cecil.Cil.OpCodes.Ldarga, i + parameter_offset ) );

                            // convert argument to string
                            Mono.Cecil.TypeReference argument_type = method_definition.Parameters[ i ].ParameterType;

                            method_definition.Body.Instructions.Add( processor.Create( Mono.Cecil.Cil.OpCodes.Constrained, argument_type ) );
                            method_definition.Body.Instructions.Add( processor.Create( Mono.Cecil.Cil.OpCodes.Callvirt, to_string_reference ) );

                            // store string in array
                            method_definition.Body.Instructions.Add( processor.Create( Mono.Cecil.Cil.OpCodes.Stelem_Ref ) );
                        }

                        #endregion parameters_to_string

                        string method_signature = method_definition.ToString();
                        // load method signature
                        method_definition.Body.Instructions.Add( processor.Create( Mono.Cecil.Cil.OpCodes.Ldstr, method_signature ) );

                        // load parameters array
                        method_definition.Body.Instructions.Add( processor.Create( Mono.Cecil.Cil.OpCodes.Ldloc, arguments_array_index ) );

                        #endregion parameters

                        // load before call instruction
                        method_definition.Body.Instructions.Add( processor.Create( Mono.Cecil.Cil.OpCodes.Call, before_callback_reference ) );

                        foreach ( var IL in original_instructions )
                            method_definition.Body.Instructions.Add( IL );
                    }
                }
            }

            injectible_assembly.Write( output_assembly_path );
        }
    }
}
