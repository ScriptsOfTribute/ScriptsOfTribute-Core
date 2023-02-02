using ScriptsOfTribute;

namespace Bots;

public class Apriori
{

    private List<string> allPatrons = new List<string> { "ANSEI", "DUKE_OF_CROWS", "RAJHIN", "PSIJIC", "ORGNUM", "HLAALU", "PELIN", "RED_EAGLE" };
    private Dictionary<List<string>, int> support = new Dictionary<List<string>, int>();
    private int rule_lenght = 4;

    private bool CheckIfContainsTheSameElements(List<string> l1, List<string> l2)
    {
        foreach (string elem in l1)
        {
            if (!l2.Contains(elem))
            {
                return false;
            }
        }
        return true;
    }

    private (bool, string) ConstainsOneAdditionalElement(List<string> l1, List<string> l2)
    {
        int differentCounter = 0;
        string difference = "";
        foreach (string elem in l1)
        {
            if (!l2.Contains(elem))
            {
                differentCounter += 1;
                difference = elem;
            }
        }
        if (differentCounter == 1)
        {
            return (true, difference);
        }
        return (false, "");
    }

    private bool SetInHistoricalData(List<string> set, List<string> historicalData)
    {
        bool allFound = true;
        foreach (string elem in set)
        {
            if (!historicalData.Contains(elem))
            {
                allFound = false;
                break;
            }
        }
        return allFound;
    }

    private IEnumerable<IEnumerable<T>> GetKCombs<T>(IEnumerable<T> list, int length) where T : IComparable
    {
        if (length == 1) return list.Select(t => new T[] { t });
        return GetKCombs(list, length - 1)
            .SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0),
                (t1, t2) => t1.Concat(new T[] { t2 }));
    }

    private string GetGoodFittingPatron(List<string> patron, double confidence)
    {
        string bestPatron = "random";
        int supportx = -1;
        if (patron.Count == 0)
        {
            foreach (KeyValuePair<List<string>, int> entry in support)
            {
                if (entry.Key.Count == 1 && entry.Value > supportx)
                {
                    supportx = entry.Value;
                    bestPatron = entry.Key[0];
                }
            }
            return bestPatron;
        }
        foreach (KeyValuePair<List<string>, int> entry in support)
        {
            if (entry.Key.Count == patron.Count && CheckIfContainsTheSameElements(entry.Key, patron))
            {
                supportx = entry.Value;
            }
        }
        if (supportx == -1)
        {
            return bestPatron;
        }
        double maxConfidenceFounded = 0.0;
        foreach (KeyValuePair<List<string>, int> entry in support)
        {
            if (entry.Key.Count == patron.Count + 1)
            {
                (bool goodSet, string effect) = ConstainsOneAdditionalElement(entry.Key, patron);
                if (goodSet && maxConfidenceFounded < ((double)entry.Value / (double)supportx))
                {
                    maxConfidenceFounded = (double)entry.Value / (double)supportx;
                    bestPatron = effect;
                }
            }
        }
        if (maxConfidenceFounded >= confidence)
        {
            return bestPatron;
        }
        return "random";
    }

    private PatronId? IdFromString(string patron)
    {
        return patron switch
        {
            "HLAALU" => PatronId.HLAALU,
            "RED_EAGLE" => PatronId.RED_EAGLE,
            "DUKE_OF_CROWS" => PatronId.DUKE_OF_CROWS,
            "ANSEI" => PatronId.ANSEI,
            "PSIJIC" => PatronId.PSIJIC,
            "PELIN" => PatronId.PELIN,
            "RAJHIN" => PatronId.RAJHIN,
            "ORGNUM" => PatronId.ORGNUM,
            "random" => null
        };
    }

    private void ReadSupportFromData(string historicalDataPath, int supportTreshold)
    {
        string[] lines = System.IO.File.ReadAllLines(historicalDataPath);
        List<List<string>> historicalData = new List<List<string>>();
        foreach (string line in lines)
        {
            historicalData.Add(line.Split(',').ToList());
        }
        for (int i = 1; i <= rule_lenght; i++)
        {
            IEnumerable<IEnumerable<string>> combinations = GetKCombs(allPatrons, i);
            List<List<string>> combinations_ = combinations.Select(z => z.Select(y => y).ToList()).ToList();
            foreach (var set in combinations_)
            {
                int counter = 0;
                foreach (List<string> data in historicalData)
                {
                    if (SetInHistoricalData(set, data))
                    {
                        counter += 1;
                    }
                }
                if (counter >= supportTreshold)
                {
                    support[set] = counter;
                }
            }
        }
    }

    public PatronId? AprioriBestChoice(List<PatronId> availablePatrons, string historicalDataPath, int supportTreshold, double confidenceTreshold)
    {
        ReadSupportFromData(historicalDataPath, supportTreshold);
        List<string> patrons = new List<string>();
        List<string> patronsToChoose = availablePatrons.Select(n => n.ToString()).ToList();
        foreach (string patron in allPatrons)
        {
            if (!patronsToChoose.Contains(patron))
            {
                patrons.Add(patron);
            }
        }
        string selectedPatron = GetGoodFittingPatron(patrons, confidenceTreshold);
        return IdFromString(selectedPatron);
    }
}