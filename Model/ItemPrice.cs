public class ItemPrice
{
    public int Id { get; set; }
    public bool Whitelisted { get; set; }
    public TradeInfo Buys { get; set; }
    public TradeInfo Sells { get; set; }
}

public class TradeInfo
{
    public int Quantity { get; set; }
    public int Unit_Price { get; set; }
}