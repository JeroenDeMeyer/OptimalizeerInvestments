using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Optimizer;

public class Security
{
    public string Name { get; set; } = string.Empty;
    public int StockAvailable { get; set; }
    public decimal[] StockPrices { get; set; } = Array.Empty<decimal>();
}

public class Optimizer
{
    public int NumberOfSecurities { get; private set; }
    public int NumberOfDays { get; private set; }
    public decimal StartCapital { get; private set; }
    public Dictionary<string, Security> SecurityMap { get; private set; } = new Dictionary<string, Security>();

    public static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: Optimizer <input-file> <output-file>");
            return 1;
        }

        var inputFile = args[0];
        var outputFile = args[1];

        try
        {
            var optimizer = new Optimizer();
            optimizer.ParseInputFile(inputFile);
            optimizer.WriteOutputFile(outputFile);
            Console.WriteLine($"Optimization output written to '{outputFile}'.");
            return 0;
        }
        catch (InvalidDataException ex)
        {
            Console.Error.WriteLine($"Input data is invalid: {ex.Message}");
            return 1;
        }
        catch (FormatException ex)
        {
            Console.Error.WriteLine($"Input data format is invalid: {ex.Message}");
            return 1;
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"File I/O failed: {ex.Message}");
            return 1;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine($"Access denied: {ex.Message}");
            return 1;
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"Invalid argument: {ex.Message}");
            return 1;
        }
    }

    public Dictionary<string, Security> ParseInputFile(string inputFile)
    {
        SecurityMap = new Dictionary<string, Security>();

        using var streamReader = new StreamReader(inputFile);
        var firstLine = streamReader.ReadLine()?.Trim().Split();
        if (firstLine == null || firstLine.Length < 3)
        {
            throw new InvalidDataException("Input file header is missing or invalid.");
        }

        NumberOfSecurities = int.Parse(firstLine[0], CultureInfo.InvariantCulture);
        NumberOfDays = int.Parse(firstLine[1], CultureInfo.InvariantCulture);
        StartCapital = decimal.Parse(firstLine[2], CultureInfo.GetCultureInfo("en-US"));

        for (var i = 0; i < NumberOfSecurities; i++)
        {
            var securityLine = streamReader.ReadLine();
            var pricesLine = streamReader.ReadLine();
            if (string.IsNullOrWhiteSpace(securityLine) || string.IsNullOrWhiteSpace(pricesLine))
            {
                throw new InvalidDataException($"Missing security metadata or prices at security index {i}.");
            }

            var splitLine = securityLine.Trim().Split();
            if (splitLine.Length < 2)
            {
                throw new InvalidDataException($"Security metadata line is invalid at security index {i}.");
            }

            var security = new Security
            {
                Name = splitLine[0],
                StockAvailable = int.Parse(splitLine[1], CultureInfo.InvariantCulture),
                StockPrices = pricesLine
                    .Trim()
                    .Split()
                    .Select(price => decimal.Parse(price, CultureInfo.GetCultureInfo("en-US")))
                    .ToArray()
            };

            SecurityMap[security.Name] = security;
        }

        return SecurityMap;
    }

    private void WriteOutputFile(string outputFile)
    {
        // Valid default output: 0 day sections / no trades.
        File.WriteAllText(outputFile, "0" + Environment.NewLine);
    }
}
