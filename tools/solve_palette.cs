// Prints the 24-colour palette for the current wood and cover texture (backlog T-010, research file 18). Paste the hex list into LamplitBuilder.GroupPalette.
double woodEffY = 0.066, coverTex = 0.733, targetRatio = 2.2;
double needEff = BookBoy.Rules.Palette.MinLuminanceFor(woodEffY, targetRatio);
double minTint = needEff / coverTex;
var wood = new BookBoy.Rules.PaletteColor(0.45, 0.25, 0.12);
var set = BookBoy.Rules.Palette.Solve(24, minTint, wood, 0.52, 0.92, 0.07, 0.18, 0.12);
var sb = new System.Text.StringBuilder();
sb.AppendLine("need effective Y " + needEff.ToString("F3") + " -> tint Y >= " + minTint.ToString("F3") + "; solver returned " + set.Count);
double minR = 1e9, sumR = 0, minPair = 1e9;
var hex = new System.Collections.Generic.List<string>();
for (int i = 0; i < set.Count; i++)
{
    double y = BookBoy.Rules.Palette.Luminance(set[i].R, set[i].G, set[i].B) * coverTex;
    double ratio = BookBoy.Rules.Palette.ContrastRatio(y, woodEffY);
    minR = System.Math.Min(minR, ratio); sumR += ratio;
    for (int j = i + 1; j < set.Count; j++) minPair = System.Math.Min(minPair, BookBoy.Rules.Palette.WorstDistance(set[i], set[j]));
    hex.Add("#" + ((int)System.Math.Round(set[i].R * 255)).ToString("X2") + ((int)System.Math.Round(set[i].G * 255)).ToString("X2") + ((int)System.Math.Round(set[i].B * 255)).ToString("X2"));
}
sb.AppendLine("effective ratio vs wood: min " + minR.ToString("F2") + " avg " + (sumR / set.Count).ToString("F2") + "; smallest pair distance (any vision) " + minPair.ToString("F3"));
sb.AppendLine(string.Join(", ", hex));
return sb.ToString();
