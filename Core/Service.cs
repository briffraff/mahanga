using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using mahanga.Core.Interface;
using mahanga.Global;

namespace mahanga.Core
{
    public class Service : IAMService
    {
        private readonly Constants _gc = new Constants();

        public void CheckAndResetWindowSize()
        {
            if (Console.WindowHeight != 120 || Console.WindowWidth != 30)
            {
                Console.SetWindowSize(120, 30);
                //Console.SetBufferSize(60, 45);
            }
        }

        public string CheckExeLocation()
        {
            DirectoryInfo dirInfo = new DirectoryInfo("./");

            return dirInfo.FullName;
        }
        
        //  ENUMERATION OPTIONS
        public EnumerationOptions EnumOptions()
        {
            var options = new EnumerationOptions()
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            };

            return options;
        }

        public void isApproved(string answer)
        {
            answer = answer.ToLower();

            this.ChangeTextColor("White");

            while (answer != null)
            {
                if (answer == "y")
                {
                    break;
                }
                else if (answer == "n")
                {
                    Environment.Exit(0);
                }
                else
                {
                    this.ChangeTextColor("Red");
                    Console.WriteLine("*You must say 'Y' or 'N' !");

                    this.ChangeTextColor("White");
                    answer = Console.ReadLine().ToLower();
                }
            }

        }

        public string chooseTypeToWorkWith(string chosenType)
        {
            var authorizedTypes = new string[] { "jpg", "psd", "png" , "jpeg" }; //Folder name will be created with given file type
            chosenType = chosenType.ToLower();
            var type = "";
            int wrongCounter = 0;

            this.ChangeTextColor("White");

            while (chosenType != null)
            {
                if (chosenType == "1")
                {
                    type = "psd";
                    break;
                }

                if (chosenType == "2")
                {
                    type = "jpg";
                    break;
                }

                if (chosenType == "3")
                {
                    type = "png";
                    break;
                }


                if (chosenType == "4")
                {
                    type = "jpeg";
                    break;
                }
                if (authorizedTypes.Contains(type) == false)
                {
                    this.ChangeTextColor("Red");
                    Console.WriteLine("*You must choose from the list !");
                    if (wrongCounter > 1)
                    {
                        Console.WriteLine("*If you want to stop, please type 'cancel' !");
                    }

                    this.ChangeTextColor("White");
                    chosenType = Console.ReadLine().ToLower();
                }

                if (chosenType == "cancel")
                {
                    Environment.Exit(0);
                }

                wrongCounter++;
            }

            return type;
        }

        public void ChangeTextColor(string color)
        {
            Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), color);
        }

        public void ChangeColor(string color)
        {
            Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), color);
        }

        public void CreateFolders(string results, string folderName)
        {
            var resultsFolder = Path.Combine(results, folderName);

            if (Directory.Exists(resultsFolder) == false)
            {
                Directory.CreateDirectory(resultsFolder);
            }
        }

        public void TransferResultFilesToFolder(string source, string destination, string typeElement)
        {
            string colors = $"[apparel]_colors_{typeElement}.txt";
            string zeros = $"[apparel]_000_{typeElement}.txt";
            string ftwcolors = $"[footwear]_colors_{typeElement}.txt";
            string ftwzeros = $"[footwear]_000_{typeElement}.txt";

            //transfer result files to script folder
            if (File.Exists(destination + colors))
            {
                File.Delete(destination + colors);
                File.Copy(source + colors, destination + colors, true);
            }
            else
            {
                File.Copy(source + colors, destination + colors, true);
            }

            //transfer result files to script folder
            if (File.Exists(destination + zeros))
            {
                File.Delete(destination + zeros);
                File.Copy(source + zeros, destination + zeros, true);
            }
            else
            {
                File.Copy(source + zeros, destination + zeros, true);
            }

            //transfer result files to script folder
            if (File.Exists(destination + ftwcolors))
            {
                File.Delete(destination + ftwcolors);
                File.Copy(source + ftwcolors, destination + ftwcolors, true);
            }
            else
            {
                File.Copy(source + ftwcolors, destination + ftwcolors, true);
            }

            //transfer result files to script folder
            if (File.Exists(destination + ftwzeros))
            {
                File.Delete(destination + ftwzeros);
                File.Copy(source + ftwzeros, destination + ftwzeros, true);
            }
            else
            {
                File.Copy(source + ftwzeros, destination + ftwzeros, true);
            }
        }

        public async Task<HashSet<string>> GetAllFilesParralelAsync(Dictionary<string, List<string>> categoriesInfo, EnumerationOptions options)
        {
            HashSet<string> filesCollection = new HashSet<string>();
            List<Task> tasks = new List<Task>() { };

            foreach (var category in categoriesInfo.Keys)
            {
                var name = categoriesInfo[category][0];
                var path = categoriesInfo[category][1];
                var searchPattern = categoriesInfo[category][2];

                tasks.Add(Task.Run(() => GetFilesParralelAsync(filesCollection, name, path, searchPattern, options)));
            }
            await Task.WhenAll(tasks.ToArray());

            return filesCollection;
        }

        private async Task GetFilesParralelAsync(HashSet<string> filesCollection, string name, string path, string searchPattern, EnumerationOptions options)
        {
            this.ChangeTextColor("White");
            Console.WriteLine($"-----------> {name}[{0}] - 0% ");

            var getFiles = await Task.Run(() => Directory.GetFiles(path, searchPattern, options));
            int count = getFiles.Length;

            foreach (var file in getFiles)
            {
                filesCollection.Add(file);
            }

            this.ChangeTextColor("White");
            this.ChangeTextColor("Green");
            Console.WriteLine($"-------------> {name}[{count}] - 100%");

        }

        public new Dictionary<string, HashSet<Texture>> CheckSeasonAndTimeSetLatestFile(Dictionary<string, HashSet<Texture>> db)
        {
            var updatedDb = new Dictionary<string, HashSet<Texture>>();

            foreach (var sku in db.Keys)
            {
                var regex = _gc.textureRegex;
                var updatedTextures = new HashSet<Texture>();
                var newest = new Texture(null, null, false);

                foreach (var textr in db[sku])
                {
                    if (newest.Name == null)
                    {
                        newest.Path = textr.Path;
                        newest.Name = textr.Name;
                        newest.isLatest = true;
                    }
                    else
                    {
                        var filename = Regex.Match(textr.Name, regex);

                        //get season
                        string getSeason = filename.Groups["season"].ToString();

                        // newest condition needed
                        bool isNewestSeasonMark = false;
                        bool isNewestSeasonNum = false;
                        bool isLastModifedFile = false;

                        // enumerate seasons
                        int HO = 1;
                        int SP = 2;
                        int SU = 3;
                        int FA = 4;

                        //get marks and num of season input
                        string currentSeasonMarks = getSeason.Substring(0, 2);
                        string currentSeasonNum = getSeason.Substring(2, 2);

                        //get marks and num of season from dict
                        string seasonMarks = newest.Name.Split('_', StringSplitOptions.RemoveEmptyEntries)[2].Substring(0, 2);
                        string seasonNum = newest.Name.Split('_', StringSplitOptions.RemoveEmptyEntries)[2].Substring(2, 2);

                        //transofrm marks to numbers
                        int seasonMarksInNum = 0;
                        int currentSeasonMarksInNum = 0;

                        if (seasonMarks == nameof(HO)) { seasonMarksInNum = HO; }
                        if (seasonMarks == nameof(SP)) { seasonMarksInNum = SP; }
                        if (seasonMarks == nameof(SU)) { seasonMarksInNum = SU; }
                        if (seasonMarks == nameof(FA)) { seasonMarksInNum = FA; }

                        if (currentSeasonMarks == nameof(HO)) { currentSeasonMarksInNum = HO; }
                        if (currentSeasonMarks == nameof(SP)) { currentSeasonMarksInNum = SP; }
                        if (currentSeasonMarks == nameof(SU)) { currentSeasonMarksInNum = SU; }
                        if (currentSeasonMarks == nameof(FA)) { currentSeasonMarksInNum = FA; }

                        // get last time modification date of the current input and current 
                        var lastTimeModified = newest.LastWriteTime;
                        var lastTimeModifiedCurrent = textr.LastWriteTime;

                        // check which mod date is latest
                        if (lastTimeModifiedCurrent > lastTimeModified)
                        {
                            isLastModifedFile = true;
                        }

                        //compare season Marks nums
                        if (seasonMarksInNum < currentSeasonMarksInNum)
                        {
                            isNewestSeasonMark = true;
                        }

                        //compare season nums
                        if (Int32.Parse(seasonNum) < Int32.Parse(currentSeasonNum))
                        {
                            isNewestSeasonNum = true;
                        }

                        // if some of marks , nums or geometry are equal true then go overwrite to dict
                        if (isNewestSeasonMark || isNewestSeasonNum || isLastModifedFile)
                        {
                            var tempTexture = new Texture(null, null, false);

                            tempTexture.Name = newest.Name;
                            tempTexture.Path = newest.Path;
                            tempTexture.isLatest = false;
                            updatedTextures.Add(tempTexture);

                            newest.Name = textr.Name;
                            newest.Path = textr.Path;
                            newest.isLatest = true;
                        }
                        else
                        {
                            updatedTextures.Add(textr);
                        }
                    }

                }
                //add final latest/newest file
                updatedTextures.Add(newest);

                //order by path then by islatest
                HashSet<Texture> ordered = updatedTextures.OrderBy(x => x.Path).ThenBy(x => x.isLatest).ToHashSet();

                // add updated hashset of textures in new db
                if (updatedDb.ContainsKey(sku) == false)
                {
                    updatedDb[sku] = ordered;
                }
            }
            //return updated data
            return updatedDb;
        }
    }
}
