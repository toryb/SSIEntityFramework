using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSIEntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace SSIEntityFramework.Tests
{
    public class TestStringField : IEntityField
    {
        public static string startingValue_ = "StartingTestValue";
        private string value_ = TestStringField.startingValue_;

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
            get { return  value_.GetType(); }
        }

        public object Clone()
        {
            TestStringField newField = new TestStringField();
            newField.Value = this.Value;
            return newField;
        }
    }

    [TestClass()]
    public class EntityFieldTests
    {
        [TestMethod()]
        public void EntityFieldConstructorTest()
        {
            EntityField field = new EntityField(new TestStringField());
            Assert.IsNotNull(field);
        }

        [TestMethod()]
        public void EntityFieldValuePropertyTest()
        {
            EntityField field = new EntityField(new TestStringField());
            Assert.IsNotNull(field);
            Assert.AreEqual(TestStringField.startingValue_, field.Value as string);
            string newValue = "New Value";
            field.Value = newValue;
            Assert.AreEqual(newValue, field.Value as string);
        }

        [TestMethod()]
        public void GetTypedValueTest()
        {
            EntityField field = new EntityField(new TestStringField());
            Assert.IsNotNull(field);
            Assert.AreEqual(TestStringField.startingValue_, field.GetTypedValue<string>());
        }

        [TestMethod()]
        [ExpectedException(typeof(System.InvalidCastException))]
        public void GetInvalidTypedValueTest()
        {
            EntityField field = new EntityField(new TestStringField());
            Assert.IsNotNull(field);
            int i = field.GetTypedValue<int>();
        }
    }
}
