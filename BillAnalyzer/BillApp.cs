using BillAnalyzer.Models;
using CrossTermLib;
using System.Numerics;

namespace BillAnalyzer;

public class BillApp
{

    public BillApp()
    {

        using var db = new BillingContext();

        using var terminal = new Terminal(
            cols: 40,
            rows: 20,
            title: "Bill Analyzer",
            fontPath: Path.Combine("Fonts", "ubuntu.ttf"),
            cursorBlinkSpeed: 0.4f,
            backgroundColor: new Vector4(0f, 0f, 0f, 1f),
            fontColor: new Vector4(1f, 1f, 1f, 1f),
            fontSize: 22);



        string choice;
        do
        {
            var numPaychecks = db.PayChecks.Count();

            terminal.Clear();
            terminal.WriteLine($"Num Paychecks: {numPaychecks}");
            terminal.WriteLine($"Add new paycheck? y or n");
            choice = terminal.ReadLine();
            if (choice == "y")
            {
                db.PayChecks.Add(new PayCheck()
                {
                    Amount = 100,
                    PayDate = new DateOnly(2024, 3, 15)
                });
                db.SaveChanges();
            }
            else if (choice == "exit")
            {
                break;
            }
            
        } while (!terminal.IsClosing);

        

        
    }

}

public enum BillCategory
{
    Rent,
    GasBill,
    Electric,
    Grocery,
    Takeout,
    Farm,
    Amazon,
    Pet,
    Service,
    Gas,
    Car,
    Investment,
    Entertainment
}