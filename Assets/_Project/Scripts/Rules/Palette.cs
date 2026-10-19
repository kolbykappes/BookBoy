using System;
using System.Collections.Generic;

namespace BookBoy.Rules
{
    /// <summary>The colour of a book set in a palette: sRGB 0..1 plus how it was chosen.</summary>
    public readonly struct PaletteColor
    {
        public PaletteColor(double r, double g, double b)
        {
            R = r;
            G = g;
            B = b;
        }

        public double R { get; }
        public double G { get; }
        public double B { get; }
    }

    public enum Vision
    {
        Normal,
        Protan,
        Deutan,
        Tritan
    }

    /// <summary>
    /// Colour maths for choosing the book colours (backlog T-010, research file 18): WCAG relative luminance and contrast ratio, Oklab, colour-blind
    /// simulation (Machado et al. 2009, severity 1.0, applied to linear RGB), and a deterministic greedy farthest-point solver that picks n colours
    /// which all keep a minimum luminance (so they stand out from the wood) and stay as far apart as possible for normal vision and for the three
    /// kinds of dichromacy. Engine-free so it is covered by Edit Mode tests.
    /// </summary>
    public static class Palette
    {
        public static double SrgbToLinear(double c)
        {
            return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        public static double LinearToSrgb(double c)
        {
            c = Math.Max(0.0, Math.Min(1.0, c));
            return c <= 0.0031308 ? 12.92 * c : 1.055 * Math.Pow(c, 1.0 / 2.4) - 0.055;
        }

        /// <summary>WCAG relative luminance of a colour given in sRGB 0..1.</summary>
        public static double Luminance(double r, double g, double b)
        {
            return LuminanceLinear(SrgbToLinear(r), SrgbToLinear(g), SrgbToLinear(b));
        }

        public static double LuminanceLinear(double lr, double lg, double lb)
        {
            return 0.2126 * lr + 0.7152 * lg + 0.0722 * lb;
        }

        /// <summary>WCAG contrast ratio of two relative luminances: (lighter + 0.05) / (darker + 0.05).</summary>
        public static double ContrastRatio(double y1, double y2)
        {
            double hi = Math.Max(y1, y2), lo = Math.Min(y1, y2);
            return (hi + 0.05) / (lo + 0.05);
        }

        /// <summary>The luminance a colour must have to reach <paramref name="ratio"/> against a darker reference of luminance <paramref name="referenceY"/>.</summary>
        public static double MinLuminanceFor(double referenceY, double ratio)
        {
            return ratio * (referenceY + 0.05) - 0.05;
        }

        // ---- Oklab (Bjorn Ottosson) ----

        public static void LinearToOklab(double r, double g, double b, out double L, out double A, out double B)
        {
            double l = Math.Cbrt(0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b);
            double m = Math.Cbrt(0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b);
            double s = Math.Cbrt(0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b);
            L = 0.2104542553 * l + 0.7936177850 * m - 0.0040720468 * s;
            A = 1.9779984951 * l - 2.4285922050 * m + 0.4505937099 * s;
            B = 0.0259040371 * l + 0.7827717662 * m - 0.8086757660 * s;
        }

        public static void OklabToLinear(double L, double A, double B, out double r, out double g, out double b)
        {
            double l = L + 0.3963377774 * A + 0.2158037573 * B;
            double m = L - 0.1055613458 * A - 0.0638541728 * B;
            double s = L - 0.0894841775 * A - 1.2914855480 * B;
            l = l * l * l;
            m = m * m * m;
            s = s * s * s;
            r = 4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s;
            g = -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s;
            b = -0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s;
        }

        // ---- colour-blind simulation on linear RGB (Machado et al. 2009, severity 1.0) ----

        public static void Simulate(Vision v, double r, double g, double b, out double sr, out double sg, out double sb)
        {
            switch (v)
            {
                case Vision.Protan:
                    sr = 0.152286 * r + 1.052583 * g - 0.204868 * b;
                    sg = 0.114503 * r + 0.786281 * g + 0.099216 * b;
                    sb = -0.003882 * r - 0.048116 * g + 1.051998 * b;
                    break;
                case Vision.Deutan:
                    sr = 0.367322 * r + 0.860646 * g - 0.227968 * b;
                    sg = 0.280085 * r + 0.672501 * g + 0.047413 * b;
                    sb = -0.011820 * r + 0.042940 * g + 0.968881 * b;
                    break;
                case Vision.Tritan:
                    sr = 1.255528 * r - 0.076749 * g - 0.178779 * b;
                    sg = -0.078411 * r + 0.930809 * g + 0.147602 * b;
                    sb = 0.004733 * r + 0.691367 * g + 0.303900 * b;
                    break;
                default:
                    sr = r;
                    sg = g;
                    sb = b;
                    break;
            }

            sr = Math.Max(0.0, Math.Min(1.0, sr));
            sg = Math.Max(0.0, Math.Min(1.0, sg));
            sb = Math.Max(0.0, Math.Min(1.0, sb));
        }

        private static readonly Vision[] Visions = { Vision.Normal, Vision.Protan, Vision.Deutan, Vision.Tritan };

        /// <summary>The Oklab distance between two colours (sRGB 0..1) as seen with the given kind of vision.</summary>
        public static double Distance(Vision v, PaletteColor a, PaletteColor b)
        {
            Lab(v, a, out double l1, out double a1, out double b1);
            Lab(v, b, out double l2, out double a2, out double b2);
            return Math.Sqrt((l1 - l2) * (l1 - l2) + (a1 - a2) * (a1 - a2) + (b1 - b2) * (b1 - b2));
        }

        /// <summary>The smallest of the four distances (normal, protan, deutan, tritan): how easily two colours can be told apart by anyone.</summary>
        public static double WorstDistance(PaletteColor a, PaletteColor b)
        {
            double worst = double.MaxValue;
            foreach (Vision v in Visions)
            {
                worst = Math.Min(worst, Distance(v, a, b));
            }

            return worst;
        }

        private static void Lab(Vision v, PaletteColor c, out double L, out double A, out double B)
        {
            Simulate(v, SrgbToLinear(c.R), SrgbToLinear(c.G), SrgbToLinear(c.B), out double r, out double g, out double b);
            LinearToOklab(r, g, b, out L, out A, out B);
        }

        /// <summary>Picks <paramref name="count"/> colours, in greedy order (the first k are the best k-subset): every colour has linear luminance of at least
        /// <paramref name="minLuminance"/> (use <see cref="MinLuminanceFor"/>), Oklab lightness within [minL, maxL], chroma within [minC, maxC], lies inside the sRGB gamut
        /// (nothing is clipped) and is at least <paramref name="minWoodDistance"/> from the wood colour; each pick maximises its smallest distance to the colours
        /// already picked under all four kinds of vision. Deterministic.</summary>
        public static List<PaletteColor> Solve(int count, double minLuminance, PaletteColor wood, double minL = 0.55, double maxL = 0.92, double minC = 0.07, double maxC = 0.16, double minWoodDistance = 0.12)
        {
            var cand = new List<double[]>();   // r, g, b (sRGB) then four Lab triples
            Lab(Vision.Normal, wood, out double wl, out double wa, out double wb);
            for (double L = minL; L <= maxL + 1e-9; L += 0.02)
            {
                for (int hue = 0; hue < 360; hue += 3)
                {
                    for (double C = minC; C <= maxC + 1e-9; C += 0.01)
                    {
                        double a = C * Math.Cos(hue * Math.PI / 180.0), b = C * Math.Sin(hue * Math.PI / 180.0);
                        OklabToLinear(L, a, b, out double r, out double g, out double bl);
                        if (r < 0 || r > 1 || g < 0 || g > 1 || bl < 0 || bl > 1)
                        {
                            continue;   // out of gamut: skip, never clip
                        }

                        if (LuminanceLinear(r, g, bl) < minLuminance)
                        {
                            continue;
                        }

                        double dw = Math.Sqrt((L - wl) * (L - wl) + (a - wa) * (a - wa) + (b - wb) * (b - wb));
                        if (dw < minWoodDistance)
                        {
                            continue;
                        }

                        var row = new double[3 + 12];
                        row[0] = LinearToSrgb(r);
                        row[1] = LinearToSrgb(g);
                        row[2] = LinearToSrgb(bl);
                        for (int v = 0; v < 4; v++)
                        {
                            Simulate(Visions[v], r, g, bl, out double sr, out double sg, out double sb);
                            LinearToOklab(sr, sg, sb, out double l2, out double a2, out double b2);
                            row[3 + 3 * v] = l2;
                            row[4 + 3 * v] = a2;
                            row[5 + 3 * v] = b2;
                        }

                        cand.Add(row);
                    }
                }
            }

            var picked = new List<PaletteColor>();
            if (cand.Count == 0 || count <= 0)
            {
                return picked;
            }

            var minDist = new double[cand.Count];
            // the first pick is the candidate farthest from the wood
            int first = 0;
            double best = -1;
            for (int i = 0; i < cand.Count; i++)
            {
                double l = cand[i][3], a = cand[i][4], b = cand[i][5];
                double d = Math.Sqrt((l - wl) * (l - wl) + (a - wa) * (a - wa) + (b - wb) * (b - wb));
                if (d > best)
                {
                    best = d;
                    first = i;
                }
            }

            for (int i = 0; i < cand.Count; i++)
            {
                minDist[i] = double.MaxValue;
            }

            int chosen = first;
            while (picked.Count < count && chosen >= 0)
            {
                double[] c = cand[chosen];
                picked.Add(new PaletteColor(c[0], c[1], c[2]));
                int next = -1;
                double nextBest = -1;
                for (int i = 0; i < cand.Count; i++)
                {
                    double[] o = cand[i];
                    double d = double.MaxValue;
                    for (int v = 0; v < 4; v++)
                    {
                        double dl = o[3 + 3 * v] - c[3 + 3 * v], da = o[4 + 3 * v] - c[4 + 3 * v], db = o[5 + 3 * v] - c[5 + 3 * v];
                        d = Math.Min(d, Math.Sqrt(dl * dl + da * da + db * db));
                    }

                    minDist[i] = Math.Min(minDist[i], d);
                    if (minDist[i] > nextBest)
                    {
                        nextBest = minDist[i];
                        next = i;
                    }
                }

                chosen = next;
            }

            return picked;
        }
    }
}
