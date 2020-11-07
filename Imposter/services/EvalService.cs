using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Mummybot.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Mummybot.Services
{
    public class EvalService : BaseService
    {
        public List<string> usings = JsonConvert.DeserializeObject<string[]>(File.ReadAllText("usings.json")).ToList();

        public EvalService()
        {
        }

        public void SaveUsings()
            => File.WriteAllText("usings.json",JsonConvert.SerializeObject(usings.ToArray()));

        public Script<object> Build(string code)
        {
            var codes = Utilities.GetCodes(code);

            var namespaces = Assembly.GetEntryAssembly()?.GetTypes()
              .Where(x => !string.IsNullOrWhiteSpace(x.Namespace))
              .Select(x => x.Namespace)
              .Distinct();

            var scriptOptions = ScriptOptions.Default.WithReferences(GetAssemblies().Select(x => MetadataReference.CreateFromFile(x.Location))).AddImports(namespaces);

            var toEval = codes.Count == 0 ? code : string.Join('\n', codes);

            return CSharpScript
                .Create($"{string.Concat(usings.Select(x => $"using {x};" + Environment.NewLine))} {toEval}",
                    scriptOptions,
                    typeof(RoslynContext));
        }

        internal IEnumerable<Assembly> GetAssemblies()
        {
            var assm = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location));

            foreach (var assembly in assm)
                yield return assembly;
        }
    }

    public class RoslynContext
    {
        public MummyContext Context;
        public IServiceProvider Services { get; set; }

        public RoslynContext(MummyContext context, IServiceProvider services)
        {
            Context = context;
            Services = services;
        }
    }
}
