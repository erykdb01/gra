using System;

// Offline synthesis of the ambience layers. Pure C# maths (no Unity API), so it can run on a worker thread.
// Design rules for comfort: everything is band-limited (nothing above ~7 kHz), there are no steady tones in the
// mid/high range, noise is pink or brown, loops are cross-faded so no seam is audible, and every clip is normalised.
public static class AudioDSP
{
    public struct Buf
    {
        public float[] data;     // interleaved
        public int channels;
        public int rate;
    }

    static double Lp(double fc, int rate) { return 1.0 - Math.Exp(-2.0 * Math.PI * fc / rate); }

    sealed class Pink
    {
        double b0, b1, b2;
        public double Next(double w)
        {
            b0 = 0.99765 * b0 + w * 0.0990460;
            b1 = 0.96300 * b1 + w * 0.2965164;
            b2 = 0.57000 * b2 + w * 1.0526913;
            return (b0 + b1 + b2 + w * 0.1848) * 0.22;
        }
    }

    static double Noise(Random r) { return r.NextDouble() * 2.0 - 1.0; }

    static void LowPass(double[] x, double fc, int rate)
    {
        double a = Lp(fc, rate), y = 0;
        for (int i = 0; i < x.Length; i++) { y += a * (x[i] - y); x[i] = y; }
    }

    // Cross-fades the tail into the head so that the looped clip has no audible seam.
    static double[] Loopify(double[] x, int n, int f)
    {
        double[] y = new double[n];
        for (int i = 0; i < n; i++) y[i] = x[i];
        for (int i = 0; i < f; i++)
        {
            double t = (double)i / f;
            double a = Math.Sin(t * Math.PI * 0.5), b = Math.Cos(t * Math.PI * 0.5);
            y[i] = x[i] * a + x[n + i] * b;
        }
        return y;
    }

    static void AddPlink(double[] x, Random rng, int pos, int rate, double f0, double amp)
    {
        int len = (int)(0.16 * rate);
        double ph = 0;
        for (int j = 0; j < len; j++)
        {
            double t = (double)j / rate;
            double f = f0 * (1.0 - 0.15 * Math.Min(1.0, t / 0.12));
            ph += 2.0 * Math.PI * f / rate;
            double v = Math.Sin(ph) * Math.Exp(-t * 45.0) * amp * Math.Min(1.0, j / (0.002 * rate));
            int i0 = pos + j;
            if (i0 < x.Length) x[i0] += v;
            int e1 = pos + j + (int)(0.11 * rate), e2 = pos + j + (int)(0.23 * rate);
            if (e1 < x.Length) x[e1] += v * 0.32;
            if (e2 < x.Length) x[e2] += v * 0.12;
        }
    }

    static void AddDrop(double[] x, Random rng, int pos, int rate, double amp, double bright)
    {
        int len = (int)(rate * (0.010 + 0.030 * rng.NextDouble()));
        double decay = 110.0 + 220.0 * rng.NextDouble();
        double a = Lp(1200.0 + 3600.0 * bright * rng.NextDouble(), rate), y = 0;
        for (int j = 0; j < len; j++)
        {
            int i0 = pos + j;
            if (i0 >= x.Length) break;
            y += a * (Noise(rng) - y);
            x[i0] += y * amp * 2.2 * Math.Exp(-(double)j / rate * decay);
        }
    }

    static Buf Finish(double[][] ch, double rms, int rate)
    {
        int n = ch[0].Length, c = ch.Length;
        float[] d = new float[n * c];
        double sum = 0;
        for (int k = 0; k < c; k++) for (int i = 0; i < n; i++) sum += ch[k][i] * ch[k][i];
        double cur = Math.Sqrt(sum / (n * c)) + 1e-9;
        double g = rms / cur;
        for (int k = 0; k < c; k++)
            for (int i = 0; i < n; i++)
                d[i * c + k] = (float)(Math.Tanh(ch[k][i] * g / 0.85) * 0.85);   // soft limiter: no clipping, no spikes
        return new Buf { data = d, channels = c, rate = rate };
    }

    // ---------------------------------------------------------------- rain
    // bed: level of the soft pink-noise wash; drops: drops per second; bright: 0..1 brightness of the drops;
    // plinks: drips per second (water on metal / puddles); lpHz: final low-pass.
    public static Buf Rain(int seed, double secs, int rate, double bed, double drops, double bright, double plinks, double lpHz)
    {
        int N = (int)(secs * rate), F = (int)(1.5 * rate), M = N + F;
        double[][] ch = new double[2][];
        for (int c = 0; c < 2; c++)
        {
            var rng = new Random(seed * 31 + c * 7 + 1);
            double[] x = new double[M];
            var pink = new Pink();
            double lpA = Lp(4200, rate), lowA = Lp(220, rate), y1 = 0, y0 = 0;
            double k1 = Math.Max(1, Math.Round(secs * 0.06)), k2 = Math.Max(1, Math.Round(secs * 0.17));
            double ph1 = rng.NextDouble() * 6.28, ph2 = rng.NextDouble() * 6.28;
            if (bed > 0)
            {
                for (int i = 0; i < M; i++)
                {
                    double p = pink.Next(Noise(rng));
                    y1 += lpA * (p - y1);
                    y0 += lowA * (y1 - y0);
                    double mod = 1.0 + 0.12 * Math.Sin(2 * Math.PI * k1 * i / N + ph1) + 0.08 * Math.Sin(2 * Math.PI * k2 * i / N + ph2);
                    x[i] = (y1 - y0) * mod * bed;
                }
            }
            int nd = (int)(drops * M / rate);
            for (int d = 0; d < nd; d++)
            {
                double r = rng.NextDouble();
                AddDrop(x, rng, rng.Next(M), rate, 0.03 + 0.14 * r * r * r, bright);
            }
            int np = (int)(plinks * M / rate);
            for (int d = 0; d < np; d++)
                AddPlink(x, rng, rng.Next(M), rate, 500.0 + 600.0 * rng.NextDouble(), 0.03 + 0.05 * rng.NextDouble());
            LowPass(x, lpHz, rate);
            LowPass(x, lpHz, rate);
            ch[c] = Loopify(x, N, F);
        }
        return Finish(ch, 0.11, rate);
    }

    // ---------------------------------------------------------------- station air: deep air + slow wind
    public static Buf Station(int seed, double secs, int rate)
    {
        int N = (int)(secs * rate), F = (int)(1.5 * rate), M = N + F;
        double[][] ch = new double[2][];
        for (int c = 0; c < 2; c++)
        {
            var rng = new Random(seed * 17 + c * 5 + 3);
            double[] x = new double[M];
            var pink = new Pink();
            double brown = 0, aLow = Lp(150, rate), yLow = 0, a1 = Lp(800, rate), a2 = Lp(250, rate), w1 = 0, w2 = 0;
            double ka = Math.Max(1, Math.Round(secs * 0.045)), kb = Math.Max(1, Math.Round(secs * 0.11));
            double pa = rng.NextDouble() * 6.28, pb = rng.NextDouble() * 6.28;
            for (int i = 0; i < M; i++)
            {
                double w = Noise(rng);
                brown = (brown + 0.02 * w) * 0.998;
                yLow += aLow * (brown - yLow);
                double p = pink.Next(w);
                w1 += a1 * (p - w1); w2 += a2 * (p - w2);
                double gust = 0.5 + 0.5 * Math.Sin(2 * Math.PI * ka * i / N + pa);
                gust = gust * gust * (0.6 + 0.4 * Math.Sin(2 * Math.PI * kb * i / N + pb));
                x[i] = yLow * 2.0 + (w1 - w2) * gust * 0.9;
            }
            int np = (int)(0.12 * M / rate);
            for (int d = 0; d < np; d++) AddPlink(x, rng, rng.Next(M), rate, 450.0 + 500.0 * rng.NextDouble(), 0.05 + 0.05 * rng.NextDouble());
            ch[c] = Loopify(x, N, F);
        }
        return Finish(ch, 0.10, rate);
    }

    // ---------------------------------------------------------------- booth room tone (all energy below ~300 Hz)
    public static Buf RoomTone(int seed, double secs, int rate)
    {
        int N = (int)(secs * rate), F = (int)(1.0 * rate), M = N + F;
        double[][] ch = new double[2][];
        for (int c = 0; c < 2; c++)
        {
            var rng = new Random(seed * 13 + c * 11 + 1);
            double[] x = new double[M];
            var pink = new Pink();
            double brown = 0, aLow = Lp(120, rate), yLow = 0, aAir = Lp(300, rate), yAir = 0;
            for (int i = 0; i < M; i++)
            {
                double w = Noise(rng);
                brown = (brown + 0.02 * w) * 0.998;
                yLow += aLow * (brown - yLow);
                yAir += aAir * (pink.Next(w) - yAir);
                x[i] = yLow * 2.2 + yAir * 0.45;
            }
            double[] y = Loopify(x, N, F);
            // coherent low tones are added after the cross-fade; every frequency has whole cycles in the loop
            double f1 = Math.Round(55.0 * secs) / secs, f2 = Math.Round(82.5 * secs) / secs;
            double k = Math.Max(1, Math.Round(secs * 0.1));
            for (int i = 0; i < N; i++)
            {
                double t = (double)i / rate;
                double am = 0.7 + 0.3 * Math.Sin(2 * Math.PI * k * i / N);
                y[i] += (0.09 * Math.Sin(2 * Math.PI * f1 * t) * am + 0.04 * Math.Sin(2 * Math.PI * f2 * t + c)) ;
            }
            ch[c] = y;
        }
        return Finish(ch, 0.09, rate);
    }

    // ---------------------------------------------------------------- idling train: low diesel-like rumble
    public static Buf TrainIdle(int seed, double secs, int rate)
    {
        int N = (int)(secs * rate), F = (int)(1.0 * rate), M = N + F;
        double[][] ch = new double[2][];
        for (int c = 0; c < 2; c++)
        {
            var rng = new Random(seed * 19 + c * 3 + 1);
            double[] x = new double[M];
            double brown = 0, aLow = Lp(95, rate), yLow = 0;
            double kk = Math.Round(9.0 * secs);
            for (int i = 0; i < M; i++)
            {
                brown = (brown + 0.02 * Noise(rng)) * 0.998;
                yLow += aLow * (brown - yLow);
                double am = 0.72 + 0.28 * Math.Sin(2 * Math.PI * kk * i / N + c * 0.7);
                x[i] = yLow * am;
            }
            ch[c] = Loopify(x, N, F);
        }
        return Finish(ch, 0.10, rate);
    }

    // ---------------------------------------------------------------- rails: soft rolling rumble and muffled clacks
    public static Buf Rails(int seed, double secs, int rate)
    {
        int N = (int)(secs * rate);
        double[][] ch = new double[2][];
        for (int c = 0; c < 2; c++)
        {
            var rng = new Random(seed * 23 + c + 1);
            int F = (int)(0.8 * rate);
            double[] raw = new double[N + F];
            double brown = 0, aLow = Lp(140, rate), yLow = 0;
            for (int i = 0; i < N + F; i++) { brown = (brown + 0.02 * Noise(rng)) * 0.998; yLow += aLow * (brown - yLow); raw[i] = yLow * 1.6; }
            double[] x = Loopify(raw, N, F);
            // wheel clacks every 0.5 s (whole number per loop), two-bogie pattern, low-passed so they thud rather than click
            double a = Lp(900, rate);
            int beats = (int)(secs / 0.5);
            for (int b = 0; b < beats; b++)
            {
                double[] offs = { 0.0, 0.085, 0.23, 0.315 };
                double[] amps = { 1.0, 0.7, 0.85, 0.6 };
                for (int o = 0; o < 4; o++)
                {
                    int p = (int)((b * 0.5 + offs[o]) * rate);
                    double yb = 0;
                    for (int j = 0; j < (int)(0.05 * rate); j++)
                    {
                        int i0 = (p + j) % N;
                        yb += a * (Noise(rng) - yb);
                        x[i0] += yb * amps[o] * 1.3 * Math.Exp(-(double)j / rate * 70.0);
                    }
                }
            }
            ch[c] = x;
        }
        return Finish(ch, 0.10, rate);
    }

    // ---------------------------------------------------------------- low unease layer (nothing above ~150 Hz)
    public static Buf HorrorDrone(int seed, double secs, int rate)
    {
        int N = (int)(secs * rate), F = (int)(1.5 * rate), M = N + F;
        double[][] ch = new double[2][];
        for (int c = 0; c < 2; c++)
        {
            var rng = new Random(seed * 29 + c * 13 + 1);
            double[] x = new double[M];
            double brown = 0, aLow = Lp(110, rate), yLow = 0;
            double k = Math.Max(1, Math.Round(secs * 0.09));
            for (int i = 0; i < M; i++)
            {
                brown = (brown + 0.02 * Noise(rng)) * 0.998;
                yLow += aLow * (brown - yLow);
                x[i] = yLow * (0.55 + 0.45 * Math.Sin(2 * Math.PI * k * i / N + c));
            }
            double[] y = Loopify(x, N, F);
            double f1 = Math.Round(41.2 * secs) / secs, f2 = f1 + Math.Round(1.8 * secs) / secs, f3 = Math.Round(61.7 * secs) / secs;
            for (int i = 0; i < N; i++)
            {
                double t = (double)i / rate;
                double am = 0.6 + 0.4 * Math.Sin(2 * Math.PI * k * i / N);
                y[i] += (0.12 * Math.Sin(2 * Math.PI * f1 * t) + 0.12 * Math.Sin(2 * Math.PI * f2 * t) + 0.05 * Math.Sin(2 * Math.PI * f3 * t + c)) * am;
            }
            ch[c] = y;
        }
        return Finish(ch, 0.10, rate);
    }

    // ================================================================ distant one-shots (mono)

    static Buf Mono(double[] x, int rate, double rms)
    {
        return Finish(new[] { x }, rms, rate);
    }

    // a train passing far away: a swell of rumble with a slowly changing rhythm of clacks
    public static Buf DistantTrain(int seed, int rate)
    {
        var rng = new Random(seed);
        double secs = 16;
        int n = (int)(secs * rate);
        double[] x = new double[n];
        double brown = 0, aLow = Lp(130, rate), yLow = 0;
        for (int i = 0; i < n; i++)
        {
            double t = (double)i / n;
            double e = Math.Pow(Math.Sin(Math.PI * t), 2.0);
            brown = (brown + 0.02 * Noise(rng)) * 0.998;
            yLow += aLow * (brown - yLow);
            x[i] = yLow * e * 2.0;
        }
        double a = Lp(800, rate);
        double tc = 1.0;
        while (tc < secs - 1.5)
        {
            double t = tc / secs;
            double e = Math.Pow(Math.Sin(Math.PI * t), 1.4);
            double[] offs = { 0.0, 0.09, 0.24, 0.33 };
            for (int o = 0; o < 4; o++)
            {
                int p = (int)((tc + offs[o]) * rate);
                double yb = 0;
                for (int j = 0; j < (int)(0.05 * rate); j++)
                {
                    if (p + j >= n) break;
                    yb += a * (Noise(rng) - yb);
                    x[p + j] += yb * e * 0.9 * Math.Exp(-(double)j / rate * 70.0);
                }
            }
            tc += 0.5 - 0.06 * Math.Sin(Math.PI * t);
        }
        return Mono(x, rate, 0.12);
    }

    // a metal structure creaking in the wind
    public static Buf Creak(int seed, int rate)
    {
        var rng = new Random(seed);
        double secs = 1.5;
        int n = (int)(secs * rate);
        double[] x = new double[n];
        double ph = 0, f0 = 62 + 30 * rng.NextDouble();
        for (int i = 0; i < n; i++)
        {
            double t = (double)i / rate;
            double f = f0 * (1.0 + 0.5 * t / secs) + 5.0 * Math.Sin(2 * Math.PI * 7 * t);
            ph += f / rate;
            double saw = 2.0 * (ph - Math.Floor(ph)) - 1.0;
            double env = Math.Pow(Math.Sin(Math.PI * t / secs), 1.5) * (0.65 + 0.35 * Math.Sin(2 * Math.PI * 11 * t));
            x[i] = saw * env;
        }
        LowPass(x, 550, rate); LowPass(x, 700, rate);
        return Mono(x, rate, 0.10);
    }

    public static Buf Drip(int seed, int rate)
    {
        var rng = new Random(seed);
        double[] x = new double[(int)(0.7 * rate)];
        AddPlink(x, rng, 0, rate, 450.0 + 350.0 * rng.NextDouble(), 0.9);
        return Mono(x, rate, 0.07);
    }

    // a relay / points changing somewhere along the line
    public static Buf Relay(int seed, int rate)
    {
        var rng = new Random(seed);
        int n = (int)(1.1 * rate);
        double[] x = new double[n];
        double a = Lp(1400, rate);
        double[] at = { 0.0, 0.34, 0.62 };
        double[] gain = { 1.0, 0.55, 0.8 };
        for (int k = 0; k < at.Length; k++)
        {
            int p = (int)(at[k] * rate);
            double yb = 0;
            for (int j = 0; j < (int)(0.4 * rate); j++)
            {
                if (p + j >= n) break;
                double t = (double)j / rate;
                yb += a * (Noise(rng) - yb);
                x[p + j] += (Math.Sin(2 * Math.PI * 58 * t) * Math.Exp(-t * 22) * 0.9 + yb * Math.Exp(-t * 200) * 0.9) * gain[k];
            }
        }
        return Mono(x, rate, 0.09);
    }

    public static Buf Gust(int seed, int rate)
    {
        var rng = new Random(seed);
        double secs = 5;
        int n = (int)(secs * rate);
        double[] x = new double[n];
        var pink = new Pink();
        double a1 = Lp(900, rate), a2 = Lp(250, rate), w1 = 0, w2 = 0;
        for (int i = 0; i < n; i++)
        {
            double t = (double)i / n;
            double p = pink.Next(Noise(rng));
            w1 += a1 * (p - w1); w2 += a2 * (p - w2);
            x[i] = (w1 - w2) * Math.Pow(Math.Sin(Math.PI * t), 2.0);
        }
        return Mono(x, rate, 0.09);
    }

    // a low, far-away impact (something heavy set down in another part of the station)
    public static Buf Thud(int seed, int rate)
    {
        var rng = new Random(seed);
        int n = (int)(1.6 * rate);
        double[] x = new double[n];
        double a = Lp(500, rate), yb = 0;
        for (int i = 0; i < n; i++)
        {
            double t = (double)i / rate;
            yb += a * (Noise(rng) - yb);
            x[i] = Math.Sin(2 * Math.PI * 46 * t) * Math.Exp(-t * 9) * 0.9 + yb * Math.Exp(-t * 60) * 0.6;
            int e = i - (int)(0.21 * rate);
            if (e >= 0) x[i] += Math.Sin(2 * Math.PI * 41 * (e / (double)rate)) * Math.Exp(-(e / (double)rate) * 11) * 0.35;
        }
        return Mono(x, rate, 0.09);
    }
}
