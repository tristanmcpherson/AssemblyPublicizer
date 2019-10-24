using System.Linq;
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
			string dllname = @".\Managed\Assembly-CSharp.dll";

			using (MonoModder mm = new MonoModder()
			{
				InputPath = dllname,
				OutputPath = dllname.Replace(".dll", "Public.dll"),
				ReadingMode = ReadingMode.Deferred
			})
			{
				mm.Read();
				mm.MapDependencies();

				foreach (var typeDefinition in mm.Module.Types)
				{
					typeDefinition.SetPublic(true);
					foreach (var field in typeDefinition.Fields)
					{
						// Ignore compiler generated fields for now,
						// Maybe check if there is an event with the same name instead
						if (field.CustomAttributes.Any(c => c.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName)) {
							continue;
						}

						field.SetPublic(true);
					}

					foreach (var ev in typeDefinition.Events) {
						ev.SetPublic(true);
					}

					foreach (var prop in typeDefinition.Properties) {
						prop.SetPublic(true);
					}

					foreach (var method in typeDefinition.Methods)
					{
						method.SetPublic((true));
					}
				}

				mm.Module.Write(dllname.Replace(".dll", ".Public.dll"));
			}
		}
	}
}
