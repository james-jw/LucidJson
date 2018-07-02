# LucidJson

Lucid Json is a set of .NET classes for aiding in work with JSON data structures
This library provides the foundation for many higher level libraries providing 
complex json functionality.

*The library is not a replacement for libraries such as Newtonsoft.Json*

## The Basics
LucidJson provides access to Json throught the `Map` object. 

### Map

A `Map` is the core object in LucidJson and represents a json feature.
Below we have an empty json object, or empty `Map`.

`{}`

```c#
   // To create an empty map simply call its constructor
   var emptyMap = new Map();

   // To create one from existing json, call the static `ParseJson` method
   var populatedMap = Map.ParseJson(jsonText);

```

`Map` impelements `IDictionary<string, dynamic>` and can be accessed as such. 

```c#
  var person = new Map();
  person["Name"] = "Sarah";
  person["Age"] = 22;

  if(person["Age"] > 22)
   // ...    

  // It also implements IDynamicObject
  dynamic dPerson = (dynamic)person;
  dPerson.Address = "800 Lane Way";

```

#### Methods

##### As< T >
Returns a new instance of type T initialized with the data from the map.
```c#
  MyCustomObject myObject = myMap.As<MyCustomObject>();
```

##### From< T >
Similar to `As< T >`. `From< T >` converts an object of the specified type to a map.
```c#
  Map myMap = Map.From<MyCustomObject>(myObject);

  /// Or simply
  Map myMap = Map.From(myObject); // Type is inferred

```

##### GetValueAs< T >(string key)
Attempts to return a property value as the specified type T.


##### Traverse 
Traverses a map recursively and returns this map and any descendant maps as an enumeration. 
```c#
  foreach(var descendant in myMap.Traverse()) {
     /// Do stuff with descendant
  }
```

##### Translate
Similar to `Traverse` but is used for the translation of descendant values provided a tranlation function
```c#
  // Rename all id fields by appending a string 'ID'
  dataMap.Translate(id => "ID" + id, "id");
```

The above code traverses the dataMap exhaustively for any property with the name "id" and runs the transformation
function `id => "ID" + id` on it. 

To be more specific, one can provide property name paths, IE: `asset/id`, in this case restricting the translation to 
only `id` properties within a map itself called or contained by an array with the key `asset`.

```c#
  // We only want update the asset ids, not other ids in the dataMap
  dataMap.Translate(id => "AssetID" + id, "asset/id");

  // Again this assumes the map containing the 'id' field
  // is referenced by a key 'asset' itself or is in an array that is.
```

##### ToStringPair
Converts a map to an enumeration of `KeyValuePair<string, string>`

##### Clone
Clones a map, creating an identical but referentially unique clone. 

##### Diff 
Performs a diff on two maps

### Schema

LucidJson supports a simple schema construct for validating and visualizing json.

#### MapSchema

To initialize a `MapSchema` item from an existing json file simple use the static method `ParseJson`

```c#
   var schema = MapSchema.ParseJson(jsonText);
```

Now to associate the schema with a json structure, simple pass it into the `ParseJson` call 
used to create the map in question. 

```c#
   // Pass the schema into the parse call to associate it to the newly created 'data' map
   var data = Map.ParseJson(dataJson, schema);
```

To aid in the generation of a schema object from an existing json structure simple parse 
the json passing in an empty `MapSchema` item instead of a populated one.

```c#
   var schema = new MapSchema();
   var json = Map.ParseJson(jsonText, schema);

   // 'schema' is now initialized based on the passed in json
```
#### MapSchemaItem

While `MapSchema` represents the top level schema object. `MapSchemaItem` represents any node
within it including the `MapSchema` itself, which is a `MapSchemaItem` as well.

This class represents all the metadata associated with any node in the json structure. Including
aliases, data types, descriptions, domains, restrictions, etc. 

### Accessing the Schema

Once you have mapped the schema to a `LucidMap` you simple call it's `Schema(string key)` method 
to retireve an items schema, or nothing to return this map's schema.

```c#
   var firstSchema = personMap.Schema("first");
   
   Assert.AreEqual("String", firstSchema.DataType);
   Assert.AreEqual("first", firstSchema.Name);
   Assert.AreEqual("", firstSchema.DefaultValue);

   var personSchema = personMap.Schema();
   
   Assert.AreEqual("Map", personSchema.DataType);
   Assert.AerEqual("{first} {last}", personSchema.TitleExpression);

```