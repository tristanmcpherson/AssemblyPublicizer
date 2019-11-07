using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using MonoMod;
using MonoMod.Utils;

namespace Publicizer
{
	class Program
	{
		static void Main(string[] args)
		{
			string basePath = Directory.GetCurrentDirectory();
			string projectFolder = new DirectoryInfo(basePath).Parent.Parent.Parent.FullName;
			string dllPath = Path.Combine(projectFolder, "Managed\\Assembly-CSharp.dll");

			using (MonoModder mm = new MonoModder()
			{
				InputPath = dllPath,
				OutputPath = dllPath.Replace(".dll", "Public.dll"),
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
						currentType.SetPublic(currentType.IsPublic);


						foreach (var field in currentType.Fields) {
							// Ignore compiler generated fields for now,
							// Maybe check if there is an event with the same name instead
							if (field.CustomAttributes.Any(c => c.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName)) {
								continue;
							}

							field.SetPublic(true);
						}

						foreach (var ev in currentType.Events) {
							ev.SetPublic(true);
						}

						foreach (var prop in currentType.Properties) {
							prop.SetPublic(true);
						}

						foreach (var method in currentType.Methods) {
							method.SetPublic((true));
						}
						



						if (currentType.HasNestedTypes) {
							foreach (var type in currentType.NestedTypes) {
								currentTypes.Enqueue(type);
							}
						}

						currentType = currentTypes.Count > 0 ? currentTypes.Dequeue() : null;
					} while (currentType != null);
				}
				
				mm.Module.Write(dllPath.Replace(".dll", ".Public.dll"));
			}

			Console.WriteLine("Finished publicizing code...");
		}
	}
}
