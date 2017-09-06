using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SSIEntityFramework
{
    public class Synchronizer
    {
        private IList<ISSIDataSource> DataSources = new List<ISSIDataSource>();
        private int Sync_Index = 0;

        public Synchronizer()
        {

        }

        public void AddDataSource(ISSIDataSource dataSource)
        {
            DataSources.Add(dataSource);
        }

        public void ClearDataSources()
        {
            DataSources.Clear();
        }

        public void Synchronize()
        {
            //Find last/highest SyncID
            foreach (ISSIDataSource ds in DataSources)
            {
                foreach(Entity e in ds.GetEntities())
                {
                    if (e.SyncID > Sync_Index) Sync_Index = e.SyncID;
                }

                foreach (Entity e in ds.GetDeletedEntities())
                {
                    if (e.SyncID > Sync_Index) Sync_Index = e.SyncID;
                }
            }

            Sync_Index++;

            //Give all entities in base source never synced a new SyncID


            //Naive technique
            //for (int i = 0; i < DataSources.Count; i++)
            //{
            //    for (int j = i + 1; j < DataSources.Count; j++)
            //    {
            //        Sync(DataSources[i], DataSources[j]);
            //    }
            //}

            /*Synchronization technique:
              Use first datasource as base, merge all other sources on to this base, then make all other data sources same as base 
              Better because at the end of it all, data sources must be the same */
            for (int i = 1; i < DataSources.Count; i++)
            {
                    Merge(DataSources[0], DataSources[i]);
            }

            //Replicate first data source in all other data sources
            for (int i = 1; i < DataSources.Count; i++)
            {
                Replicate(DataSources[0], DataSources[i]);
            }
        }

        public void Merge(ISSIDataSource sourceA, ISSIDataSource sourceB)
        {
            /*
                3.A get a list of deleted entity keys from Source A and remove from Source B
	            3.B get a list of deleted entity keys from Source B and remove from Source A
	            3.C get a list of added entities in Source A and add to Source B
	            3.E compare remaining entities & updated from latest version
	            3.D get a list of added entities in Source B and add to Source A
            */

            //Delete => Add & Update

            foreach(Entity e in sourceB.GetDeletedEntities())
            {
                sourceA.DeleteSyncEntity(e.SyncID);
            }

            foreach (Entity e in sourceA.GetDeletedEntities())
            {
                sourceB.DeleteSyncEntity(e.SyncID);
            }

            //Give sync ids to all unsynced items in the base source (A)
            foreach (Entity e in sourceA.GetEntities())
            {
                if (e.SyncID == 0)
                {
                    e.SyncID = Sync_Index;
                    Sync_Index++;
                }
            }

            foreach (Entity e in sourceB.GetEntities())
            {
                Entity next = sourceA.GetSyncEntity(e.SyncID);
                if (next == null)
                {
                    e.SyncID = Sync_Index;
                    sourceA.CreateSyncEntity(e);
                    Sync_Index++;
                }
                else
                {
                    if (e.ModifiedVersion > next.ModifiedVersion)
                    {
                        sourceA.UpdateSyncEntity(e);
                    }
                }
            }
        }

        public void Replicate(ISSIDataSource sourceA, ISSIDataSource sourceB)
        {
            foreach (Entity e in sourceA.GetEntities())
            {
                Entity next = sourceB.GetSyncEntity(e.SyncID);
                if (next == null)
                {
                    sourceB.CreateSyncEntity(e);
                }
                else
                {
                    if (e.ModifiedVersion > next.ModifiedVersion)
                    {
                        sourceB.UpdateSyncEntity(e);
                    }
                }
            }
        }

    }
}
