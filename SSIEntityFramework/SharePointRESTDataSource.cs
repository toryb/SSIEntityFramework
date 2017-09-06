using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using SP = Microsoft.SharePoint.Client;
using System.Security;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Dynamic;

namespace SSIEntityFramework
{
    public class SharePointRESTDataSource : ISSIDataSource
    {
        //private string siteUrl_;
        private string listName_;
        private Dictionary<dynamic, Entity> records_;
        private Dictionary<dynamic, Entity> deletedRecords_;
        private Entity entity_;
        private bool connected_ = false;
        public string ConnectionString { get; set; }
        public string listName
        {
            get { return listName_; }
            set { listName_ = value; }
        }

        //User Credentials
        public string UserName { get; set; }
        public string Password { get; set; }

        public Type EntityDotNetType
        {
            get { return entity_.DotNetType; }
        }

        public SharePointRESTDataSource(Entity entity)
        {
            // Clone the entity so there are no references to the internal entity
            entity_ = new Entity(entity);
            records_ = new Dictionary<dynamic, Entity>();
            deletedRecords_ = new Dictionary<dynamic, Entity>();
        }

        public void Connect()
        {
            //Try to get the list
            using (var client = new SPHttpClient(new Uri(ConnectionString), UserName, Password))
            {
                var endpointUrl = string.Format("{0}_api/web/lists/GetByTitle('{1}')", new Uri(ConnectionString), listName);
                var data = client.ExecuteJson(endpointUrl);

                if (data != null)       //If it exists
                {
                    endpointUrl = string.Format("{0}_api/web/lists/GetByTitle('{1}')/items", new Uri(ConnectionString), listName);
                    data = client.ExecuteJson(endpointUrl);

                    foreach (var listItem in data["value"])
                    {
                        foreach (EntityField field in entity_)
                        {
                            entity_[field.Name].Value = Convert.ChangeType(listItem[field.Name],
                                entity_[field.Name].ValueType);
                            UpsertRecord(entity_);
                        }
                    }
                    connected_ = true;
                }
                else
                {
                    connected_ = true;          //We'll create one on disconnect
                }


                //get the deleted list
                endpointUrl = string.Format("{0}_api/web/lists/getbytitle('{1}')", new Uri(ConnectionString), listName + "_deleted");
                data = client.ExecuteJson(endpointUrl);

                if (data != null)
                {
                    endpointUrl = string.Format("{0}_api/web/lists/GetByTitle('{1}')/items", new Uri(ConnectionString), listName);
                    data = client.ExecuteJson(endpointUrl);

                    foreach (var listItem in data["value"])
                    {
                        foreach (EntityField field in entity_)
                        {
                            entity_[field.Name].Value = Convert.ChangeType(listItem[field.Name],
                                entity_[field.Name].ValueType);

                            deletedRecords_[entity_.ID] = entity_;
                        }
                    }
                    connected_ = true;
                }

            }
        }

        public void Connect(string connectionString)
        {
            ConnectionString = connectionString;
            Connect();
        }

        public Entity CreateEntity(Entity entity)
        {
            UpsertRecord(entity);
            return new Entity(records_[entity.ID]);
        }

        public void DeleteEntity(Entity entity)
        {
            DeleteEntity(entity.ID);
        }

        public void DeleteEntity<IDType>(IDType id)
        {
            deletedRecords_[id] = records_[id];
            records_.Remove(id);
        }

        public void Disconnect()
        {

            //Write entities
            WriteList(listName_, records_);
            records_.Clear();

            //Write deleted entities
            if (deletedRecords_.Count > 0) WriteList(listName_ + "_deleted", deletedRecords_);
            deletedRecords_.Clear();
            deleteTheDeleted();
            connected_ = false;
        }


        private void WriteList(string listTitle, Dictionary<dynamic, Entity> theRecordSet)
        {

            JObject list;
            using (var client = new SPHttpClient(new Uri(ConnectionString), UserName, Password))
            {
                //Try to get the list
                var endpointUrl = string.Format("{0}_api/web/lists/getbytitle('{1}')", new Uri(ConnectionString), listTitle);
                list = client.ExecuteJson(endpointUrl);

                if (list == null)   //if list does not exist..
                {
                    CreateList(listTitle, client);
                }
            }
            //Write the list
            foreach (Entity entity_ in theRecordSet.Values)
            {
                JObject item;
                var itemId = entity_.ID;
                using (var client = new SPHttpClient(new Uri(ConnectionString), UserName, Password))
                {
                    var endpointUrl = string.Format("{0}_api/web/lists/getbytitle('{1}')/items?$filter=SyncID eq '{2}'", new Uri(ConnectionString), listTitle, itemId);
                    item = client.ExecuteJson(endpointUrl);
                }

                if (item["value"].Count() == 0)   //if item does not exist.. create
                {
                    //Create dynamic object from entity that will be serialized as JSON 
                    dynamic itemPayload = new ExpandoObject();
                    ((IDictionary<string, object>)itemPayload).Add("__metadata", new { type = "SP.Data." + listTitle + "ListItem" });   //metadata

                    foreach (var field in entity_)  //All the entities fields
                    {
                        ((IDictionary<string, object>)itemPayload).Add(field.Name, field.Name == entity_.IDFieldName ? Convert.ToString(field.Value) : field.Value);
                    }

                    //Post
                    using (var client = new SPHttpClient(new Uri(ConnectionString), UserName, Password))
                    {
                        var endpointUrl = string.Format("{0}_api/web/lists/getbytitle('{1}')/items", new Uri(ConnectionString), listTitle);
                        var data = client.ExecuteJson(endpointUrl, HttpMethod.Post, itemPayload);
                    }
                }
                else     //If it exists.. Update
                {
                    dynamic itemPayload = new ExpandoObject();
                    ((IDictionary<string, object>)itemPayload).Add("__metadata", new { type = "SP.Data." + listTitle + "ListItem" });

                    foreach (var field in entity_)
                    {
                        ((IDictionary<string, object>)itemPayload).Add(field.Name, field.Name == entity_.IDFieldName ? Convert.ToString(field.Value) : field.Value);
                    }

                    //Use sharepoint ID for item originally fetched
                    itemId = item["value"][0]["ID"];
                    using (var client = new SPHttpClient(new Uri(ConnectionString), UserName, Password))
                    {
                        var endpointUrl = string.Format("{0}_api/web/lists/getbytitle('{1}')/items({2})", new Uri(ConnectionString), listTitle, itemId);
                        var headers = new Dictionary<string, string>();
                        headers["IF-MATCH"] = "*";
                        headers["X-HTTP-Method"] = "MERGE";
                        client.ExecuteJson(endpointUrl, HttpMethod.Post, headers, itemPayload);
                    }


                }

            }
        }

        public void CreateList(String listTitle, SPHttpClient client)
        {
            var itemPayload = new { __metadata = new { type = "SP.List" }, Title = listTitle, AllowContentTypes = true, BaseTemplate = 100, ContentTypesEnabled = true };
            var endpointUrl = string.Format("{0}_api/web/lists", new Uri(ConnectionString));
            var data = client.ExecuteJson(endpointUrl, HttpMethod.Post, itemPayload);

            foreach (var field in entity_)
            {   //Sharepoint already has a sort of Name Field (Title), Created Timestamp (Created), and Modified Timestamp (Modified)
                if (field.Name != entity_.ModifiedVersionFieldName && field.Name != entity_.CreatedVersionFieldName && field.Name != entity_.NameFieldName)
                    CreateField(listTitle, field.Name);
            }
        }



        public void CreateField(String listTitle, String fieldName)
        {
            using (var client = new SPHttpClient(new Uri(ConnectionString), UserName, Password))
            {
                var itemPayload = new { __metadata = new { type = "SP.Field" }, Title = fieldName, FieldTypeKind = 2 };
                var endpointUrl = string.Format("{0}_api/web/lists/getbytitle('{1}')/Fields", new Uri(ConnectionString), listTitle);
                var data = client.ExecuteJson(endpointUrl, HttpMethod.Post, itemPayload);
            }
        }

        public void deleteList()
        {
            using (var client = new SPHttpClient(new Uri(ConnectionString), UserName, Password))
            {
                var listTitle = listName;
                var endpointUrl = string.Format("{0}_api/web/lists/getbytitle('{1}')", new Uri(ConnectionString), listTitle);
                var headers = new Dictionary<string, string>();
                headers["IF-MATCH"] = "*";
                headers["X-HTTP-Method"] = "DELETE";
                client.ExecuteJson(endpointUrl, HttpMethod.Post, headers, new { });
                Debug.WriteLine("Task item has been deleted");
            }
        }

        public void deleteList_deleted()
        {
            using (var client = new SPHttpClient(new Uri(ConnectionString), UserName, Password))
            {
                var listTitle = listName + "_deleted";
                var endpointUrl = string.Format("{0}_api/web/lists/getbytitle('{1}')", new Uri(ConnectionString), listTitle);
                var headers = new Dictionary<string, string>();
                headers["IF-MATCH"] = "*";
                headers["X-HTTP-Method"] = "DELETE";
                client.ExecuteJson(endpointUrl, HttpMethod.Post, headers, new { });
                Debug.WriteLine("Task item has been deleted");
            }
        }

        public void deleteTheDeleted()
        {

            foreach (Entity entity_ in deletedRecords_.Values)
            {
                JObject item;
                var itemId = entity_.ID;
                using (var client = new SPHttpClient(new Uri(ConnectionString), UserName, Password))
                {
                    var endpointUrl = string.Format("{0}_api/web/lists/getbytitle('{1}')/items?$filter=SyncID eq '{2}'", new Uri(ConnectionString), listName, itemId);
                    item = client.ExecuteJson(endpointUrl);
                }

                if (item["value"].Count() != 0)
                {

                    //Use sharepoint ID for item originally fetched
                    itemId = item["value"][0]["ID"];

                    using (var client = new SPHttpClient(new Uri(ConnectionString), UserName, Password))
                    {
                        var listTitle = listName + "_deleted";
                        var endpointUrl = string.Format("{0}_api/web/lists/getbytitle('{1}')/items({2}", new Uri(ConnectionString), listTitle);
                        var headers = new Dictionary<string, string>();
                        headers["IF-MATCH"] = "*";
                        headers["X-HTTP-Method"] = "DELETE";
                        client.ExecuteJson(endpointUrl, HttpMethod.Post, headers, new { });
                        Debug.WriteLine("Task item has been deleted");
                    }

                }
            }
        }

        public IEnumerable<Entity> GetAddedEntities(dynamic version)
        {
            return records_.Values.Where(e => e.CreatedVersion >= version).ToList();
        }

        public IEnumerable<Entity> GetDeletedEntities(dynamic version)
        {
            return deletedRecords_.Values.Where(e => e.ModifiedVersion >= version).ToList();
        }

        public IEnumerable<Entity> GetDeletedEntities()
        {
            return deletedRecords_.Values.ToList();
        }

        public IEnumerable<Entity> GetAddedEntities()
        {
            return records_.Values.ToList();
        }

        public IEnumerable<Entity> GetModifiedEntities()
        {
            return records_.Values.ToList();
        }

        public IEnumerable<Entity> GetEntities()
        {
            return records_.Values.ToList();
        }

        public Entity GetEntity<IDType>(IDType id)
        {
            // Clone the entity so there are no references to the internal entity
            System.Diagnostics.Debug.Assert(null != records_);
            return new Entity(records_[id]);
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return records_.Values.GetEnumerator();
        }

        public IEnumerable<Entity> GetModifiedEntities(dynamic version)
        {
            return records_.Values.Where(e => e.ModifiedVersion >= version).ToList();
        }

        public bool IsConnected()
        {
            return connected_;
        }

        public void UpdateEntity(Entity entity)
        {
            // Don't insert - make sure entity exists
            if (!records_.ContainsKey(entity.ID)) throw new ArgumentOutOfRangeException("entity",
                 string.Format("Entity {0} does not exist and can not be updated.", entity.ID));

            UpsertRecord(entity);
        }

        private void UpsertRecord(Entity entity)
        {
            System.Diagnostics.Debug.Assert(null != records_);
            records_[entity.ID] = new Entity(entity);
        }



        //Sync Methods
        #region Sync_Functions
        public Entity GetSyncEntity(dynamic id)
        {
            // Clone the entity so there are no references to the internal entity
            Debug.Assert(null != records_);
            var entity = records_.FirstOrDefault(x => x.Value.SyncID == id).Value;
            if (entity != null) { return new Entity(entity); }
            else return null;

        }

        public Entity CreateSyncEntity(Entity entity)
        {
            Debug.Assert(null != records_);
            //Get id of last item in list
            int lastId = 0;
            try
            {
                lastId = records_.ToList()[records_.Count - 1].Value.ID;
            }
            catch (Exception x) { }

            entity.ID = lastId + 1;

            records_[entity.ID] = new Entity(entity);

            return new Entity(records_[entity.ID]);
        }

        public void DeleteSyncEntity(dynamic id)
        {

            Entity entity = records_.FirstOrDefault(x => x.Value.SyncID == id).Value;

            if (entity != null)
            {
                deletedRecords_[entity.ID] = records_[entity.ID];
                records_.Remove(entity.ID);
            }
        }

        public void DeleteSyncEntity(Entity entity)
        {
            DeleteSyncEntity(entity.SyncID);
        }

        public void UpdateSyncEntity(Entity entity)
        {
            Entity entityhere = records_.FirstOrDefault(x => x.Value.SyncID == entity.SyncID).Value;

            if (entityhere == null) throw new ArgumentOutOfRangeException("entity",
                 string.Format("Entity {0} does not exist and can not be updated.", entity.SyncID));


            //update everything but id
            foreach (var field in entity.FieldDictionary)
            {
                if (field.Key != entity.IDFieldName)
                {
                    entityhere.WriteField(field.Key, field.Value.Value);
                }
            }
        }

        #endregion Sync_Functions


        #region Properties
        public Entity this[dynamic id]
        {
            get
            {
                return GetEntity(id);
            }
            set
            {
                if (value.ID != id) throw new ArgumentException(
                     string.Format("Invalid ID. Entity with ID {0} can't be assigned at key {1}", value.ID, id));
                UpsertRecord(value);
            }
        }
        #endregion Properties
    }
}
