using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public class ProfanityFilter
{
    private readonly HashSet<string> _badWords;

    public ProfanityFilter()
    {
        _badWords = new HashSet<string>();

        LoadBadWords();
    }

    private void LoadBadWords()
    {
        TextAsset file = Resources.Load<TextAsset>("Profanity/bad_words");

        if (file == null)
        {
            Debug.LogError("No se ha encontrado Profanity/bad_words.txt");
            return;
        }

        string[] lines = file.text.Split('\n');

        foreach (string line in lines)
        {
            string word = line.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(word))
                continue;

            _badWords.Add(word);
        }
    }

    public bool IsNameAllowed(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        string normalizedName = Normalize(name);

        foreach (string badWord in _badWords)
        {
            if (normalizedName.Contains(badWord))
            {
                return false;
            }
        }

        return true;
    }

    private string Normalize(string text)
    {
        text = text.ToLowerInvariant();

        StringBuilder result = new StringBuilder();

        foreach (char c in text)
        {
            switch (c)
            {
                // Leetspeak habitual
                case '4':
                case '@':
                    result.Append('a');
                    break;

                case '3':
                    result.Append('e');
                    break;

                case '1':
                    result.Append('i');
                    break;

                case '0':
                    result.Append('o');
                    break;

                case '5':
                    result.Append('s');
                    break;

                default:
                    // Ignoramos separadores
                    if (char.IsLetterOrDigit(c))
                    {
                        result.Append(c);
                    }
                    break;
            }
        }

        return RemoveAccents(result.ToString());
    }

    private string RemoveAccents(string text)
    {
        string normalized = text.Normalize(NormalizationForm.FormD);
        StringBuilder result = new StringBuilder();

        foreach (char c in normalized)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);

            if (category != UnicodeCategory.NonSpacingMark)
            {
                result.Append(c);
            }
        }

        return result
            .ToString()
            .Normalize(NormalizationForm.FormC);
    }
}