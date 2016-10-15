using System;
using System.Linq;

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

namespace MyFirstApp
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

            // Step 4: Create the readonly repository  for an entity "Employee"
            // using disposes the repository. It is a good practice to dispose the repository when finished
            using(var repository = manager.Repository.GetReader<Model.Employee>())
            {
                // Step 5: Get the emmployees "SELECT TOP 3 * FROM [Employee]" 
                var list = repository.All().Take(3).ToList();

                // Step 6: Print the data
                foreach(var emp in list)
                {
                    Console.WriteLine($"Id: {emp.ID}     Name:  {emp.FullName}         Department: {emp.Department.Name}    Email:{emp.Email}");
                }
            } // The repository disposed here

            manager.Dispose();  // Dispose the manager now.
        }
    }
}
