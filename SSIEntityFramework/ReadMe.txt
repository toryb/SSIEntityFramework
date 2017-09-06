
*********** Entities *********
1. Entities have a type name. The type name represents what the user would call the entity.
2. Entities may have a .NET type - this is the class that represents the entity in the CLR.
	For example, a "Customer" entity type may be represented in memory as a MyNameSpace.Customer 
	object. MyNameSpace.Customer would have a property with the same name for each field in the 
	FieldDictionary.
3. Entities have a Name field - this field (property) holds the name of the instance of the entity.
4. An entity has a dictionary list of EntityFields (Dictionary<string, EntityField>)
5. The name of an entity is the value of a field that represents the name of a specific instance 
	of an entity. For example, the name of a customer may be stored in the "Name" field so
    the Name property would return FieldDictionary["Name"] which may contain a value of "Acme Company"
6. Entities have an ID field. This represents a field that contains a value guaranteed
	to be unique in the context of the current instance of this library. Typically this will
	be a generated number or GUID.
7. Entities have an ID type. This represents the Type of the value used for the unique ID for the Entity
8. Entities have two version fields. A version field represents a value that can be compared 
	to ascertain if an instance is a higher or lower (or the same) version. An entity has
	a CreatedVersion and a ModifiedVersion field. The CreatedVersion represents the version
	of the created entity. The ModifiedVersion represents the version of the last modification
	to the entity (including deleting). A Deleted entity has a ModifiedVersion representing
	the version at which the entity was deleted. 
9. Entities have a version type. This type will hold the value of both the CreatedVersion 
	and the ModifiedVersion.
10. To use the DotNetType it must have a default constructor (no parameters)
11. 

********* EntityField **********
1. An EntityField aggregates an object that implements IEntityField.
2. An IEntityField object implements a Validate(dynamic value) that can validate that value
	can be stored in the IEntityField object. 
3.


********** SSIDataSource ***********
1. A Datasource has a connection string
2. A Datasource is something to which one can connect 
3. A Datasource can perform all CRUD operations on an entity


********** CSV DataSource ******************
1. Reads the entire file on open.
2. Deals with the file in memory.
3. Writes the entire file when disconnecting.

********** Sharepoint DataSource ******************
1. Should we interact dynamically with the list or read the entire list & then synchronize?
2. 

********** Synchronization ******************
1. Need to use a "label" concept to get all records that have a version => label
2. To support deleted entities a datasource must have the ability to "mark" as deleted
	2.A I have chosen not to support the use of another datasource to determine if
		an entity has been deleted from a source datasource. For example, in previous 
		versions we attempt to determine if entity in destination datasource (DSd) was 
		deleted from source datasource (DSs) by finding entities in DSd that have 
		values provided by DSs. The problem is when synchronizing multiple datasources
		this will only work if the entity existed in all datasources prior to being
		deleted - which can not be guaranteed.
3. To synchronize, 
	3.A get a list of deleted entity keys from Source A and remove from Source B
	3.B get a list of deleted entity keys from Source B and remove from Source A
	3.C get a list of added entities in Source A and add to Source B
	3.D get a list of added entities in Source B and add to Source A
	3.E compare remaining entities & updated from latest version
4. Synchronization map is defined by two datasources and a list of "mapped" fields
	4.A "Mapped" fields represent the field name of source A and field name of Source B 
	and a transformation function for each direction (A->B & B->A)
5. Need to use the map to know which field in Source A represents the key field from
Source B and vice versa. 
6. Need to expose the ability to get an enumerable list of items marked for deletion from
a datasource
7. Need to expose the ability to get an enumerable list of items that have been added 
to a datasource
	7.A Could do that by providing a Hashset of keys from one source and corresponding
	Hashset of representative values from the other source. Then use Except() to get 
	entities that don't have a corresponding entity in the other source. This will only
	work after synchronizing deleted entities.


Ideas:
1. Add an attribute that will allow a property on the .NET Type to be mapped to an entity field
	by field name - allowing the property to be named differently.
2. Add reflection that can be used to generate a .NET entity class


Next Steps:
Maybe a field will contain converters - maybe these can be added by extension