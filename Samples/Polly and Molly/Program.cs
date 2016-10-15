using System;
using System.Linq;
using System.Threading;

// Minimum namespaces references for GenericDataManager
using GenericDataManager;
using GenericDataManager.Common;
using GenericDataManager.Interfaces;

// Entities are here in this namespace.
// be sure to add a reference to [EntityFramework.SqlServer.dll] otherwise your model would not be loaded. 
using Model = Sample.Entities;


// Before running this besure that you have your sql server (version 2005 or higher) is up and running.
// Also create a new database or you can use any exisiting database. 
// Run the script "Sample Database Script.sql" to initialize the database with tables and sample data

namespace Polly_and_Molly
{
    class Program
    {
        const string KConnectionString = @"data source=<your sql server>;initial catalog=<database>;integrated security=True;";
        const string KModelName = @"Entities";

        static void Main(string[] args)
        {
            // Step 1: Create the connection parameters with a connection string and 
            var connection = new ConnectionParameters(KConnectionString, KModelName);

            // Step 2: Use the default policy
            var policy = new ExecutionPolicy();

            // Step 3: Create the manager and pass the connection and policy
            IDataRepositoryProvider manager = new DataManagerWithPolicy(connection, policy);


            //Step 4: Here is polly, uses the same manager, gets ids of employee alphabetically sorted
            // adds the note and update them
            var polly = new Thread(() => {
                using (var rep = manager.Repository.Get<Model.Employee>())
                {
                    var ids = rep.All().OrderBy(x=>x.FullName).Select(x => x.ID).ToList();

                    foreach(var id in ids)
                    {
                        var emp = rep.One(x => x.ID == id);
                        emp.Notes += "Polly touched you...";
                        rep.Update(emp);
                        Console.WriteLine($"Polly touched {emp.FullName}");
                    }
                }
            });

            //Step 5: Here is molly, uses the same manager, gets ids of employee alphabetically reverse order
            // adds the note and update them. at the middle record they both meet.
            var molly = new Thread(() => {
                using (var rep = manager.Repository.Get<Model.Employee>())
                {
                    var ids = rep.All().OrderByDescending(x=>x.FullName).Select(x => x.ID).ToList();

                    foreach (var id in ids)
                    {
                        var emp = rep.One(x => x.ID == id);
                        emp.Notes += "Molly touched you...";
                        rep.Update(emp);
                        Console.WriteLine($"Molly touched {emp.FullName}");
                    }
                }
            });


            //Step 6: Now we start polly and molly
            polly.Start();
            molly.Start();

            // Step 7: Wait for the polly and molly to end
            polly.Join();
            molly.Join();


            //Step 8: Dispose manager
            manager.Dispose(); 
        }
    }

}
