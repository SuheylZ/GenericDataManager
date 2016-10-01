[![1475334088_database.png](https://s15.postimg.org/e3e6com23/1475334088_database.png)](https://postimg.org/image/vgogrjhd3/)
# GenericDataManager  
### Release Notes for version 1.5.5
- Fixed: sometimes the QuickLock is already in locked state on creation from worker threads 
- Fixed: "Cannot start a new transaction" on DbContext when multiple threads are involved
- Fixed: "There is already an open DataReader" when one thread is iterating over a list and other threads try to access
- Fixed: DataManager correctly keeps track of DbContexts with the threads 
- Changed: DataManager uses ConnectionParameters struct for initialization rather than multiple parameters 
- Changed: DataManager throws AggregateException once when disposing off the DbContexts rather than stopping at the 1st exception 
- Fixed: multiple threads can simaltaneously access database without locking each other often and throwing TimeoutExpired exception
 
### Features requested and planned for next releases
- Separate implementations of IEntityReader & IEntityWriter
- ability to use Repositories without calling dispose
- Ability to provide custom implementations for IContextConsumer and IContextProvider
- Events by data manager

_______
Please report the features you want or issues you encounter, your feedback means alot, [Contact me](mailto:suheylz@hotmail.com) if you have any queries
_______ 
## Notes
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

### Performance
I'm in the process of updating unit tests and gathering some performance metrics. it will take a while.

### QuickLock
As a utility class, Quicklock provides extremely performant inter thread synchronization. You can use it instead of C#’s `lock` keyword which internally uses Monitor and try/catch/finally. It uses Interlocked.Exchange methods to provide the functionality. If the lock cannot be acquired for a certain time it throws the TimeoutException. Here is how you would use:

```csharp
var  qlock = new QuickLock(TimeSpan.FromSeconds(30));  //wait for 30 seconds after which throw exception
    int data = 20;  //our shared data

    private void FuncA()
    {
      for(int i =0; i<30; i++)
      {
      	 qlock.Lock();
         data = 20 + i;
         qlock.Unlock();
       }
    }
	 
    private void FuncB()
    {
      for(int i =0; i<30; i++)
      {
      	qlock.Lock();
      	data = 20%(i+1);
      	qlock.Unlock();
      }
    }
```
*I’m omitting the actual thread creation and stuff but it already gives you the idea of how to use it.*

---------------------------------------------------------------------
## Important Interfaces
### IEntityWriter
Provides functions that change the state of the database by adding, changing or removing the data.
*Currently the data manager does not provide any implementation for this interface. See IEntityReaderWriter below*

#### Single Record Commands

| Functions              |  Notes                           |
|:-----------------------|:---------------------------------|
| Add(TEntity)           | Adds an entitiy to the database  | 
| Delete(TEntity expr)   | Deletes an entitiy from the database|   
| Update(TEntity arg)    | Updates an entitiy into the database  |   


#### Bulk Commands

| Functions |     Notes    |
|:----------|:-------------|
| Add(IEnumerable&lt;TEntity&gt; list) | Adds a list of entities to the database | 


#### Bulk Commands LINQ based
| Functions |     Notes    |
|:----------|:-------------|
| Update(Expression&lt;Func&lt;TEntity, bool&gt;&gt; expr, Action&lt;TEntity&gt; statement) | Specify a linq expression and an action, the function is equivalent to Update - Where |   
| Delete(Expression&lt;Func&lt;TEntity, bool&gt;&gt; expr) | Deletes entities from  the database that match the expression you specify|


### IEntityReader
Provies queries that do not change the state of the database, that is, provides functions that only retrieve data but cannot alter it. *Currently the data manager does not provide any implementation for this interface. See IEntityReaderWriter below*
`Queries`

|Functions	|Returns |Notes   	|
|---	|---	|---	|
|Exists(Expression&lt;Func&lt;TEntity, bool&gt;&gt; expr) | bool | true if at least 1 entity satisfies the expression	|
|All(Expression&lt;Func&lt;TEntity, bool&gt;&gt; expr) | IQueryable<TEntity>| Linq Where() eqvivalent 	|
|Count(Expression&lt;Func&lt;TEntity, bool&gt;&gt; expr)|long |Linq Count() with expression speciying the criteria |   
|One(Expression&lt;Func&lt;TEntity, bool&gt;&gt; expr) | TEntity| returns a single record, if there are many then only the first one of the list|

### IEntityReaderWriter: IEntityReader, IEntityWriter
The data manager returns only this interface implementation so you get all the queries and commands mentioned above. Separate reader and writer interface implementations are planned for future versions so stay tuned. 
