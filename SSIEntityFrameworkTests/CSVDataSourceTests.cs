using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSIEntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace SSIEntityFramework.Tests
{
    [TestClass()]
    public class CSVDataSourceTests
    {
        [TestMethod()]
        public void ConnectTest()
        {
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.ConnectionString = @"TestDataSource.txt";
            csvDS.Connect();
            Assert.IsTrue(csvDS.IsConnected());
        }

        [TestMethod()]
        public void ConnectNoFileTest()
        {
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.ConnectionString = @"TestEmptyDataSource.txt";
            csvDS.Connect();
            Assert.IsTrue(csvDS.IsConnected());
        }


        [TestMethod()]
        public void ConnectWithConnectionStringTest()
        {
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.Connect(@"TestDataSource.txt");
            Assert.IsTrue(csvDS.IsConnected());
        }


        [TestMethod()]
        public void IsConnectedTest()
        {
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            Assert.IsFalse(csvDS.IsConnected());
            csvDS.Connect(@"TestDataSource.txt");
            Assert.IsTrue(csvDS.IsConnected());
        }

        [TestMethod()]
        public void GetEntityTest()
        {
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.Connect(@"TestDataSource.txt");
            Assert.IsTrue(csvDS.IsConnected());
            Entity newEntity = csvDS.GetEntity(1);
            Assert.AreNotSame(entity, newEntity);
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            Assert.AreNotEqual(entity["ModifiedTimeStamp"].Value, newEntity["ModifiedTimeStamp"].Value);
            Assert.AreNotEqual(entity["CreatedTimeStamp"].Value, newEntity["CreatedTimeStamp"].Value);
        }


        [TestMethod()]
        [ExpectedException(typeof(System.Collections.Generic.KeyNotFoundException))]
        public void GetEntityInvalidIDTest()
        {
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.Connect(@"TestDataSource.txt");
            Assert.IsTrue(csvDS.IsConnected());
            Entity newEntity = csvDS.GetEntity(1);
            Assert.AreNotSame(entity, newEntity);
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            Assert.AreNotEqual(entity["CreatedTimeStamp"].Value, newEntity["CreatedTimeStamp"].Value);
            Assert.AreNotEqual(entity["ModifiedTimeStamp"].Value, newEntity["ModifiedTimeStamp"].Value);

            newEntity = csvDS.GetEntity(22);

        }

        [TestMethod()]
        public void CreateEntityTest()
        {
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.Connect(@"TestDataSource.txt");
            Assert.IsTrue(csvDS.IsConnected());
            entity.ID = 2;
            entity.Name = "Simplicity Software, Inc.";
            entity.ModifiedVersion = DateTime.Now;
            Entity newEntity = csvDS.CreateEntity(entity);
            Assert.AreNotSame(entity, newEntity);
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            Assert.AreEqual(entity["CreatedTimeStamp"].Value.ToString(), newEntity["CreatedTimeStamp"].Value.ToString());
            Assert.AreEqual(entity["ModifiedTimeStamp"].Value.ToString(), newEntity["ModifiedTimeStamp"].Value.ToString());

            entity = csvDS.GetEntity(2);
            Assert.AreNotSame(entity, newEntity);
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            Assert.AreEqual(entity["CreatedTimeStamp"].Value.ToString(), newEntity["CreatedTimeStamp"].Value.ToString());
            Assert.AreEqual(entity["ModifiedTimeStamp"].Value.ToString(), newEntity["ModifiedTimeStamp"].Value.ToString());

            // Now test to make sure it is not a reference
            newEntity.ModifiedVersion = DateTime.Now;
            entity = csvDS.GetEntity(2);
            Assert.AreNotEqual(entity["ModifiedTimeStamp"].Value, newEntity["ModifiedTimeStamp"].Value);

        }

        [TestMethod()]
        [ExpectedException(typeof(System.Collections.Generic.KeyNotFoundException))]
        public void DeleteEntityByIDTest()
        {
            // Add a new entity
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.Connect(@"TestDataSource.txt");
            Assert.IsTrue(csvDS.IsConnected());
            entity.ID = 2;
            entity.Name = "Simplicity Software, Inc.";
            entity.ModifiedVersion = DateTime.Now;
            Entity newEntity = csvDS.CreateEntity(entity);

            // Verify the entity is in the records
            entity = csvDS.GetEntity(2);
            Assert.AreNotSame(entity, newEntity);
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            Assert.AreEqual(entity["CreatedTimeStamp"].Value, newEntity["CreatedTimeStamp"].Value);
            Assert.AreEqual(entity["ModifiedTimeStamp"].Value, newEntity["ModifiedTimeStamp"].Value);

            csvDS.DeleteEntity(2);
            entity = csvDS.GetEntity(2);

        }

        [TestMethod()]
        [ExpectedException(typeof(System.Collections.Generic.KeyNotFoundException))]
        public void DeleteEntityTest()
        {
            // Add a new entity
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.Connect(@"TestDataSource.txt");
            Assert.IsTrue(csvDS.IsConnected());
            entity.ID = 2;
            entity.Name = "Simplicity Software, Inc.";
            entity.ModifiedVersion = DateTime.Now;
            Entity newEntity = csvDS.CreateEntity(entity);

            // Verify the entity is in the records
            entity = csvDS.GetEntity(2);
            Assert.AreNotSame(entity, newEntity);
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            Assert.AreEqual(entity["CreatedTimeStamp"].Value, newEntity["CreatedTimeStamp"].Value);
            Assert.AreEqual(entity["ModifiedTimeStamp"].Value, newEntity["ModifiedTimeStamp"].Value);

            csvDS.DeleteEntity(entity);
            entity = csvDS.GetEntity(entity.ID);

        }

        [TestMethod()]
        public void UpdateEntityTest()
        {
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.Connect(@"TestDataSource.txt");
            Assert.IsTrue(csvDS.IsConnected());
            entity.ModifiedVersion = DateTime.Now;
            csvDS.UpdateEntity(entity);

            Entity updatedEntity = csvDS.GetEntity(entity.ID);
            Assert.AreNotSame(entity, updatedEntity);
            Assert.AreEqual(entity.Name, updatedEntity.Name);
            Assert.AreEqual(entity.ID, updatedEntity.ID);
            Assert.AreEqual(entity["CreatedTimeStamp"].Value, updatedEntity["CreatedTimeStamp"].Value);
            Assert.AreEqual(entity["ModifiedTimeStamp"].Value, updatedEntity["ModifiedTimeStamp"].Value);

        }

        [TestMethod()]
        public void GetEntityFieldsTest()
        {
            List<EntityField> fields = CSVDataSource.GetEntityFields(@"TestDataSource.txt");
            Assert.AreEqual(4, fields.Count);
        }

        [TestMethod()]
        public void ReadEntityTest()
        {
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.Connect(@"TestDataSource.txt");
            Assert.IsTrue(csvDS.IsConnected());
            entity = csvDS.GetEntity(1);
            foreach (var e in csvDS)
            {
                Assert.AreEqual(entity.ID, e.ID);
                Assert.AreEqual(entity.Name, e.Name);
                Assert.AreEqual(entity.ModifiedVersion.ToString(), e.ModifiedVersion.ToString());
                break;
            }
        }

        [TestMethod()]
        public void ReadEntityDotNetTypeTest()
        {
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.Connect(@"TestDataSource.txt");
            Assert.IsTrue(csvDS.IsConnected());
            entity = csvDS.GetEntity(1);
            foreach (var e in csvDS)
            {
                Assert.AreEqual(entity.ID, ((Customer)e.ReadEntity()).ID);
                Assert.AreEqual(entity.Name, ((Customer)e.ReadEntity()).Name);
                Assert.AreEqual(entity.CreatedVersion.ToString(), ((Customer)e.ReadEntity()).CreatedTimeStamp.ToString());
                Assert.AreEqual(entity.ModifiedVersion.ToString(), ((Customer)e.ReadEntity()).ModifiedTimeStamp.ToString());
                break;
            }
        }

        [TestMethod()]
        public void DisconnectTest()
        {
            // Add a new entity
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.Connect(@"TestDataSource.txt");
            Assert.IsTrue(csvDS.IsConnected());
            entity.ID = 2;
            entity.Name = "Simplicity Software, Inc.";
            entity.ModifiedVersion = DateTime.Now;
            Entity newEntity = csvDS.CreateEntity(entity);

            // Verify the entity is in the records
            entity = csvDS.GetEntity(2);
            Assert.AreNotSame(entity, newEntity);
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            Assert.AreEqual(entity["CreatedTimeStamp"].Value, newEntity["CreatedTimeStamp"].Value);
            Assert.AreEqual(entity["ModifiedTimeStamp"].Value, newEntity["ModifiedTimeStamp"].Value);

            // disconnect
            csvDS.Disconnect();
            Assert.IsFalse(csvDS.IsConnected());

            // Make sure we can get the entity back now
            csvDS.Connect();
            Assert.IsTrue(csvDS.IsConnected());
            entity = csvDS.GetEntity(2);
            Assert.AreNotSame(entity, newEntity);
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            // Test ToString() because the millisecond value doesn't get serialized
            Assert.AreEqual(entity["CreatedTimeStamp"].Value.ToString(), newEntity["CreatedTimeStamp"].Value.ToString());
            Assert.AreEqual(entity["ModifiedTimeStamp"].Value.ToString(), newEntity["ModifiedTimeStamp"].Value.ToString());

            // remove the entity and delete the delted entity file
            csvDS.DeleteEntity(2);
            System.IO.File.Delete(csvDS.ConnectionString + ".deleted");

        }

        [TestMethod()]
        [ExpectedException(typeof(System.Collections.Generic.KeyNotFoundException))]
        public void DisconnectDeletedRecordsTest()
        {
            //TODO: Need this test to pass
            // Add a new entity
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.Connect(@"TestDataSource.txt");
            Assert.IsTrue(csvDS.IsConnected());
            entity.ID = 2;
            entity.Name = "Simplicity Software, Inc.";
            entity.ModifiedVersion = DateTime.Now;
            Entity newEntity = csvDS.CreateEntity(entity);

            // Verify the entity is in the records
            entity = csvDS.GetEntity(2);
            Assert.AreNotSame(entity, newEntity);
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            Assert.AreEqual(entity["CreatedTimeStamp"].Value, newEntity["CreatedTimeStamp"].Value);
            Assert.AreEqual(entity["ModifiedTimeStamp"].Value, newEntity["ModifiedTimeStamp"].Value);
            csvDS.DeleteEntity(2);

            // disconnect
            csvDS.Disconnect();
            Assert.IsFalse(csvDS.IsConnected());

            // check to make sure the entity exists in the ".deleted" file
            CSVDataSource csvDeletedDS = new CSVDataSource(entity);
            csvDeletedDS.Connect(csvDS.ConnectionString + ".deleted");
            Assert.IsTrue(csvDeletedDS.IsConnected());
            entity = csvDeletedDS.GetEntity(2);
            Assert.AreEqual(2, entity.ID);
            csvDeletedDS.Disconnect();

            // Make sure the entity was actually delted
            csvDS.Connect();
            Assert.IsTrue(csvDS.IsConnected());
            entity = csvDS.GetEntity(2);
            csvDS.Disconnect();

            // get rid of the delted file
            System.IO.File.Delete(csvDS.ConnectionString + ".deleted");
        }

        [TestMethod()]
        public void DisconnectNewFileTest()
        {

            // Add a new entity
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.Connect(@"TestDataSource.txt");
            Assert.IsTrue(csvDS.IsConnected());
            entity.ID = 2;
            entity.Name = "Simplicity Software, Inc.";
            entity.ModifiedVersion = DateTime.Now;
            Entity newEntity = csvDS.CreateEntity(entity);

            // Verify the entity is in the records
            entity = csvDS.GetEntity(2);
            Assert.AreNotSame(entity, newEntity);
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            Assert.AreEqual(entity["ModifiedTimeStamp"].Value, newEntity["ModifiedTimeStamp"].Value);
            Assert.AreEqual(entity["CreatedTimeStamp"].Value, newEntity["CreatedTimeStamp"].Value);

            // disconnect
            csvDS.Disconnect();
            Assert.IsFalse(csvDS.IsConnected());

            // Make sure we can get the entity back now
            csvDS.Connect();
            Assert.IsTrue(csvDS.IsConnected());
            entity = csvDS.GetEntity(2);
            Assert.AreNotSame(entity, newEntity);
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            // Test ToString() because the millisecond value doesn't get serialized
            Assert.AreEqual(entity["ModifiedTimeStamp"].Value.ToString(), newEntity["ModifiedTimeStamp"].Value.ToString());
            Assert.AreEqual(entity["CreatedTimeStamp"].Value.ToString(), newEntity["CreatedTimeStamp"].Value.ToString());

            // Now delete the new file
            // @"C:\Users\tory.CORP\OneDrive - Stoneridge Software Inc\Source\Workspaces\Tory-Surface\toryb.visualstudio.com\Visual Studio Projects\SSIEntityFramework\SSIEntityFrameworkTests\TestEmptyDataSource.txt");
            // System.IO.File.Delete(@"TestEmptyDataSource.txt");

        }

        /// <summary>
        /// This test method will make sure that when an existing "deleted" file exists, 
        /// newly deleted entities are appended to it correctly
        /// </summary>
        [TestMethod()]
        public void AppendDeletedEntityTest()
        {
            // TODO: Need this test to pass
            // Add two new entities
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.Connect(@"TestDataSource.txt");
            Assert.IsTrue(csvDS.IsConnected());
            entity.ID = 2;
            entity.Name = "Simplicity Software, Inc.";
            entity.ModifiedVersion = DateTime.Now;
            Entity newEntity = csvDS.CreateEntity(entity);

            entity.ID = 3;
            entity.Name = "Acme Company, Inc.";
            entity.ModifiedVersion = DateTime.Now;
            newEntity = csvDS.CreateEntity(entity);

            // now delte one of the entities
            csvDS.DeleteEntity(2);

            // disconnect
            csvDS.Disconnect();
            Assert.IsFalse(csvDS.IsConnected());

            // check to make sure the entity exists in the ".deleted" file
            CSVDataSource csvDeletedDS = new CSVDataSource(entity);
            csvDeletedDS.Connect(csvDS.ConnectionString + ".deleted");
            Assert.IsTrue(csvDeletedDS.IsConnected());
            newEntity = csvDeletedDS.GetEntity(2);
            Assert.AreEqual(2, newEntity.ID);
            csvDeletedDS.Disconnect();

            // now delete another entity
            csvDS.Connect();
            // first make sure it is there (the last value added was 3)
            newEntity = csvDS[entity.ID];
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            // Test ToString() because the millisecond value doesn't get serialized
            Assert.AreEqual(entity["CreatedTimeStamp"].Value.ToString(), newEntity["CreatedTimeStamp"].Value.ToString());
            Assert.AreEqual(entity["ModifiedTimeStamp"].Value.ToString(), newEntity["ModifiedTimeStamp"].Value.ToString());

            csvDS.DeleteEntity(entity.ID);
            // disconnect
            csvDS.Disconnect();
            Assert.IsFalse(csvDS.IsConnected());

            // check to make sure the entity exists in the ".deleted" file
            csvDeletedDS.Connect();
            csvDeletedDS.Connect(csvDS.ConnectionString + ".deleted");
            Assert.IsTrue(csvDeletedDS.IsConnected());
            entity = csvDeletedDS.GetEntity(entity.ID);
            Assert.AreEqual(3, entity.ID);
            csvDeletedDS.Disconnect();
            // get rid of the delted file
            System.IO.File.Delete(csvDeletedDS.ConnectionString);


        }


        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void UpdateEntityInvalidTest()
        {
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.Connect(@"TestDataSource.txt");
            Assert.IsTrue(csvDS.IsConnected());
            entity.ID = 22;
            csvDS.UpdateEntity(entity);
        }

        [TestMethod()]
        public void GetDeletedEntitiesTest()
        {
            // Add two new entities
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.Connect(@"TestDataSource.txt");
            Assert.IsTrue(csvDS.IsConnected());
            entity.ID = 2;
            entity.Name = "Simplicity Software, Inc.";
            // Use the parsed string because the write to a file does not keep the milliseconds
            entity.CreatedVersion =  entity.ModifiedVersion = DateTime.Parse(DateTime.Now.ToString());
            Entity newEntity = csvDS.CreateEntity(entity);

            entity.ID = 3;
            entity.Name = "Acme Company, Inc.";
            // Use the parsed string because the write to a file does not keep the milliseconds
            entity.CreatedVersion = entity.ModifiedVersion = DateTime.Parse(DateTime.Now.ToString());
            csvDS.CreateEntity(entity);

            // now delte one of the entities
            csvDS.DeleteEntity(entity.ID);

            // Now get the deleted entities
            IEnumerable<Entity> e = csvDS.GetDeletedEntities(entity.ModifiedVersion);

            Assert.AreEqual(1, e.Count());
            Assert.AreEqual(entity.ID, e.ElementAt(0).ID);
            Assert.AreEqual(entity, e.ElementAt(0));
            
            // Now test to make sure the delted entity lives past disconnect
            // disconnect
            csvDS.Disconnect();
            Assert.IsFalse(csvDS.IsConnected());

            // now delete another entity
            csvDS.Connect();

            // make sure the delted entity is still there
            e = csvDS.GetDeletedEntities(DateTime.Parse(entity.ModifiedVersion.ToString()));
            Assert.AreEqual(1, e.Count());
            Entity testEntity = e.ElementAt(0);
            Assert.AreEqual(entity.ID, testEntity.ID);
            Assert.AreEqual(entity.Name, testEntity.Name);
            Assert.AreEqual(entity["CreatedTimeStamp"].Value.ToString(), testEntity["CreatedTimeStamp"].Value.ToString());
            Assert.AreEqual(entity["ModifiedTimeStamp"].Value.ToString(), testEntity["ModifiedTimeStamp"].Value.ToString());
            Assert.AreEqual(entity, e.ElementAt(0));

            // first make sure the next entity is there to delete
            Assert.AreEqual(csvDS[2].ID, newEntity.ID);
            // Test ToString() because the millisecond value doesn't get serialized
            Assert.AreEqual(csvDS[2]["CreatedTimeStamp"].Value.ToString(), newEntity["CreatedTimeStamp"].Value.ToString());
            Assert.AreEqual(csvDS[2]["ModifiedTimeStamp"].Value.ToString(), newEntity["ModifiedTimeStamp"].Value.ToString());

            csvDS.DeleteEntity(newEntity.ID);

            // make sure the delted entity is still there
            e = csvDS.GetDeletedEntities(newEntity.ModifiedVersion);

            Assert.AreEqual(2, e.Count());
            Assert.AreEqual(newEntity.ID, e.ElementAt(1).ID);
            Assert.AreEqual(newEntity, e.ElementAt(1));
            
            // disconnect
            csvDS.Disconnect();
            Assert.IsFalse(csvDS.IsConnected());

            // get rid of the delted file
            System.IO.File.Delete(csvDS.ConnectionString + ".deleted");

        }

        [TestMethod()]
        public void GetAddedEntitiesTest()
        {
            //Add a new entity
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.Connect(@"TestDataSource.txt");
            Assert.IsTrue(csvDS.IsConnected());
            entity.ID = 2;
            entity.Name = "Halo Systems";
            entity.CreatedVersion = entity.ModifiedVersion = DateTime.Parse(DateTime.Now.ToString());
            csvDS.CreateEntity(entity);


            //Get added entities
            IEnumerable<Entity> e = csvDS.GetAddedEntities(entity.CreatedVersion);
            Assert.AreEqual(1, e.Count());


            //Test to see if added entity lives after disconnect
            csvDS.Disconnect();
            Assert.IsFalse(csvDS.IsConnected());

            csvDS.Connect();
            Assert.IsTrue(csvDS.IsConnected());

            //Assert it's the same entity
            e = csvDS.GetAddedEntities(entity.CreatedVersion);
            Assert.AreEqual(1, e.Count());
            Entity testEntity = e.ElementAt(0);
            Assert.AreEqual(entity.ID, testEntity.ID);
            Assert.AreEqual(entity.Name, testEntity.Name);
            Assert.AreEqual(entity["CreatedTimeStamp"].Value.ToString(), testEntity["CreatedTimeStamp"].Value.ToString());
            Assert.AreEqual(entity["ModifiedTimeStamp"].Value.ToString(), testEntity["ModifiedTimeStamp"].Value.ToString());
            Assert.AreEqual(entity, e.ElementAt(0));

            DateTime secondTimeAdded = entity.CreatedVersion;

            //Try to add a second entity
            entity.ID = 3;
            entity.Name = "App Factory";

            entity.CreatedVersion = entity.ModifiedVersion = DateTime.Parse(DateTime.Now.ToString());
            csvDS.CreateEntity(entity);

            e = csvDS.GetAddedEntities(secondTimeAdded);

            Assert.AreEqual(2, e.Count());

            //clean up...
            csvDS.DeleteEntity(2);
            csvDS.DeleteEntity(3);


            // disconnect
            csvDS.Disconnect();
            Assert.IsFalse(csvDS.IsConnected());
        }

        [TestMethod()]
        public void GetModifiedEntitiesTest()
        {
            //Add a new entity
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDS = new CSVDataSource(entity);
            csvDS.Connect(@"TestDataSource.txt");
            Assert.IsTrue(csvDS.IsConnected());
            entity.ID = 2;
            entity.Name = "Halo Systems";
            entity.CreatedVersion = entity.ModifiedVersion = DateTime.Parse(DateTime.Now.ToString());
            csvDS.CreateEntity(entity);

            Entity added = csvDS.GetEntity<int>(2);


            //Modify the entity
            added.Name = "App Factory";
            added.ModifiedVersion = DateTime.Parse("1/1/2020 1:00:00 AM");

            csvDS.UpdateEntity(added);

            //Get modified entities
            IEnumerable<Entity> e = csvDS.GetModifiedEntities(DateTime.Parse("1/1/2020 1:00:00 AM"));
            Assert.AreEqual(1, e.Count());


            //Test to see if modified entity lives after disconnect
            Assert.IsTrue(csvDS.IsConnected());
            csvDS.Disconnect();
            Assert.IsFalse(csvDS.IsConnected());

            csvDS.Connect();

            e = csvDS.GetModifiedEntities(DateTime.Parse("1/1/2020 1:00:00 AM"));

            //Assert it's same entity
            Assert.AreEqual(1, e.Count());
            Entity testEntity = e.ElementAt(0);
            Assert.AreEqual(added.ID, testEntity.ID);
            Assert.AreEqual(added.Name, "App Factory");
            Assert.AreEqual(added["CreatedTimeStamp"].Value.ToString(), testEntity["CreatedTimeStamp"].Value.ToString());
            Assert.AreEqual(added["ModifiedTimeStamp"].Value.ToString(), testEntity["ModifiedTimeStamp"].Value.ToString());


            //clean up...
            csvDS.DeleteEntity(2);


            // disconnect
            csvDS.Disconnect();
            Assert.IsFalse(csvDS.IsConnected());

        }
    }
}
