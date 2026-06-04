using System.Numerics;
using OpenTK.Mathematics;

namespace Utils;

public static class MathUtil
{
    public const double Epsilon = 1e-6;

    public static List<double> SolveQuartic(
        double a4,
        double a3,
        double a2,
        double a1,
        double a0)
    {
        // Degenerate cases
        if (Math.Abs(a4) < Epsilon)
            return SolveCubic(a3, a2, a1, a0);

        // Normalize:
        // x^4 + b*x^3 + c*x^2 + d*x + e = 0
        double b = a3 / a4;
        double c = a2 / a4;
        double d = a1 / a4;
        double e = a0 / a4;

        // Cauchy root bound: all roots are inside this radius
        double radius = 1.0 + Math.Max(
            Math.Max(Math.Abs(b), Math.Abs(c)),
            Math.Max(Math.Abs(d), Math.Abs(e)));

        Complex[] roots =
        {
            Complex.FromPolarCoordinates(radius, 0.0),
            Complex.FromPolarCoordinates(radius, Math.PI / 2.0),
            Complex.FromPolarCoordinates(radius, Math.PI),
            Complex.FromPolarCoordinates(radius, 3.0 * Math.PI / 2.0)
        };

        // Durand-Kerner iteration
        for (int iter = 0; iter < 100; iter++)
        {
            bool converged = true;

            for (int i = 0; i < 4; i++)
            {
                Complex denominator = Complex.One;

                for (int j = 0; j < 4; j++)
                {
                    if (i != j)
                        denominator *= roots[i] - roots[j];
                }

                if (denominator.Magnitude < Epsilon)
                    denominator = new Complex(Epsilon, Epsilon);

                Complex value = EvaluateQuartic(roots[i], b, c, d, e);
                Complex delta = value / denominator;

                roots[i] -= delta;

                if (delta.Magnitude > 1e-12)
                    converged = false;
            }

            if (converged)
                break;
        }

        var realRoots = new List<double>();

        foreach (Complex root in roots)
        {
            if (Math.Abs(root.Imaginary) < 1e-7)
            {
                double real = root.Real;

                // Avoid duplicates from repeated roots
                bool duplicate = false;
                foreach (double existing in realRoots)
                {
                    if (Math.Abs(existing - real) < 1e-6)
                    {
                        duplicate = true;
                        break;
                    }
                }

                if (!duplicate)
                    realRoots.Add(real);
            }
        }

        realRoots.Sort();
        return realRoots;
    }

    private static Complex EvaluateQuartic(
        Complex x,
        double b,
        double c,
        double d,
        double e)
    {
        // x^4 + b*x^3 + c*x^2 + d*x + e
        return (((x + b) * x + c) * x + d) * x + e;
    }

    public static List<double> SolveQuadratic(double a, double b, double c)
    {
        if (Math.Abs(a) < Epsilon)
            return SolveLinear(b, c);

        double discriminant = b * b - 4.0 * a * c;

        if (discriminant < -Epsilon)
            return new List<double>();

        if (Math.Abs(discriminant) < Epsilon)
            return new List<double> { -b / (2.0 * a) };

        double sqrtD = Math.Sqrt(discriminant);

        double x1 = (-b - sqrtD) / (2.0 * a);
        double x2 = (-b + sqrtD) / (2.0 * a);

        return x1 < x2
            ? new List<double> { x1, x2 }
            : new List<double> { x2, x1 };
    }

    private static List<double> SolveLinear(double a, double b)
    {
        if (Math.Abs(a) < Epsilon)
            return new List<double>();

        return new List<double> { -b / a };
    }

    private static List<double> SolveCubic(double a, double b, double c, double d)
    {
        if (Math.Abs(a) < Epsilon)
            return SolveQuadratic(b, c, d);

        // Normalize:
        // x^3 + A*x^2 + B*x + C = 0
        double A = b / a;
        double B = c / a;
        double C = d / a;

        // Depressed cubic:
        // y^3 + p*y + q = 0
        // x = y - A / 3
        double p = B - A * A / 3.0;
        double q = 2.0 * A * A * A / 27.0 - A * B / 3.0 + C;

        double discriminant = q * q / 4.0 + p * p * p / 27.0;

        var roots = new List<double>();

        if (discriminant > Epsilon)
        {
            double sqrtD = Math.Sqrt(discriminant);

            double u = CubeRoot(-q / 2.0 + sqrtD);
            double v = CubeRoot(-q / 2.0 - sqrtD);

            roots.Add(u + v - A / 3.0);
        }
        else if (Math.Abs(discriminant) < Epsilon)
        {
            double u = CubeRoot(-q / 2.0);

            roots.Add(2.0 * u - A / 3.0);
            roots.Add(-u - A / 3.0);
        }
        else
        {
            double r = 2.0 * Math.Sqrt(-p / 3.0);
            double phi = Math.Acos(
                (3.0 * q / (2.0 * p)) * Math.Sqrt(-3.0 / p));

            roots.Add(r * Math.Cos(phi / 3.0) - A / 3.0);
            roots.Add(r * Math.Cos((phi + 2.0 * Math.PI) / 3.0) - A / 3.0);
            roots.Add(r * Math.Cos((phi + 4.0 * Math.PI) / 3.0) - A / 3.0);
        }

        roots.Sort();
        return RemoveDuplicates(roots);
    }

    private static double CubeRoot(double x)
    {
        return x >= 0.0
            ? Math.Pow(x, 1.0 / 3.0)
            : -Math.Pow(-x, 1.0 / 3.0);
    }

    private static List<double> RemoveDuplicates(List<double> values)
    {
        var result = new List<double>();

        foreach (double value in values)
        {
            bool duplicate = false;

            foreach (double existing in result)
            {
                if (Math.Abs(existing - value) < 1e-6)
                {
                    duplicate = true;
                    break;
                }
            }

            if (!duplicate)
                result.Add(value);
        }

        return result;
    }
}
