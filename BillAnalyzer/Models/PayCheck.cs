namespace BillAnalyzer.Models;

public class PayCheck
{
    public int Id { get; set; }
    public DateOnly PayDate { get; set; }
    public float Amount { get; set; }
}