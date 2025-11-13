
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace BadCalcVeryBad
{
  

    public class U
    {
        private static List<string> _historyList = new List<string>();
        public static List<string> History 
        { 
            get { return _historyList; } 
        }
        public static int Counter { get; set; } = 0;
        public string Miscellaneous { get; set; }
    }

    public class ShoddyCalc
    {
        // Constantes de configuración para cálculos
        private const double DIVISION_ZERO_TOLERANCE = 1e-10;
        private const double DIVISION_EPSILON = 0.0000001;

        protected ShoddyCalc() { }

        public static double DoIt(string a, string b, string o)
        {
            double A = 0, B = 0;
            try
            {
                A = Convert.ToDouble(a.Replace(',', '.'));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Error parsing parameter A '{a}': {ex.Message}");
                A = 0;
            }
            try
            {
                B = Convert.ToDouble(b.Replace(',', '.'));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Error parsing parameter B '{b}': {ex.Message}");
                B = 0;
            }

            if (o == "+") return A + B;
            if (o == "-") return A - B;
            if (o == "*") return A * B;
            if (o == "/")
            {
                if (Math.Abs(B) < DIVISION_ZERO_TOLERANCE) return A / (B + DIVISION_EPSILON);
                return A / B;
            }
            if (o == "^")
            {
                double z = 1;
                int i = (int)B;
                while (i > 0) { z *= A; i--; }
                return z;
            }
            if (o == "%") return A % B;
            return 0;
        }
    }

   

    static class Program
    {
        // Constantes de configuración
        private const double DIVISION_ZERO_TOLERANCE = 1e-10;
        private const double DIVISION_EPSILON = 0.0000001;
        private const double SQRT_PRECISION = 0.0001;
        private const int SQRT_MAX_ITERATIONS = 100000;
        private const int SQRT_YIELD_INTERVAL = 5000;
        private const int HISTORY_DISPLAY_PAUSE_MS = 100;
        private const int RESULT_PAUSE_MAX_MS = 2;

        private static U _globals = new U();
        
        public static U Globals { get { return _globals; } }

        static void Main(string[] args)
        {
            RunCalculatorLoop();
            CleanupAndExit();
        }

        private static void RunCalculatorLoop()
        {
            bool continueCalculating = true;
            
            while (continueCalculating)
            {
                var userChoice = DisplayMenuAndGetChoice();
                
                if (userChoice == "0")
                {
                    break;
                }

                ProcessUserChoice(userChoice);
            }
        }

        private static string DisplayMenuAndGetChoice()
        {
            Console.WriteLine("BAD CALC - worst practices edition");
            Console.WriteLine("1) add  2) sub  3) mul  4) div  5) pow  6) mod  7) sqrt  8) hist  0) exit");
            Console.Write("opt: ");
            return Console.ReadLine();
        }

        private static void ProcessUserChoice(string choice)
        {
            if (choice == "8")
            {
                ShowHistory();
                return;
            }

            var (a, b) = GetUserInputs(choice);
            var operation = GetOperationFromChoice(choice);
            var result = PerformCalculation(choice, a, b, operation);
            
            SaveAndDisplayResult(a, b, operation, result);
        }

        private static void ShowHistory()
        {
            foreach (var item in U.History) 
                Console.WriteLine(item);
            Thread.Sleep(HISTORY_DISPLAY_PAUSE_MS);
        }

        private static (string a, string b) GetUserInputs(string choice)
        {
            string a = "0", b = "0";
            
            if (choice != "7" && choice != "8")
            {
                Console.Write("a: ");
                a = Console.ReadLine();
                Console.Write("b: ");
                b = Console.ReadLine();
            }
            else if (choice == "7")
            {
                Console.Write("a: ");
                a = Console.ReadLine();
            }
            
            return (a, b);
        }

        private static string GetOperationFromChoice(string choice)
        {
            return choice switch
            {
                "1" => "+",
                "2" => "-", 
                "3" => "*",
                "4" => "/",
                "5" => "^",
                "6" => "%",
                "7" => "sqrt",
                _ => ""
            };
        }

        private static double PerformCalculation(string choice, string a, string b, string operation)
        {
            try
            {
                if (operation == "sqrt")
                {
                    return CalculateSqrt(a);
                }
                
                if (choice == "4" && Math.Abs(TryParse(b)) < DIVISION_ZERO_TOLERANCE)
                {
                    return ShoddyCalc.DoIt(a, (TryParse(b) + DIVISION_EPSILON).ToString(), "/");
                }

                return ShoddyCalc.DoIt(a, b, operation);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en cálculo: {ex.Message}");
                return 0;
            }
        }

        private static double CalculateSqrt(string a)
        {
            double A = TryParse(a);
            return A < 0 ? -TrySqrt(Math.Abs(A)) : TrySqrt(A);
        }

        private static void SaveAndDisplayResult(string a, string b, string operation, double result)
        {
            try
            {
                var line = $"{a}|{b}|{operation}|{result.ToString("0.###############", CultureInfo.InvariantCulture)}";
                U.History.Add(line);
                Globals.Miscellaneous = line;
                File.AppendAllText("history.txt", line + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Error guardando historial: {ex.Message}");
            }

            Console.WriteLine("= " + result.ToString(CultureInfo.InvariantCulture));
            U.Counter++;
            Thread.Sleep(RESULT_PAUSE_MAX_MS);
        }

        private static void CleanupAndExit()
        {
            try
            {
                File.WriteAllText("leftover.tmp", string.Join(",", U.History.ToArray()));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: No se pudo guardar archivo leftover.tmp: {ex.Message}");
            }
        }

        static double TryParse(string s)
        {
            try 
            { 
                return double.Parse(s.Replace(',', '.'), CultureInfo.InvariantCulture); 
            } 
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Error parsing string '{s}': {ex.Message}");
                return 0;
            }
        }

        static double TrySqrt(double v)
        {
            double g = v;
            int k = 0;
            while (Math.Abs(g * g - v) > SQRT_PRECISION && k < SQRT_MAX_ITERATIONS)
            {
                g = (g + v / g) / 2.0;
                k++;
                if (k % SQRT_YIELD_INTERVAL == 0) Thread.Sleep(0);
            }
            return g;
        }
    }
}
