# Introduction
**SSIEntityFramework** is a lightweight framework used to define Entities that can be used in Synchronization and ETL applications. **SSIEntityFramework** began primarily as a non-intrusive synchronization framework. What is meant by a "non-intrusive synchronization framework" is a framework that allows you to define entities within systems that don't support entities directly, and to synchronize entities between these systems without modifying the respective schemas of each system.


# Concepts
## How to edit this page
[Read this](https://www.visualstudio.com/en-us/docs/reference/markdown-guidance)

## Entities
### Entity Types
1. Entities have a type name. The type name represents what the user would call the entity.
2. Entities have fields. Fields are named values that contain the entity data. (Note: Fields should be changed to properties)
2. Entities may have a .NET type - this is the class that represents the entity in the CLR.	 
For example, a "Customer" entity type may be represented in memory as a MyNameSpace.Customer  
object. MyNameSpace.Customer would have a property with the same name for each field in the  
FieldDictionary.
3. Entities have a Name field - this field (property) holds the name of the instance of the entity.
4. An entity has a dictionary list of EntityFields (Dictionary<string, EntityField>)
6. Entities have an ID field. This represents a field that contains a value guaranteed 
to be unique in the context of the current instance of this library. Typically this will  
be a generated number or GUID.
7. Entities have an ID type. This represents the Type of the value used for the unique ID for the Entity
8. Entities have a version field ...
9. Entities have a version type. This type will hold the value of both the CreatedVersion  
and the ModifiedVersion.
10. To use the DotNetType it must have a default constructor (no parameters)

### Entity Instances
1. The name of an entity instance is the value of a field that represents the name of a specific instance  
of an entity. For example, the name of a customer may be stored in the "Name" field so  
the Name property would return FieldDictionary["Name"] which may contain a value of "Acme Company" 

## EntityField
1. An EntityField aggregates an object that implements IEntityField.
2. An IEntityField object implements a Validate(dynamic value) method that can validate the value to determine if it can be stored in the IEntityField object.

## SSIDataSource
1. A Datasource has a connection string
2. A Datasource is something to which one can connect 
3. A Datasource can perform all CRUD operations on an entity

## CSV DataSource
1. Reads the entire file on open.
2. Deals with the file in memory.
3. Writes the entire file when disconnecting.

## Sharepoint DataSource
1. Should we interact dynamically with the list or read the entire list & then synchronize?

## Synchronization
1. Need to use a "label" concept to get all records that have a version => label
2. To support deleted entities a datasource must have the ability to "mark" as deleted
    1. I have chosen not to support the use of another datasource to determine if an entity has been deleted from a source datasource. For example, in previous versions we attempt to determine if entity in destination datasource (DSd) was deleted from source datasource (DSs) by finding entities in DSd that have values provided by DSs. The problem is when synchronizing multiple datasources this will only work if the entity existed in all datasources prior to being deleted - which can not be guaranteed.
    
3. To synchronize  
    1. get a list of deleted entity keys from Source A and remove from Source B
    1. get a list of deleted entity keys from Source B and remove from Source A
    1. get a list of added entities in Source A and add to Source B
    1. get a list of added entities in Source B and add to Source A
    1. compare remaining entities & updated from latest version
  
4. Synchronization map is defined by two datasources and a list of "mapped" fields  
    1. "Mapped" fields represent the field name of source A and field name of Source B and a transformation function for each direction (A->B & B->A)
   
5. Need to use the map to know which field in Source A represents the key field from Source B and vice versa.
 
6. Need to expose the ability to get an enumerable list of items marked for deletion from a datasource

7. Need to expose the ability to get an enumerable list of items that have been added to a datasource
    1. Could do that by providing a Hashset of keys from one source and corresponding Hashset of representative values from the other source. Then use Except() to get entities that don't have a corresponding entity in the other source. This will only work after synchronizing deleted entities.

# Fixing Missing References
1. The first missing reference had to do with a project on MSDN that was referenced in the project. Specifically, it had to do with the call of the function MSDN.Samples.ClaimsAuth.ClaimClientContext.GetAuthenticatedContext() in SharePointDataSource.cs.
I fixed the problem by downloading the project and copying all class files that the whole namespace in the function call depended on into this project. The external project was found in the following URL: https://code.msdn.microsoft.com/vstudio/Remote-Authentication-in-b7b6f43c/sourcecode?fileId=21439&pathId=1351975828

2. The second missing reference had to do with the SharePoint Client Library. I found a nuget package (Microsoft.Sharepoint) that fixed the issue.

# Getting Started
TODO: Guide users through getting your code up and running on their own system. In this section you can talk about:
1. Installation process
2. Software dependencies
3. Latest releases
4. API references

# Build and Test
TODO: Describe and show how to build your code and run the tests. 
## References:
This project uses a code from the following article: <https://msdn.microsoft.com/en-us/library/hh147177.aspx>
The sample code from this project is used as a DLL to support the claims auth necessary for connecting to SharePoint


# Contribute
TODO: Explain how other users and developers can contribute to make your code better. 

If you want to learn more about creating good readme files then refer the following [guidelines](https://www.visualstudio.com/en-us/docs/git/create-a-readme). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)
