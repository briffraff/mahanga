using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using mahanga.Core.Interface;
using mahanga.Global;

namespace mahanga.Core
{
    public class Engine : IEngine
    {
        ////    INTIALIZING
        private readonly Constants _gc = new Constants();
        private readonly Stopwatch _watch = new Stopwatch();
        private readonly Printing _print = new Printing();
        private readonly Service _service = new Service();
        private readonly Debug _debug = new Debug();

        public void Run()
        {
            //// Empty hash 
            HashSet<string> allFiles = new HashSet<string>();
            HashSet<string> allFootwears = new HashSet<string>();

            //// Enumeration options
            var options = _service.EnumOptions();

            string toolResultsPath = "";
            var categoriesApparelInfo = new Dictionary<string, List<string>>();
            var categoriesFTWInfo = new Dictionary<string, List<string>>();

            string exeLocation = _service.CheckExeLocation();
            string resultsLocation = exeLocation + _gc.toolResultsPath;
            
            _service.CheckAndResetWindowSize();
            

            if (Console.BackgroundColor == ConsoleColor.Black)
            {
                _service.ChangeColor("Black");
                _service.ChangeTextColor("Gray");
                Console.Clear();
            }

            //// tool header
            _service.ChangeTextColor("Green");
            _print.ToolHeader(_gc.toolName, _gc.toolDescription, _gc.toolVersion);
            _service.ChangeTextColor("White");
            _print.NewLine();
            ////-------------------INTIALIZING ENDS HERE !!!

            // init folders and chose file type
            Console.WriteLine("Choose the file type: ");
            Console.WriteLine("1: [psd,psb]");
            Console.WriteLine("2: [jpg]");
            Console.WriteLine("3: [png]");
            Console.WriteLine("4: [jpeg]");
            var fileType = Console.ReadLine();
            var folderName = _service.chooseTypeToWorkWith(fileType);
            _service.CreateFolders(resultsLocation, folderName);
            fileType = "*." + folderName;

            _service.ChangeTextColor("DarkRed");

            //// DEBUG
            if (_debug.mode == true)
            {
                toolResultsPath = _debug.scriptsFolder + _debug.toolResultsPath;

                categoriesApparelInfo = new Dictionary<string, List<string>>()
                {
                    //// category name = [category NAME, PATH to folder, EXTENSION TO SEARCH]
                    [nameof(_debug.path)] = new List<string>() { nameof(_debug.path), _debug.path, _debug.psdPattern },
                };
            }
            else
            {
                toolResultsPath = _gc.scriptsFolder + _gc.toolResultsPath;

                categoriesApparelInfo = new Dictionary<string, List<string>>()
                {
                    //// category name = [category NAME, PATH to folder, EXTENSION TO SEARCH]
                    ////[nameof(_gc.temp)] = new List<string>() { nameof(_gc.temp), _gc.temp, _gc.psdPattern },
                    [_gc.MWEQ] = new List<string>() { _gc.MWEQ, _gc.pathRootMWEQ, fileType },
                    [_gc.MA] = new List<string>() { _gc.MA, _gc.pathRootMA, fileType },
                    [_gc.PS] = new List<string>() { _gc.PS, _gc.pathRootPlusSize, fileType },
                    [_gc.YA] = new List<string>() { _gc.YA, _gc.pathRootYA, fileType },
                    [_gc.YAPS] = new List<string>() { _gc.YAPS, _gc.pathRootYAPS, fileType },
                };

                categoriesFTWInfo = new Dictionary<string, List<string>>()
                {
                    //// category name = [category NAME, PATH to folder, EXTENSION TO SEARCH]
                    [_gc.FTW] = new List<string>() { _gc.FTW, _gc.pathRootFTW, fileType },
                };

                if (folderName == "psd")
                {
                    categoriesApparelInfo[nameof(_gc.fullRangePSBfiles)] = new List<string>()
                        { nameof(_gc.fullRangePSBfiles), _gc.fullRangePSBfiles, _gc.psbPattern };
                }
            }

            ////  DEBUG - check categoriesApparelInfo object
            //debug.CheckCategoriesInfoObject(categoriesApparelInfo);

            ////    SCAN THE FILES
            ////Idle message
            Console.WriteLine("SCANNING SERVER ...\n");
            _watch.Reset();
            _watch.Start();
            _print.NewLine();

            ////Parralel async - get files
            var files = _service.GetAllFilesParralelAsync(categoriesApparelInfo, options);
            var footwearFiles = _service.GetAllFilesParralelAsync(categoriesFTWInfo, options);

            while (true)
            {
                var isCompleted = files.IsCompleted;
                var isFootwearCompleted = footwearFiles.IsCompleted;
                if (!isCompleted || !isFootwearCompleted)
                {
                    continue;
                }
                break;
            }

            allFiles.UnionWith(files.Result);
            allFootwears.UnionWith(footwearFiles.Result);

            _print.NewLine();
            Console.WriteLine("--SCANNING FINISHED !");
            ////-------------------SCAN ENDS HERE !!!


            ////    PRINT SCAN
            _service.ChangeTextColor("Yellow");
            _watch.Stop();
            _print.NewLine();
            _print.PrintingScanResults(_watch, allFiles, allFootwears);
            _watch.Reset();
            _print.NewLine();
            ////-------------------PRINT SCAN INFO ENDS HERE !!!


            //// Q?=>
            _service.ChangeTextColor("Yellow");
            _print.NewLine();
            Console.WriteLine("DO YOU WANT TO CHECK TWEEDLEDUMS !? - (y / n)");
            _service.ChangeTextColor("White");
            string answerInput = Console.ReadLine();
            _service.isApproved(answerInput);
            ////----------Q?=>>


            ////    UPDATE DATABASE
            Dictionary<string, HashSet<Texture>> dbApparel = new Dictionary<string, HashSet<Texture>>();
            Dictionary<string, HashSet<Texture>> dbFtw = new Dictionary<string, HashSet<Texture>>();

            //////work with allFiles
            UpdateInternalDataBase(dbApparel, allFiles);
            UpdateInternalDataBase(dbFtw, allFootwears);

            var db = _service.CheckSeasonAndTimeSetLatestFile(dbApparel);
            var dbFootwear = _service.CheckSeasonAndTimeSetLatestFile(dbFtw);

            _print.NewLine();
            _service.ChangeTextColor("Green");
            Console.WriteLine("--'TWEEDLEDUMS' CHECKED !");
            ////-------------------DATA UPDATE ENDS HERE !!!

            //// RESULT
            StringBuilder resultZeros = new StringBuilder();
            StringBuilder resultColors = new StringBuilder();

            StringBuilder resultFootwearZeros = new StringBuilder();
            StringBuilder resultFootwearColors = new StringBuilder();

            int counterSkuZeros = 0;
            int counterZeroMatches = 0;
            int counterSkuColors = 0;
            int counterColorsMatches = 0;

            int counterSkuZerosFootwear = 0;
            int counterZeroMatchesFootwear = 0;
            int counterSkuColorsFootwear = 0;
            int counterColorsMatchesFootwear = 0;

            foreach (var sku in db.Keys)
            {
                if (sku.Contains('-') && !sku.Contains("000000-000") && sku.EndsWith("000"))
                {
                    if (db[sku].Count() > 1)
                    {
                        resultZeros.AppendLine(sku);
                        counterSkuZeros++;

                        foreach (var texture in db[sku])
                        {
                            var skuInfo = $"-- {texture.Path} --> {texture.Name}";
                            if (texture.isLatest)
                            {
                                resultZeros.AppendLine(skuInfo + "   <<<---");
                            }
                            else
                            {
                                resultZeros.AppendLine(skuInfo);
                            }
                            counterZeroMatches++;
                        }

                        resultZeros.AppendLine();
                    }
                }

                if (sku.Contains('-') && !sku.Contains("000000") && !sku.EndsWith("000"))
                {
                    if (db[sku].Count() > 1)
                    {
                        resultColors.AppendLine(sku);
                        counterSkuColors++;

                        foreach (var texture in db[sku])
                        {
                            var skuInfo = $"-- {texture.Path} --> {texture.Name}";
                            if (texture.isLatest)
                            {
                                resultColors.AppendLine(skuInfo + "   <<<---");
                            }
                            else
                            {
                                resultColors.AppendLine(skuInfo);
                            }
                            counterColorsMatches++;
                        }

                        resultColors.AppendLine();
                    }
                }
            }

            foreach (var sku in dbFootwear.Keys)
            {
                if (sku.Contains('-') && !sku.Contains("000000-000") && sku.EndsWith("000") && dbFootwear[sku].Count() > 1)
                {
                    resultFootwearZeros.AppendLine(sku);
                    counterSkuZerosFootwear++;

                    foreach (var texture in dbFootwear[sku])
                    {
                        var skuInfo = $"-- {texture.Path} --> {texture.Name}";
                        if (texture.isLatest)
                        {
                            resultFootwearZeros.AppendLine(skuInfo + "   <<<---");
                        }
                        else
                        {
                            resultFootwearZeros.AppendLine(skuInfo);
                        }
                        counterZeroMatchesFootwear++;
                    }

                    resultFootwearZeros.AppendLine();
                }

                if (sku.Contains('-') && !sku.Contains("000000") && !sku.EndsWith("000") && dbFootwear[sku].Count() > 1)
                {
                    resultFootwearColors.AppendLine(sku);
                    counterSkuColorsFootwear++;

                    foreach (var texture in dbFootwear[sku])
                    {
                        var skuInfo = $"-- {texture.Path} --> {texture.Name}";
                        if (texture.isLatest)
                        {
                            resultFootwearColors.AppendLine(skuInfo + "   <<<---");
                        }
                        else
                        {
                            resultFootwearColors.AppendLine(skuInfo);
                        }
                        counterColorsMatchesFootwear++;
                    }

                    resultFootwearColors.AppendLine();
                }
            }

            resultZeros.AppendLine($"Sku counter: {counterSkuZeros.ToString()} -> sku matches: {counterZeroMatches.ToString()}");
            resultColors.AppendLine($"Sku counter: {counterSkuColors.ToString()} -> sku matches: {counterColorsMatches.ToString()}");

            resultFootwearZeros.AppendLine($"Sku counter: {counterSkuZerosFootwear.ToString()} -> sku matches: {counterZeroMatchesFootwear.ToString()}");
            resultFootwearColors.AppendLine($"Sku counter: {counterSkuColorsFootwear.ToString()} -> sku matches: {counterColorsMatchesFootwear.ToString()}");

            ////--------------------- RESULT end here

            //// Print to file
            var typeElement = folderName == "psd" ? $"[{folderName},psb]" : $"[{folderName}]";
            
            var toSaveApparelZeroes = resultsLocation + "\\" + folderName + "\\" + $"[apparel]_000_{typeElement}.txt";
            StreamWriter sw = new StreamWriter(toSaveApparelZeroes);

            var toSaveApparelColors = resultsLocation + "\\" + folderName + "\\" + $"[apparel]_colors_{typeElement}.txt";
            StreamWriter swColors = new StreamWriter(toSaveApparelColors);

            var toSaveFootwearZeroes = resultsLocation + "\\" + folderName + "\\" + $"[footwear]_000_{typeElement}.txt";
            StreamWriter swFootwear = new StreamWriter(toSaveFootwearZeroes);

            var toSaveFootwearColors = resultsLocation + "\\" + folderName + "\\" + $"[footwear]_colors_{typeElement}.txt";
            StreamWriter swFootwearColors = new StreamWriter(toSaveFootwearColors);

            sw.WriteLine(resultZeros.ToString());
            swColors.WriteLine(resultColors.ToString());

            swFootwear.WriteLine(resultFootwearZeros.ToString());
            swFootwearColors.WriteLine(resultFootwearColors.ToString());

            sw.Flush();
            swColors.Flush();
            sw.Close();
            swColors.Close();

            swFootwear.Flush();
            swFootwearColors.Flush();
            swFootwear.Close();
            swFootwearColors.Close();
            ////---------------- Print to file ends here !!!


            _print.NewLine();
            _service.ChangeTextColor("Red");
            Console.WriteLine($"results here -> ");
            _service.ChangeTextColor("White");
            Console.Write($"{resultsLocation}");
            _service.ChangeTextColor("Red");
            Console.Write(" <-");
            ////-------------------TRANSFER ENDS HERE !!!

            Console.ReadLine();
        }

        private void UpdateInternalDataBase(Dictionary<string, HashSet<Texture>> db, HashSet<string> allFiles)
        {
            foreach (var file in allFiles)
            {
                var regex = _gc.textureRegex;
                var filename = Regex.Match(file, regex);

                if (filename.Success)
                {
                    var path = Path.GetDirectoryName(file);
                    var extension = Path.GetExtension(file);
                    var sku = filename.Groups["sku"].ToString();

                    Texture texture = new Texture(path, filename.Value + extension, false);

                    if (db.ContainsKey(sku) == false)
                    {
                        db[sku] = new HashSet<Texture>() { texture };
                    }
                    else
                    {
                        db[sku].Add(texture);
                    }
                }
            }
        }
    }
}