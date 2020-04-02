using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Options;
using MonoMod;
namespace Publicizer
{
	class Program
	{
		static bool TypeFlag, FieldFlag, EventFlag, PropertyFlag, MethodFlag, AllFlags;
		static string OutputFileName = null;
		static OptionSet options = new OptionSet
		{
			{ "t|types","publicize types.",v=>{ if(v !=null) TypeFlag = true; } },
			{ "f|fields","publicize fields.",v=>{ if(v !=null) FieldFlag = true; } },
			{ "e|events","publicize events.",v=>{ if(v !=null) EventFlag = true; } },
			{ "p|properties","publicize properties.",v=>{ if(v !=null) PropertyFlag = true; } },
			{ "m|methods","publicize methods.",v=>{ if(v !=null) MethodFlag = true; } },
			{ "o|output=","Set the ouputfile.",v=>OutputFileName = v},
			{ "h|help","write help.",v=>{ if(v !=null) {
					ShowHelpAndExit(0);
				} 
			} },
		};

		static void Main(string[] args)
		{
			string targetFile = null;
			try
			{
				targetFile = string.Join(" ",options.Parse(args));
			}
			catch(OptionException e)
			{
				Console.WriteLine("Exception parsing args: ");
				Console.WriteLine(e.Message);
				ShowHelpAndExit(1);
			}
			AllFlags = !(TypeFlag || FieldFlag || EventFlag || PropertyFlag || MethodFlag);
			VerifyNeededFilePath(targetFile, true, "input file");


			if (OutputFileName == null)
				OutputFileName = targetFile.Replace(".dll", ".Public.dll");
			VerifyNeededFilePath(targetFile, false, "output file");

			using (MonoModder mm = new MonoModder()
			{
				InputPath = targetFile,
				OutputPath = OutputFileName,
				ReadingMode = ReadingMode.Deferred
			})
			{
				mm.Read();
				mm.MapDependencies();

				foreach (var typeDefinition in mm.Module.Types)
				{

					var currentTypes = new Queue<TypeDefinition>();
					var currentType = typeDefinition;

					do {
						if (AllFlags || TypeFlag)
							currentType.SetPublic(currentType.IsPublic);

						if (AllFlags || FieldFlag)
						{
							foreach (var field in currentType.Fields)
							{
								// Ignore compiler generated fields for now,
								// Maybe check if there is an event with the same name instead
								if (field.CustomAttributes.Any(c => c.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName))
								{
									continue;
								}

								field.SetPublic(true);
							}
						}

						if (AllFlags || EventFlag)
						{
							foreach (var ev in currentType.Events)
							{
								ev.SetPublic(true);
							}
						}

						if (AllFlags || PropertyFlag)
						{
							foreach (var prop in currentType.Properties)
							{
								prop.SetPublic(true);
							}
						}

						if (AllFlags || MethodFlag)
						{
							foreach (var method in currentType.Methods)
							{
								method.SetPublic((true));
							}
						}
						
						if (currentType.HasNestedTypes) {
							foreach (var type in currentType.NestedTypes) {
								currentTypes.Enqueue(type);
							}
						}

						currentType = currentTypes.Count > 0 ? currentTypes.Dequeue() : null;
					} while (currentType != null);
				}
				
				mm.Module.Write(OutputFileName);
			}

			Console.WriteLine("Finished publicizing code...");
		}

		static void ShowHelpAndExit(int code)
		{
			Console.WriteLine("Usage: dotnet Publicizer.dll [FLAGS] targetDll");
			Console.WriteLine("Publicize the specified assembly. Outputs a targetDLL.pulic.dll");
			Console.WriteLine("If no flags enabled, all flags are assumed. The following flags exist:");
			options.WriteOptionDescriptions(Console.Out);
			Environment.Exit(code);
		}

		static void VerifyNeededFilePath(string targetFile, bool FileMustExist, string errorFileIdentifier)
		{
			FileInfo file = null;
			try
			{
				file = new FileInfo(string.Join(" ", targetFile));
				if (file == null)
					throw new ArgumentNullException("FileInfo gave null!");
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception parsing args: ");
				Console.WriteLine(e.Message);
				ShowHelpAndExit(1);
			}

			if (FileMustExist && file.Exists == false)
			{
				Console.WriteLine($"Specified {errorFileIdentifier} does not exist.");
				Environment.Exit(1);
			}
		}
	}


}
