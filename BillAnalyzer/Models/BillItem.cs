
namespace BillAnalyzer.Models;

public class BillItem
{
    public int Id { get; set; }
    public DateOnly BillDate { get; set; }
    public float Balance { get; set; }
    public BillCategory Category { get; set; }
    public bool Paid { get; set; }
}