using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace BDInfo {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static int Main(string[] args) {
            var app = new CommandLineApplication();
            app.HelpOption();

            var listOption = app.Option("-l|--list", "Print the list of playlists", CommandOptionType.SingleOrNoValue);
            var mplsOption = app.Option("-m|--mpls <PLAYLISTS>", "Comma separated list of playlists to scan.",
                CommandOptionType.SingleValue);
            var wholeOption = app.Option("-w|--whole", "Scan whole disc - every playlist",
                CommandOptionType.SingleOrNoValue);
            var versionOption = app.Option("-v|--version", "Print the version", CommandOptionType.SingleOrNoValue);
            var jsonOption = app.Option("-j|--json", "Generate the report in JSON format",
                CommandOptionType.SingleOrNoValue);
            var reportOption = app.Option("-o|--output <OUTPUT_FILE>", "File to write report to", CommandOptionType.SingleValue);

            var bdPath = app.Argument("BDPath", "Path to BD").IsRequired();

            app.OnExecute(() => {
                var whole = wholeOption.HasValue();
                var list = listOption.HasValue();
                var json = jsonOption.HasValue();
                if (list) {
                    whole = true;
                }

                if (!File.Exists(bdPath.Value) && !Directory.Exists(bdPath.Value)) {
                    Console.Error.WriteLine(String.Format("error: {0} does not exist", bdPath.Value));
                    return -1;
                }

                if (reportOption.HasValue()) {
                    var path = reportOption.Value();
                    if (Directory.Exists(path)) {
                        Console.Error.WriteLine("Value for --output cannot be a directory");
                        return -1;
                    }
                    if (new[]{Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}.Any(x => path.EndsWith(x))) {
                        Console.Error.WriteLine("Value for --output cannot end with a path separator");
                        return -1;
                    }
                }

                System.Console.Error.WriteLine("Please wait while we scan the disc...");
                DoWorkEventArgs eventArgs = new DoWorkEventArgs(bdPath.Value);
                var main = new FormMain();

                main.InitBDROMWork(null, eventArgs);

                if (mplsOption.HasValue()) {
                    var selectedPlaylists = mplsOption.Value().Split(',').ToList();
                    main.LoadPlaylists(selectedPlaylists);
                } else if (whole) {
                    var playlists = main.LoadPlaylists(true);
                    if (list) {
                        try {
                            if (json) {
                                GenerateOutput(reportOption, JsonConvert.SerializeObject(playlists, Formatting.Indented));
                                return 0;
                            } else {
                                GenerateOutput(reportOption, playlists.ToString());
                                return 0;
                            }
                        } catch (Exception e) {
                            Console.Error.WriteLine(e);
                            return -1;
                        }
                    }
                }

                main.ScanBDROMWork(null, null);
                try {
                    var report = main.GenerateReport();

                    if (jsonOption.HasValue()) {
                        GenerateOutput(reportOption, JsonConvert.SerializeObject(report, Formatting.Indented, new JsonSerializerSettings { 
                                NullValueHandling = NullValueHandling.Ignore
                            }));
                    } else {
                        GenerateOutput(reportOption, report.ToString());
                    }
                }
                catch (Exception e) {
                    Console.Error.WriteLine(e);
                    return -1;
                }


                return 0;
            });

            return app.Execute(args);
        }

        static void GenerateOutput(CommandOption reportPath, string output) {
            if (reportPath.HasValue()) {
                var path = reportPath.Value();
                var dir = Path.GetDirectoryName(path);
                Directory.CreateDirectory(dir);
                using (var w = File.CreateText(path)) {
                    w.WriteLine(output);
                }
            } else {
                System.Console.WriteLine(output);
            }
        }
    }
}