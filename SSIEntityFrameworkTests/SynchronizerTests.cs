using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using SSIEntityFramework;

namespace SSIEntityFramework.Tests
{
    [TestClass]
    public class SynchronizerTests
    {
        [TestMethod]
        public void Synchronize2CSVSources()
        {
            //Arrange
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDSA = new CSVDataSource(entity);
            csvDSA.Connect(@"SyncTestDataSources/SourceA.txt");

            CSVDataSource csvDSB = new CSVDataSource(entity);
            csvDSB.Connect(@"SyncTestDataSources/SourceB.txt");

            Synchronizer sync = new Synchronizer();

            sync.AddDataSource(csvDSA);
            sync.AddDataSource(csvDSB);

            //Act
            sync.Synchronize();

            //Assert
            Assert.AreEqual(csvDSA.GetEntities().Count(), 2); //Count after synchronization
            Assert.AreEqual(csvDSB.GetEntities().Count(), 2);

            Assert.IsTrue(csvDSA.GetSyncEntity(1) != null);
            Assert.IsTrue(csvDSA.GetSyncEntity(3) != null);

            //"Acme Company" should be updated to "Acme Company Inc." from a later version in source B
            Assert.IsTrue(csvDSA.GetEntities().FirstOrDefault(x => x.Name == "Acme Company Inc.") != null);

            //"Halo Systems" should be there
            Assert.IsTrue(csvDSA.GetEntities().FirstOrDefault(x => x.Name == "Halo Systems") != null);

            //Simplicity Software should not be there because it's deleted in datasource B
            Assert.IsTrue(csvDSA.GetEntities().FirstOrDefault(x => x.Name == "Simplicity Software") == null); 


        }

        [TestMethod]
        public void Synchronize3CSVSources()
        {

            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDSA = new CSVDataSource(entity);
            csvDSA.Connect(@"SyncTestDataSources/SourceA.txt");

            CSVDataSource csvDSB = new CSVDataSource(entity);
            csvDSB.Connect(@"SyncTestDataSources/SourceB.txt");

            CSVDataSource csvDSC = new CSVDataSource(entity);
            csvDSC.Connect(@"SyncTestDataSources/SourceC.txt");

            Synchronizer sync = new Synchronizer();

            sync.AddDataSource(csvDSA);
            sync.AddDataSource(csvDSB);
            sync.AddDataSource(csvDSC);


            //Act
            sync.Synchronize();

            //Assert
            Assert.AreEqual(csvDSA.GetEntities().Count(), 3); //Count after synchronization
            Assert.AreEqual(csvDSB.GetEntities().Count(), 3);

            Assert.IsTrue(csvDSA.GetSyncEntity(1) != null);
            Assert.IsTrue(csvDSA.GetSyncEntity(3) != null);
            Assert.IsTrue(csvDSA.GetSyncEntity(4) != null);


            //"Acme Company" should be updated to "Acme Company Inc." from a later version in source B
            Assert.IsTrue(csvDSA.GetEntities().FirstOrDefault(x => x.Name == "Acme Company Inc.") != null);

            //"Halo Systems" should be there
            Assert.IsTrue(csvDSA.GetEntities().FirstOrDefault(x => x.Name == "Halo Systems") != null);
            
            //"App Factory" should be there
            Assert.IsTrue(csvDSA.GetEntities().FirstOrDefault(x => x.Name == "App Factory") != null);

            //Simplicity Software should not be there because it's deleted in datasource B
            Assert.IsTrue(csvDSA.GetEntities().FirstOrDefault(x => x.Name == "Simplicity Software") == null);


        }


        private Entity NewEntity()
        {
            EntityTests.IDFieldName_ = "ID";
            EntityTests.nameFieldName_ = "Title";
            EntityTests.createdVersionFieldName_ = "Created";
            EntityTests.modifiedVersionFieldName_ = "Modified";
            return EntityTests.CreateTestEntity();
        }

        [TestMethod]
        public void Synchronize2SharePointSources()
        {
            //Arrange
            Entity entity = NewEntity();

            //Connect
            SharePointDataSource spDSA = new SharePointDataSource(entity);
            spDSA.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDSA.AuthType = SharePointDataSource.AuthenticationType.User;
            spDSA.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDSA.Password = "UMUachala*32";
            spDSA.listName = "SourceA";
            spDSA.Connect();


            SharePointDataSource spDSB = new SharePointDataSource(entity);
            spDSB.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDSB.AuthType = SharePointDataSource.AuthenticationType.User;
            spDSB.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDSB.Password = "UMUachala*32";
            spDSB.listName = "SourceB";
            spDSB.Connect();

            Synchronizer sync = new Synchronizer();

            sync.AddDataSource(spDSA);
            sync.AddDataSource(spDSB);

            //Act
            sync.Synchronize();

            //Assert
            Assert.AreEqual(spDSA.GetEntities().Count(), 2); //Count after synchronization
            Assert.AreEqual(spDSB.GetEntities().Count(), 2);

            Assert.IsTrue(spDSA.GetSyncEntity(1) != null);
            Assert.IsTrue(spDSA.GetSyncEntity(3) != null);

            //"Acme Company" should be updated to "Acme Company Inc." from a later version in source B
            Assert.IsTrue(spDSA.GetEntities().FirstOrDefault(x => x.Name == "Acme Company Inc.") != null);

            //"Halo Systems" should be there
            Assert.IsTrue(spDSA.GetEntities().FirstOrDefault(x => x.Name == "Halo Systems") != null);

            //Simplicity Software should not be there because it's deleted in datasource B
            Assert.IsTrue(spDSA.GetEntities().FirstOrDefault(x => x.Name == "Simplicity Software") == null);

        }

        [TestMethod]
        public void Synchronize3PointSources()
        {
            //Arrange
            Entity entity = NewEntity();

            //Connect
            SharePointDataSource spDSA = new SharePointDataSource(entity);
            spDSA.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDSA.AuthType = SharePointDataSource.AuthenticationType.User;
            spDSA.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDSA.Password = "UMUachala*32";
            spDSA.listName = "SourceA";
            spDSA.Connect();


            SharePointDataSource spDSB = new SharePointDataSource(entity);
            spDSB.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDSB.AuthType = SharePointDataSource.AuthenticationType.User;
            spDSB.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDSB.Password = "UMUachala*32";
            spDSB.listName = "SourceB";
            spDSB.Connect();


            SharePointDataSource spDSC = new SharePointDataSource(entity);
            spDSC.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDSC.AuthType = SharePointDataSource.AuthenticationType.User;
            spDSC.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDSC.Password = "UMUachala*32";
            spDSC.listName = "SourceC";
            spDSC.Connect();

            Synchronizer sync = new Synchronizer();

            sync.AddDataSource(spDSA);
            sync.AddDataSource(spDSB);
            sync.AddDataSource(spDSC);


            //Act
            sync.Synchronize();

            //Assert
            Assert.AreEqual(spDSA.GetEntities().Count(), 3); //Count after synchronization
            Assert.AreEqual(spDSB.GetEntities().Count(), 3);

            Assert.IsTrue(spDSA.GetSyncEntity(1) != null);
            Assert.IsTrue(spDSA.GetSyncEntity(3) != null);
            Assert.IsTrue(spDSA.GetSyncEntity(4) != null);


            //"Acme Company" should be updated to "Acme Company Inc." from a later version in source B
            Assert.IsTrue(spDSA.GetEntities().FirstOrDefault(x => x.Name == "Acme Company Inc.") != null);

            //"Halo Systems" should be there
            Assert.IsTrue(spDSA.GetEntities().FirstOrDefault(x => x.Name == "Halo Systems") != null);

            //"App Factory" should be there
            Assert.IsTrue(spDSA.GetEntities().FirstOrDefault(x => x.Name == "App Factory") != null);

            //Simplicity Software should not be there because it's deleted in datasource B
            Assert.IsTrue(spDSA.GetEntities().FirstOrDefault(x => x.Name == "Simplicity Software") == null);

        }

        [TestMethod]
        public void SynchronizeSharePointAndCSV()
        {

            //Arrange
            Entity entity = EntityTests.CreateTestEntity();
            CSVDataSource csvDSA = new CSVDataSource(entity);
            csvDSA.Connect(@"SyncTestDataSources/SourceA.txt");

            SharePointDataSource spDSB = new SharePointDataSource(entity);
            spDSB.ConnectionString = @"https://halosystemsgh.sharepoint.com/SSIEntities/";
            spDSB.AuthType = SharePointDataSource.AuthenticationType.User;
            spDSB.UserName = "alex@halosystemsgh.onmicrosoft.com";
            spDSB.Password = "UMUachala*32";
            spDSB.listName = "SourceB";
            spDSB.Connect();


            Synchronizer sync = new Synchronizer();

            sync.AddDataSource(csvDSA);
            sync.AddDataSource(spDSB);

            //Act
            sync.Synchronize();

            //Assert
            Assert.AreEqual(csvDSA.GetEntities().Count(), 2); //Count after synchronization
            Assert.AreEqual(spDSB.GetEntities().Count(), 2);

            Assert.IsTrue(csvDSA.GetSyncEntity(1) != null);
            Assert.IsTrue(csvDSA.GetSyncEntity(3) != null);

            //"Acme Company" should be updated to "Acme Company Inc." from a later version in source B
            Assert.IsTrue(csvDSA.GetEntities().FirstOrDefault(x => x.Name == "Acme Company Inc.") != null);

            //"Halo Systems" should be there
            Assert.IsTrue(csvDSA.GetEntities().FirstOrDefault(x => x.Name == "Halo Systems") != null);

            //Simplicity Software should not be there because it's deleted in datasource B
            Assert.IsTrue(csvDSA.GetEntities().FirstOrDefault(x => x.Name == "Simplicity Software") == null);


        }
    }
}
