﻿using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Content
{
    /// <summary>
    /// String in international FFG format. Including all available languages supported by MoM App
    /// </summary>
    public class EntryI18n
    {

        private DictionaryI18n referedDictionary;

        /// <summary>
        /// Instance info of the current translations
        /// </summary>
        private string[] translations;
       
        /// <summary>
        /// Creates an empty instance of a Multilanguage String
        /// </summary>
        public EntryI18n(DictionaryI18n dict)
        {
            referedDictionary = dict;
            translations = new string[dict.getLanguages().Length];
        }

        private const char QUOTES = '\"';
        private const char COMMA = ',';

        /// <summary>
        /// Constructor with the complete localisation elements
        /// </summary>
        /// <param name="completeLocalisationString"></param>
        public EntryI18n(DictionaryI18n dict,string completeLocalisationString)
        {
            referedDictionary = dict;

            if (completeLocalisationString.Contains(QUOTES))
            {
                // with quotes, commas inside quotes isn't considered separator
                List<string> partialTranslation = new List<string>(completeLocalisationString.Split(COMMA));
                List<string> finalTranslation = new List<string>();
                string currentTranslation = "";
                bool oddity = false;
                foreach (string suposedTranslation in partialTranslation)
                {
                    currentTranslation += suposedTranslation;

                    // Counting quotes inside string to know oddity.
                    bool newOddity = (suposedTranslation.Count(ch => ch == QUOTES) % 2) == 1;

                    if (oddity ^ newOddity)
                    {
                        // If oddity changes we are still inside quotes
                        currentTranslation += COMMA;
                    }
                    else
                    {
                        // If opening and closing quotes, we supress it.
                        if (currentTranslation.Length > 0 && currentTranslation[0] == QUOTES)
                        {
                            currentTranslation = currentTranslation.Substring(1, currentTranslation.Length - 2);
                        }

                        // escaping double quotes
                        finalTranslation.Add(currentTranslation.Replace("\"\"", "\""));
                        currentTranslation = "";
                    }

                    oddity = oddity ^ newOddity;

                }
                translations = finalTranslation.ToArray();
            }
            else
            {
                // Without quotes, all commas are separators
                translations = completeLocalisationString.Split(COMMA);
            }

            if (translations.Length > dict.getLanguages().Length)
            {
                ValkyrieDebug.Log("Incoherent DictI18n with " + dict.getLanguages().Length + " languages including StringI18n: " + completeLocalisationString + System.Environment.NewLine);
            }
        }

        // The key is que position 0 of the array
        public string key
        {
            get
            {
                return translations[0];
            }
        }

        public string getSpecificLanguageString(int nLanguage)
        {
            return translations[nLanguage];
        }

        /// <summary>
        /// In translation of texts. If we don't have current language text, a
        /// specific language text will be got. In order to know if there is a 
        /// current language text the method HasTextInCurrentLanguage can be used.
        /// </summary>
        /// <param name="nLanguage">number of the language to use</param>
        /// <returns></returns>
        public string getCurrentOrDefaultLanguageString()
        {
            if (HasTextInCurrentLanguage)
            {
                return currentLanguageString;
            } else
            {
                return getSpecificLanguageString(referedDictionary.defaultLanguage);
            }            
        }

        /// <summary>
        /// The string value of the key whith the current language
        /// </summary>
        public string currentLanguageString
        {
            get
            {
                if (referedDictionary.currentLanguage < translations.Length)
                {
                    return translations[referedDictionary.currentLanguage];
                }
                else
                {
                    return "";
                }

            }
            set
            {
                translations[referedDictionary.currentLanguage] = value;
            }
        }

        public bool HasTextInCurrentLanguage
        {
            get
            {
                return currentLanguageString.Length > 0;
            }
        }

        /// <summary>
        /// String representation of the multilanguage element
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            bool first = true;
            foreach (string oneTranslation in translations)
            {
                if (!first)
                {
                    result.Append(COMMA);
                }

                if (oneTranslation.Contains(COMMA) || oneTranslation.Contains(QUOTES))
                {
                    // The serializable text should repeat mid quotes and add initial and final quotes
                    result.Append(QUOTES).Append(oneTranslation.Replace(QUOTES.ToString(),"\"\"")).Append(QUOTES);
                }
                else
                {
                    result.Append(oneTranslation);
                }

                if (first)
                {
                    first = false;
                }
            }

            return result.ToString();
        }
    }
}
