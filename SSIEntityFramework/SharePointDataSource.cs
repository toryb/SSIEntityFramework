using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SP = Microsoft.SharePoint.Client;

namespace SSIEntityFramework
{
  class SharePointDataSource : ISSIDataSource
  {

    #region Class Variables
    private SP.ClientContext context_;
    private string siteUrl_;
    private string listName_;
    private Entity entity_;
    private Dictionary<dynamic, Entity> entities_;
    #endregion Class Variables

    //todo: Add Ctors

    #region ISSIDataSource
    /// <summary>
    /// Get and set the connection string
    /// </summary>
    /// <remarks>Connection string is in the form of URL,ListName, where URL is the SharePoint site URL and ListName 
    /// is the name of the list containing the entities.</remarks>
    public string ConnectionString
    {
      get
      {
        return string.Join(",", new string[] { siteUrl_, listName_ });
      }
      set
      {
        string[] parms = value.Split(new char[] { ',' }, 2);
        siteUrl_ = parms[0];
        listName_ = parms[1];
      }
    }

    public void Connect()
    {
      // Get user identify - authtenticate if necessary

      // Get any cached credentials
      // Obtain information for communicating with the service:

      // spContext.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
      using (context_ = MSDN.Samples.ClaimsAuth.ClaimClientContext.GetAuthenticatedContext(ConnectionString))
      {

        SP.ListItemCollection collection;
        SP.List list = context_.Web.Lists.GetByTitle(listName_);
        SP.CamlQuery query = CreateListQuery();
        collection = list.GetItems(query);
        context_.Load(collection);

        context_.ExecuteQuery();

        foreach (var item in collection)
        {

          foreach (System.Collections.Generic.KeyValuePair<string, object> dictItem in item.FieldValues)
          {
            if ((null != dictItem.Value) &&
                (dictItem.Value.GetType() == typeof(SP.FieldLookupValue)))
            {
              SP.FieldLookupValue lookupValue = dictItem.Value as SP.FieldLookupValue;
              if (null != lookupValue) entity_[StripHTML.StripTagsCharArray(dictItem.Key)].Value =
                  StripHTML.StripTagsCharArray(lookupValue.LookupValue);
            }
            else if ((null != dictItem.Value) &&
                dictItem.Value.GetType() == typeof(SP.FieldUserValue[]))
            {
              SP.FieldUserValue[] userValues = dictItem.Value as SP.FieldUserValue[];
              System.Diagnostics.Debug.Assert(null != userValues);

              // clear out the value befor updating
              foreach (SP.FieldUserValue userValue in userValues)
              {
                if (null != userValue)
                {
                  entity_[StripHTML.StripTagsCharArray(dictItem.Key)].Value =
                      StripHTML.StripTagsCharArray(userValue.LookupValue) + ";";
                }
              }

            }
            else if ((null != dictItem.Value) &&
                dictItem.Value.GetType() == typeof(SP.FieldUserValue))
            {
              SP.FieldUserValue userValue = dictItem.Value as SP.FieldUserValue;
              if (null != userValue)
                entity_[StripHTML.StripTagsCharArray(dictItem.Key)].Value =
                    StripHTML.StripTagsCharArray(userValue.LookupValue);
            }
            else if ((null != dictItem.Value) &&
                (dictItem.Value.GetType() == typeof(SP.FieldLookupValue[])))
            {
              SP.FieldLookupValue[] lookupValues = dictItem.Value as SP.FieldLookupValue[];
              System.Diagnostics.Debug.Assert(null != lookupValues);

              // clear out the value befor updating
              foreach (SP.FieldLookupValue lookupValue in lookupValues)
              {
                if (null != lookupValue)
                {
                  entity_[StripHTML.StripTagsCharArray(dictItem.Key)].Value =
                      StripHTML.StripTagsCharArray(lookupValue.LookupValue) + ";";
                }
              }

            }
            else if (null != dictItem.Value)
            {
              entity_[StripHTML.StripTagsCharArray(dictItem.Key)].Value = dictItem.Value;
            }

          }
          entities_[entity_.ID] = new Entity(entity_);
        }

      }
      context_ = null;
    }

    public void Connect(string connectionString)
    {
      ConnectionString = connectionString;
      Connect();
    }

    public void Disconnect()
    {
      context_ = null;

      // Update the sharepoint List
      // Get the item from sharepoint (if it exists)
      SP.ListItem item = GetSPItem(siteUrl_, listName_, entity_.ID);

      // Write the values to the sharepoint item

      // IF it doesn't exist add it to sharepoint

      entities_.Clear();
    }

    public bool IsConnected()
    {
      return true;
    }

    public Type EntityDotNetType
    {
      get
      {
        return entity_.DotNetType;
      }
    }

    public IEnumerator<Entity> GetEnumerator()
    {
      return entities_.Values.GetEnumerator();
    }

    public Entity GetEntity<IDType>(IDType id)
    {
      return entities_[id];
    }

    public Entity CreateEntity(Entity entity)
    {
      System.Diagnostics.Debug.Assert(null != entities_);
      entities_[entity.ID] = new Entity(entity);

      return new Entity(entities_[entity.ID]);
    }

    public void DeleteEntity<IDType>(IDType id)
    {
      entities_.Remove(id);
    }

    public void DeleteEntity(Entity entity)
    {
      DeleteEntity(entity.ID);
    }

    public void UpdateEntity(Entity entity)
    {
      if (!entities_.ContainsKey(entity.ID)) throw new ArgumentOutOfRangeException("entity",
           string.Format("Entity {0} does not exist and can not be updated.", entity.ID));
      entities_[entity.ID] = new Entity(entity);
    }

    public IEnumerable<Entity> GetDeletedEntities(dynamic version)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<Entity> GetAddedEntities(dynamic version)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<Entity> GetModifiedEntities(dynamic version)
    {
      throw new NotImplementedException();
    }
    #endregion ISSIDataSource

    #region Internal Methods
    public static SP.ListItem GetSPItem(string site, string listName, int ID)
    {
      SP.ListItem item = null;
      try
      {
        using (var spContext = MSDN.Samples.ClaimsAuth.ClaimClientContext.GetAuthenticatedContext(site))
        {
          SP.List list = spContext.Web.Lists.GetByTitle(listName);

          item = list.GetItemById(ID);
          spContext.Load(item);

          // Commit
          spContext.ExecuteQuery();
        }
      }
      catch (SP.ServerException e)
      {
        item = null;
        throw new ArgumentOutOfRangeException("ID", string.Format("Item {0} not found in SharePoint LIst {1}. Has it been deleted?", ID, listName));
      }

      return item;
    }


    private SP.CamlQuery CreateListQuery()
    {
      SP.CamlQuery query = new SP.CamlQuery();
      StringBuilder sb = new StringBuilder("<View>");
      sb.Append("<ViewFields>");
      foreach (var field in entity_)
      {
        sb.Append(string.Format("<FieldRef Name='{0}'/>", field.Name));
      }
      sb.Append("</ViewFields>");
      sb.Append("</View>");
      query.ViewXml = sb.ToString();
      return query;
    }
    #endregion  Internal Methods

    //todo: Add Helper Methods

    //todo: Add Instance Methods
  }
}
