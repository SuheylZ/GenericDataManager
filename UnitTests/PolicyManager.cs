using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using GenericDataManager;
using GenericDataManager.Common;
using GenericDataManager.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class PolicyManager: TestBase
    {
        [TestMethod]
        public void SingleInsert()
        {
            using(var rep = manager.Repository.Get<Person>())
            {
                var id = 5577;
                rep.Add(new Person { id = id, FullName = "Test User 1", Email = "testuser@email.com", NIC = "1232233344"});
                var p = rep.One(x => x.id == id);
                if (p == null)
                    throw new ApplicationException("could not insert");
                rep.Delete(p);
            }
        }

        [TestMethod]
        public void MultipleInserts()
        {
            var list = new List<Employee>(400);
            var delta = 3456;

            for(var i=0;i < list.Capacity; i++)
            {
                var tmpID = i + delta;
                var emp = new Employee
                {
                    ID = tmpID,
                    FullName = $"Test User {tmpID}",
                    Email = "testuser@email.com",
                    NIC = $"{tmpID}32987"
                };
                list.Add(emp);
            }

            using(var rep = manager.Repository.GetWriter<Employee>())
            {
                rep.Add(list);
            }
        }

        [TestMethod]
        public void MultipleThreadsUpdate()
        {
            Func<string, Thread> CreateThread = (threadName) => {
                Action task = () =>
                {
                    using (var rep = manager.Repository.Get<Employee>())
                    {
                        var list = rep.All().Select(x => x.ID).ToList();
                        foreach (var it in list)
                        {
                            var tmp = rep.One(x => x.ID == it);
                            tmp.Notes = tmp.Notes + $"{Thread.CurrentThread.Name}..";
                            rep.Update(tmp);
                            Thread.Sleep(500);
                        }
                    }
                };

                var th = new Thread(() => task());
                th.Name = threadName;
                return th;
            };


            var threads = new List<Thread>(10);
            for (var i = 1; i <= threads.Capacity; i++)
                threads.Add(CreateThread($"t{i}"));

            foreach (var it in threads)
                it.Start();

            while (threads.Count(x => x.IsAlive) > 0)
                Thread.Sleep(5000);
        }

        [TestMethod]
        public void LengthyThreads()
        {
            Func<string, Thread> CreateTurtleThread = (threadName) => {
                Action turtleTask = () =>
                {
                    using (var rep = manager.Repository.Get<Employee>())
                    {
                        var list = rep.All().Take(50).Select(x => x.ID).ToList();
                        foreach (var it in list)
                        {
                            var tmp = rep.One(x => x.ID == it);
                            tmp.Notes = tmp.Notes + $"{Thread.CurrentThread.Name}..";
                            rep.Update(tmp);

                            Thread.Sleep(1000);
                        }
                    }
                };

                var th = new Thread(() => turtleTask());
                th.Name = threadName;
                return th;
            };

            var threads = new List<Thread>(3);
            for (var i = 1; i <= threads.Capacity; i++)
                threads.Add(CreateTurtleThread($"turtle{i}"));

            foreach (var it in threads)
                it.Start();


            while (threads.Count(x => x.IsAlive) > 0)
                Thread.Sleep(TimeSpan.FromMinutes(1));
        }
    }
}
