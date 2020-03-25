# C# Zenserdes.Protobuf - Maintained by SirJosh3917
* Very WIP

**TODO:** Tell the user how to generate Zenserdes classes for their code

**TODO:** Show users examples of using ZenserdesProtobuf.Serialize/Deserialize

## Checklist

The checklist is for measuring progress on specific protobuf features. The initial release of Zenserdes.Protobuf will contain a minimum amount of these features, and features will be procedurally added as more features are requested. It doesn't make sense to work on a feature if the feature isn't requested! Each item on the checklist will link to a github issue when appropriate. Items on the checklist are marked as done when there are unit tests for the feature.

Checklist:

[ ] - `option csharp_namespace`
[ ] - Support for arbitrarily sized field IDs
[ ] - `repeated` fields
[ ] - all protobuf field types:
    [ ] - double
    [ ] - float
    [ ] - int32
    [ ] - int64
    [ ] - uint32
    [ ] - uint64
    [ ] - sint32
    [ ] - sint64
    [ ] - fixed32
    [ ] - fixed64
    [ ] - sfixed32
    [ ] - sfixed64
    [ ] - bool
    [ ] - string
    [ ] - bytes
    [ ] - other messages
[ ] - enums
[ ] - importing definitions
[ ] - nested types
[ ] - oneof
[ ] - maps
[ ] - packages
[ ] - gRPC mapping
[ ] - JSON support

## Roadmap

The roadmap is for measuring where the project needs to go next. It is the rough outline that describes the project. In a perfect world, the project would have all of these features. But this is not a perfect world. In no particular order, here is the roadmap

[ ] - Implementing the protobuf spec
[ ] - ZGen generates protobuf files
[ ] - MSBuild Generation (inspired by gRPC protobuf files in Visual Studio 2019 & [capnp MSBuild Generation](https://github.com/c80k/capnproto-dotnetcore))