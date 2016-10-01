# GenericDataManager

**Release Notes for version 1.5.5** 
- Fixed the issue in QuickLock that causesed it to be in locked state at creation sometimes 
- Fixed the issue of "cannot start a new transaction" on DbContext when multiple threads are involved 
- DataManager correctly keeps track of DbContexts with the threads 
- DataManager requires ConnectionParameters struct for initialization rather than multiple parameters 
- DataManager throws exception if disposed when the DbContext seem to be in use by other threads 
- multiple threads can simaltaneously access database without locking each other in isolated manner (Entities attached to different thread's DbContext cannot be used by other threads unless detached).

## Notes
Generic data manager is a thread safe IUnitOfWork repository tailered for your entities and takes care of all the plumbing so that you only concentrate on the database design and using that data. How to fetch it, common functions for manipulating the data, handiong the DbContexts, thread synchronization for DB calls, properly initializing and cleaning up everything ... well! forget about all that. just provide your model name and plain old simple connection to your database and enjoy the beer! DataManager will take care of 
 - Create per thread DbContexts
 - Properly initialize and dispose them
 - Provides you the repository for each entity you require and totally tailored for it
 - Takes care of all the retrieval and saving of the data
 - In case of errors, it tries to give you the actual errors returned by the database rather than the usual *"Something Horrendeous Happend (which we don't care about). Keep seeing the InnerException for details ( and enjoy the rest of your day admiring how deep InnerException goes!)"*
 - if you forget to properly dispose any of the repositories, the data manager also takes care of that
 - gives you a functional or LINQy way so you are more concentrated on WHAT to do rather than HOW to do *(See Update() and Delete() variants)* 

So it gives you all the EntityFramework benefits without you using the EntityFramework directly and tries to do even more. All it asks you is provide a simple connection string in simple english [ConnectionStrings.com](www.connectionstrings.com) and only the name of your model. and it does the rest. 

**Sample Working**

To use it
 - Use the nuget package [GenericDataManager](https://www.nuget.org/packages/GenericDataManager) OR 
 - Use the command from package console 
 
 PM> Install-Package GenericDataManager

The DataManager will build the rest of the connection string itself and create the context. You can safely delete the context from your entities project, if you have that in your Model. Here is how you would use the DataManager.

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
      Console.WriteLine(string.Format("There are {0} persons who are 20 years old", repository.Count(x=>x.Age==20)));

      //Now update all of them with a comment. note that you are not iterating over a list of persons
      repository.Update(x=>x.Age==20, x=>x.Notes = "We got ya");

      //Delete all below 10. See! you only supply the predicate and tell it what to do. Functional style Eh?
      repository.Delete(x=>x.Age=<10);
             
   } // as soon as Dispose is called on repository the DataManager will take care of the rest. 
```
**How is it different** 
The Data Manager keeps a Map that holds all the Repositories. if the requested repository for that Entity type is already in the map, it reference counts it and returns so the same Repository for that Entity is shared across different requests. If the repository is not there, The DataManager creates ones and returns the instance. The operations are thread safe unless you start tossing around entities from one thread to another, but worry not. Plan are underway to get around this limitation, for future versions

**Performance**
I'm in the process of updating unit tests and gathering some performance metrics. it will take a while.

**QuickLock**
Quicklock is an inter thread synchronization class. You can use it instead of C#’s `lock` keyword which internally uses Monitor. It provides performance using the Interlocked.Exchange methods. If the lock cannot be acquired, it retries for few times after which it throws the TimeoutException. Here is how you would use:


```csharp
var  qlock = new QuickLock();  //use defaults
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
*I’m omitting the actual thread creation and stuff but it already gives you the idea of using it.*

---------------------------------------------------------------------
## Important Interfaces
**IEntityWriter**
Provides functions that change the state of the database by adding, changing or removing the data.
*Currently the data manager does not provide any implementation for this interface. See IEntityReaderWriter below*

`Single Record Commands`

| Functions              |  Notes                                   |
|:-----------------------|:-----------------------------------------|
| Add(TEntity)           | Adds an entities to the database         | 
| Delete(TEntity expr)   | Deletes a single entity from the database|   
| Update(TEntity arg)    | Updates a single entity to the database  |   


`Bulk Commands`

| Functions   |     Notes     |
|:----------|:-------------|
| Add(IEnumerable< TEntity> list) | Adds a list of entities to the database | 
| Update(Expression<Func<TEntity, bool>> expr, Action<TEntity> statement) | Specify a linq expression and an action, the function is equivalent to Update - Where |   
| Delete(Expression<Func<TEntity, bool>> expr) | Deletes entities from  the database that match the expression you specify|


**IEntityReader**
Provies queries that do not change the state of the database, that is, provides functions that only retrieve data but cannot alter it. *Currently the data manager does not provide any implementation for this interface. See IEntityReaderWriter below*
`Queries`

|Functions	|Returns |Notes   	|
|---	|---	|---	|
|Exists(Expression<Func <TEntity, bool>> expr) | bool | true if at least 1 entity satisfies the expression	|
|All(Expression<Func<TEntity, bool>> expr) | IQueryable<TEntity>| Linq Where() eqvivalent 	|
|Count(Expression<Func<TEntity, bool>> expr)|long |Linq Count() with expression speciying the criteria |   
|One(Expression<Func<TEntity, bool>> expr) | TEntity| returns a single record, if there are many then only the first one of the list|

**IEntityReaderWriter: IEntityReader, IEntityWriter**
The data manager returns only this interface implementation so you get all the queries and commands mentioned above. Separate reader and writer interface implementations are planned for future versions so stay tuned. 
