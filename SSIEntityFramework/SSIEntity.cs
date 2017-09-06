using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SSIEntityFramework
{
    /// <summary>
    /// Represents an Entity in a Datasource
    /// </summary>
    /// <remarks>
    /// Design requirements: An Entity Must
    /// <list type="text">
    ///     <item Have a field that contians the name of a specific instance of the Entity (<seealso cref="Name"/>)/>
    ///     <item Have an Entity Type Name />
    ///     <item Have one or more fields />
    ///     <item Have have an ID field that contains a value gauranteed to be unique in the context of the current
    ///     instance of this library />
    ///     <item Have an ID type. This represents the Type of the value used for the unique ID for the Entity />
    ///     <item Have two version fields, a CreatedVersion and a ModifiedVersion/>
    ///     <item Have a version type/>
    /// </list>
    /// </remarks>
    public class Entity : ICloneable, IComparable<Entity>, IEquatable<Entity>
    {
        #region Class Variables
        /// <summary>
        /// The internal dictionary used to store the fields in this Entity
        /// </summary>
        private Dictionary<string, EntityField> fieldDict_ = new Dictionary<string, EntityField>();
        #endregion //Class Variables

        #region Ctors and Dtors
        /// <summary>
        /// Constructs an Entity and sets the EntityTypeName
        /// </summary>
        /// <param name="entityTypeName"></param>
        public Entity(string entityTypeName)
        {
            EntityTypeName = entityTypeName;
        }


        /// <summary>
        /// Constructs an Entity by copying <paramref name="entity"/>
        /// </summary>
        /// <param name="entity"></param>
        public Entity(Entity entity)
        {
            CopyFields(entity);
        }
        #endregion Ctors and Dtors

        #region Internal Methods
        /// <summary>
        /// Compares the ID values of the two entities.
        /// </summary>
        /// <param name="sourceEntity"></param>
        /// <param name="camparisonEntity"></param>
        /// <returns>
        /// Less than zero if <paramref name="sourceEntity"/> preceeds <paramref name="camparisonEntity in the sort order"/>
        /// Zero if <paramref name="sourceEntity"/> occurs in the same position in the sort order as <paramref name="camparisonEntity in the sort order"/>
        /// Greater than zero if <paramref name="sourceEntity"/> follows <paramref name="camparisonEntity in the sort order"/>
        /// </returns>
        private static int CompareEntity(Entity sourceEntity, Entity camparisonEntity)
        {
            return sourceEntity.ID - camparisonEntity.ID;
        }

        private static bool CompareForEquality(Entity sourceEntity, Entity camparisonEntity)
        {
            bool ret = camparisonEntity.EntityTypeName == sourceEntity.EntityTypeName &&
                camparisonEntity.DotNetType == sourceEntity.DotNetType &&
                camparisonEntity.NameFieldName == sourceEntity.NameFieldName &&
                camparisonEntity.IDFieldName == sourceEntity.IDFieldName &&
                camparisonEntity.IDType == sourceEntity.IDType &&
                camparisonEntity.CreatedVersionFieldName == sourceEntity.CreatedVersionFieldName &&
                camparisonEntity.ModifiedVersionFieldName == sourceEntity.ModifiedVersionFieldName &&
                camparisonEntity.VersionType == sourceEntity.VersionType;
            if (ret)
            {
                ret = camparisonEntity.FieldDictionary.Count == sourceEntity.FieldDictionary.Count;
                if (ret)
                {
                    foreach (EntityField field in sourceEntity.FieldDictionary.Values)
                    {
                        // if the field does not exist or the values aren't the same return false
                        if (!camparisonEntity.FieldDictionary.Keys.Contains(field.Name) ||
                            camparisonEntity[field.Name].Value != field.Value) return false;
                    }
                }
            }
            return ret;

        }

        private static void CopyFields(Entity sourceEntity, Entity destinationEntity)
        {
            // Copy the properties
            destinationEntity.EntityTypeName = sourceEntity.EntityTypeName;
            destinationEntity.DotNetType = sourceEntity.DotNetType;
            destinationEntity.NameFieldName = sourceEntity.NameFieldName;
            destinationEntity.IDFieldName = sourceEntity.IDFieldName;
            destinationEntity.IDType = sourceEntity.IDType;
            destinationEntity.SyncIDFieldName = sourceEntity.SyncIDFieldName;
            destinationEntity.SyncIDType = sourceEntity.SyncIDType;
            destinationEntity.CreatedVersionFieldName = sourceEntity.CreatedVersionFieldName;
            destinationEntity.ModifiedVersionFieldName = sourceEntity.ModifiedVersionFieldName;
            destinationEntity.VersionType = sourceEntity.VersionType;

            // Copy fields
            destinationEntity.fieldDict_ = sourceEntity.FieldDictionary.ToDictionary(entry => entry.Key,
                                   entry => entry.Value.Clone() as EntityField);

        }

        /// <summary>
        /// Copies all the properties and field values into this Entity
        /// </summary>
        /// <param name="entity">The Entity from which to copy the values</param>
        private void CopyFields(Entity entity)
        {
            Entity.CopyFields(entity, this);
        }
        #endregion Internal Methods

        #region Properties

        /// <summary>
        /// Indexer for Entity - Get or Set the EntityField for this Entity
        /// </summary>
        /// <param name="fieldName">The name of the Field to get or set</param>
        /// <returns>EntityField named <paramref name="filedName"/></returns>
        public EntityField this[string fieldName]
        {
            get
            {
                return fieldDict_[fieldName];
            }
            set
            {
                fieldDict_[fieldName] = value;
            }
        }
        /// <summary>
        /// Get the Field Dictionary for this Entity.
        /// </summary>
        /// <remarks>
        /// The Field Dictionary provides a collection of <typeparamref name="EntityField"/> objects
        /// that are used to define the fields on this entity.
        /// </remarks>
        public Dictionary<string, EntityField> FieldDictionary
        {
            get { return fieldDict_; }
        }

        /// <summary>
        /// Get or Set the Name of the Entity Type
        /// </summary>
        /// <remarks>Each type of entity is identified by the name of the entity type.
        /// For example, a "Customer" entity will store customers.</remarks>
        public string EntityTypeName { get; set; }

        /// <summary>
        /// Gets or sets the name of this entity instance.
        /// </summary>
        /// <exception cref="KeyNotFoundException"> The Name field does not exist in the Field Dictionary.</exception>
        /// <remarks>
        /// This will Get or Set the Value for the <see cref="NameFieldName"/>field.
        /// If the field does not exist it will return null
        /// 
        /// The name of an entity is the value of a field that represents the name
        /// of a specific instance of an entity. 
        /// For example, the name of a customer may be stored in the "Name" field so
        /// this property would return FieldDictionary["Name"] which may containt a value
        /// of "Acme Company"</remarks>
        /// <remarks>
        /// </remarks>
        public string Name
        {
            // Get the value from the NameField field
            get
            {
                // Contract.Requires(null != FieldMap);
                if (null == fieldDict_) throw new System.InvalidOperationException("FieldDictionary has not been initialized");
                return fieldDict_[NameFieldName].Value;
            }
            // Set the value on the NameField
            set
            {
                if (null == fieldDict_) throw new System.InvalidOperationException("FieldDictionary has not been initialized");
                fieldDict_[NameFieldName].Value = value;
            }
        }

        /// <summary>
        /// Gets or Sets the .Net type that can represent this entity in memory
        /// </summary>
        /// <remarks>The .Net type is an object that represents an instance of an Entity.
        /// For example, a "Customer" entity type may be represnted in memory as a MyNameSpace.Customer
        /// object. MyNameSpace.Customer would have a property with the same name for each field
        /// in the <seealso cref="FieldDictionary"/>.
        /// </remarks>
        public System.Type DotNetType { get; set; }

        /// <summary>
        /// Get or Set the name of the field used for this Entity's name. 
        /// This is the same as the key for the field in the map that represents the Name of the Entity.
        /// </summary>
        /// <remarks>
        /// The default value is "Name" which in general will map to the "Title" field in SP and "Name" in EA
        /// </remarks>
        public string NameFieldName { get; set; }

        /// <summary>
        /// The field name that contains the unique ID for the Entity
        /// </summary>
        public string IDFieldName { get; set; }

        /// <summary>
        /// The Type of the value used for the unique ID for the Entity
        /// </summary>
        public Type IDType { get; set; }

        /// <summary>
        /// The field name that contains the unique SyncID for the Entity
        /// </summary>
        public string SyncIDFieldName { get; set; }

        /// <summary>
        /// The Type of the value used for the unique SyncID for the Entity
        /// </summary>
        public Type SyncIDType { get; set; }


        /// <summary>
        /// Gets and Sets the ID value for the Entity
        /// </summary>
        public dynamic ID
        {
            // Get the value from the NameField field
            get
            {
                // Contract.Requires(null != FieldMap);
                if (null == fieldDict_) throw new System.InvalidOperationException("FieldDictionary has not been initialized");
                return fieldDict_[IDFieldName].Value;
            }
            // Set the value on the NameField
            set
            {
                if (null == fieldDict_) throw new System.InvalidOperationException("FieldDictionary has not been initialized");
                fieldDict_[IDFieldName].Value = value;
            }
        }

        public dynamic SyncID
        {
            // Get the value from the NameField field
            get
            {
                // Contract.Requires(null != FieldMap);
                if (null == fieldDict_) throw new System.InvalidOperationException("FieldDictionary has not been initialized");
                return fieldDict_[SyncIDFieldName].Value;
            }
            // Set the value on the NameField
            set
            {
                if (null == fieldDict_) throw new System.InvalidOperationException("FieldDictionary has not been initialized");
                fieldDict_[SyncIDFieldName].Value = value;
            }
        }

        public string ModifiedVersionFieldName { get; set; }
        public string CreatedVersionFieldName { get; set; }
        public Type VersionType { get; set; }

        public dynamic CreatedVersion
        {
            get
            {
                // Contract.Requires(null != FieldMap);
                if (null == fieldDict_) throw new System.InvalidOperationException("FieldDictionary has not been initialized");
                return fieldDict_[CreatedVersionFieldName].Value;
            }
            // Set the value on the NameField
            set
            {
                if (null == fieldDict_) throw new System.InvalidOperationException("FieldDictionary has not been initialized");
                fieldDict_[CreatedVersionFieldName].Value = value;
            }
        }

        public dynamic ModifiedVersion
        {
            get
            {
                // Contract.Requires(null != FieldMap);
                if (null == fieldDict_) throw new System.InvalidOperationException("FieldDictionary has not been initialized");
                return fieldDict_[ModifiedVersionFieldName].Value;
            }
            // Set the value on the NameField
            set
            {
                if (null == fieldDict_) throw new System.InvalidOperationException("FieldDictionary has not been initialized");
                fieldDict_[ModifiedVersionFieldName].Value = value;
            }
        }

        public Func<Entity, bool> DeletedPredicate
        {
            get;
            set;
        }

        public bool IsDeleted
        {
            get { return DeletedPredicate(this); }
        }
        #endregion Properties

        #region Static Helper Methods
        #endregion Static Helper Methods

        #region Instance Methods

        public Dictionary<string, EntityField>.ValueCollection.Enumerator GetEnumerator()
        {
            return FieldDictionary.Values.GetEnumerator();
        }

        /// <summary>
        /// Adds a field to the <seealso cref="FieldDictionary"/>
        /// </summary>
        /// <param name="fieldName">The name of the field to add to the dictionary</param>
        /// <param name="field">An instance of the field to be added</param>
        public void AddField(string fieldName, EntityField field)
        {
            FieldDictionary.Add(fieldName, field);
        }


        /// <summary>
        /// Read the value for field <paramref name="fieldName"/> from the data source
        /// </summary>
        /// <typeparam name="T">The type of value stored in <paramref name="fieldName"/> 
        /// in the data source</typeparam>
        /// <param name="fieldName">The name of the field to read from the data source</param>
        /// <returns>The value stored in <paramref name="fieldName"/> in the data source</returns>
        public T ReadField<T>(string fieldName)
        {
            if (!fieldDict_.ContainsKey(fieldName)) throw new ArgumentException(string.Format("Field {0} is not defined in Entity {1}", fieldName, this.EntityTypeName));
            EntityField field = FieldDictionary[fieldName];

            // Make sure template parameter type is correct
            Type typedValue = typeof(T);
            if (typedValue != field.ValueType) throw new InvalidCastException(
                 string.Format("Entity ValueType {0} does not match requested type", field.ValueType.ToString()));
            return field.GetTypedValue<T>();
        }

        /// <summary>
        /// Write <paramref name="fieldValue"/> to the Data source
        /// </summary>
        /// <typeparam name="T">The type of fieldValue</typeparam>
        /// <param name="fieldName">The name of the field to write</param>
        /// <param name="fieldValue">The value to write to <paramref name="fieldName"/></param>
        /// <returns>Returns True if field is written</returns>
        public bool WriteField<T>(string fieldName, T fieldValue)
        {
            // Make sure the field name is correct.
            if (!fieldDict_.ContainsKey(fieldName)) throw new ArgumentException(string.Format("Field {0} is not defined in Entity {1}", fieldName, this.EntityTypeName));
            EntityField field = FieldDictionary[fieldName];

            // Make sure template parameter type is correct
            Type typedValue = fieldValue.GetType();
            if (typedValue != field.ValueType) throw new InvalidCastException(
                 string.Format("Entity ValueType {0} does not match requested type", field.ValueType.ToString()));

            field.Value = fieldValue;
            return true;
        }

        /// <summary>
        /// Read the Entity ID from the Data Source and return as IDType
        /// </summary>
        /// <typeparam name="IDType">The Type of value used for the ID</typeparam>
        /// <returns>Entity ID</returns>
        public IDType ReadID<IDType>()
        {
            return ReadField<IDType>(IDFieldName);
        }

        /// <summary>
        /// Write the Entity ID to the Data Source
        /// </summary>
        /// <typeparam name="IDType">The Type of value used for the ID</typeparam>
        /// <param name="id"></param>
        /// <returns>True if the ID value was updated</returns>
        public bool WriteID<IDType>(IDType id)
        {
            return WriteField<IDType>(IDFieldName, id);
        }

        /// <summary>
        /// Read the Entity CreatedVersion from the Data Source and return as VerType
        /// </summary>
        /// <typeparam name="VerType">The Type of value used for the CreatedVersion</typeparam>
        /// <returns>Entity Entity CreatedVersion</returns>
        public VerType ReadCreatedVersion<VerType>()
        {
            return ReadField<VerType>(CreatedVersionFieldName);
        }

        /// <summary>
        /// Read the Entity ModifiedVersion from the Data Source and return as VerType
        /// </summary>
        /// <typeparam name="VerType">The Type of value used for the ModifiedVersion</typeparam>
        /// <returns>Entity Entity ModifiedVersion</returns>
        public VerType ReadModifiedVersion<VerType>()
        {
            return ReadField<VerType>(ModifiedVersionFieldName);
        }

        /// <summary>
        /// Write the Entity CreatedVersion to the Data Source
        /// </summary>
        /// <typeparam name="VerType">The Type of value used for the CreatedVersion</typeparam>
        /// <param name="version"></param>
        /// <returns>True if the version value was updated</returns>
        public bool WriteCreatedVersion<VerType>(VerType version)
        {
            return WriteField<VerType>(CreatedVersionFieldName, version);
        }

        /// <summary>
        /// Write the Entity ModifiedVersion to the Data Source
        /// </summary>
        /// <typeparam name="VerType">The Type of value used for the ModifiedVersion</typeparam>
        /// <param name="version"></param>
        /// <returns>True if the version value was updated</returns>
        public bool WriteModifiedVersion<VerType>(VerType version)
        {
            return WriteField<VerType>(ModifiedVersionFieldName, version);
        }

        /// <summary>
        /// Return an entity as the .NET type
        /// </summary>
        /// <returns>an instance of the .NET type for this entity</returns>
        public dynamic ReadEntity()
        {
            foreach (EntityField ef in fieldDict_.Values)
            {
                var v = ef.Value;
            }

            var entity = DotNetType.GetConstructor(new Type[] { }).Invoke(new object[] { });

            PropertyInfo[] pi = DotNetType.GetProperties();
            foreach (PropertyInfo p in pi)
            {
                if (fieldDict_.ContainsKey(p.Name) && p.CanWrite)
                {
                    p.SetValue(entity, this[p.Name].Value);
                }
            }

            return entity;
        }

        /// <summary>
        /// Clone the Entity
        /// </summary>
        /// <returns>A deep copy clone of this Entity</returns>
        public object Clone()
        {
            Entity newEntity = new Entity(this.EntityTypeName);
            Entity.CopyFields(this, newEntity);
            return newEntity;
        }

        #region IComparable<Entity>

        /// <summary>
        /// Compares this instance to other and returns 
        /// </summary>
        /// <param name="other"></param>
        /// <returns>
        /// Less than zero - This object precedes <paramref name="other"/> in the sort order
        /// Zero - This current instance occurs in the same position in the sort order as <paramref name="other"/>
        /// Greater than zero - This current instance follows <paramref name="other"/> in the sort order
        /// </returns>
        public int CompareTo(Entity other)
        {
            if (other == null)
                return 1;
            return Entity.CompareEntity(this, other);
        }
        // Define the is greater than operator.
        public static bool operator >(Entity lhs, Entity rhs)
        {
            return lhs.CompareTo(rhs) > 0;
        }

        // Define the is less than operator.
        public static bool operator <(Entity lhs, Entity rhs)
        {
            return lhs.CompareTo(rhs) < 0;
        }

        // Define the is greater than or equal to operator.
        public static bool operator >=(Entity lhs, Entity rhs)
        {
            return lhs.CompareTo(rhs) >= 0;
        }

        // Define the is less than or equal to operator.
        public static bool operator <=(Entity lhs, Entity rhs)
        {
            return lhs.CompareTo(rhs) <= 0;
        }

        #endregion IComparable<Entity>

        #region IEquatable<Entity>

        public bool Equals(Entity other)
        {
            if (other == null)
                return false;

            return Entity.CompareForEquality(this, other);
        }
        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;
            Entity entity = obj as Entity;
            if (entity == null)
                return false;
            else
                return Entity.CompareForEquality(this, entity);
        }

        public override int GetHashCode()
        {
            int ret = EntityTypeName.GetHashCode() +
                DotNetType.GetHashCode() +
                NameFieldName.GetHashCode() +
                IDFieldName.GetHashCode() +
                IDType.GetHashCode() +
                CreatedVersionFieldName.GetHashCode() +
                ModifiedVersionFieldName.GetHashCode() +
                VersionType.GetHashCode();
            foreach (EntityField field in FieldDictionary.Values)
            {
                ret += field.Name.GetHashCode() + field.ValueType.GetHashCode() + field.Value.ToString().GetHashCode();
            }
            return ret;
        }

        public static bool operator ==(Entity entity1, Entity entity2)
        {
            if (((object)entity1) == null || ((object)entity2) == null)
                return Object.Equals(entity1, entity2);

            return entity1.Equals(entity2);
        }

        public static bool operator !=(Entity entity1, Entity entity2)
        {
            if (((object)entity1) == null || ((object)entity2) == null)
                return !Object.Equals(entity1, entity2);

            return !(entity1.Equals(entity2));
        }
        #endregion IEquatable<Entity>

        #endregion Instance Methods

    }

    /// <summary>
    /// Reperesents a field in an entity datasource
    /// </summary>
    /// <typeparam name="T">The type of value stored in the field</typeparam>
    public class EntityField : ICloneable
    {
        #region Class Variables

        private IEntityField fieldImpl_;
        #endregion //Class Variables

        #region Ctors and Dtors
        /// <summary>
        /// Hidden (Private) default constructor
        /// </summary>
        /// <remarks>You must pass the IEntityField implementation object to instantiate an EntityField</remarks>
        private EntityField()
        {

        }

        /// <summary>
        /// Constructs an EntityField aggregating <paramref name="fieldImpl"/> for the IEntityField implementation.
        /// </summary>
        /// <param name="fieldImpl"></param>
        /// <remarks>You must use this consturctor to instantiate an EntityField</remarks>
        public EntityField(IEntityField fieldImpl)
        {
            if (null == fieldImpl) throw new ArgumentNullException("fieldImpl", "Parameter can not be null.");
            fieldImpl_ = fieldImpl;
        }

        #endregion Ctors and Dtors

        #region Properties
        /// <summary>
        /// Gets or Sets the field value
        /// </summary>
        /// <remarks>This property could call the ReadField() or WriteField() methods or 
        /// utilize some form of caching.
        /// </remarks>
        public dynamic Value
        {
            get
            {
                return fieldImpl_.ReadField();
            }
            set
            {
                fieldImpl_.WriteField(value);
            }
        }

        /// <summary>
        /// Gets or sets the Field Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Get the type used to store value
        /// </summary>
        public System.Type ValueType
        {
            get { return fieldImpl_.ValueType; }
        }
        #endregion Properties

        #region Instance Methods
        /// <summary>
        /// Get the value of the field in a type safe manner
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Value as type T</returns>
        public T GetTypedValue<T>()
        {
            Type typedValue = typeof(T);
            if (typedValue != fieldImpl_.ValueType) throw new InvalidCastException(
                 string.Format("Entity ValueType {0} does not match requested type", fieldImpl_.ValueType.ToString()));
            return (T)Value;
        }

        /// <summary>
        /// Clone this EnityField
        /// </summary>
        /// <returns>A deep copy clone of the this EntityField</returns>
        public object Clone()
        {
            EntityField newField = new EntityField(fieldImpl_.Clone() as IEntityField);
            newField.Name = this.Name;
            return newField;
        }

        #endregion Instance Methods

    }

    public interface IEntityField : ICloneable
    {
        string FieldName { get; set; }
        /// <summary>
        /// Reads the field value from the data source.
        /// </summary>
        /// <returns>The value stored in the field value</returns>
        dynamic ReadField();
        /// <summary>
        /// Writes value to the field in the data source.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True if value was succesfully writen to the data source</returns>
        /// <remarks>value will be validated prior to writing.</remarks>
        bool WriteField(dynamic value);
        /// <summary>
        /// Gets or Sets the field value
        /// </summary>
        /// <remarks>This property could call the ReadField() or WriteField() methods or 
        /// utilize some form of caching.
        /// </remarks>
        dynamic Value { get; set; }

        /// <summary>
        /// Validates value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True if value contains a valid value. Otherwise returns false.</returns>
        bool Validate(dynamic value);

        /// <summary>
        /// Get the type used to store value
        /// </summary>
        System.Type ValueType { get; }

    }


    public class EqualEntities : EqualityComparer<Entity>
    {
        public override bool Equals(Entity e1, Entity e2)
        {
            if (e1 == null && e2 == null)
                return true;
            else if (e1 == null || e2 == null)
                return false;

            foreach (EntityField field in e1.FieldDictionary.Values)
            {
                try
                {
                    if (field.Value != e2.FieldDictionary[field.Name].Value) return false;
                }
                catch (Exception ex) { }
            }

            foreach (EntityField field in e2.FieldDictionary.Values)
            {
                try
                {
                    if (field.Value != e1.FieldDictionary[field.Name].Value) return false;
                }
                catch (Exception ex) { }
            }

            return true;
        }

    public override int GetHashCode(Entity e)
    {
        return e.GetHashCode();
    }
}

}


