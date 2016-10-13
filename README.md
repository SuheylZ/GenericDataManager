[![1475334088_database.png](https://s15.postimg.org/e3e6com23/1475334088_database.png)](https://postimg.org/image/vgogrjhd3/)
# GenericDataManager  

Generic data manager is a thread safe IUnitOfWork repository tailered for your entities and takes care of all the plumbing so that you only concentrate on the database design and using that data. How to fetch it, common functions for manipulating the data, handiong the DbContexts, thread synchronization for DB calls, properly initializing and cleaning up everything ... well! forget about all that. just provide your model name and plain old simple connection to your database and enjoy the beer! DataManager will take care of 
 - Create per thread DbContexts
 - Properly initialize and dispose them
 - Provides you the repository for each entity you require and totally tailored for it
 - Takes care of all the retrieval and saving of the data
 - In case of errors, it tries to give you the actual errors returned by the database rather than the usual *"Something Horrendeous Happend (which we don't care about). Keep seeing the InnerException for details ( and enjoy the rest of your day admiring how deep InnerException goes!)"*
 - if you forget to properly dispose any of the repositories, the data manager also takes care of that
 - gives you a functional or LINQy way so you are more concentrated on WHAT to do rather than HOW to do *(See Update() and Delete() variants)* 

So it gives you all the EntityFramework benefits without you using the EntityFramework directly and tries to do even more. All it asks you is provide a simple connection string in simple english [ConnectionStrings.com](www.connectionstrings.com) and only the name of your model. The data manager will build the rest of the connection string itself and creates the contexts per thread. You can safely delete the context from your entities model. It is not needed at all. Here is how you would use the DataManager.

### How to use it?
 - Use the nuget package [GenericDataManager](https://www.nuget.org/packages/GenericDataManager) to install it OR 
 - Use the command `PM> Install-Package GenericDataManager` from package console OR
 - Download the source I have placed here and compile it yourself *(only if you have a lot of free time :) )*
 
### Sample Usage
```csharp
 // See! no convoluted connection string "metadata=res://*/MyModel.csdl|res://*/MyModel.ssdl|res://*/MyModel.msl;provider ....."
 var connection = new ConnectionParameters(
 			"Server=myServer; Database=myDataBase; User Id=myuserName; Password=myPassword;",   
			"MyModel"
		      );
	
 var manager = new DataManager.Core.Manager(connection);  // no csdl or similar extension
            
   // Suppose you have an Entity Person. Here's how you would use it
   using(var repository = manager.Get<Person>())
   {
      Console.WriteLine($"There are {repository.Count(x=>x.Age==20)} persons who are 20 years old");

      //Now update all of them with a comment. note that you are not iterating over a list of persons
      repository.Update(x=>x.Age==20, x=>x.Notes = "We got ya");

      //Delete all below 10. See! you only supply the predicate and tell it what to do. Functional style Eh?
      repository.Delete(x=>x.Age=<10);
             
   } // as soon as Dispose is called on repository the DataManager will take care of the rest. 
```
### How is it different 
The Data Manager keeps a Map that holds all the Repositories. if the requested repository for that Entity type is already in the map, it reference counts it and returns so the same Repository for that Entity is shared across different requests. If the repository is not there, The DataManager creates ones and returns the instance. The operations are thread safe unless you start tossing around entities from one thread to another, but worry not. Plan are underway to get around this limitation, for future versions

___________
Please report the features you want or issues you encounter, your feedback means alot, [Contact me](mailto:suheylz@hotmail.com) if you have any queries
