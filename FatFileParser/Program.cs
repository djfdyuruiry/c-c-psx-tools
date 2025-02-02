﻿using static System.Console;

using CommandLine;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

using CncPsxLib;

namespace FatFileParser
{ 
    internal static class Program
    {
        private static async Task OutputFatEntriesAsTable(List<FatFileEntry> fileEntries)
        {   
            await Out.WriteLineAsync(
                @"┌──────────────┬────────────────┬──────────────┐
                  │ File Name    │ Offset         │ Size         │
                  ├──────────────┼────────────────┼──────────────┤".StripLeadingWhitespace()
            );

            foreach (var entry in fileEntries)
            {
                await Out.WriteLineAsync(
                    $"│ {entry.FileName,-12} " +
                    $"│ 0x{entry.HexOffsetInBytes}     " +
                    $"│ {entry.SizeInBytes.FormatAsByteUnit(),-12} │"
                );
            }

            await Out.WriteLineAsync("└──────────────┴────────────────┴──────────────┘");
        }
 
        private static async Task OutputFatFileAsTable(FatFile fatFile)
        {
            await Out.WriteLineAsync($"File Path:       {fatFile.Path}");
            await Out.WriteLineAsync($"MIX Entry Count: {fatFile.MixEntryCount}");
            await Out.WriteLineAsync($"XA Entry Count:  {fatFile.XaEntryCount}\n");

            await Out.WriteLineAsync("******************* MIX Files ******************\n");
            await OutputFatEntriesAsTable(fatFile.MixFileEntries);

            await Out.WriteLineAsync("\n******************* XA Files *******************\n");
            await OutputFatEntriesAsTable(fatFile.XaFileEntries);
        }

        private static async Task<int> Run(CliOptions opts)
        {
            try
            {
                var fileReader = new FatFileReader();
                var fatFile = await fileReader.Read(opts.FatFilePath);

                if (opts.OutputYaml)
                {
                    var yamlSerializer = new SerializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();

                    await Out.WriteLineAsync(
                        yamlSerializer.Serialize(fatFile)
                    );
                }
                else
                {
                    await OutputFatFileAsTable(fatFile);
                }
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync(e.Message);
                return -1;
            }

            return 0;
        }

        private static async Task<int> Main(string[] args) => 
            await Parser.Default
                .ParseArguments<CliOptions>(args)
                .MapResult(
                    Run,
                    errs => Task.FromResult(-1)
                );
    }}
