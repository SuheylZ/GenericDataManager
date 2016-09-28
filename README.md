# GenericDataManager
A generic data manager which  is thread safe and implements Repository pattern. All that you have to provide is 
 - A connection string that you commonly use without the entity framework model. 
 - the name of the model 
 
The DataManager will build the rest of the connection string itself and create the context. You can safely delete the context, if you have in your Model. Here is how you would use the DataManager.

            var str = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            manager = new DataManager.Core.Manager(str, @"MyModel");  // no csdl or similar extension
            
            // Suppose you have an Entity Person. Here's how you would use it
            using(var repository = manager.Get<Person>()){
            
              Console.WriteLine(string.Format("There are {0} persons in the database", repository.Count()));
              
              //blah blah
             
            
            
            
            } // as soon as Dispose is called on repository the DataManager will take care of the rest. 
            
How is it different: 
The Data Manager keeps a Map that holds all the Repositories. if the requested repository for that Entity type is already in the map, it reference counts it and returns so the same Repository for that Entity is shared across different requests. If the repository is not there, The DataManager creates ones and returns the instance. The operations are thread safe as the DataManager utilizes QuickLock.





