
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSIEntityFramework
{

    public interface ISSIDataSource
    {
        /// <summary>
        /// Get and Set the Connection String
        /// </summary>
        string ConnectionString { get; set; }
        
        /// <summary>
        /// Connect to the data source
        /// </summary>
        void Connect();
        void Connect(string connectionString);

        /// <summary>
        /// Disconnects from the Data Source
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Returns true if Datasource is connected.
        /// </summary>
        /// <returns></returns>
        bool IsConnected();

        /*
        /// <summary>
        /// Get the Entity fields as a List of EntityFields
        /// </summary>
        /// <returns>A list of EntityFields</returns>
        // static List<EntityField> GetEntityFields(string connectionString);
        */


        /// <summary>
        /// Get the type of entity in the datasource
        /// </summary>
        System.Type EntityDotNetType { get; }


        /// <summary>
        /// Get an enumerator that can be used to iterate through the datasource
        /// </summary>
        /// <returns></returns>
        System.Collections.Generic.IEnumerator<Entity> GetEnumerator();

        /// <summary>
        /// Get an Entity using its ID value
        /// </summary>
        /// <typeparam name="IDType">The value type used for the Entity ID</typeparam>
        /// <param name="id">The ID value to use to get the Entity</param>
        /// <returns>An Entity from the data source</returns>
        /// <remarks>Depending on the DataSource, this may require a sequential read
        /// until the entity is found, or until the end of the data source is reached.</remarks>
        Entity GetEntity<IDType>(IDType id);

        /// <summary>
        /// Create a new Entity in the data source
        /// </summary>
        /// <param name="entity">The Entity contianing the values to use when creating the 
        /// new Entity in the data source</param>
        /// <returns>The newly created entity with any values populated by the data source
        /// during the create operation</returns>
        Entity CreateEntity(Entity entity);

        /// <summary>
        /// Delete the Entity with ID value of <paramref name="id"/> from the data source
        /// </summary>
        /// <typeparam name="IDType">The value type used for the Entity ID</typeparam>
        /// <param name="id">The ID value to use to identify the Entity to delete</param>
        void DeleteEntity<IDType>(IDType id);
        /// <summary>
        /// Delete the <paramref name="entity"/> from the data source
        /// </summary>
        /// <param name="entity">The Entity to delete</param>
        void DeleteEntity(Entity entity);
        void UpdateEntity(Entity entity);

        /// <summary>
        /// Return an enumerator of entities that have been deleted since <paramref name="version"/>
        /// </summary>
        /// <param name="version">The version indicator for filtering the deleted items</param>
        /// <returns></returns>
        IEnumerable<Entity> GetDeletedEntities(dynamic version);

        IEnumerable<Entity> GetAddedEntities(dynamic version);

        IEnumerable<Entity> GetModifiedEntities(dynamic version);

        IEnumerable<Entity> GetDeletedEntities();

        IEnumerable<Entity> GetAddedEntities();

        IEnumerable<Entity> GetModifiedEntities();

        IEnumerable<Entity> GetEntities();




        //////////////////////////////////
        Entity GetSyncEntity(dynamic id);


        Entity CreateSyncEntity(Entity entity);


        void DeleteSyncEntity(dynamic id);

        void DeleteSyncEntity(Entity entity);

        void UpdateSyncEntity(Entity entity);
        

    }

    public class FieldMap
    {
        public IEntityField FieldA { get; set; }
        public IEntityField FieldB { get; set; }

        public Func<dynamic, dynamic, dynamic> A2BTransform { get; set; }
        public Func<dynamic, dynamic, dynamic> B2ATransform { get; set; }
    }

    /// <summary>
    /// Defines the mapping between two Entities in two data sources
    /// </summary>
    public class SynchronizationMap<AID, BID>
    {
        private Dictionary<string, FieldMap> fieldMap_ = new Dictionary<string, FieldMap>();
        private Dictionary<string, string> A2BFieldMap_ = new Dictionary<string, string>();
        private Dictionary<string, string> B2AFieldMap_ = new Dictionary<string, string>();
        public SynchronizationMap()
        {

        }
        public ISSIDataSource DataSourceA { get; set; }
        public ISSIDataSource DataSourceB { get; set; }

        public Dictionary<string, string> DSA2BFieldMap
        {
            get { return A2BFieldMap_; }
        }

        public Dictionary<string, string> DSB2AFieldMap
        {
            get { return B2AFieldMap_; }
        }

        public void AddFieldMap(FieldMap fm)
        {
            fieldMap_[fm.FieldA.FieldName] = fm;
            A2BFieldMap_[fm.FieldA.FieldName] = fm.FieldB.FieldName;
            B2AFieldMap_[fm.FieldB.FieldName] = fm.FieldA.FieldName;
        }

        public void AddFieldMap<TA, TB>(IEntityField SourceAField, IEntityField SourceBField,
        Func<TA,TB,TB> A2BTransform, Func<TB, TA, TA> B2ATransform)
        {
            FieldMap fm = new FieldMap();
            fm.FieldA = SourceAField;
            fm.FieldB = SourceBField;
            fm.A2BTransform = A2BTransform as Func<dynamic, dynamic, dynamic>;
            fm.B2ATransform = B2ATransform as Func<dynamic, dynamic, dynamic>;
            AddFieldMap(fm);
        }

        public void AddFieldMap<TA, TB>(IEntityField SourceAField, IEntityField SourceBField)
        {
            FieldMap fm = new FieldMap();
            fm.FieldA = SourceAField;
            fm.FieldB = SourceBField;
            AddFieldMap(fm);
        }

        private HashSet<AID> SynchronizeNewDSAEntities()
        {
            // Check for each 
            HashSet<AID> NewBEntities = new HashSet<AID>();
            return NewBEntities;
        }

        private HashSet<BID> SynchronizeNewDSBEntities()
        {
            HashSet<BID> NewBEntities = new HashSet<BID>();
            return NewBEntities; 
        }

        /// <summary>
        /// Synchronize the items that were deleted in Datasource A
        /// </summary>
        /// <param name="permanentlyRemove">
        /// Permanently remove deleted items from Datasource A (if True)
        /// </param>
        /// <remarks>
        /// Primarily this means to delete the same entities from Datasource B
        /// (if they exist) and then permanently remove them from Datasource A if requested
        /// </remarks>
        private HashSet<AID> SynchronizeDeletedDSAEntities(bool permanentlyRemove)
        {
            HashSet<AID> NewBEntities = new HashSet<AID>();
            return NewBEntities;
        }


        /// <summary>
        /// Synchronize the items that were deleted in Datasource B
        /// </summary>
        /// <param name="permanentlyRemove">
        /// Permanently remove deleted items from Datasource B (if True)
        /// </param>
        /// <remarks>
        /// Primarily this means to delete the same entities from Datasource A
        /// (if they exist) and then permanently remove them from Datasource B if requested
        /// </remarks>
        private HashSet<BID> SynchronizeDeletedDSBEntities(bool permanentlyRemove)
        {
            HashSet<BID> NewBEntities = new HashSet<BID>();
            return NewBEntities;
        }

    }
}
