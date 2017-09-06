using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSIEntityFramework
{

    public class CSVField<T> : IEntityField
    {
        private T value_;

        public string FieldName { get; set; }

        private CSVField()
        { }

        public CSVField(string fieldName)
        {
            FieldName = fieldName;
        }

        public dynamic ReadField()
        {

            return value_;
        }

        public bool WriteField(T value)
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

        public bool Validate(T value)
        {

            return (value.GetType() == typeof(T));
        }


        public Type ValueType
        {
            get { return typeof(T); }
        }

        public object Clone()
        {
            CSVField<T> newField = new CSVField<T>(FieldName);
            newField.Value = this.Value;
            return newField;
        }


        public bool WriteField(dynamic value)
        {
            return WriteField((T)value);
        }

        public bool Validate(dynamic value)
        {
            return Validate((T)value);
        }
    }


    /// <summary>
    /// This DataSource represents a CSV file. It will read the entire file into memory if
    /// you attempt to get an entity by ID.
    /// </summary>
    /// <remarks>
    /// This Datasource assumes that if there is a file that is named exactly the same as the
    /// CSV file, only with an appended ".deleted" it will contain all entity instances 
    /// (records) that have been deleted.
    /// </remarks>
    public class CSVDataSource : ISSIDataSource
    {
        #region Class Variables
        private string connectionString_;
        private CsvHelper.CsvReader csvReader_;
        private CsvHelper.CsvWriter csvWriter_;
        private bool connected_ = false;
        private bool readYet_ = false;
        private Dictionary<dynamic, Entity> records_;
        private Dictionary<dynamic, Entity> deletedRecords_;
        private Entity entity_;

        #endregion Class Variables

        #region ctors & dtors

        /// <summary>
        /// Hide the default constructor
        /// </summary>
        private CSVDataSource()
        { }

        /// <summary>
        /// Requires Entity Definition for instantiation
        /// </summary>
        /// <param name="entity"></param>
        public CSVDataSource(Entity entity)
        {
            // Clone the entity so there are no references to the internal entity
            entity_ = new Entity(entity);
            records_ = new Dictionary<dynamic, Entity>();
            deletedRecords_ = new Dictionary<dynamic, Entity>();
        }

        #endregion ctors & dtors

        #region ISSIDataSource

        public string ConnectionString
        {
            get
            {
                return connectionString_;
            }
            set
            {
                // If the file does not exist we will create one when we write
                connectionString_ = value;
            }
        }

        public Type EntityDotNetType
        {
            get
            {
                return entity_.DotNetType;
            }
        }

        public void Connect()
        {
            // If the file does not exist we will create one when we write
            if (System.IO.File.Exists(ConnectionString))
            {
                using (csvReader_ = new CsvHelper.CsvReader(new System.IO.StreamReader(ConnectionString)))
                {
                    connected_ = true;
                    ReadAllRecords();
                }
                csvReader_ = null;
            }
            else connected_ = true;

            // first load the existing deleted records
            if (System.IO.File.Exists(ConnectionString + ".deleted"))
            {
                using (csvReader_ = new CsvHelper.CsvReader(new System.IO.StreamReader(ConnectionString + ".deleted")))
                {
                    ReadAllDeletedRecords(csvReader_);
                }
            }
        }

        public void Connect(string connectionString)
        {
            ConnectionString = connectionString;
            Connect();
        }

        public bool IsConnected()
        {
            return connected_;
        }


        public void Disconnect()
        {
            using (csvWriter_ = new CsvHelper.CsvWriter(new System.IO.StreamWriter(ConnectionString)))
            {
                WriteAllRecords();
            }
            records_.Clear();
            csvWriter_ = null;

            // Write deleted records if any records have been deleted
            if (deletedRecords_.Count > 0)
            {
                using (System.IO.StreamWriter deltedStream = new System.IO.StreamWriter(ConnectionString + ".deleted"))
                {
                    // deltedStream.BaseStream.Position = deltedStream.BaseStream.Length;

                    using (csvWriter_ = new CsvHelper.CsvWriter(deltedStream))
                    {
                        WriteDeletedRecords();
                    }
                }
            }
            connected_ = false;
            deletedRecords_.Clear();
        }

        public System.Collections.Generic.IEnumerator<Entity> GetEnumerator()
        {
            return records_.Values.GetEnumerator();
        }

        public Entity GetEntity<IDType>(IDType id)
        {
            // Clone the entity so there are no references to the internal entity
            System.Diagnostics.Debug.Assert(null != records_);
            return new Entity(records_[id]);
        }

        public Entity CreateEntity(Entity entity)
        {
            UpsertRecord(entity);
            return new Entity(records_[entity.ID]);
        }

        public void DeleteEntity<IDType>(IDType id)
        {
            deletedRecords_[id] = records_[id];
            records_.Remove(id);
        }

        public void DeleteEntity(Entity entity)
        {
            DeleteEntity(entity.ID);
        }

        public void UpdateEntity(Entity entity)
        {
            // Don't insert - make sure entity exists
            if (!records_.ContainsKey(entity.ID)) throw new ArgumentOutOfRangeException("entity",
                 string.Format("Entity {0} does not exist and can not be updated.", entity.ID));

            UpsertRecord(entity);
        }

        public IEnumerable<Entity> GetDeletedEntities(dynamic version)
        {
            return deletedRecords_.Values.Where(e => e.ModifiedVersion >= version).ToList();
        }

        public IEnumerable<Entity> GetAddedEntities(dynamic version)
        {
            return records_.Values.Where(e => e.CreatedVersion >= version).ToList();
        }

        public IEnumerable<Entity> GetModifiedEntities(dynamic version)
        {
            return records_.Values.Where(e => e.ModifiedVersion >= version).ToList();
        }

        #endregion ISSIDataSource

        #region Static Helper Methods

        /// <summary>
        /// Returns a list of EntityFields that store their value as strings
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the 
        /// CSV data source.</param>
        /// <returns>a list of EntityFields</returns>
        public static List<EntityField> GetEntityFields(string connectionString)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(connectionString));
            System.Diagnostics.Debug.Assert(System.IO.File.Exists(connectionString));

            List<EntityField> list = new List<EntityField>();
            // Create a temporary reader
            using (System.IO.StreamReader textReader = new System.IO.StreamReader(connectionString))
            {

                // Get the import file
                CsvHelper.CsvReader csvReader = new CsvHelper.CsvReader(textReader);

                csvReader.Read();

                foreach (string fieldName in csvReader.FieldHeaders)
                {
                    EntityField field = new EntityField(new CSVField<string>(fieldName));
                    list.Add(field);
                }
            }
            return list;
        }
        #endregion Static Helper Methods

        #region Internal Methods
        private bool Read()
        {
            System.Diagnostics.Debug.Assert(null != csvReader_);
            try
            {
                if (csvReader_.Read())
                {
                    ReadEntity(csvReader_, entity_);
                    //todo: does this need to be the return or is entity_ passed by ref?
                    UpsertRecord(entity_);
                    readYet_ = true;
                }
                else return false;
                return true;
            }
            catch (CsvHelper.CsvReaderException e)
            {
                // Eat the exception & return
                return false;
            }
        }

        /// <summary>
        /// Reads a record from a CsvReader into an entity
        /// </summary>
        /// <param name="reader"></param>
        private static Entity ReadEntity(CsvHelper.CsvReader reader, Entity entity)
        {
            foreach (EntityField field in entity)
            {
                entity[field.Name].Value = System.Convert.ChangeType(reader.GetField(field.Name),
                    entity[field.Name].ValueType);
            }
            return entity;
        }

        private bool ReadDeleted(CsvHelper.CsvReader reader)
        {
            System.Diagnostics.Debug.Assert(null != reader);
            try
            {
                if (reader.Read())
                {
                    Entity newEntity = new Entity(entity_);
                    ReadEntity(reader, newEntity);
                    //todo: does this need to be the return or is newEntity passed by ref?
                    deletedRecords_[newEntity.ID] = newEntity;
                }
                else return false;
                return true;
            }
            catch (CsvHelper.CsvReaderException e)
            {
                // Eat the exception & return
                return false;
            }
        }

        private void WriteHeader()
        {
            foreach (var field in entity_)
            {
                csvWriter_.WriteField(field.Name);
            }
            csvWriter_.NextRecord();
        }

        private void WriteEntity(Entity entity)
        {
            foreach (var field in entity)
            {
                csvWriter_.WriteField(field.Value);
            }
            csvWriter_.NextRecord();
        }

        private void WriteAllRecords()
        {
            WriteHeader();
            foreach (var record in records_)
            {
                WriteEntity(record.Value);
            }
        }

        private void WriteDeletedRecords()
        {
            WriteHeader();
            foreach (var record in deletedRecords_)
            {
                WriteEntity(record.Value);
            }
        }

        private void UpsertRecord(Entity entity)
        {
            System.Diagnostics.Debug.Assert(null != records_);
            records_[entity.ID] = new Entity(entity);
        }

        private void ReadAllRecords()
        {
            while (Read()) { };
        }

        private void ReadAllDeletedRecords(CsvHelper.CsvReader reader)
        {
            while (ReadDeleted(reader)) { };
        }
        #endregion Internal Methods

        #region Properties
        public Entity this[dynamic id]
        {
            get
            {
                return GetEntity(id);
            }
            set
            {
                System.Diagnostics.Debug.Assert(value.ID == id);
                if (value.ID != id) throw new ArgumentException(
                     string.Format("Invalid ID. Entity with ID {0} can't be assigned at key {1}", value.ID, id));
                UpsertRecord(value);
            }
        }
        #endregion Properties

        #region Instance Methods
        #endregion Instance Methods

    }
}
