using System;
using System.Collections.Generic;
using System.Linq;
using SSIEntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace SSIEntityFramework.Tests
{
    [TestClass]
    public class SharePointRESTDataSourceTests
    {
        private Entity NewEntity()
        {
            EntityTests.IDFieldName_ = "Sync_ID";
            EntityTests.nameFieldName_ = "Title";
            EntityTests.createdVersionFieldName_ = "Created";
            EntityTests.modifiedVersionFieldName_ = "Modified";
            return EntityTests.CreateTestEntity();
        }


        [TestMethod]
        public void ConnectTest()
        {
            Entity entity = NewEntity();

            //Connect
            SharePointRESTDataSource spDS = new SharePointRESTDataSource(entity);
            spDS.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDS.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDS.Password = "UMUachala*32";
            spDS.listName = "Entities";
            spDS.Connect();
            Assert.IsTrue(spDS.IsConnected());
            spDS.Disconnect();
            Assert.IsFalse(spDS.IsConnected());
        }

        
        [TestMethod()]
        public void ConnectWithConnectionStringTest()
        {
            Entity entity = NewEntity();

            //Connect
            SharePointRESTDataSource spDS = new SharePointRESTDataSource(entity);
            spDS.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDS.Password = "UMUachala*32";
            spDS.listName = "Entities";
            spDS.Connect("https://halosystemsgh.sharepoint.com/SSIEntities/");
            Assert.IsTrue(spDS.IsConnected());
            spDS.Disconnect();
            Assert.IsFalse(spDS.IsConnected());
        }

        [TestMethod()]
        public void ConnectNoList()
        {
            Entity entity = NewEntity();

            //Connect
            SharePointRESTDataSource spDS = new SharePointRESTDataSource(entity);
            spDS.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDS.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDS.Password = "UMUachala*32";
            spDS.listName = "SomeNonExistentList";
            spDS.Connect();
            Assert.IsTrue(spDS.IsConnected());
            spDS.Disconnect();
            Assert.IsFalse(spDS.IsConnected());

            //Clean up: delete that list
            spDS.deleteList();
            spDS.deleteList_deleted();
        }


        [TestMethod()]
        public void IsConnectedTest()
        {
            Entity entity = NewEntity();

            //Connect
            SharePointRESTDataSource spDS = new SharePointRESTDataSource(entity);
            spDS.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/"; 
            spDS.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDS.Password = "UMUachala*32";
            spDS.listName = "Entities";

            Assert.IsFalse(spDS.IsConnected());
            spDS.Connect();
            Assert.IsTrue(spDS.IsConnected());
        }


        [TestMethod()]
        public void GetEntityTest()
        {
            Entity entity = NewEntity();

            // Connect
            SharePointRESTDataSource spDS = new SharePointRESTDataSource(entity);
            spDS.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDS.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDS.Password = "UMUachala*32";
            spDS.listName = "Entities";
            spDS.Connect();
            Assert.IsTrue(spDS.IsConnected());

            Entity newEntity = spDS.GetEntity(1);
            Assert.AreNotSame(entity, newEntity);
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            Assert.AreNotEqual(entity["Modified"].Value, newEntity["Modified"].Value);
            Assert.AreNotEqual(entity["Created"].Value, newEntity["Created"].Value);
        }


        [TestMethod()]
        [ExpectedException(typeof(System.Collections.Generic.KeyNotFoundException))]
        public void GetEntityInvalidIDTest()
        {
            Entity entity = NewEntity();

            // Connect
            SharePointRESTDataSource spDS = new SharePointRESTDataSource(entity);
            spDS.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDS.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDS.Password = "UMUachala*32";
            spDS.listName = "Entities";
            spDS.Connect();
            Assert.IsTrue(spDS.IsConnected());

            //Why this block of code first?
            Entity newEntity = spDS.GetEntity(1);
            Assert.AreNotSame(entity, newEntity);
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            Assert.AreNotEqual(entity["Modified"].Value, newEntity["Modified"].Value);
            Assert.AreNotEqual(entity["Created"].Value, newEntity["Created"].Value);

            newEntity = spDS.GetEntity(22);

        }


        [TestMethod()]
        public void CreateEntityTest()
        {
            Entity entity = NewEntity();

            // Connect
            SharePointRESTDataSource spDS = new SharePointRESTDataSource(entity);
            spDS.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDS.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDS.Password = "UMUachala*32";
            spDS.listName = "Entities";
            spDS.Connect();
            Assert.IsTrue(spDS.IsConnected());


            entity.ID = 2;
            entity.Name = "Simplicity Software, Inc.";
            entity.ModifiedVersion = DateTime.Now;
            Entity newEntity = spDS.CreateEntity(entity);
            Assert.AreNotSame(entity, newEntity);
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            Assert.AreEqual(entity["Created"].Value.ToString(), newEntity["Created"].Value.ToString());
            Assert.AreEqual(entity["Modified"].Value.ToString(), newEntity["Modified"].Value.ToString());

            entity = spDS.GetEntity(2);
            Assert.AreNotSame(entity, newEntity);
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            Assert.AreEqual(entity["Created"].Value.ToString(), newEntity["Created"].Value.ToString());
            Assert.AreEqual(entity["Modified"].Value.ToString(), newEntity["Modified"].Value.ToString());

            // Now test to make sure it is not a reference
            newEntity.ModifiedVersion = DateTime.Now;
            entity = spDS.GetEntity(2);
            Assert.AreNotEqual(entity["Modified"].Value, newEntity["Modified"].Value);
        }

        [TestMethod()]
        [ExpectedException(typeof(System.Collections.Generic.KeyNotFoundException))]
        public void DeleteEntityByIDTest()
        {
            Entity entity = NewEntity();

            // Connect
            SharePointRESTDataSource spDS = new SharePointRESTDataSource(entity);
            spDS.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDS.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDS.Password = "UMUachala*32";
            spDS.listName = "Entities";
            spDS.Connect();
            Assert.IsTrue(spDS.IsConnected());

            entity.ID = 2;
            entity.Name = "Simplicity Software, Inc.";
            entity.ModifiedVersion = DateTime.Now;
            Entity newEntity = spDS.CreateEntity(entity);

            // Verify the entity is in the records
            entity = spDS.GetEntity(2);
            Assert.AreNotSame(entity, newEntity);
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            Assert.AreEqual(entity["Created"].Value, newEntity["Created"].Value);
            Assert.AreEqual(entity["Modified"].Value, newEntity["Modified"].Value);

            spDS.DeleteEntity(2);
            entity = spDS.GetEntity(2);

        }


        [TestMethod()]
        [ExpectedException(typeof(System.Collections.Generic.KeyNotFoundException))]
        public void DeleteEntityTest()
        {
            // Add a new entity
            Entity entity = NewEntity();

            // Connect
            SharePointRESTDataSource spDS = new SharePointRESTDataSource(entity);
            spDS.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDS.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDS.Password = "UMUachala*32";
            spDS.listName = "Entities";
            spDS.Connect();
            Assert.IsTrue(spDS.IsConnected());

            entity.ID = 2;
            entity.Name = "Simplicity Software, Inc.";
            entity.ModifiedVersion = DateTime.Now;
            Entity newEntity = spDS.CreateEntity(entity);

            // Verify the entity is in the records
            entity = spDS.GetEntity(2);
            Assert.AreNotSame(entity, newEntity);
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            Assert.AreEqual(entity["Created"].Value, newEntity["Created"].Value);
            Assert.AreEqual(entity["Modified"].Value, newEntity["Modified"].Value);

            spDS.DeleteEntity(entity);
            entity = spDS.GetEntity(entity.ID);
        }


        [TestMethod()]
        public void UpdateEntityTest()
        {
            // Add a new entity
            Entity entity = NewEntity();

            // Connect
            SharePointRESTDataSource spDS = new SharePointRESTDataSource(entity);
            spDS.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDS.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDS.Password = "UMUachala*32";
            spDS.listName = "Entities";
            spDS.Connect();
            Assert.IsTrue(spDS.IsConnected());

            entity.ModifiedVersion = DateTime.Now;
            spDS.UpdateEntity(entity);

            Entity updatedEntity = spDS.GetEntity(entity.ID);
            Assert.AreNotSame(entity, updatedEntity);
            Assert.AreEqual(entity.Name, updatedEntity.Name);
            Assert.AreEqual(entity.ID, updatedEntity.ID);
            Assert.AreEqual(entity["Created"].Value, updatedEntity["Created"].Value);
            Assert.AreEqual(entity["Modified"].Value, updatedEntity["Modified"].Value);
        }


      
        [TestMethod()]
        public void ReadEntityDotNetTypeTest()
        {

            // Add a new entity
            Entity entity = NewEntity();

            // Connect
            SharePointRESTDataSource spDS = new SharePointRESTDataSource(entity);
            spDS.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDS.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDS.Password = "UMUachala*32";
            spDS.listName = "Entities";
            spDS.Connect();
            Assert.IsTrue(spDS.IsConnected());

            entity = spDS.GetEntity(1);
            foreach (var e in spDS)
            {
                Assert.AreEqual(entity.ID, e.ID);
                Assert.AreEqual(entity.Name, e.Name);
                Assert.AreEqual(entity.ModifiedVersion.ToString(), e.ModifiedVersion.ToString());
                break;      //Why?
            }
        }


        [TestMethod()]
        public void DisConnectTest()
        {
            Entity entity = NewEntity();

            SharePointRESTDataSource spDS = new SharePointRESTDataSource(entity);
            spDS.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDS.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDS.Password = "UMUachala*32";
            spDS.listName = "Entities";
            spDS.Connect();
            Assert.IsTrue(spDS.IsConnected());
            spDS.Disconnect();

            Assert.IsTrue(spDS.IsConnected() == false);
        }

        [TestMethod()]
        [ExpectedException(typeof(System.Collections.Generic.KeyNotFoundException))]
        public void DisconnectDeletedRecordsTest()
        {

            // Add a new entity
            Entity entity = NewEntity();

            SharePointRESTDataSource spDS = new SharePointRESTDataSource(entity);
            spDS.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDS.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDS.Password = "UMUachala*32";
            spDS.listName = "Entities";
            spDS.Connect();
            Assert.IsTrue(spDS.IsConnected());

            entity.ID = 2;
            entity.Name = "Simplicity Software, Inc.";
            entity.ModifiedVersion = DateTime.Now;
            Entity newEntity = spDS.CreateEntity(entity);

            // Verify the entity is in the records
            entity = spDS.GetEntity(2);
            Assert.AreNotSame(entity, newEntity);
            Assert.AreEqual(entity.Name, newEntity.Name);
            Assert.AreEqual(entity.ID, newEntity.ID);
            Assert.AreEqual(entity["Created"].Value, newEntity["Created"].Value);
            Assert.AreEqual(entity["Modified"].Value, newEntity["Modified"].Value);
            spDS.DeleteEntity(2);

            // disconnect
            spDS.Disconnect();
            Assert.IsFalse(spDS.IsConnected());

            // check to make sure the entity exists in the ".deleted" file
            SharePointRESTDataSource spDeletedDS = new SharePointRESTDataSource(entity);
            spDeletedDS.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDS.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDS.Password = "UMUachala*32";
            spDeletedDS.listName = "Entities_deleted";
            spDeletedDS.Connect();
            Assert.IsTrue(spDeletedDS.IsConnected());

            entity = spDeletedDS.GetEntity(2);
            Assert.AreEqual(2, entity.ID);
            spDeletedDS.Disconnect();

            // Make sure the entity was actually deleted
            spDS.Connect();
            Assert.IsTrue(spDS.IsConnected());
            try
            {
                entity = spDS.GetEntity(2);
            }
            catch (KeyNotFoundException x)
            {
                spDS.Disconnect();
                // get rid of the deleted list ("Entities_deleted")
                spDeletedDS.deleteList();
                throw x;
            }

        }
        

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void UpdateEntityInvalidTest()
        {
            // Add a new entity
            Entity entity = NewEntity();

            // Connect
            SharePointRESTDataSource spDS = new SharePointRESTDataSource(entity);
            spDS.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            
            spDS.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDS.Password = "UMUachala*32";
            spDS.listName = "Entities";
            spDS.Connect();
            Assert.IsTrue(spDS.IsConnected());

            entity.ID = 22;
            spDS.UpdateEntity(entity);
        }

        [TestMethod()]
        public void GetDeletedEntitiesTest()
        {
            // Add two new entities
            Entity entity = NewEntity();

            // Connect
            SharePointRESTDataSource spDS = new SharePointRESTDataSource(entity);
            spDS.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            
            spDS.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDS.Password = "UMUachala*32";
            spDS.listName = "Entities";
            spDS.Connect();
            Assert.IsTrue(spDS.IsConnected());

            entity.ID = 2;
            entity.Name = "Simplicity Software, Inc.";
            // Use the parsed string because the write to a file does not keep the milliseconds
            entity.CreatedVersion = entity.ModifiedVersion = DateTime.Parse(DateTime.Now.ToString());
            Entity newEntity = spDS.CreateEntity(entity);

            entity.ID = 3;
            entity.Name = "Acme Company, Inc.";
            // Use the parsed string because the write to a file does not keep the milliseconds
            entity.CreatedVersion = entity.ModifiedVersion = DateTime.Parse(DateTime.Now.ToString());
            spDS.CreateEntity(entity);

            // now delte one of the entities
            spDS.DeleteEntity(entity.ID);

            // Now get the deleted entities
            IEnumerable<Entity> e = spDS.GetDeletedEntities(entity.ModifiedVersion);

            Assert.AreEqual(1, e.Count());
            Assert.AreEqual(entity.ID, e.ElementAt(0).ID);
            Assert.AreEqual(entity, e.ElementAt(0));

            // Now test to make sure the delted entity lives past disconnect
            // disconnect
            spDS.Disconnect();
            Assert.IsFalse(spDS.IsConnected());

            // now delete another entity
            spDS.Connect();

            // make sure the delted entity is still there
            e = spDS.GetDeletedEntities(DateTime.Parse(entity.ModifiedVersion.ToString()));
            Assert.AreEqual(1, e.Count());
            Entity testEntity = e.ElementAt(0);
            Assert.AreEqual(entity.ID, testEntity.ID);
            Assert.AreEqual(entity.Name, testEntity.Name);
            Assert.AreEqual(entity["Created"].Value.ToString(), testEntity["Created"].Value.ToString());
            Assert.AreEqual(entity["Modified"].Value.ToString(), testEntity["Modified"].Value.ToString());
            Assert.AreEqual(entity, e.ElementAt(0));

            // first make sure the next entity is there to delete
            Assert.AreEqual(spDS[2].ID, newEntity.ID);
            // Test ToString() because the millisecond value doesn't get serialized
            Assert.AreEqual(spDS[2]["Created"].Value.ToString(), newEntity["Created"].Value.ToString());
            Assert.AreEqual(spDS[2]["Modified"].Value.ToString(), newEntity["Modified"].Value.ToString());

            spDS.DeleteEntity(newEntity.ID);

            // make sure the deelted entity is still there
            e = spDS.GetDeletedEntities(newEntity.ModifiedVersion);

            Assert.AreEqual(2, e.Count());
            Assert.AreEqual(newEntity.ID, e.ElementAt(1).ID);
            Assert.AreEqual(newEntity, e.ElementAt(1));

            // disconnect
            spDS.Disconnect();
            Assert.IsFalse(spDS.IsConnected());

            //// get rid of the delted file
            //System.IO.File.Delete(csvDS.ConnectionString + ".deleted");
        }

        [TestMethod()]
        public void GetAddedEntitiesTest()
        {
            //Add a new entity
            Entity entity = NewEntity();

            // Connect
            SharePointRESTDataSource spDS = new SharePointRESTDataSource(entity);
            spDS.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDS.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDS.Password = "UMUachala*32";
            spDS.listName = "Entities";
            spDS.Connect();
            Assert.IsTrue(spDS.IsConnected());

            entity.ID = 2;
            entity.Name = "Halo Systems";
            entity.CreatedVersion = entity.ModifiedVersion = DateTime.Parse(DateTime.Now.ToString());
            spDS.CreateEntity(entity);


            //Get added entities
            IEnumerable<Entity> e = spDS.GetAddedEntities(entity.CreatedVersion);
            Assert.AreEqual(1, e.Count());


            //Test to see if added entity lives after disconnect
            spDS.Disconnect();
            Assert.IsFalse(spDS.IsConnected());

            spDS.Connect();
            Assert.IsTrue(spDS.IsConnected());

            //Assert it's the same entity
            e = spDS.GetAddedEntities(entity.CreatedVersion);
            Assert.AreEqual(1, e.Count());
            Entity testEntity = e.ElementAt(0);
            Assert.AreEqual(entity.ID, testEntity.ID);
            Assert.AreEqual(entity.Name, testEntity.Name);
            //Assert.AreEqual(entity["Created"].Value.ToString(), testEntity["Created"].Value.ToString());
            //Assert.AreEqual(entity["Modified"].Value.ToString(), testEntity["Modified"].Value.ToString());
            //Assert.AreEqual(entity, e.ElementAt(0));

            DateTime secondTimeAdded = entity.CreatedVersion;


            //Try to add a second entity
            entity.ID = 3;
            entity.Name = "App Factory";

            entity.CreatedVersion = entity.ModifiedVersion = DateTime.Parse(DateTime.Now.ToString());
            spDS.CreateEntity(entity);

            e = spDS.GetAddedEntities(secondTimeAdded);

            Assert.AreEqual(2, e.Count());

            //clean up...
            spDS.DeleteEntity(2);
            spDS.DeleteEntity(3);


            // disconnect
            spDS.Disconnect();
            spDS.deleteList_deleted();
            Assert.IsFalse(spDS.IsConnected());
        }

        [TestMethod()]
        public void GetModifiedEntitiesTest()
        {
            Entity entity = NewEntity();

            // Connect
            SharePointRESTDataSource spDS = new SharePointRESTDataSource(entity);
            spDS.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/"; 
            spDS.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDS.Password = "UMUachala*32";
            spDS.listName = "Entities";
            spDS.Connect();
            Assert.IsTrue(spDS.IsConnected());

            entity.ID = 2;
            entity.Name = "Halo Systems";
            entity.CreatedVersion = entity.ModifiedVersion = DateTime.Parse(DateTime.Now.ToString());
            spDS.CreateEntity(entity);

            Entity added = spDS.GetEntity<int>(2);


            //Modify the entity
            added.Name = "App Factory";
            added.ModifiedVersion = DateTime.Parse("1/1/2020 1:00:00 AM");

            spDS.UpdateEntity(added);

            //Get modified entities
            IEnumerable<Entity> e = spDS.GetModifiedEntities(DateTime.Parse("1/1/2020 1:00:00 AM"));
            Assert.AreEqual(1, e.Count());


            //Test to see if modified entity lives after disconnect
            Assert.IsTrue(spDS.IsConnected());
            spDS.Disconnect();
            Assert.IsFalse(spDS.IsConnected());

            spDS.Connect();

            e = spDS.GetModifiedEntities(DateTime.Parse("1/1/2020 1:00:00 AM"));

            //Assert it's same entity
            Assert.AreEqual(1, e.Count());
            Entity testEntity = e.ElementAt(0);
            Assert.AreEqual(added.ID, testEntity.ID);
            Assert.AreEqual(added.Name, "App Factory");
            Assert.AreEqual(added["Created"].Value.ToString(), testEntity["Created"].Value.ToString());
            Assert.AreEqual(added["Modified"].Value.ToString(), testEntity["Modified"].Value.ToString());

            //clean up...
            spDS.DeleteEntity(2);

            // disconnect
            spDS.Disconnect();
            spDS.deleteList_deleted();
            Assert.IsFalse(spDS.IsConnected());

        }
    }
}
