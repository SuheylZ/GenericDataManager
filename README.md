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
 - Use the nuget package https://www.nuget.org/packages/GenericDataManager OR 
 - Use the command from package console 
 
 PM> Install-Package GenericDataManager

The DataManager will build the rest of the connection string itself and create the context. You can safely delete the context from your entities project, if you have that in your Model. Here is how you would use the DataManager.

            var str = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            manager = new DataManager.Core.Manager(str, @"MyModel");  // no csdl or similar extension
            
            // Suppose you have an Entity Person. Here's how you would use it
            using(var repository = manager.Get<Person>()){
            
              Console.WriteLine(string.Format("There are {0} persons in the database", repository.Count()));
              
              //blah blah
             
            
            
            
            } // as soon as Dispose is called on repository the DataManager will take care of the rest. 
            
How is it different: 
The Data Manager keeps a Map that holds all the Repositories. if the requested repository for that Entity type is already in the map, it reference counts it and returns so the same Repository for that Entity is shared across different requests. If the repository is not there, The DataManager creates ones and returns the instance. The operations are thread safe as the DataManager utilizes QuickLock.

<b>Performance</b>
I'm in the process of updating unit tests and gathering some performance metrics. it will take a while.

<h2>QuickLock</h2><p>Quicklock is an inter thread synchronization class. You can use it instead of C#’s lock{} keyword which internally uses Monitor. It provides performance using the Interlocked.Exchange methods. If the lock cannot be acquired, it retries for few times after which it throws the TimeoutException. Here is how you would use:</p><p><br></p><p><span class="Apple-tab-span" style="white-space:pre">	</span>var &nbsp;qlock = new QuickLock(); &nbsp;//use defaults</p><p>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp;int data = 20;</p><p><br></p><p><span class="Apple-tab-span" style="white-space:pre">	</span>private void FuncA(){</p><p><span class="Apple-tab-span" style="white-space:pre">		</span>for(int i =0; i&lt;30;i++){</p><p><span class="Apple-tab-span" style="white-space:pre">			</span>qlock.Lock();</p><p><span class="Apple-tab-span" style="white-space:pre">			</span>data = 20 + i;</p><p><span class="Apple-tab-span" style="white-space:pre">			</span>qlock.Unlock();</p><p><span class="Apple-tab-span" style="white-space:pre">		</span>}</p><p>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;}</p><p><br></p><p><span class="Apple-tab-span" style="white-space:pre">	</span>private void FuncB(){</p><p><span class="Apple-tab-span" style="white-space:pre">		</span>for(int i =0; i&lt;30;i++){</p><p><span class="Apple-tab-span" style="white-space:pre">			</span>qlock.Lock();</p><p><span class="Apple-tab-span" style="white-space:pre">			</span>data = 20%(i+1);</p><p><span class="Apple-tab-span" style="white-space:pre">			</span>qlock.Unlock();</p><p><span class="Apple-tab-span" style="white-space:pre">		</span>}</p><p>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;}</p><p>&nbsp;</p><p>I’m omitting the actual thread creation and stuff but it should already give you an idea on using it.&nbsp;</p><h2>Functions</h2><p></p><ul><li><b>Add(TEntity): </b>Adds a single entity to the database.</li><li><b>Add(IEnumerable&lt;TEntity&gt;)</b> Performs a bulk insert on the database</li><li><b>Queryable&lt;TEntity&gt; All(Expression&lt;Func&lt;TEntity, bool&gt;&gt; expr)</b>: same as the Where() of LINQ. If you do not supply an expression it returns all the entities otherwise it filters them and returns</li><li><b>long Count(Expression&lt;Func&lt;TEntity, bool&gt;&gt; expr)</b>: returns the count of all the entities or the count of the entities that satisfy the criteria. &nbsp;eg: &nbsp;rep.Count(x=&gt;x.Age &gt;20)</li><li><b>void Delete(Expression&lt;Func&lt;TEntity, bool&gt;&gt; expr)</b>: performs a bulk delete for the entities that satisfy a particular criteria.&nbsp;</li><li>.Delete(TEntity expr) deletes a single entity.</li><li><b>Exists(Expression&lt;Func&lt;TEntity, bool&gt;&gt; expr)</b>: checks if there are any entities that satisfy the criteria.&nbsp;</li><li><b>One(Expression&lt;Func&lt;TEntity, bool&gt;&gt; expr)</b>: returns a single entity that satisfies the criteria. If there are multiple entities then it returns the first one.&nbsp;</li><li><b>Update(TEntity arg)</b>: Updates a single entity in the database</li><li><b>Update(Expression&lt;Func&lt;TEntity, bool&gt;&gt; expr, Action&lt;TEntity&gt; statement)</b>: updates multiple entities based on the criteria. for example : rep.Update(x=&gt; x.Age&gt;20, x=&gt;IsProcessed = true) // Update XXX set IsProcessed= 1 Where Age &gt;20</li><li><b>IQuerable&lt;TEntity&gt; ReadOnly(Expression&lt;Func&lt;TEntity, bool&gt;&gt; expr)</b>: Retrieves a list of detached entities. a really fast way to get your records but you cannot update or perform any operations on them.&nbsp;</li></ul><p></p>
