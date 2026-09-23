// Dump one class's type tree as JSON, for one backend, so two backends (or a before and an
// after) can be compared without going through the native AOT library.
//   TypeTreeDump <unityVersion> <backend> <GameAssembly.dll> <global-metadata.dat> <assembly> <class>
using TypeTreeGeneratorAPI;
using TypeTreeGeneratorAPI.TypeTreeGenerator;

if (args.Length != 6)
{
    Console.Error.WriteLine("usage: TypeTreeDump <unityVersion> <backend> <gameAssembly> <metadata> <assembly> <class>");
    return 2;
}
var (unityVersion, backend, gameAssembly, metadata, assemblyName, className) =
    (args[0], args[1], args[2], args[3], args[4], args[5]);

var handle = new TypeTreeGeneratorHandle(backend, unityVersion);
handle.Instance.LoadIl2Cpp(File.ReadAllBytes(gameAssembly), File.ReadAllBytes(metadata));
var nodes = handle.Instance.GenerateTreeNodes(assemblyName, className);
// Same merge the native API does, so the output matches what the Python wrapper consumes.
if (nodes != null && nodes.Count > 0 && className != "UnityEngine.MonoBehaviour")
{
    var rootNodes = handle.Instance.GetMonoBehaviourRootNodes();
    if (nodes[0].m_Level == 0)
    {
        rootNodes[0] = nodes[0];
        nodes.RemoveAt(0);
    }
    nodes = rootNodes.Concat(nodes).ToList();
}
if (nodes == null || nodes.Count == 0)
{
    Console.Error.WriteLine($"no nodes for {assemblyName}/{className} with {backend}");
    return 1;
}
Console.Error.WriteLine($"{backend}: {nodes.Count} nodes for {assemblyName}/{className}");
Console.WriteLine(TypeTreeNodeSerializer.ToJson(nodes));
return 0;
