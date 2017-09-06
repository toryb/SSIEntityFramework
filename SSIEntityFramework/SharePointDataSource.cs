using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.SharePoint.Client;
using SP = Microsoft.SharePoint.Client;
using System.Security;
using System.Diagnostics;

namespace SSIEntityFramework
{
    public class SharePointDataSource : ISSIDataSource
    {
        private string listName_;
        private Dictionary<dynamic, Entity> records_;
        private Dictionary<dynamic, Entity> deletedRecords_;
        private Entity entity_;
        ClientContext clientContext_;
        private bool connected_ = false;
        public string ConnectionString { get; set; }
        public string listName
        {
            get { return listName_; }
            set { listName_ = value; }
        }
        public enum AuthenticationType
        {
            User,
            AddInPrincipal
        }
        public AuthenticationType AuthType { get; set; }

        //User Credentials
        public string UserName { get; set; }
        public string Password { get; set; }

        //Add-In Principal Credentials
        public string ClientId
        {
            get { return TokenHelper.ClientId; }

            set { TokenHelper.ClientId = value; }
        }
        public string ClientSecret
        {
            get { return TokenHelper.ClientSecret; }

            set { TokenHelper.ClientSecret = value; }
        }

        public Type EntityDotNetType
        {
            get { return entity_.DotNetType; }
        }

        public SharePointDataSource(Entity entity)
        {
            // Clone the entity so there are no references to the internal entity
            entity_ = new Entity(entity);
            records_ = new Dictionary<dynamic, Entity>();
            deletedRecords_ = new Dictionary<dynamic, Entity>();
        }

        public void Connect()
        {
            clientContext_ = Context();
            if (clientContext_ != null)
            {
                // Try to get the list
                List entities = null;

                try
                {
                    entities = clientContext_.Web.Lists.GetByTitle(listName_);
                    clientContext_.ExecuteQuery();
                }
                catch (Exception x)
                {
                    entities = null;
                }


                if (entities != null)       //If list exists
                {
                    CamlQuery query = CamlQuery.CreateAllItemsQuery();
                    ListItemCollection items = entities.GetItems(query);
                    clientContext_.Load(items);
                    clientContext_.ExecuteQuery();

                    foreach (ListItem listItem in items)
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
                else connected_ = true;     //We'll create one on disconnect


                // load the existing deleted records
                List deletedEntities = null;

                try
                {
                    deletedEntities = clientContext_.Web.Lists.GetByTitle(listName_ + "_deleted");
                    clientContext_.ExecuteQuery();
                }
                catch (Exception x)
                {
                    deletedEntities = null;
                }

                if (deletedEntities != null)       //If deleted list exists
                {

                    CamlQuery query2 = CamlQuery.CreateAllItemsQuery();
                    ListItemCollection deletedItems = deletedEntities.GetItems(query2);
                    clientContext_.Load(deletedItems);
                    clientContext_.ExecuteQuery();

                    foreach (ListItem listItem in deletedItems)
                    {
                        foreach (EntityField field in entity_)
                        {
                            entity_[field.Name].Value = Convert.ChangeType(listItem[field.Name],
                                entity_[field.Name].ValueType);

                            deletedRecords_[entity_.ID] = entity_;
                        }
                    }
                }

                clientContext_ = null;
            }
        }

        public void Connect(string connectionString)
        {
            ConnectionString = connectionString;
            Connect();
        }

        /// <summary>
        /// Connect based on choice of type of Authentication (User, Add-In Principal)
        /// </summary>
        /// <returns></returns>
        private ClientContext Context()
        {
            switch (AuthType)
            {
                case AuthenticationType.User: return UserContext();
                case AuthenticationType.AddInPrincipal: return AddInPrincipalContext();
                default: return null;
            }
        }

        /// <summary>
        /// Create Context object using a SharePoint User Credentials
        /// </summary>
        /// <returns></returns>
        private ClientContext UserContext()
        {
            ClientContext clientContext = new ClientContext(ConnectionString);
            SecureString passWord = new SecureString();
            foreach (char c in this.Password.ToCharArray()) passWord.AppendChar(c);
            clientContext.Credentials = new SharePointOnlineCredentials(this.UserName, passWord);
            return clientContext;
        }


        /// <summary>
        /// Create Context object using SharePoint Add-In Principal's credentials
        /// </summary>
        /// <returns></returns>
        private ClientContext AddInPrincipalContext()
        {
            Uri siteUri = new Uri(ConnectionString);
            string realm = TokenHelper.GetRealmFromTargetUrl(siteUri);
            string accessToken = TokenHelper.GetAppOnlyAccessToken(TokenHelper.SharePointPrincipal,
                                                                  siteUri.Authority, realm).AccessToken;
            return TokenHelper.GetClientContextWithAccessToken(siteUri.ToString(), accessToken);
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
            if (records_.ContainsKey(id))
            {
                deletedRecords_[id] = records_[id];
                records_.Remove(id);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Disconnect()
        {
            clientContext_ = Context();

            //Write entities
            WriteList(listName_, records_);
            records_.Clear();

            //Write deleted entities
            if (deletedRecords_.Count > 0) WriteList(listName_ + "_deleted", deletedRecords_);
            deleteTheDeleted();
            deletedRecords_.Clear();

            connected_ = false;
            clientContext_ = null;
        }


        /// <summary>
        /// Writes list currently in held in memory to list on SharePoint site.
        /// </summary>
        /// <param name="listName"></param>
        /// <param name="theRecordSet"></param>
        private void WriteList(string listName, Dictionary<dynamic, Entity> theRecordSet)
        {

            SP.List list = null;

            //Try to get the list
            try
            {
                list = clientContext_.Web.Lists.GetByTitle(listName);
                clientContext_.ExecuteQuery();
            }
            catch (Exception x)
            {
                list = null;
            }

            if (list == null)   //if list does not exist...
            {
                list = CreateList(listName);
            }

            foreach (Entity entity_ in theRecordSet.Values)
            {
                SP.ListItem item = null;

                //Check if list item exists
                String query = @"<View><Query><Where><Eq><FieldRef Name='" + entity_.IDFieldName + "'/><Value Type = 'Text'>" + entity_.ID + "</Value></Eq></Where></Query></View>";
                CamlQuery cQuery = new CamlQuery();
                cQuery.ViewXml = query;
                ListItemCollection listItemsCollection = list.GetItems(cQuery);
                clientContext_.Load(listItemsCollection);
                clientContext_.ExecuteQuery();

                if (listItemsCollection.Count > 0) item = listItemsCollection[0];

                if (item == null)   //if item does not exist create one..
                {
                    ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                    ListItem newItem = list.AddItem(itemCreateInfo);

                    foreach (var field in entity_)
                    {
                        newItem[field.Name] = field.Value;
                    }

                    newItem.Update();
                    clientContext_.ExecuteQuery();
                }
                else    //update
                {

                    foreach (var field in entity_)
                    {
                        item[field.Name] = field.Value;
                    }

                    item.Update();
                    clientContext_.ExecuteQuery();
                }
            }
        }

        public SP.List CreateList(String listTitle)
        {
            //Create the list
            SP.List list;
            ListCreationInformation creationInfo = new ListCreationInformation();
            creationInfo.Title = listTitle;
            creationInfo.TemplateType = (int)ListTemplateType.GenericList;

            list = clientContext_.Web.Lists.Add(creationInfo);
            list.Update();

            //Create the fields
            foreach (var field in entity_)
            {   //Sharepoint already has a sort of Name Field (Title), Created Timestamp (Created), and Modified Timestamp (Modified)
                if (field.Name != entity_.ModifiedVersionFieldName && field.Name != entity_.CreatedVersionFieldName && field.Name != entity_.NameFieldName)
                    list.Fields.AddFieldAsXml("<Field DisplayName='" + field.Name + "' Type='Text' />", true, AddFieldOptions.DefaultValue);
            }

            clientContext_.ExecuteQuery();
            return list;
        }

        public void deleteTheDeleted()
        {
            SP.List list = null;

            //Try to get the list
            try
            {
                list = clientContext_.Web.Lists.GetByTitle(listName);
                clientContext_.ExecuteQuery();
            }
            catch (Exception x)
            {
                list = null;
            }

            if (list != null)   //if list exists..

                foreach (Entity entity_ in deletedRecords_.Values)
                {
                    SP.ListItem item = null;

                    //Check if list item exists
                    String query = @"<View><Query><Where><Eq><FieldRef Name='" + entity_.IDFieldName + "'/><Value Type = 'Text'>" + entity_.ID + "</Value></Eq></Where></Query></View>";
                    CamlQuery cQuery = new CamlQuery();
                    cQuery.ViewXml = query;
                    ListItemCollection listItemsCollection = list.GetItems(cQuery);
                    clientContext_.Load(listItemsCollection);
                    clientContext_.ExecuteQuery();

                    if (listItemsCollection.Count > 0) item = listItemsCollection[0];

                    if (item != null)   //if item exists delete..
                    {
                        ListItem listItem = list.GetItemById(item.Id);
                        listItem.DeleteObject();
                        clientContext_.ExecuteQuery();
                    }
                }
        }

        public void deleteList()
        {
            clientContext_ = Context();

            try
            {
                List list = clientContext_.Web.Lists.GetByTitle(listName_);
                list.DeleteObject();
                clientContext_.ExecuteQuery();
            }
            catch (Exception e) { };
        }

        public void deleteList_deleted()
        {
            clientContext_ = Context();

            try
            {
                List deletedlist = clientContext_.Web.Lists.GetByTitle(listName_ + "_deleted");
                deletedlist.DeleteObject();
                clientContext_.ExecuteQuery();
            }
            catch (Exception x) { }
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
