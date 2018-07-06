# LucidJson

Lucid Json is a set of .NET classes for aiding in work with JSON data structures
This library provides the foundation for many higher level libraries providing 
complex json functionality.

*This library is not a replacement for libraries such as Newtonsoft.Json*

## LucidMap
LucidJson provides access to Json throught the `LucidMap` object. 


A `LucidMap` is the core object in LucidJson and represents a json feature.
Below we have an empty json object, or empty `LucidMap`.

`{}`

To create an empty map simply call its constructor:
```c#
   var emptyMap = new Map();
```

To create one from existing json, call the static `ParseJson` method:
```c#
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

### Methods

#### As< T >
###### `T As< T >()`
Returns a new instance of type T initialized with the data from the map.
```c#
  MyCustomObject myObject = myMap.As<MyCustomObject>();
```

The returned type must have a default or annotated constructor.

#### From< T >
###### `Map From< T >(T fromObject)`
Similar to `As< T >`. `From< T >` converts an object of the specified type to a map.
```c#
  Map myMap = Map.From<MyCustomObject>(myObject);

  /// Or simply
  Map myMap = Map.From(myObject); // Type is inferred

```

#### GetValueAs< T >

###### `T GetValueAs< T >(string key, T defaultValue = default(T))`

Attempts to return a property value as the specified type T.


#### Traverse
###### `IEnumerable<Map> Traverse()`
Simply Traverses a map recursively returning the calling map and any descendant maps as an enumeration. 

```c#
  foreach(var descendant in myMap.Traverse()) {
     /// Do stuff with descendant
  }
```

#### Traverse< T >
###### `IEnumerable<T> Traverse<T>(params string[] paths)`
Similar to traverse with no parameters, but returns values instead of maps alone given a set of 
keys or paths.

For example, given the following json.
```json
{
   "id": "res1232",
   "assets": [
     { "id": "asset1" },
     { "id": "asset2" }
   ]
}
```

```c#
  foreach(var assetId in myMap.Traverse<string>("assets/id")) { 
    // ... "asset1", "asset2"
  }

  // In the above example, all "id" keys in the map contained by a map which is itself 
  // referenced with the key "assets" or contained within an array referenced by the 
  // key "assets". 

  foreach(var id in myMap.Traverse<string>("id")) { 
    // ... "res1232" "asset1", "asset2"
  }
```

#### Translate< T >
###### `void Translate<T>(Func<T, T> translation, params string[] paths)`
Similar to `Traverse` but used for the translation of descendant values provided a tranlation function instead of the
retrieval.

```c#
  // Rename all id fields by appending a string 'ID'
  dataMap.Translate(id => "ID" + id, "id");
```

The above code traverses the dataMap exhaustively for any property with the name "id" and runs the transformation
function `id => "ID" + id` on it. 

Just like `Traverse` complex paths are supported as well. For example: `assets/id`.

```c#
  // We only want update the asset ids, not other ids in the dataMap
  dataMap.Translate(id => "AssetID" + id, "assets/id");
```

#### ToString
`string ToString()`

Returns the indented json representing the map data.

```c#
   // This is basically what the Clone function, below, does
   var clone = Map.ParseJson(myMap.ToString());
```

#### ToStringPair
`KeyValuePair<string, string> ToStringPair()`

Converts a map to an enumeration of `KeyValuePair<string, string>`

This is useful for serializing to `FormUrlEncodedContent`. All objects are serialized to string
and properly escaped. If for example a property value is a Map, the value will be returned as escaped
json.

#### Clone
`Map Clone()`

Clones a map, creating an identical but referentially unique clone. 

```c#
   var clone = myMap.Clone();
   
   Assert.IsFalse(clone == myMap);
   Assert.IsTrue(clone.ToString() == myMap.ToString();
```

#### Diff 
Performs a diff on two maps

## Schema

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

This works for complex and deep maps as well.

```c#
   var firstSchema = personMap.Schema("first");
   
   Assert.AreEqual("String", firstSchema.DataType);
   Assert.AreEqual("first", firstSchema.Name);
   Assert.AreEqual("", firstSchema.DefaultValue);

   var personSchema = personMap.Schema();
   
   Assert.AreEqual("Map", personSchema.DataType);
   Assert.AerEqual("{first} {last}", personSchema.TitleExpression);

```
