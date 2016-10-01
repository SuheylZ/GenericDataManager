# GenericDataManager
A generic data manager which  is thread safe and implements Repository pattern. All that you have to provide is 
 - A connection string that you commonly use without the entity framework model. 
 - the name of the model 

**Release Notes for version 1.5.5** 
- Fixed the issue in QuickLock that causesed it to be in locked state at creation sometimes 
- Fixed the issue of "cannot start a new transaction" on DbContext when multiple threads are involved 
- DataManager correctly keeps track of DbContexts with the threads 
- DataManager requires ConnectionParameters struct for initialization rather than multiple parameters 
- DataManager throws exception if disposed when the DbContext seem to be in use by other threads 
- multiple threads can simaltaneously access database without locking each other in isolated manner (Entities attached to different thread's DbContext cannot be used by other threads unless detached).

**Sample Working**

To use it
 - Use the nuget package [GenericDataManager](https://www.nuget.org/packages/GenericDataManager) OR 
 - Use the command from package console 
 
 PM> Install-Package GenericDataManager

The DataManager will build the rest of the connection string itself and create the context. You can safely delete the context from your entities project, if you have that in your Model. Here is how you would use the DataManager.

```csharp
            var str = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            manager = new DataManager.Core.Manager(str, @"MyModel");  // no csdl or similar extension
            
            // Suppose you have an Entity Person. Here's how you would use it
            using(var repository = manager.Get<Person>()){
            
              Console.WriteLine(string.Format("There are {0} persons in the database", repository.Count()));
              
              //blah blah
             
            
            
            
            } // as soon as Dispose is called on repository the DataManager will take care of the rest. 
```
How is it different: 
The Data Manager keeps a Map that holds all the Repositories. if the requested repository for that Entity type is already in the map, it reference counts it and returns so the same Repository for that Entity is shared across different requests. If the repository is not there, The DataManager creates ones and returns the instance. The operations are thread safe as the DataManager utilizes QuickLock.

<b>Performance</b>
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

**IEntityReaderWriter**

`Commands`

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


`Queries`

|Functions	|Returns |Notes   	|
|---	|---	|---	|
|Exists(Expression<Func <TEntity, bool>> expr)|bool| true if at least 1 entity satisfies the expression	|
|All(Expression<Func<TEntity, bool>> expr) | IQueryable<TEntity>| Linq Where() eqvivalent 	|
|  	|long |Linq Count() with expression speciying the criteria|   
