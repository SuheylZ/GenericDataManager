[![1475334088_database.png](https://s15.postimg.org/e3e6com23/1475334088_database.png)](https://postimg.org/image/vgogrjhd3/)
# GenericDataManager  

Generic data manager is a thread safe IUnitOfWork repository tailered for your entities and takes care of all the plumbing so that you only concentrate on the database design and using that data. How to fetch it, common functions for manipulating the data, handiong the DbContexts, thread synchronization for DB calls, properly initializing and cleaning up everything ... well! forget about all that. just provide your model name and plain old simple connection to your database and enjoy the beer! DataManager will take care of 
 - Create per thread DbContexts
 - Properly initialize and dispose them
 - Provides you the repository for each entity you require and totally tailored for it
 - Takes care of all the retrieval and saving of the data
 - In case of errors, it tries to give you the actual errors returned by the database
 - if you forget to properly dispose any of the repositories, the data manager also takes care of that
 - gives you a functional or LINQy way so you are more concentrated on WHAT to do rather than HOW to do *(See Update() and Delete() variants)* 

So it gives you all the EntityFramework benefits without you using the EntityFramework directly and tries to do even more. All it asks you is provide a simple connection string in simple english [ConnectionStrings.com](www.connectionstrings.com) and only the name of your model. The data manager will build the rest of the connection string itself and creates the contexts per thread. You can safely delete the context from your entities model. It is not needed at all. Here is how you would use the DataManager.

Please refer to the [wiki](https://github.com/MercedeX/GenericDataManager/wiki) to understand its working and other details.
___________
Please report the features you want or issues you encounter, your feedback means alot, [Contact me](mailto:suheylz@hotmail.com) if you have any queries
