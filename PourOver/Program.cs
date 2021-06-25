namespace PourOver
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using CsvHelper;
    using CsvHelper.Configuration;
    using Maroontress.Cui;

    /// <summary>
    /// The bootstrap class.
    /// </summary>
    public sealed class Program
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Program"/> class.
        /// </summary>
        /// <param name="args">
        /// The command-line options.
        /// </param>
        public Program(string[] args)
        {
            static void ShowUsageAndExit(Option o)
            {
                Usage(o.Schema, Console.Out);
                throw new TerminateProgramException(1);
            }

            static void ShowVersionAndExit()
            {
                Version(Console.Out);
                throw new TerminateProgramException(1);
            }

            static void SetUiCulture(string s)
            {
                Thread.CurrentThread.CurrentUICulture
                    = CultureInfo.GetCultureInfo(s);
            }

            var ignoreBlankField = false;
            var doIfVerbose = false;
            var schema = Options.NewSchema()
                .Add(
                    "culture",
                    'L',
                    "CULTURE",
                    "Specify culture (e.g., en-US)",
                    o => SetUiCulture(o.ArgumentValue))
                .Add(
                    "ignore-blank",
                    'b',
                    "Ignore blank field",
                    o => ignoreBlankField = true)
                .Add(
                    "verbose",
                    'v',
                    "Be verbose",
                    o => doIfVerbose = true)
                .Add(
                    "version",
                    'V',
                    "Show version and exit",
                    o => ShowVersionAndExit())
                .Add(
                    "help",
                    'h',
                    "Show this message and exit",
                    ShowUsageAndExit);

            var setting = schema.Parse(args);
            var arguments = setting.Arguments.ToArray();
            if (!(arguments.Length is 1))
            {
                Usage(schema, Console.Error);
                throw new TerminateProgramException(1);
            }
            Path = arguments[0];
            DoIfVerbose = doIfVerbose
                ? new Action<Action>(a => a())
                : a => {};
            IgnoreBlankField = ignoreBlankField
                ? new Func<string, bool>(s => false)
                : s => s.Length is 0;
        }

        private string Path { get; }

        private Action<Action> DoIfVerbose { get; }

        private Func<string, bool> IgnoreBlankField { get; }

        /// <summary>
        /// The entry point.
        /// </summary>
        /// <param name="args">
        /// The command line options.
        /// </param>
        public static void Main(string[] args)
        {
            try
            {
                var program = new Program(args);
                program.Launch();
                Environment.Exit(0);
            }
            catch (CsvHelper.MissingFieldException e)
            {
                var stderr = Console.Error;
                stderr.WriteLine(e.Message);
                Environment.Exit(1);
            }
            catch (OptionParsingException e)
            {
                var stderr = Console.Error;
                stderr.WriteLine(e.Message);
                Usage(e.Schema, stderr);
                Environment.Exit(1);
            }
            catch (TerminateProgramException e)
            {
                Environment.Exit(e.StatusCode);
            }
        }

        private static string GetDllName()
            => typeof(Program).Assembly.GetName().Name;

        private static void Version(TextWriter @out)
        {
            var dllName = GetDllName();
            var version = Assembly.GetEntryAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;
            @out.WriteLine($"{dllName} version {version}");
        }

        private static void Usage(OptionSchema schema, TextWriter @out)
        {
            var dllName = GetDllName();
            var all = new[]
            {
                $"usage: dotnet {dllName}.dll [Options]... [--] FILE.csv",
                "",
                "Options are:",
            };
            var lines = all.Concat(schema.GetHelpMessage());
            foreach (var m in lines)
            {
                @out.WriteLine(m);
            }
        }

        private void Print(Diagnostics d)
        {
            var m = d.Message;
            DoIfVerbose(() =>
            {
                m = $"({d.Kind}) {m}";
            });
            Console.WriteLine($"{Path}:{d.Row}: {d.Id}: {m}");
        }

        private void Launch()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
            };
            using var reader = new CsvReader(
                new StreamReader(Path, Encoding.UTF8),
                config);
            if (!reader.Read()
                || !reader.ReadHeader())
            {
                throw new IOException("can't read header row");
            }
            var drip = new Drip(reader, Print, IgnoreBlankField);
            drip.Start();
        }
    }
}
