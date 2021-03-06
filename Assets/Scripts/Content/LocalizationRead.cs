﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content
{
    // Helper class to read an ini file into a nested dictionary
    // This exists because .NET/Mono doesn't have one!!
    public static class LocalizationRead
    {
        public static DictionaryI18n ffgDict = null;
        public static DictionaryI18n valkyrieDict = null;
        public static DictionaryI18n scenarioDict = null;

        /// <summary>
        /// Change all dictionary languages
        /// </summary>
        /// <param name="newLang">string for new language</param>
        public static void changeCurrentLangTo(string newLang)
        {
            if (ffgDict != null)
            {
                ffgDict.setCurrentLanguage(newLang);
            }
            if (valkyrieDict != null)
            {
                valkyrieDict.setCurrentLanguage(newLang);
            }
            if (scenarioDict != null)
            {
                scenarioDict.setCurrentLanguage(newLang);
            }
        }

        // Function takes path to ini file and returns data object
        // Returns null on error
        public static DictionaryI18n ReadFromLocalization(string path, string newLang)
        {
            string[] lines;

            // Read the whole file
            try
            {
                lines = System.IO.File.ReadAllLines(path);
            }
            catch (System.Exception e)
            {
                ValkyrieDebug.Log("Error loading localization file " + path + ":" + e.Message);
                return null;
            }
            // Parse text data
            return new DictionaryI18n(lines, newLang);
        }


        // Function ini file contents as a string and returns data object
        // Returns null on error
        public static DictionaryI18n ReadFromString(string content, string newLang)
        {
            // split text into array of lines
            string[] lines = content.Split(new string[] { "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            return new DictionaryI18n(lines, newLang);
        }


        // Check for FFG text lookups and insert required text
        /// <summary>
        /// Replaces a FFG key with his current text
        /// </summary>
        /// <param name="input">{ffg:XXXX} like input</param>
        /// <returns>Translation to current language</returns>
        public static string FFGLookup(StringKey input)
        {
            string output = input.key;
            // While there are more lookups
            while (output.IndexOf("{ffg:") != -1)
            {
                // Can be nested
                int bracketLevel = 1;
                // Start of lookup
                int lookupStart = output.IndexOf("{ffg:") + "{ffg:".Length;

                // Loop to find end of lookup
                int lookupEnd = lookupStart;
                while (bracketLevel > 0)
                {
                    lookupEnd++;
                    if (output[lookupEnd].Equals('{'))
                    {
                        bracketLevel++;
                    }
                    if (output[lookupEnd].Equals('}'))
                    {
                        bracketLevel--;
                    }
                }

                // Extract lookup key
                string lookup = output.Substring(lookupStart, lookupEnd - lookupStart);
                // Get key result
                string result = FFGQuery(lookup);
                // We (unity) don't support underlines
                // Unity uses <> not []
                result = result.Replace("[u]", "<b>").Replace("[/u]", "</b>");
                result = result.Replace("[i]", "<i>").Replace("[/i]", "</i>");
                result = result.Replace("[b]", "<b>").Replace("[/b]", "</b>");
                // Replace the lookup
                output = output.Replace("{ffg:" + lookup + "}", result);
            }
            return output;
        }

        /// <summary>
        /// Look up a key in the FFG text Localization. Can have parameters divided by ":"
        /// Example: A_GOES_B_MESSAGE:Peter:Dinning Room
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string FFGQuery(string input)
        {
            int bracketLevel = 0;
            int lastSection = 0;
            List<string> elements = new List<string>();

            // Separate the input into sections
            for (int index = 0; index < input.Length; index++)
            {
                if (input[index].Equals('{'))
                {
                    bracketLevel++;
                }
                if (input[index].Equals('}'))
                {
                    bracketLevel--;
                }
                // Section divider
                if (input[index].Equals(':'))
                {
                    // Not in brackets
                    if (bracketLevel == 0)
                    {
                        // Add previous element
                        elements.Add(input.Substring(lastSection, index - lastSection));
                        lastSection = index + 1;
                    }
                }
            }
            // Add previous element
            elements.Add(input.Substring(lastSection, input.Length - lastSection));

            // Look up the first element (key)
            string fetched = FFGKeyLookup(elements[0]);

            // Find and replace with other elements
            for (int i = 2; i < elements.Count; i += 2)
            {
                fetched = fetched.Replace(elements[i - 1], elements[i]);
            }
            return fetched;
        }


        /// <summary>
        /// Transform a ffg key (without ffg prefig, into current language text
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string FFGKeyLookup(string key)
        {
            if (ffgDict != null)
            {
                //try
                //{
                    EntryI18n valueOut;

                    if (ffgDict.tryGetValue(key, out valueOut))
                    {
                        return valueOut.getCurrentOrDefaultLanguageString();
                    }
                    else
                    {
                        return key;
                    }
                //}
                /*catch (System.Exception e)
                {
                    ValkyrieDebug.Log("Warning: Unable to process imported Localization string with key: " + key + ". Exception:" + e.Message + System.Environment.NewLine);
                }*/
            } else
            {
                ValkyrieDebug.Log("Error: FFG dictionary not loaded");
            }
            return key;
        }
    }
}
