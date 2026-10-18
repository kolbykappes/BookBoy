using UnityEngine;

/// <summary>
/// The magical theme of each book color. Players are told the theme (on the sign above each bookcase and in each book's
/// title), never the color: "Healing Magic" books are the white ones, and so on.
/// </summary>
public static class BookThemes
{
    // Indexed by (int)BookCategory. Sign lines are shown in capitals on the wall plaque; the title word starts a book's name.
    private static readonly string[] SignLine1 = { "ALCHEMY", "ASTRAL", "BEAST", "FORBIDDEN", "TIDAL", "ILLUSION", "CHARM", "FIRE", "HERBAL", "STORM", "EARTH", "RUNE", "HEALING", "SHADOW", "RADIANT", "RENEWAL" };
    private static readonly string[] SignLine2 = { "& POTIONS", "MAGIC", "LORE", "LORE", "MAGIC", "MAGIC", "MAGIC", "MAGIC", "LORE", "MAGIC", "MAGIC", "CRAFT", "MAGIC", "MAGIC", "MAGIC", "MAGIC" };
    private static readonly string[] Titles = { "Alchemy", "Astral", "Beast", "Forbidden", "Tidal", "Illusion", "Charm", "Fire", "Herbal", "Storm", "Earth", "Rune", "Healing", "Shadow", "Radiant", "Renewal" };

    public static int Count => Titles.Length;

    public static string TitleWord(BookCategory category)
    {
        return Titles[Index(category)];
    }

    /// <summary>The two lines of the wall sign, e.g. "HEALING" and "MAGIC".</summary>
    public static string[] SignLines(BookCategory category)
    {
        int i = Index(category);
        return new[] { SignLine1[i], SignLine2[i] };
    }

    /// <summary>The whole theme on one line, e.g. "Healing Magic".</summary>
    public static string ThemeName(BookCategory category)
    {
        int i = Index(category);
        string[] words = (SignLine1[i] + " " + SignLine2[i]).Split(' ');
        for (int w = 0; w < words.Length; w++)
        {
            words[w] = Capitalize(words[w]);
        }

        return string.Join(" ", words);
    }

    private static int Index(BookCategory category)
    {
        return Mathf.Clamp((int)category, 0, Titles.Length - 1);
    }

    private static string Capitalize(string upper)
    {
        if (string.IsNullOrEmpty(upper))
        {
            return upper;
        }

        string lower = upper.ToLowerInvariant();
        return char.ToUpperInvariant(lower[0]) + lower.Substring(1);
    }
}
