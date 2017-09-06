using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSIEntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace SSIEntityFramework.Tests
{
    public class TestField<T> : IEntityField
    {
        private T value_ = default(T);

        public string FieldName { get; set; }

        public dynamic ReadField()
        {
            return value_;
        }

        public bool WriteField(dynamic value)
        {
            value_ = value;
            return true;
        }

        public dynamic Value
        {
            get
            {
                return ReadField();
            }
            set
            {
                WriteField(value);
            }
        }

        public bool Validate(dynamic value)
        {
            return true;
        }


        public Type ValueType
        {
            get { return value_.GetType(); }
        }

        public object Clone()
        {
            TestField<T> newField = new TestField<T>();
            newField.Value = this.Value;
            return newField;
        }
    }
    

    public class Customer
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public DateTime CreatedTimeStamp { get; set; }
        public DateTime ModifiedTimeStamp { get; set; }
    }


    [TestClass()]
    public class EntityTests
    {
        private static string entityTypeName_ = "TestEntity";
        private static string nameFieldName_ = "Name";
        private static string nameFieldValue_ = "Acme Company";
        private static string IDFieldName_ = "ID";
        private static int IDFieldValue_ = 1;
        private static string createdVersionFieldName_ = "CreatedTimeStamp";
        private static DateTime createdVersionFieldValue_ = DateTime.Now;
        private static string modifiedVersionFieldName_ = "ModifiedTimeStamp";
        private static DateTime modifiedVersionFieldValue_ = DateTime.Now;


        public static Entity CreateTestEntity()
        {
            Entity e = new Entity(entityTypeName_);
            Assert.IsNotNull(e);
            Assert.IsNotNull(e.FieldDictionary);

            e.AddField(IDFieldName_, new EntityField(new TestField<int>()) { Name = IDFieldName_, Value = IDFieldValue_ });
            e.IDFieldName = IDFieldName_;
            e.IDType = IDFieldValue_.GetType();

            // Set the name of the field to "Name"
            e.AddField(nameFieldName_, new EntityField(new TestField<string>())
            { Name = nameFieldName_, Value = nameFieldValue_ });
            e.NameFieldName = nameFieldName_;

            e.VersionType = modifiedVersionFieldValue_.GetType();
            e.AddField(createdVersionFieldName_, new EntityField(new TestField<DateTime>()) { Name = createdVersionFieldName_, Value = createdVersionFieldValue_ });
            e.CreatedVersionFieldName = createdVersionFieldName_;
            e.AddField(modifiedVersionFieldName_, new EntityField(new TestField<DateTime>()) { Name = modifiedVersionFieldName_, Value = modifiedVersionFieldValue_ });
            e.ModifiedVersionFieldName = modifiedVersionFieldName_;
            e.EntityTypeName = " Customer";
            e.DotNetType = typeof(Customer);

            return e;
        }



        [TestMethod()]
        public void GetFieldDictionaryTest()
        {
            Entity e = new Entity(entityTypeName_);

            Assert.IsNotNull(e.FieldDictionary);
        }

        [TestMethod()]
        public void EntityNamePropertyTest()
        {
            Entity e = EntityTests.CreateTestEntity();
            Assert.AreEqual(nameFieldValue_ as string, e.Name);
        }

        [TestMethod()]
        public void ReadFieldTest()
        {
            Entity e = EntityTests.CreateTestEntity();
            Assert.AreEqual(nameFieldValue_ as string, e.ReadField<string>(nameFieldName_));
        }

        [TestMethod()]
        public void AddFieldTest()
        {
            Entity e = new Entity(entityTypeName_);
            Assert.IsNotNull(e);
            Assert.IsNotNull(e.FieldDictionary);
            EntityField field = new EntityField(new TestStringField());
            Assert.IsNotNull(field);
            // Set the name of the field to "Name"
            field.Name = nameFieldName_;
            field.Value = nameFieldValue_;
            e.AddField(field.Name, field);
            Assert.AreEqual(nameFieldName_, e.FieldDictionary.ElementAt(0).Key);
            Assert.AreEqual(field, e.FieldDictionary.ElementAt(0).Value);
        }

        [TestMethod()]
        public void WriteFieldTest()
        {
            Entity e = EntityTests.CreateTestEntity();
            string newString = "New Company Name";
            Assert.AreEqual(nameFieldValue_ as string, e.ReadField<string>(nameFieldName_));
            e.WriteField<string>(nameFieldName_, newString);
            Assert.AreEqual(newString as string, e.ReadField<string>(nameFieldName_));
        }

        [TestMethod()]
        public void EntityCopyConstructorTest()
        {
            Entity e = EntityTests.CreateTestEntity();
            Entity e2 = new Entity(e);

            // Compare Properties
            Assert.AreEqual(e.EntityTypeName, e2.EntityTypeName);
            Assert.AreEqual(e.DotNetType, e2.DotNetType);
            Assert.AreEqual(e.NameFieldName, e2.NameFieldName);
            // Assume we want clone and not reference
            Assert.AreEqual(e.Name, e2.Name);

            Assert.AreNotSame(e.FieldDictionary, e2.FieldDictionary);
            Assert.AreNotSame(e, e2);

            // Now change a value to make sure it isn't a reference
            e.FieldDictionary[nameFieldName_].Value = "New Value";
            Assert.AreNotEqual(e.Name, e2.Name);
        }

        [TestMethod()]
        public void CloneTest()
        {
            Entity e = EntityTests.CreateTestEntity();
            Entity e2 = e.Clone() as Entity;

            // Compare Properties
            Assert.AreEqual(e.EntityTypeName, e2.EntityTypeName);
            Assert.AreEqual(e.DotNetType, e2.DotNetType);
            Assert.AreEqual(e.NameFieldName, e2.NameFieldName);
            // Assume we want clone and not reference
            Assert.AreEqual(e.Name, e2.Name);

            Assert.AreEqual(e.IDFieldName, e2.IDFieldName);
            Assert.AreEqual(e.IDType, e2.IDType);
            Assert.AreEqual(e.ModifiedVersionFieldName, e2.ModifiedVersionFieldName);
            Assert.AreEqual(e.VersionType, e2.VersionType);


            Assert.AreNotSame(e.FieldDictionary, e2.FieldDictionary);
            Assert.AreNotSame(e, e2);

            // Now change a value to make sure it isn't a reference
            e.FieldDictionary[nameFieldName_].Value = "New Value";
            Assert.AreNotEqual(e.Name, e2.Name);
        }

        [TestMethod()]
        public void EntityIndexerTest()
        {
            Entity e = EntityTests.CreateTestEntity();

            // Test getting the field
            EntityField f = e[e.NameFieldName];
            Assert.IsNotNull(f);
            Assert.AreEqual(nameFieldName_, f.Name);

            f = e[IDFieldName_];
            Assert.IsNotNull(f);
            Assert.AreEqual(IDFieldName_, f.Name);
            Assert.AreEqual(IDFieldValue_.GetType(), f.ValueType);

            // Test Adding a field 
            string fieldName = "NewField";
            string fieldValue = "New Field Value";
            Assert.AreEqual(e.FieldDictionary.Count, 4);
            f = new EntityField(new TestStringField()) { Name = fieldName, Value = fieldValue };
            e[fieldName] = f;
            Assert.AreEqual(e.FieldDictionary.Count, 5);
            Assert.AreEqual(fieldValue, e.ReadField<string>(f.Name));
            Assert.AreEqual(fieldValue, e[fieldName].Value as string);


        }

        [TestMethod()]
        public void ReadIDTest()
        {
            Entity e = EntityTests.CreateTestEntity();
            Assert.AreEqual(IDFieldValue_, e.ReadID<int>());
        }

        [TestMethod()]
        public void WriteIDTest()
        {
            Entity e = EntityTests.CreateTestEntity();
            Assert.AreEqual(IDFieldValue_, e.ReadID<int>());
            int i = IDFieldValue_ + 1;
            bool ret = e.WriteID<int>(i);
            Assert.IsTrue(ret);
            Assert.AreEqual(i, e.ReadID<int>());

        }

        [TestMethod()]
        public void ReadVersionTest()
        {
            Entity e = EntityTests.CreateTestEntity();
            Assert.AreEqual(modifiedVersionFieldValue_, e.ReadModifiedVersion<DateTime>());
        }

        [TestMethod()]
        public void WriteVersionTest()
        {
            Entity e = EntityTests.CreateTestEntity();
            Assert.AreEqual(modifiedVersionFieldValue_, e.ReadModifiedVersion<DateTime>());
            DateTime newVersion = DateTime.Now;
            bool ret = e.WriteModifiedVersion<DateTime>(newVersion);
            Assert.IsTrue(ret);
            Assert.AreEqual(newVersion, e.ReadModifiedVersion<DateTime>());
        }

        [TestMethod()]
        public void ReadEntityTest()
        {
            Entity e = EntityTests.CreateTestEntity();
            e.DotNetType = typeof(Customer);
            Assert.AreEqual(nameFieldValue_ as string, e.Name);
            Assert.AreEqual(nameFieldValue_ as string, e.ReadField<string>(nameFieldName_));
            Assert.AreEqual(modifiedVersionFieldValue_, e.ReadModifiedVersion<DateTime>());
            Assert.AreEqual(createdVersionFieldValue_, e.ReadCreatedVersion<DateTime>());
            Assert.AreEqual(IDFieldValue_, e.ReadID<int>());
            Customer cust = e.ReadEntity();
            Assert.AreEqual(nameFieldValue_ as string, cust.Name);
            Assert.AreEqual(createdVersionFieldValue_, cust.CreatedTimeStamp);
            Assert.AreEqual(modifiedVersionFieldValue_, cust.ModifiedTimeStamp);
            Assert.AreEqual(IDFieldValue_, cust.ID);
        }

        [TestMethod()]
        public void ReadModifiedVersionTest()
        {
            Entity e = EntityTests.CreateTestEntity();
            e.DotNetType = typeof(Customer);
            Assert.AreEqual(modifiedVersionFieldValue_, e.ReadModifiedVersion<DateTime>());
        }

        [TestMethod()]
        public void GetHashCodeTest()
        {
            Assert.Fail();
        }
    }
}
