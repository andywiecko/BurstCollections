# BurstCollections

Burst friendly (special) native collections for Unity.

- [BurstCollections](#burstcollections)
  - [Getting started](#getting-started)
  - [NativeStack{T}](#nativestackt)
  - [BoundingVolumeTree{T}](#boundingvolumetreet)
    - [Example usage](#example-usage)
    - [Results](#results)
  - [Dependencies](#dependencies)
  - [TODO](#todo)
  - [Contributors](#contributors)

## Getting started

To use the package choose one of the following:

- Clone or download this repository and then select `package.json` using Package Manager (`Window/Package Manager`).

- Use package manager via git install: `https://github.com/andywiecko/BurstCollections/`.

## NativeStack{T}

Simple implementation of the stack data structure.

Example usage:

```csharp
var stack = new NativeStack<int>(capacity: 10, Allocator.Persistent);

stack.Push(0);
stack.Push(1);
stack.Push(2);

while(stack.TryPop(out var item))
{
    UnityEngine.Debug.Log(item);
} // Expected logs: 2, 1, 0

stack.Dispose();
```

Remarks: implementation probably will be deprecated in the future, when Unity team add stack implementation to `Unity.Collections`.

## BoundingVolumeTree{T}

A bounding volume tree is a data structure that is especially useful as supporting operations during collision detection or ray tracing algorithms.
Briefly, it allows reducing the number of checks from _O(n²)_ to _O(nk)_.

`BoundingVolumeTree{T}` requires that generic parameter `T` implements the `IBoundingVolume<TSelf>` interface.
Currently, the package provides the following implementations

- `AABB` –⁠ [axis-aligned bounding box][aabb] (two dimensional),
- `MBC` –⁠ [minimum bounding circle][mbc].

**Note:** AABB 3d is easy to implement by changing `float2` to `float3` in AABB (2d).

### Example usage

To work with the tree one has to allocate native data by declaring the number of leaves (i.e. number of bounding volume objects from which tree will be constructed):

```csharp
var tree = new BoundingVolumeTree(leaves: 10, Allocator.Persistent);
var volumes = new NativeArray<AABB>(10, Allocator.Persistent);
```

Then to construct the tree (which could be injected into the Unity jobs pipeline with the given `JobHandle dependencies`).
Currently, there is support only for incremental construction of the tree

```csharp
tree.Construct(volumes.AsReadOnly());
```

When the volumes are static (over time) then this is it.
Traverse the tree using _Breadth First Search_ to optimize your algorithms...

```csharp
var bfs = tree.BreadthFirstSearch;
foreach(var (id, nodeAABB) in bfs)
{
  bfs.Traverse(id);
}
```

...or you could use the provided extension in the package to collect all intersections with the given `AABB aabb`

```csharp
tree.GetIntersectionsWithAABB(aabb, result);
```

In the case when your objects are not static, you have to update the leaves volumes during your simulation you could inject this into the Unity jobs pipeline as well)

```csharp
tree.UpdateLeavesVolumes(volumes);
```

Remember to dispose of all native data related to the tree:

```csharp
tree.Dispose();
volumes.Dispose();
```

To summarize below one can find the MWE

```csharp
var tree = new BoundingVolumeTree<AABB>(leavesCount: 4, Allocator.Persistent);
var volumes = new NativeArray<AABB>(4, Allocator.Persistent);

tree.Construct(volumes.AsReadOnly());

// ...

tree.UpdateLeavesVolumes(volumes);

var aabb = new AABB(0, 1);
var result = new NativeList<int>(4, Allocator.Persistent);
tree.GetIntersectionsWithAABB(aabb, result)

// ...

tree.Dispose();
volumes.Dispose();
result.Dispose();
```

**Note:** the current implementation of the tree does not contain the `Optimize()` method as a consequence it is recommended to use the tree only for consistent structures (like the ones presented in the [Results](#results) section).

### Results

Below one can find an example result of the constructed `BoundingVolumeTree{T}` for `AABB` and `MBC`, respectively.
The tree was constructed from triangle mesh marked with blue.
On each frame of the `.gif` animations, one can see the contours of the bounding volumes for the given tree level.

Traversing bounding volume tree made of AABB:
![nyan-cat-aabb](Documentation~/aabb.gif)

Traversing bounding volume tree made of MBC:
![nyan-cat-mbc](Documentation~/mbc.gif)

**Note:** the triangle mesh was obtained using the related package: [**BurstTriangulator**](https://github.com/andywiecko/BurstTriangulator).

## Dependencies

- [`Unity.Burst`](https://docs.unity3d.com/Packages/com.unity.burst@1.6/manual/index.html)
- [`Unity.Mathematics`](https://docs.unity3d.com/Packages/com.unity.mathematics@1.2/manual/index.html)
- [`Unity.Collections`](https://docs.unity3d.com/Packages/com.unity.collections@1.0/manual/index.html)
- [`Unity.Jobs`](https://docs.unity3d.com/Manual/JobSystem.html)

## TODO

- Implement `.Optimize()` for `BoundingVolumeTree{T}`,
- Implement `Depth-first search` for `BoundingVolumeTree{T}`,
- Implement `DynamicBoundingVolumeTree{T}`,
- Implement `NativeQuad/OctTree}`,
- Implement `NativeGrid` (2d/3d),
- Implement `NativeArray2d{T}`,
- CI/CD setup.

## Contributors

- [Andrzej Więckowski, Ph.D](https://andywiecko.github.io/).

[aabb]:https://en.wikipedia.org/wiki/Axis-aligned_bounding_box
[mbc]:https://en.wikipedia.org/wiki/Minimum_bounding_circle
