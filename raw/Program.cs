using HearthMirror.;
using HearthDb;
using System.Text.Json;

Console.WriteLine("Press Enter to continue...");
Console.ReadLine();
Console.WriteLine("Hello, World!");

try
{
    var coll = await Task.Run(() =>
    {
        using (_ = Reflection.ClientReadTimeout(10000))
        {
            return Reflection.Client.GetFullCollection();
        }
    });

    if (coll?.Cards.Any() ?? false)
    {
        Console.WriteLine("Collection is not empty");
    }
    else
    {
        Console.WriteLine("Collection is empty");
        return;
    }

    var myCards = SummarizeCollection(coll);
    var CardsByName = HearthDb.Cards.All.Select(card => card.Value).DistinctBy(card => card.Name).ToDictionary(card => card.Name);
    WriteCardDetailsToJson(myCards, "collection.json");


    void WriteCardDetailsToJson(List<CardSummary> myCards, string filePath)
    {
        var cardDetailsList = myCards.Select(cardSummary => new CardDetail
        {
            Name = cardSummary.Name,
            TotalCount = cardSummary.TotalCount,
            CardDetails = new CardDetailFiltered
            {
                Name = CardsByName[cardSummary.Name].Name,
                Text = CardsByName[cardSummary.Name].Text?
                .Replace("\n", " ")
                .Replace("<b>", "")
                .Replace("</b>", ""),
                Cost = CardsByName[cardSummary.Name].Cost,
                Attack = CardsByName[cardSummary.Name].Attack,
                Health = CardsByName[cardSummary.Name].Health,
                Durability = CardsByName[cardSummary.Name].Durability,
                Type = CardsByName[cardSummary.Name].Type.ToString(),
                Class = CardsByName[cardSummary.Name].Class.ToString(),
                Rarity = CardsByName[cardSummary.Name].Rarity.ToString()
            }
        }).ToList();

        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        var json = JsonSerializer.Serialize(cardDetailsList, options);
        File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
    }

    List<CardSummary> SummarizeCollection(HearthMirror.Objects.Collection coll)
    {
        Console.WriteLine("Summarizing collection...");
        var cardCollection = coll.Cards.Select(card => new { Name = HearthDb.Cards.All[card.Id].Name ?? "Unamed", card.Count }).Where(card => card.Count > 0);
        var distinctCardCollection = cardCollection.DistinctBy(card => card.Name);
        var groupedCardCollection = distinctCardCollection.GroupBy(card => card.Name);
        var summarizedCards = groupedCardCollection.Select(group => new CardSummary
        {
            Name = group.Key,
            TotalCount = group.Sum(card => card.Count)
        })
            .Where(summary => summary.TotalCount > 0)
            .ToList();

        return summarizedCards;
    }
}

catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}
public class CardSummary
{
    public string Name { get; set; }
    public int TotalCount { get; set; }
}

public class CardDetail
{
    public string Name { get; set; }
    public int TotalCount { get; set; }
    public CardDetailFiltered CardDetails { get; set; }
}

public class CardDetailFiltered
{
    public string Name { get; set; }
    public string Text { get; set; }
    public int? Cost { get; set; }
    public int? Attack { get; set; }
    public int? Health { get; set; }
    public int? Durability { get; set; }
    public string Type { get; set; }
    public string Class { get; set; }
    public string Rarity { get; set; }
}
