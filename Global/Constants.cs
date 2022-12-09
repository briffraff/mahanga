using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace mahanga.Global
{
    public class Constants
    {
        // tool init info
        public string toolName = "M ā H A N G A";
        public string toolDescription = "Catch sku's tweedledum";
        public string toolVersion = "v0.4";
        public string dateOfCreation = "9th December 2021";

        // extension patterns
        public string psdPattern = "*.psd";
        public string psbPattern = "*.psb";

        // categories
        public string MWEQ = "MWEQ";
        public string MA = "MA";
        public string PS = "PS";
        public string YA = "YA";
        public string YAPS = "YAPS";
        public string FTW = "FTW";

        //paths
        public string fullRangePSBfiles = @"N:\";
        public string temp = @"K:\TEMP PSDs"; // temp
        public string scriptsFolder = @"M:\Z_Software Assets\3ds Max\BorakaScriptPack_vol1";
        public string toolResultsPath = @"\MaHANGA\results\";
        public string pathRootMWEQ = @"N:\Garments"; // the root folder for Men-Women-Equipment Garments
        public string pathRootMA = @"N:\Garments MA"; // the root folder to Maternity Garments
        public string pathRootPlusSize = @"N:\Garments PS"; // the root folder for PlusSize Garments
        public string pathRootYA = @"N:\Garments YA"; // the root folder for Young Athletes Garments
        public string pathRootYAPS = @"N:\Garments YAPS"; //the root folder for YAPS
        public string pathRootFTW = @"N:\Footwear"; // the root folder for Footwear garments  

        //updated regex for textures
        public string textureRegex = new Regex(@"\bT_(?<garment>[N|S]\d{2}[A-Z]\d{3})_(?<season>[A-Z][A-Z]\d{2})_(?<category>[A-Z][A-Z])_(?<sku>(?<skuNum>.{6})[-|_](?<skuColor>.{3}))_D\b").ToString();


        public string ryme =
            "Tweedledum and Tweedledee\r\n        Agreed to have a battle;\r\n    For Tweedledum said Tweedledee\r\n        Had spoiled his nice new rattle.\r\n\r\n    Just then flew down a monstrous crow,\r\n        As black as a tar-barrel;\r\n    Which frightened both the heroes so,\r\n        They quite forgot their quarrel.";
    }
}
