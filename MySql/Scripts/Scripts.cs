using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LightestNight.System.EventSourcing.Checkpoints.MySql.Scripts
{
    internal class Scripts
    {
        private readonly ConcurrentDictionary<string, string> _scripts = new ConcurrentDictionary<string,string>();

        internal string CreateSchema => GetScript();
        internal string SetCheckpoint => GetScript();
        internal string GetCheckpoint => GetScript();
        internal string DeleteCheckpoint => GetScript();

        private string GetScript([CallerMemberName] string? name = default)
            => _scripts.GetOrAdd(name ?? string.Empty,
                (key, assembly) =>
                {
                    using var stream =
                        assembly.GetManifestResourceStream(
                            $"LightestNight.System.EventSourcing.Checkpoints.MySql.Scripts.{key}.sql");
                    if (stream == null)
                        throw new FileNotFoundException($"Embedded resource '{key}' was not found.");

                    using var reader = new StreamReader(stream);
                    return reader.ReadToEnd().Replace("__table-name__", Constants.TableName);
                }, typeof(Scripts).GetTypeInfo().Assembly);
    }
}